using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace WondrousTailsSolver;

/// <summary>
/// Read-only IPC surface: board mask in, probabilities out.
/// </summary>
/// <remarks>
/// Endpoints run on the caller's thread, so they must stay pure. They may only touch the
/// tables PerfectTails freezes in its constructor, never PerfectTails.GameState, which the
/// framework thread rewrites every frame. No throttle and no framework gate is needed.
/// </remarks>
internal sealed class IpcProvider : IDisposable {
    /// <summary>Bumped only when an existing endpoint's shape changes.</summary>
    private const int Version = 1;

    private readonly ICallGateProvider<int> apiVersionProvider;
    private readonly ICallGateProvider<int, double[]> lineChancesProvider;

    public IpcProvider(IDalamudPluginInterface pluginInterface) {
        this.apiVersionProvider = pluginInterface.GetIpcProvider<int>("WondrousTailsSolver.ApiVersion");
        this.apiVersionProvider.RegisterFunc(() => Version);

        this.lineChancesProvider = pluginInterface.GetIpcProvider<int, double[]>("WondrousTailsSolver.GetLineChances");
        this.lineChancesProvider.RegisterFunc(GetLineChances);
    }

    public void Dispose() {
        this.apiVersionProvider.UnregisterFunc();
        this.lineChancesProvider.UnregisterFunc();
    }

    /// <summary>
    /// Chances of 1/2/3 lines for a 16-bit board mask (bit i set = cell i already has a
    /// sticker). All -1 when the mask is not a reachable board, never null.
    /// </summary>
    private static double[] GetLineChances(int mask)
        => System.PerfectTails.GetLineChances(mask);
}
