using Assistant.Security;
using DirectoryZync.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryZync {
	internal class DirectoryZyncFileHandler {
		private readonly Logger Logger = new Logger(nameof(DirectoryZyncFileHandler));
		private readonly BackupHandler BackupHandler;

		internal DirectoryZyncFileHandler(BackupHandler _handler) {
			BackupHandler = _handler;
		}

		internal static string GetZyncLockFileFor(string dir) => Path.Combine(dir, Constants.LOCK_FILE);

		private async Task<bool> CreateZyncFilesAsync(DirectoryZyncFile defaultZyncConfig = null) {
			if(!Directory.Exists(BackupHandler.SourceDirectory) || File.Exists(GetZyncLockFileFor(BackupHandler.SourceDirectory))){
				return false;
			}

			DirectoryZyncFile defaultZyncFile = defaultZyncConfig != null ? defaultZyncConfig : new DirectoryZyncFile(DateTime.MinValue, false);
			await WriteZyncFileAsync(defaultZyncFile, BackupHandler.SourceDirectory).ConfigureAwait(false);
			await WriteZyncFileAsync(defaultZyncFile, BackupHandler.TargetDirectory).ConfigureAwait(false);

			return File.Exists(GetZyncLockFileFor(BackupHandler.SourceDirectory)) && File.Exists(GetZyncLockFileFor(BackupHandler.TargetDirectory));
		}

		private async Task<bool> WriteZyncFileAsync(DirectoryZyncFile zyncConfig, string dirPath) {
			if(zyncConfig == null || string.IsNullOrEmpty(dirPath)) {
				return false;
			}

			using (StreamWriter writer = new StreamWriter(File.Create(GetZyncLockFileFor(dirPath)), Encoding.ASCII)) {
				string json = GenerateEncryptedZyncFileJson(zyncConfig);

				if (string.IsNullOrEmpty(json)) {
					return false;
				}

				await writer.WriteAsync(json).ConfigureAwait(false);
				await writer.FlushAsync().ConfigureAwait(false);
			}

			return true;
		}

		private string GenerateEncryptedZyncFileJson(DirectoryZyncFile zyncConfig) {
			if(zyncConfig == null) {
				return null;
			}

			return Crypto.Encrypt(JsonConvert.SerializeObject(zyncConfig));
		}

		private async Task<DirectoryZyncFile> GetDecryptedZyncFileForDirectory(string dir) {
			if(string.IsNullOrEmpty(dir) || !Directory.Exists(dir) || !File.Exists(GetZyncLockFileFor(dir))) {
				return default;
			}

			using(StreamReader reader = new StreamReader(GetZyncLockFileFor(dir))) {
				string encryptedJson = await reader.ReadToEndAsync().ConfigureAwait(false);

				if (string.IsNullOrEmpty(encryptedJson)) {
					return default;
				}

				return JsonConvert.DeserializeObject<DirectoryZyncFile>(Crypto.Decrypt(Encoding.ASCII.GetBytes(encryptedJson)));
			}
		}
	}
}
