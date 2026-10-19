using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace WondrousTailsSolver;

/// <summary>
/// Minigame solver.
/// </summary>
public sealed partial class PerfectTails {
    /// <summary>
    /// Largest sticker count the shuffle average is defined for. The game only offers the
    /// shuffle at 3..7 stickers placed (Addon 5630).
    /// </summary>
    private const int MaxShuffleStickers = 7;

    /// <summary>
    /// Lower end of that range; the average itself is still computed from 1 sticker up.
    /// </summary>
    private const int MinShuffleStickers = 3;

    private const int BoardMaskCount = 1 << 16;

    /// <summary>
    /// Masks the precompute reaches: every board with 0..9 stickers, sum of C(16, k). Only a
    /// capacity hint; a wrong value just means the dictionary resizes.
    /// </summary>
    private const int ReachableBoardCount = 50643;

    private readonly FrozenDictionary<int, long[]> possibleBoards;
    private readonly FrozenDictionary<int, double[]> shuffleAverages;

    /// <summary>
    /// The live board, rewritten every frame by the framework thread. Anything that can run
    /// off that thread must take a mask parameter instead of reading this.
    /// </summary>
    public readonly bool[] GameState = new bool[16];
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PerfectTails"/> class.
    /// </summary>
    /// <remarks>
    /// Both tables are filled here and frozen afterwards, so every later read is lock-free
    /// and thread-safe (the IPC endpoints run on the caller's thread).
    /// </remarks>
    public PerfectTails() {
        var boards = new Dictionary<int, long[]>(ReachableBoardCount);
        CalculateBoards(boards, 0, 0, 0, 0, 0);
        this.possibleBoards = boards.ToFrozenDictionary();
        this.shuffleAverages = CalculateShuffleAverages(this.possibleBoards);
    }

    private static double[] Error { get; } = [-1, -1, -1];

    private double[] Solve(bool[] cells) {
        var counts = this.Values(cells);

        if (counts == null)
            return Error;

        var divisor = (double)counts[0];
        var probabilities = counts.Skip(1).Select(c => Math.Round(c / divisor, 4)).ToArray();

        return probabilities;
    }

    /// <summary>
    /// Line chances for a raw 16-bit board mask (bit i set = cell i already has a sticker).
    /// </summary>
    /// <param name="mask">Board mask, bit i = cell i.</param>
    /// <returns>
    /// Chances of 1/2/3 lines, or all -1 when the mask is not a reachable board
    /// (more than 9 stickers, or bits outside the low 16).
    /// </returns>
    /// <remarks>
    /// Pure: reads only the frozen table, so it is callable from any thread. Hands back a
    /// fresh array rather than the shared <see cref="Error"/> instance, which a caller
    /// could otherwise mutate.
    /// </remarks>
    public double[] GetLineChances(int mask) {
        if (!this.possibleBoards.TryGetValue(mask, out var counts) || counts[0] == 0)
            return [-1, -1, -1];

        var divisor = (double)counts[0];
        return [
            Math.Round(counts[1] / divisor, 4),
            Math.Round(counts[2] / divisor, 4),
            Math.Round(counts[3] / divisor, 4),
        ];
    }

    private double[] GetShuffleAverage(int stickersPlaced) {
        return this.shuffleAverages.GetValueOrDefault(stickersPlaced, Error);
    }

    private long[]? Values(bool[] cells) {
        return this.possibleBoards.GetValueOrDefault(CellsToMask(cells));
    }

    private static long[] CalculateBoards(Dictionary<int, long[]> boards, int mask, int numStickers, int numRows, int numCols, int numDiags) {
        if (boards.TryGetValue(mask, out var result))
            return result;

        if (numStickers == 9) {
            var lines = numRows + numCols + numDiags;
            return boards[mask] = [
                1,
                lines >= 1 ? 1 : 0,
                lines >= 2 ? 1 : 0,
                lines >= 3 ? 1 : 0,
            ];
        }

        if (numStickers > 9) {
            return boards[mask] = [0, 0, 0, 0];
        }

        result = boards[mask] = [0, 0, 0, 0];

        for (var r = 0; r < 4; r++) {
            for (var c = 0; c < 4; c++) {
                if (MaskHasBit(mask, r, c))
                    continue;

                var nMask = SetMaskBit(mask, r, c);
                var nRows = MaskHasRow(nMask, r) ? 1 : 0;
                var nCols = MaskHasCol(nMask, c) ? 1 : 0;
                var nDiag1 = MaskHasDiag1(nMask) && r == c ? 1 : 0;
                var nDiag2 = MaskHasDiag2(nMask) && r == 3 - c ? 1 : 0;
                var nResult = CalculateBoards(boards, nMask, numStickers + 1, numRows + nRows, numCols + nCols, numDiags + nDiag1 + nDiag2);

                for (var i = 0; i < 4; i++)
                {
                    result[i] += nResult[i];
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Mean line chance over every board with the same sticker count, i.e. all C(16, k)
    /// masks for k in 1..<see cref="MaxShuffleStickers"/> (26,332 of them in total).
    /// </summary>
    /// <remarks>
    /// Exact, not sampled: the shuffle re-places every sticker uniformly at random, so its
    /// expected value is the unweighted mean over those masks. All of them are present in
    /// <paramref name="boards"/>, which holds every mask of up to 9 stickers.
    /// Every row comes out equal to the empty board's chances - that is the right answer,
    /// not a bug: averaging over all k-subsets is the same as drawing 9 cells at random.
    /// </remarks>
    private static FrozenDictionary<int, double[]> CalculateShuffleAverages(FrozenDictionary<int, long[]> boards) {
        var sums = new double[MaxShuffleStickers + 1][];
        var boardCounts = new long[MaxShuffleStickers + 1];

        for (var mask = 0; mask < BoardMaskCount; mask++) {
            var stickersPlaced = BitOperations.PopCount((uint)mask);
            if (stickersPlaced is < 1 or > MaxShuffleStickers)
                continue;

            var counts = boards[mask];
            var divisor = (double)counts[0];
            var sum = sums[stickersPlaced] ??= new double[3];

            for (var i = 0; i < 3; i++)
                sum[i] += counts[i + 1] / divisor;

            boardCounts[stickersPlaced]++;
        }

        var averages = new Dictionary<int, double[]>();
        for (var stickersPlaced = 1; stickersPlaced <= MaxShuffleStickers; stickersPlaced++) {
            var sum = sums[stickersPlaced]!;
            var divisor = (double)boardCounts[stickersPlaced];
            averages[stickersPlaced] = [
                Math.Round(sum[0] / divisor, 4),
                Math.Round(sum[1] / divisor, 4),
                Math.Round(sum[2] / divisor, 4),
            ];
        }

        return averages.ToFrozenDictionary();
    }
}

/// <summary>
/// Static calculations.
/// </summary>
public sealed partial class PerfectTails {
    private static int CellsToMask(bool[] cells) {
        var mask = 0;
        for (var r = 0; r < 4; r++) {
            for (var c = 0; c < 4; c++) {
                if (cells[(r * 4) + c])
                    mask = SetMaskBit(mask, r, c);
            }
        }

        return mask;
    }

    private static int GetMaskBit(int r, int c)
        => 1 << ((4 * r) + c);

    private static int SetMaskBit(int mask, int r, int c)
        => mask | GetMaskBit(r, c);

    private static bool MaskHasBit(int mask, int r, int c)
        => (mask & GetMaskBit(r, c)) == GetMaskBit(r, c);

    // Whole-line bit masks instead of LINQ over the four cells: the board precompute calls these
    // about 1.5 million times per load, and each LINQ call allocated a closure, a delegate and an iterator.
    private const int RowMask = 0x000F;
    private const int ColMask = 0x1111;
    private const int Diag1Mask = 0x8421;
    private const int Diag2Mask = 0x1248;

    private static bool MaskHasAll(int mask, int lineMask)
        => (mask & lineMask) == lineMask;

    private static bool MaskHasRow(int mask, int r)
        => MaskHasAll(mask, RowMask << (4 * r));

    private static bool MaskHasCol(int mask, int c)
        => MaskHasAll(mask, ColMask << c);

    private static bool MaskHasDiag1(int mask)
        => MaskHasAll(mask, Diag1Mask);

    private static bool MaskHasDiag2(int mask)
        => MaskHasAll(mask, Diag2Mask);
}

/// <summary>
/// Getting formatted results
/// </summary>
public sealed unsafe partial class PerfectTails {
    public SeString SolveAndGetProbabilitySeString() {
        var stickersPlaced = PlayerState.Instance()->WeeklyBingoNumPlacedStickers;

        // > 9 returns Error {-1,-1,-1} by the solver
        var values = Solve(this.GameState);

        double[]? samples = null;
        if (stickersPlaced is > 0 and <= MaxShuffleStickers)
            samples = GetShuffleAverage(stickersPlaced);

        if (values == Error) {
            return new SeStringBuilder()
                .AddText("Line Chances:".Loc() + " ")
                .AddUiForeground("error".Loc() + " ", 704)
                .AddUiForeground("error".Loc() + " ", 704)
                .AddUiForeground("error".Loc() + " ", 704)
                .Build();
        }

        var valuePayloads = this.StringFormatDoubles(values);
        var seString = new SeStringBuilder()
            .AddText("Line Chances:".Loc() + " ");

        if (samples != null) {
            foreach (var (value, sample, valuePayload) in Enumerable.Range(0, values.Length).Select(i => (values[i], samples[i], valuePayloads[i]))) {
                const double bound = 0.05;
                var sampleBoundLower = Math.Max(0, sample - bound);
                // var sampleBoundUpper = Math.Min(1, sample + bound);

                if (Math.Abs(value - 1) < 0.1f)
                    seString.AddUiGlow(valuePayload, 2);
                else if (value < 1 && value >= sample)
                    seString.AddUiForeground(valuePayload, 67);
                else if (sample > value && value > sampleBoundLower)
                    seString.AddUiForeground(valuePayload, 66);
                else if (sampleBoundLower > value && value > 0)
                    seString.AddUiForeground(valuePayload, 561);
                else if (value == 0)
                    seString.AddUiForeground(valuePayload, 704);
                else
                    seString.AddText(valuePayload);

                seString.AddText("  ");
            }

            if (stickersPlaced >= MinShuffleStickers) {
                seString.AddText("\r" + "Shuffle Average:".Loc() + " ");
                seString.AddText(string.Join(" ", this.StringFormatDoubles(samples)));
            }
        }
        else {
            seString.AddText(string.Join(" ", valuePayloads));
        }
        
        return seString.Build();
    }

    private string[] StringFormatDoubles(IEnumerable<double> values)
        => values.Select(v => $"{v * 100:F2}%").ToArray();
}