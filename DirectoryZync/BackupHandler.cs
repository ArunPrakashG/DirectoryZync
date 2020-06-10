using DirectoryZync.FileLock;
using DirectoryZync.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DirectoryZync {
	internal sealed class BackupHandler {
		private readonly Logger Logger = new Logger(nameof(BackupHandler));
		private readonly MainWindow MainWindow;
		private readonly DirectoryZyncFileHandler DirectoryZyncFileHandler;
		private readonly ZyncLock SourceZyncLock;
		private readonly ZyncLock TargetZyncLock;
		private readonly List<string> IgnoreList = new List<string>();
		private readonly DirectoryConfig DirectoryConfig;

		internal BackupHandler(MainWindow _mainWindow, DirectoryConfig _directoryConfig) {
			MainWindow = _mainWindow ?? throw new ArgumentNullException(nameof(_mainWindow));
			DirectoryConfig = _directoryConfig ?? throw new ArgumentNullException(nameof(_directoryConfig));
			ValidateConfig();
			DirectoryZyncFileHandler = new DirectoryZyncFileHandler();
			SourceZyncLock = new ZyncLock(DirectoryConfig.SourceDirectory, DirectoryZyncFileHandler);
			TargetZyncLock = new ZyncLock(DirectoryConfig.TargetDirectory, DirectoryZyncFileHandler);
			IgnoreList = DirectoryConfig.IgnoreList ?? new List<string>();
			DirectoryConfig.DirectoryConfigID = GenerateDirectoryID();
		}

		private void ValidateConfig() {
			if (string.IsNullOrEmpty(DirectoryConfig.SourceDirectory) || !Directory.Exists(DirectoryConfig.SourceDirectory)) {
				throw new InvalidOperationException("Source directory cannot be invalid.");
			}

			if (string.IsNullOrEmpty(DirectoryConfig.TargetDirectory)) {
				throw new NullReferenceException(nameof(DirectoryConfig.TargetDirectory) + " cannot be null.");
			}
		}

		private bool IsBackupPossible => Directory.Exists(DirectoryConfig.SourceDirectory);

		private string GetTargetPath(string item, string targetDirectory = null) => Path.Combine(targetDirectory ?? DirectoryConfig.TargetDirectory, item);

		internal async Task RunBackup() {
			if (!Directory.Exists(DirectoryConfig.SourceDirectory)) {
				return;
			}

			await SourceZyncLock.AcquireLockAsync().ConfigureAwait(false);

			try {
				ProcessFilesForDirectory(DirectoryConfig.SourceDirectory);
				ProcessSubDirectories();
			}
			finally {
				await SourceZyncLock.ReleaseLockAsync().ConfigureAwait(false);
				DirectoryConfig.LastBackupTime = DateTime.Now;
			}
		}

		internal async Task InitDirectorySync() {

		}

		private void ProcessSubDirectories() {
			foreach (string dir in GetSubDirectories(DirectoryConfig.SourceDirectory)) {
				if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) {
					continue;
				}

				try {
					DirectoryInfo dirInfo = new DirectoryInfo(dir);
					Directory.CreateDirectory(GetTargetPath(dirInfo.Name));
					ProcessFilesForDirectory(dir);
				}
				catch (Exception e) {
					Logger.Error($"Error occurred while processing directory -> {dir}");
					Logger.Exception(e);
					continue;
				}
			}
		}

		private void ProcessFilesForDirectory(string directory) {
			if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory)) {
				return;
			}

			foreach (string file in GetFiles(directory)) {
				if (string.IsNullOrEmpty(file) || !File.Exists(file)) {
					continue;
				}

				try {
					FileInfo fileInfo = new FileInfo(file);

					if (ShouldIgnore(fileInfo)) {
						Logger.Info($"Ignoring file -> {fileInfo.Name}");
						continue;
					}

					File.Copy(file, GetTargetPath(fileInfo.Name, directory), true);
				}
				catch (Exception e) {
					Logger.Error($"Error occurred while copying file -> {file}");
					Logger.Exception(e);
					continue;
				}
			}
		}

		private bool ShouldIgnore(FileInfo file) {
			if (file == null || IgnoreList.Count <= 0) {
				return true;
			}

			return IgnoreList.Where(x =>
				x.Equals(file.Name, StringComparison.OrdinalIgnoreCase) ||
				x.Equals(file.Extension, StringComparison.OrdinalIgnoreCase)).Count() > 0;
		}

		[Obsolete]
		private string ParseWildCards(string ignoreFormat) {
			if (string.IsNullOrEmpty(ignoreFormat)) {
				return default;
			}


			for (int i = 0; i < ignoreFormat.Length; i++) {
				char c = ignoreFormat[i];

				// *.ext
				if ((c == '*') && (ignoreFormat[i + 1] == '.')) {
					return ignoreFormat.Substring(ignoreFormat.IndexOf(ignoreFormat[i + 1]));
				}
			}

			return default;
		}

		private IEnumerable<string> GetFiles(string directory) {
			if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory)) {
				yield return "";
			}

			string[] files = Directory.GetFiles(directory, "*", new EnumerationOptions() {
				ReturnSpecialDirectories = false
			});

			for (int i = 0; i < files.Length; i++) {
				if (string.IsNullOrEmpty(files[i]) || !File.Exists(files[i])) {
					continue;
				}

				if (IsZyncLockFile(files[i])) {
					continue;
				}

				yield return files[i];
			}
		}

		private IEnumerable<string> GetSubDirectories(string path) {
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) {
				yield return "";
			}

			string[] subDirs = Directory.GetDirectories(path, "*", new EnumerationOptions() {
				ReturnSpecialDirectories = false
			});

			for (int i = 0; i < subDirs.Length; i++) {
				if (string.IsNullOrEmpty(subDirs[i]) || !Directory.Exists(subDirs[i])) {
					continue;
				}

				yield return subDirs[i];
			}
		}

		private static long GenerateDirectoryID() => DateTime.Now.Ticks + DateTime.Now.AddMilliseconds(new Random().NextDouble()).Ticks;

		private bool IsZyncLockFile(string file) {
			if (string.IsNullOrEmpty(file) || !File.Exists(file)) {
				return false;
			}

			return new FileInfo(file).Name.Equals(Constants.LOCK_FILE);
		}
	}
}
