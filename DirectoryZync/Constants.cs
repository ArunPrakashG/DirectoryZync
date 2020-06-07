using System;
using System.IO;

namespace DirectoryZync {
	internal sealed class Constants {
		internal const string LOCK_FILE = "dir_lock.zynclock";
		internal static string LocalAppDirectory => GetConfigPath(true);
		internal static string ConfigPath => GetConfigPath(false);

		internal const string ConfigName = "config.json";

		private static string GetConfigPath(bool isDirectoryOnly) {
			string appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(DirectoryZync));
			Directory.CreateDirectory(appFolder);
			return isDirectoryOnly ? appFolder : Path.Combine(appFolder, ConfigName);
		}
	}
}
