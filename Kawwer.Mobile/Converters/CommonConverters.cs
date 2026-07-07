using System.Globalization;

namespace Kawwer.Mobile.Converters;

/// <summary>Inverts a boolean (e.g. enable a button while not busy).</summary>
public sealed class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;
}

/// <summary>True when a string has content. Used to toggle error banners.</summary>
public sealed class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>True when an integer is greater than zero. Used for count badges.</summary>
public sealed class IntGreaterThanZeroConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i > 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>True when the value is not null. Used to toggle hero cards.</summary>
public sealed class NotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>True when an integer is at least the given parameter. Drives star rating fills.</summary>
public sealed class IntAtLeastConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && parameter is string s && int.TryParse(s, out var threshold) && i >= threshold;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Maps a boolean to one of two colors, given as "trueHex|falseHex" in the ConverterParameter
/// (e.g. "#CDF564|#00000000"). Used to light up the active side of a segmented toggle.
/// </summary>
public sealed class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var parts = (parameter as string ?? "|").Split('|');
        var hex = value is true ? parts[0] : parts.Length > 1 ? parts[1] : parts[0];
        return Color.FromArgb(hex);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
