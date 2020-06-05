using Assistant.Security;
using DirectoryZync.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirectoryZync {
	internal sealed class Config {
		[JsonProperty]
		internal int BackupInterval { get; set; }

		[JsonProperty]
		internal bool StartOnBoot { get; set; }

		[JsonProperty]
		internal bool RunMinimizedToTrayOnStartup { get; set; }

		[JsonProperty]
		internal List<DirectoryConfig> DirectoryConfigCollection { get; set; }

		private static readonly SemaphoreSlim Sync = new SemaphoreSlim(1, 1);

		internal async Task<bool> LoadAsync() {
			if (!File.Exists(Constants.ConfigPath)) {
				Config _defaultConfig = new Config() {
					BackupInterval = 3,
					DirectoryConfigCollection = new List<DirectoryConfig>(),
					RunMinimizedToTrayOnStartup = true,
					StartOnBoot = true
				};

				await SaveAsync(_defaultConfig).ConfigureAwait(false);
			}

			await Sync.WaitAsync().ConfigureAwait(false);

			try {
				using (StreamReader reader = new StreamReader(Constants.ConfigPath)) {
					string configString = await reader.ReadToEndAsync();

					if (string.IsNullOrEmpty(configString) || configString.Length < 10) {
						return false;
					}

					configString = Crypto.Decrypt(Encoding.UTF8.GetBytes(configString)) ?? throw new InvalidOperationException(nameof(configString) + " failed to decrypt.");
					Config tempConfig = JsonConvert.DeserializeObject<Config>(configString);

					if (tempConfig == null) {
						return false;
					}

					BackupInterval = tempConfig.BackupInterval;
					StartOnBoot = tempConfig.StartOnBoot;
					RunMinimizedToTrayOnStartup = tempConfig.RunMinimizedToTrayOnStartup;
					DirectoryConfigCollection = tempConfig.DirectoryConfigCollection;
					return true;
				}
			}
			finally {
				Sync.Release();
			}			
		}

		internal async Task<bool> SaveAsync() {
			if (string.IsNullOrEmpty(Constants.LocalAppDirectory)) {
				return false;
			}

			await Sync.WaitAsync().ConfigureAwait(false);

			try {
				using(StreamWriter writer = new StreamWriter(Constants.ConfigPath)) {
					string encryptedJson = Crypto.Encrypt(JsonConvert.SerializeObject(this, Formatting.Indented));

					if (string.IsNullOrEmpty(encryptedJson)) {
						return false;
					}

					await writer.WriteAsync(encryptedJson);
					await writer.FlushAsync();
					return true;
				}
			}
			finally {
				Sync.Release();
			}
		}

		internal static async Task<bool> SaveAsync(Config config) {
			if (string.IsNullOrEmpty(Constants.LocalAppDirectory)) {
				return false;
			}

			await Sync.WaitAsync().ConfigureAwait(false);

			try {
				using (StreamWriter writer = new StreamWriter(Constants.ConfigPath)) {
					string encryptedJson = Crypto.Encrypt(JsonConvert.SerializeObject(config, Formatting.Indented));

					if (string.IsNullOrEmpty(encryptedJson)) {
						return false;
					}

					await writer.WriteAsync(encryptedJson);
					await writer.FlushAsync();
					return true;
				}
			}
			finally {
				Sync.Release();
			}
		}
	}
}
