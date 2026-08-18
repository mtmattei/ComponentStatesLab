using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace PartsFinderLab.Converters;

/// <summary>
/// Visible when the bound value's string form equals the ConverterParameter.
/// Used for the state layers (Loading/Data/Empty/Error) and for plain bool flags
/// (ConverterParameter="True").
/// </summary>
public sealed class EqualsToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var expected = parameter as string;
        var actual = value?.ToString();

        return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
