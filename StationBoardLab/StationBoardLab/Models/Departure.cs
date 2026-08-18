namespace StationBoardLab.Models;

public sealed record Departure(
    string Line,
    string Destination,
    string ScheduledTime,
    string Track,
    string Note);
