namespace InventoryCardLab;

public partial record InventoryItem(string Sku, string Name, int UnitsOnHand, int ReorderPoint)
{
    public bool IsLowStock => UnitsOnHand <= ReorderPoint;
}
