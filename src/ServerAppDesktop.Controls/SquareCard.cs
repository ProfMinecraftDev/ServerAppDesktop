using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ServerAppDesktop.Controls
{
    public sealed partial class SquareCard : Control
    {
        public SquareCard()
        {
            DefaultStyleKey = typeof(SquareCard);
        }

		public IconElement Icon
		{
			get => (IconElement)GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}
		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register(nameof(Icon), typeof(IconElement), typeof(SquareCard), new PropertyMetadata(null));

		public string Subtitle
		{
			get => (string)GetValue(SubtitleProperty);
			set => SetValue(SubtitleProperty, value);
		}
		public static readonly DependencyProperty SubtitleProperty =
			DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(SquareCard), new PropertyMetadata(string.Empty));

		public string Header
		{
			get => (string)GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}
		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register(nameof(Header), typeof(string), typeof(SquareCard), new PropertyMetadata(string.Empty));

		public string Description
		{
			get => (string)GetValue(DescriptionProperty);
			set => SetValue(DescriptionProperty, value);
		}
		public static readonly DependencyProperty DescriptionProperty =
			DependencyProperty.Register(nameof(Description), typeof(string), typeof(SquareCard), new PropertyMetadata(string.Empty));

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Padding = new Thickness(16);
			CornerRadius = Application.Current.Resources.TryGetValue("ControlCornerRadius", out var cornerRadiusResource) && cornerRadiusResource is CornerRadius cornerRadius
				? cornerRadius
				: new CornerRadius(4);
			Background = Application.Current.Resources.TryGetValue("CardBackgroundFillColorDefaultBrush", out var backgroundResource) && backgroundResource is Brush backgroundBrush
				? backgroundBrush
				: throw new InvalidOperationException("Unable to find background brush.");
			BorderThickness = new Thickness(1);
			BorderBrush = Application.Current.Resources.TryGetValue("CardStrokeColorDefaultBrush", out var borderBrushResource) && borderBrushResource is Brush borderBrush
				? borderBrush
				: throw new InvalidOperationException("Unable to find border brush.");
		}
	}
}
