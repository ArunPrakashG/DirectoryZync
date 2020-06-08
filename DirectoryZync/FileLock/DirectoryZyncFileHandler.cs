using Assistant.Security;
using DirectoryZync.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryZync.FileLock {
	internal class DirectoryZyncFileHandler {
		private readonly Logger Logger;

		internal DirectoryZyncFileHandler(Logger _logger) {
			Logger = _logger;
		}

		internal static string GetZyncLockFileFor(string dir) => Path.Combine(dir, Constants.LOCK_FILE);

		internal async Task<bool> CreateZyncFilesAsync(string _sourceDir, string _targetDir, DirectoryZyncFile defaultZyncConfig = null) {
			if(string.IsNullOrEmpty(_sourceDir) || string.IsNullOrEmpty(_targetDir)) {
				return false;
			}

			if(!Directory.Exists(_sourceDir)){
				return false;
			}

			DirectoryZyncFile defaultZyncFile = defaultZyncConfig != null ? defaultZyncConfig : new DirectoryZyncFile(DateTime.MinValue, false);
			await WriteZyncFileAsync(defaultZyncFile, _sourceDir).ConfigureAwait(false);
			await WriteZyncFileAsync(defaultZyncFile, _targetDir).ConfigureAwait(false);

			return File.Exists(GetZyncLockFileFor(_sourceDir)) && File.Exists(GetZyncLockFileFor(_targetDir));
		}

		internal async Task<bool> WriteZyncFileAsync(DirectoryZyncFile zyncConfig, string dirPath = null, Stream _stream = null) {
			if(zyncConfig == null || (string.IsNullOrEmpty(dirPath) && _stream == null)) {
				return false;
			}

			using (StreamWriter writer = new StreamWriter(_stream ?? File.Create(GetZyncLockFileFor(dirPath)), Encoding.ASCII, 1024, leaveOpen: _stream != null ? true : false)) {
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
