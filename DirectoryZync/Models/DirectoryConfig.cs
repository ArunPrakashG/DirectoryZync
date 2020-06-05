using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DirectoryZync.Models {
	internal sealed class DirectoryConfig {
		[JsonProperty]
		internal bool ShouldBackup { get; set; }

		[JsonProperty]
		internal bool EnableDirectorySync { get; set; }

		[JsonProperty]
		internal DateTime LastBackupTime { get; set; }

		[JsonProperty]
		internal DateTime LastSyncTime { get; set; }

		[JsonProperty]
		internal List<string> IgnoreList { get; set; }

		[JsonProperty]
		internal string SourceDirectory { get; set; }

		[JsonProperty]
		internal string TargetDirectory { get; set; }

		[JsonProperty]
		internal bool SaveAsZip { get; set; }

		[JsonProperty]
		internal long DirectoryConfigID { get; set; }
	}
}
