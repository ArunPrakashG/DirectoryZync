using System;
using System.Collections.Generic;
using System.IO;

namespace DirectoryZync {
	internal sealed class BackupHandler {
		private readonly Logger Logger = new Logger(nameof(BackupHandler));
		private readonly MainWindow MainWindow;
		private readonly List<string> IgnoreList = new List<string>();

		internal readonly string SourceDirectory;
		internal readonly string TargetDirectory;

		internal BackupHandler(MainWindow _mainWindow, string _sourceDir, string _targetDir) {
			MainWindow = _mainWindow ?? throw new ArgumentNullException(nameof(_mainWindow));
			SourceDirectory = _sourceDir ?? throw new ArgumentNullException(nameof(_sourceDir));
			TargetDirectory = _targetDir ?? throw new ArgumentNullException(nameof(_targetDir));
		}

		private bool IsBackupPossible => Directory.Exists(SourceDirectory);

		private bool InitSourceDirectory() {

		}

		private bool CreateTargetWithInit() {
			
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

		private bool IsZyncLockFile(string file) {
			if (string.IsNullOrEmpty(file) || !File.Exists(file)) {
				return false;
			}

			return new FileInfo(file).Name.Equals(Constants.LOCK_FILE);
		}
	}
}
