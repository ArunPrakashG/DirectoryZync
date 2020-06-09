using DirectoryZync.Models;
using Synergy.Extensions;
using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace DirectoryZync.FileLock {
	internal sealed class ZyncLock {
		private readonly DirectoryZyncFileHandler Handler;
		private readonly string ZyncDirectoryPath;
		private readonly bool IsInitSuccess;

		private FileStream ZyncStream;
		private CancellationTokenSource ZyncToken;

		internal ZyncLock(string _zyncDirPath) {
			ZyncDirectoryPath = _zyncDirPath ?? throw new ArgumentNullException(nameof(_zyncDirPath));			
			Handler = new DirectoryZyncFileHandler();
			IsInitSuccess = Handler.WriteZyncFileAsync(new DirectoryZyncFile(DateTime.Now, false), ZyncDirectoryPath).Result;
		}

		internal async Task AcquireLockAsync(CancellationTokenSource timeout = null, CancellationTokenSource autoReleaseToken = null, bool throwOnCancelleation = false) {
			if (!IsInitSuccess) {
				throw new NotImplementedException();
			}

			if(timeout == null) {
				timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
			}

			bool acquired = false;
			while (!timeout.IsCancellationRequested) {
				try {
					ZyncStream = new FileStream(ZyncDirectoryPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
					ZyncToken = autoReleaseToken;
					await UpdateLockFile(true).ConfigureAwait(false);
					acquired = true;

					if (ZyncToken != null && ZyncToken.Token.CanBeCanceled) {
						Helpers.InBackground(async () => {
							while (!ZyncToken.IsCancellationRequested) {
								await Task.Delay(1).ConfigureAwait(false);
							}

							if (ZyncStream == null) {
								// Lock is already released
								return;
							}

							await OnTokenCanceled().ConfigureAwait(false);

							if (throwOnCancelleation) {
								throw new TaskCanceledException("Lock has been released.");
							}
						}, true);
					}
				}
				catch (UnauthorizedAccessException) {
					continue;
				}
				catch (IOException) {
					continue;
				}

				await Task.Delay(1).ConfigureAwait(false);
			}

			if(!acquired && timeout.IsCancellationRequested) {
				throw new OperationCanceledException();
			}
		}

		internal async Task ReleaseLockAsync() {
			if (ZyncStream == null) {
				throw new ObjectDisposedException(nameof(ZyncStream));
			}

			await UpdateLockFile(false).ConfigureAwait(false);
			await ZyncStream.FlushAsync().ConfigureAwait(false);
			await ZyncStream.DisposeAsync().ConfigureAwait(false);
		}

		private async Task OnTokenCanceled() => await ReleaseLockAsync().ConfigureAwait(false);

		private async Task UpdateLockFile(bool isLocked) {
			if (ZyncStream == null) {
				throw new ObjectDisposedException(nameof(ZyncStream));
			}

			await Handler.WriteZyncFileAsync(new DirectoryZyncFile(DateTime.Now, isLocked), null, ZyncStream).ConfigureAwait(false);
		}
	}
}
