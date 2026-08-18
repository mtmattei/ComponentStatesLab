using Uno.Extensions.Reactive;

namespace InventoryCardLab;

public partial record MainModel(IInventoryService Inventory)
{
    public IListFeed<InventoryItem> Stock => ListFeed.Async(Inventory.GetStockLevelsAsync);
}
