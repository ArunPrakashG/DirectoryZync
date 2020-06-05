using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DirectoryZync {
	internal class Logger {
		private readonly string Identifier;
		private const string LOG_FILE = "log.txt";
		private static readonly SemaphoreSlim FileSemaphore = new SemaphoreSlim(1, 1);

		internal Logger(string? _identifier = null) => Identifier = _identifier ?? "NA";

		internal void Info(string? msg, [CallerMemberName] string? methodName = null, [CallerLineNumber] int lineNumber = 0, bool skipToFile = false) {
			if (string.IsNullOrEmpty(msg)) {
				return;
			}

			Console.WriteLine($"{DateTime.Now.ToString()} [ {Identifier} ] {msg}");

			if (!skipToFile) {
				ToFile($"{methodName}() | ln {lineNumber} | INFO | {DateTime.Now.ToString()} [ {Identifier} ] {msg}");
			}			
		}

		internal void Trace(string? msg, [CallerMemberName] string? methodName = null, [CallerLineNumber] int lineNumber = 0) {
			if (string.IsNullOrEmpty(msg)) {
				return;
			}

			if (Core.IsInDebugMode) {
				Info($"DEBUG | {msg}", methodName, lineNumber, true);
			}

			ToFile($"{methodName}() | ln {lineNumber} | TRACE | {DateTime.Now.ToString()} [ {Identifier} ] {msg}");
		}

		internal void Error(string? msg, [CallerMemberName] string? methodName = null, [CallerLineNumber] int lineNumber = 0) {
			if (string.IsNullOrEmpty(msg)) {
				return;
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"{DateTime.Now.ToString()} [ {Identifier} ] {msg}");
			Console.ResetColor();
			ToFile($"{methodName}() | ln {lineNumber} | ERROR | {DateTime.Now.ToString()} [ {Identifier} ] {msg}");
		}

		internal void Exception(Exception? e, [CallerMemberName] string? methodName = null, [CallerLineNumber] int lineNumber = 0) {
			if (e == null) {
				return;
			}

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine($"{DateTime.Now.ToString()} [ {Identifier} ] {e.Message}");
			Console.ResetColor();
			ToFile($"{methodName}() | ln {lineNumber} | ERROR | {DateTime.Now.ToString()} [ {Identifier} ] {e.Message} \n {e.StackTrace}");
		}

		private void ToFile(string? msg) {
			if (string.IsNullOrEmpty(msg)) {
				return;
			}

			FileSemaphore.Wait();

			try {
				using (StreamWriter writer = new StreamWriter(LOG_FILE, true)) {
					writer.WriteLine(msg);
					writer.Flush();
				}
			}
			catch (Exception) { }
			finally {
				FileSemaphore.Release();
			}
		}
	}
}
