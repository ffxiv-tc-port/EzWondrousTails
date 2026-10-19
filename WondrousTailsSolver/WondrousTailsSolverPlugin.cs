using Dalamud.Plugin;
using KamiToolKit;

namespace WondrousTailsSolver;

public sealed class WondrousTailsSolverPlugin : IDalamudPlugin {
    private readonly IpcProvider ipcProvider;

    public WondrousTailsSolverPlugin(IDalamudPluginInterface pluginInterface) {
        // Load the zh-TW store first: AddonWeeklyBingoController builds its probability
        // SeString during construction, so anything later would render English once.
        Localization.Init(pluginInterface.AssemblyLocation.DirectoryName);

        System.PerfectTails = new PerfectTails();
        System.NativeController = new NativeController(pluginInterface);
        System.AddonWeeklyBingoController = new AddonWeeklyBingoController(pluginInterface);

        // Registered last: the endpoints read System.PerfectTails.
        this.ipcProvider = new IpcProvider(pluginInterface);
    }

    public void Dispose() {
        this.ipcProvider.Dispose();
        System.AddonWeeklyBingoController.Dispose();
        System.NativeController.Dispose();
    }
}