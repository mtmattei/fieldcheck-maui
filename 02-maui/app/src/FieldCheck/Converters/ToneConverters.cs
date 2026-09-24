using System.Globalization;
using FieldCheck.Core.ViewModels;

namespace FieldCheck.Converters;

/// <summary>Maps a semantic <see cref="Tone"/> to its foreground (default) or soft background color (parameter "soft").</summary>
public sealed class ToneToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var soft = parameter as string == "soft";
        var key = (value as Tone? ?? Tone.Success) switch
        {
            Tone.Success => soft ? "SoftSuccess" : "Success",
            Tone.Attention => soft ? "SoftAttention" : "Attention",
            _ => soft ? "SoftCritical" : "Critical",
        };
        return Application.Current!.Resources.TryGetValue(key, out var color) ? color : Colors.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not true;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not true;
}

public sealed class IsNotNullOrEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string s ? !string.IsNullOrEmpty(s) : value is not null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
