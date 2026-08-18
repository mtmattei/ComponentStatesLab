using System.Collections.ObjectModel;
using System.ComponentModel;
using FluentStatesProbe.Models;
using FluentStatesProbe.Services;

namespace FluentStatesProbe.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly OrderService _orderService;

    private bool _isLoading;
    private bool _hasError;

    public ObservableCollection<Order> Orders { get; } = new();

    public bool IsLoading => _isLoading;

    public bool HasError => !_isLoading && _hasError;

    public bool IsEmpty => !_isLoading && !_hasError && Orders.Count == 0;

    public bool HasData => !_isLoading && !_hasError && Orders.Count > 0;

    public MainViewModel(OrderService orderService)
    {
        _orderService = orderService;
        _ = LoadAsync();
    }

    public void Retry() => _ = LoadAsync();

    private async Task LoadAsync()
    {
        _isLoading = true;
        _hasError = false;
        RaiseStateChanged();

        try
        {
            var orders = await _orderService.GetRecentOrdersAsync();
            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
        }
        catch (Exception)
        {
            _hasError = true;
        }
        finally
        {
            _isLoading = false;
            RaiseStateChanged();
        }
    }

    private void RaiseStateChanged()
    {
        var handler = PropertyChanged;
        if (handler is null)
        {
            return;
        }

        handler(this, new PropertyChangedEventArgs(nameof(IsLoading)));
        handler(this, new PropertyChangedEventArgs(nameof(HasError)));
        handler(this, new PropertyChangedEventArgs(nameof(IsEmpty)));
        handler(this, new PropertyChangedEventArgs(nameof(HasData)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
