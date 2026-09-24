namespace FieldCheck.Controls;

/// <summary>
/// Shows keyboard focus on the FieldCheck input frame (the parent Border) since the native
/// Entry/Editor chrome is removed. Clearing the local value on unfocus restores style/trigger colors.
/// </summary>
public sealed class FocusFrameBehavior : Behavior<InputView>
{
    protected override void OnAttachedTo(InputView bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.Focused += OnFocusChanged;
        bindable.Unfocused += OnFocusChanged;
    }

    protected override void OnDetachingFrom(InputView bindable)
    {
        bindable.Focused -= OnFocusChanged;
        bindable.Unfocused -= OnFocusChanged;
        base.OnDetachingFrom(bindable);
    }

    private static void OnFocusChanged(object? sender, FocusEventArgs e)
    {
        if (sender is not Element element || FindFrame(element) is not { } frame)
        {
            return;
        }

        if (e.IsFocused)
        {
            frame.Stroke = (Color)Application.Current!.Resources["Ink"];
            frame.StrokeThickness = 2;
        }
        else
        {
            frame.ClearValue(Border.StrokeProperty);
            frame.ClearValue(Border.StrokeThicknessProperty);
        }
    }

    private static Border? FindFrame(Element element)
    {
        for (var parent = element.Parent; parent is not null; parent = parent.Parent)
        {
            if (parent is Border border)
            {
                return border;
            }
        }

        return null;
    }
}
