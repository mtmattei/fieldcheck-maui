using Android.Graphics.Drawables;
using Android.Views;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using AColor = Android.Graphics.Color;

namespace FieldCheck.Platforms.Android;

/// <summary>Bottom navigation per DESIGN_SPEC: quiet Surface bar, Ink active item with a small Accent indicator.</summary>
public sealed class FieldCheckShellRenderer : ShellRenderer
{
    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem) =>
        new IndicatorTracker(this, shellItem);

    private sealed class IndicatorTracker(IShellContext context, ShellItem item) : ShellBottomNavViewAppearanceTracker(context, item)
    {
        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            base.SetAppearance(bottomView, appearance);

            var density = bottomView.Context?.Resources?.DisplayMetrics?.Density ?? 1f;
            var surface = AColor.White;

            var indicator = new GradientDrawable();
            indicator.SetColor(AColor.ParseColor("#DFFF45"));
            indicator.SetCornerRadius(2 * density);

            var checkedLayer = new LayerDrawable([new ColorDrawable(surface), indicator]);
            checkedLayer.SetLayerGravity(1, GravityFlags.Top | GravityFlags.CenterHorizontal);
            checkedLayer.SetLayerSize(1, (int)(54 * density), (int)(4 * density));
            checkedLayer.SetLayerInsetTop(1, (int)(8 * density));

            var background = new StateListDrawable();
            background.AddState([global::Android.Resource.Attribute.StateChecked], checkedLayer);
            background.AddState([], new ColorDrawable(surface));

            bottomView.ItemBackground = background;
            bottomView.SetBackgroundColor(surface);
        }
    }
}
