using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ServerAppDesktop.Controls
{
    public sealed partial class InfoCard : Control
    {
        public InfoCard()
        {
            DefaultStyleKey = typeof(InfoCard);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(IconElement), typeof(InfoCard), new PropertyMetadata(null));
        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(InfoCard), new PropertyMetadata(string.Empty));
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(InfoCard), new PropertyMetadata(string.Empty));
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Padding = new Thickness(16);
            CornerRadius = Application.Current.Resources.TryGetValue("ControlCornerRadius", out var cornerRadiusResource) && cornerRadiusResource is CornerRadius cornerRadius
                ? cornerRadius
                : new CornerRadius(4);
            BorderThickness = new Thickness(1);
        }
    }
}
