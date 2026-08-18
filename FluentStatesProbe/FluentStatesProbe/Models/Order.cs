namespace FluentStatesProbe.Models;

public sealed record Order(
    string Id,
    string Customer,
    string TotalDisplay,
    string Status,
    string PlacedOn);
