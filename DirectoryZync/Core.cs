using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryZync {
	internal sealed class Core {
		private readonly Logger Logger = new Logger(nameof(Core));
		private readonly MainWindow MainWindowInstance;
		private readonly Config Config;

		internal static bool IsInDebugMode { get; private set; }

		internal Core(MainWindow _mainWindow) {
			MainWindowInstance = _mainWindow;
			Config = new Config();
		}


	}
}
