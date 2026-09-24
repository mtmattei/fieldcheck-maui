using System.Windows.Input;

namespace FieldCheck.Controls;

/// <summary>Loading / empty / no-results / error presentation with an optional action (Retry, Clear search).</summary>
public partial class StatePanel : VerticalStackLayout
{
    public static readonly BindableProperty TitleProperty = Create<string>(nameof(Title), string.Empty);
    public static readonly BindableProperty MessageProperty = Create<string>(nameof(Message), string.Empty);
    public static readonly BindableProperty ActionTextProperty = Create<string>(nameof(ActionText), string.Empty);
    public static readonly BindableProperty ActionCommandProperty = Create<ICommand?>(nameof(ActionCommand), null);
    public static readonly BindableProperty IsBusyProperty = Create<bool>(nameof(IsBusy), false);

    public StatePanel()
    {
        InitializeComponent();
    }

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Message { get => (string)GetValue(MessageProperty); set => SetValue(MessageProperty, value); }
    public string ActionText { get => (string)GetValue(ActionTextProperty); set => SetValue(ActionTextProperty, value); }
    public ICommand? ActionCommand { get => (ICommand?)GetValue(ActionCommandProperty); set => SetValue(ActionCommandProperty, value); }
    public bool IsBusy { get => (bool)GetValue(IsBusyProperty); set => SetValue(IsBusyProperty, value); }

    private static BindableProperty Create<T>(string name, T defaultValue) =>
        BindableProperty.Create(name, typeof(T), typeof(StatePanel), defaultValue, propertyChanged: (b, _, _) => ((StatePanel)b).Update());

    private void Update()
    {
        Spinner.IsVisible = Spinner.IsRunning = IsBusy;
        TitleLabel.Text = Title;
        TitleLabel.IsVisible = !string.IsNullOrEmpty(Title);
        MessageLabel.Text = Message;
        MessageLabel.IsVisible = !string.IsNullOrEmpty(Message);
        ActionButton.Text = ActionText;
        ActionButton.Command = ActionCommand;
        ActionButton.IsVisible = ActionCommand is not null && !string.IsNullOrEmpty(ActionText);
    }
}
