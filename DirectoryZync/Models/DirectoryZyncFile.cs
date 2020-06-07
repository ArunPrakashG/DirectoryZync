using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryZync.Models {
	internal class DirectoryZyncFile {
		[JsonProperty]
		internal DateTime LastLockTime { get; set; }

		[JsonProperty]
		internal bool IsZyncLocked { get; set; }

		internal DirectoryZyncFile(DateTime _lastLockTime, bool _isZyncLocked) {
			LastLockTime = _lastLockTime;
			IsZyncLocked = _isZyncLocked;
		}

		[JsonConstructor]
		internal DirectoryZyncFile() { }
	}
}
