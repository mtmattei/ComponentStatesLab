using StationBoardLab.Models;

namespace StationBoardLab.Services;

public sealed class DepartureService
{
    // Dev fixture. The mode is re-read from disk on EVERY call so the board's
    // states can be driven while the app keeps running - that is what makes
    // Retry provable (drive to error, flip the file to ok, click Retry).
    // Modes: ok (default) | slow | empty | error
    private static readonly string ModeFile =
        Path.Combine(Path.GetTempPath(), "stationboard-mode.txt");

    // One line per call, so a click can be shown to have reached the service.
    private static readonly string TraceFile =
        Path.Combine(Path.GetTempPath(), "stationboard-trace.log");

    public async Task<IReadOnlyList<Departure>> GetDeparturesAsync(CancellationToken ct = default)
    {
        var mode = ReadMode();
        Trace(mode);

        await Task.Delay(mode == "slow" ? 6000 : 1400, ct);

        if (mode == "error")
        {
            throw new InvalidOperationException("Station feed unavailable.");
        }

        // Absence is data, never an exception: an empty list is what makes the
        // empty state reachable.
        if (mode == "empty")
        {
            return Array.Empty<Departure>();
        }

        return new List<Departure>
        {
            new("RE 12", "Trois-Rivieres", "18:04", "3", "On time"),
            new("IC 41", "Quebec City", "18:19", "1", "On time"),
            new("RE 7", "Saint-Jerome", "18:26", "5", "Delayed 4 min"),
            new("IC 58", "Ottawa", "18:40", "2", "On time"),
        };
    }

    private static string ReadMode()
    {
        try
        {
            return File.Exists(ModeFile)
                ? File.ReadAllText(ModeFile).Trim().ToLowerInvariant()
                : "ok";
        }
        catch (IOException)
        {
            return "ok";
        }
    }

    private static void Trace(string mode)
    {
        try
        {
            File.AppendAllText(TraceFile, $"{DateTime.Now:HH:mm:ss.fff} call mode={mode}{Environment.NewLine}");
        }
        catch (IOException)
        {
            // Tracing is diagnostic only; never fail a load because of it.
        }
    }
}
