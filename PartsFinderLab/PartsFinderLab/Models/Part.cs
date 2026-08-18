namespace PartsFinderLab.Models;

public sealed record Part(
    string Sku,
    string Name,
    string Bin,
    int OnHand);

public sealed record WarehouseInfo(
    string Name,
    string City,
    string LastSync);
