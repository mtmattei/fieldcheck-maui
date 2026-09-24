using FieldCheck.Converters;
using FieldCheck.Core.ViewModels;

namespace FieldCheck.Controls;

/// <summary>Compact status label: text plus semantic color (never color alone).</summary>
public partial class StatusChip : Border
{
    private static readonly ToneToColorConverter ToneColors = new();

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(StatusChip), string.Empty, propertyChanged: (b, _, _) => ((StatusChip)b).Update());

    public static readonly BindableProperty ToneProperty =
        BindableProperty.Create(nameof(Tone), typeof(Tone), typeof(StatusChip), Tone.Success, propertyChanged: (b, _, _) => ((StatusChip)b).Update());

    public StatusChip()
    {
        InitializeComponent();
        Update();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Tone Tone
    {
        get => (Tone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    private void Update()
    {
        ChipLabel.Text = Text;
        ChipLabel.TextColor = (Color?)ToneColors.Convert(Tone, typeof(Color), null, System.Globalization.CultureInfo.InvariantCulture);
        BackgroundColor = (Color?)ToneColors.Convert(Tone, typeof(Color), "soft", System.Globalization.CultureInfo.InvariantCulture);
        SemanticProperties.SetDescription(this, $"Status: {Text}");
    }
}
