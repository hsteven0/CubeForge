using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CubeForge.Controls
{
    public class AnimatedExpander : UserControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(string),
                typeof(AnimatedExpander),
                new PropertyMetadata("Section", OnHeaderChanged)
            );

        public static readonly DependencyProperty ExpanderContentProperty =
            DependencyProperty.Register(
                nameof(ExpanderContent),
                typeof(object),
                typeof(AnimatedExpander),
                new PropertyMetadata(null, OnExpanderContentChanged)
            );

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(
                nameof(IsExpanded),
                typeof(bool),
                typeof(AnimatedExpander),
                new PropertyMetadata(true, OnIsExpandedChanged)
            );

        private readonly TextBlock headerTextBlock;
        private readonly TextBlock arrowTextBlock;
        private readonly RotateTransform arrowRotateTransform;

        private readonly Border headerBorder;
        private readonly Border outerBorder;
        private readonly Border contentClipper;
        private readonly ContentPresenter contentPresenter;
        private readonly ScaleTransform contentScaleTransform;

        private bool isAnimating = false;

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public object ExpanderContent
        {
            get => GetValue(ExpanderContentProperty);
            set => SetValue(ExpanderContentProperty, value);
        }

        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public AnimatedExpander()
        {
            Margin = new Thickness(0, 0, 0, 16);

            headerTextBlock = new TextBlock
            {
                Text = Header,
                Foreground = GetBrush("TextPrimaryBrush", Brushes.White),
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                LineHeight = 22
            };

            arrowRotateTransform = new RotateTransform(0);

            arrowTextBlock = new TextBlock
            {
                Text = "▼",
                Foreground = GetBrush("TextPrimaryBrush", Brushes.White),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = arrowRotateTransform
            };

            Grid headerGrid = new Grid
            {
                MinHeight = 32,
                Background = Brushes.Transparent
            };

            headerGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });

            headerGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });

            Grid.SetColumn(headerTextBlock, 0);
            Grid.SetColumn(arrowTextBlock, 1);

            headerGrid.Children.Add(headerTextBlock);
            headerGrid.Children.Add(arrowTextBlock);

            Button headerButton = new Button
            {
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(12, 8, 12, 8),
                MinHeight = 44,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
                Content = headerGrid,
                Cursor = System.Windows.Input.Cursors.Hand,
                FocusVisualStyle = null,
                Template = CreateHeaderButton()
            };

            headerButton.Click += OnHeaderClicked;
            headerButton.MouseEnter += OnHeaderMouseEnter;
            headerButton.MouseLeave += OnHeaderMouseLeave;
            headerButton.PreviewMouseDown += OnHeaderMouseDown;
            headerButton.PreviewMouseUp += OnHeaderMouseUp;

            headerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0xCC, 0x0D, 0x0D, 0x0D)),
                CornerRadius = new CornerRadius(6, 6, 0, 0),
                Child = headerButton
            };

            contentScaleTransform = new ScaleTransform(1, 1);

            contentPresenter = new ContentPresenter
            {
                Margin = new Thickness(12, 12, 12, 12),
                Content = ExpanderContent,
                Opacity = 1,
                RenderTransformOrigin = new Point(0.5, 0),
                RenderTransform = contentScaleTransform
            };

            contentClipper = new Border
            {
                ClipToBounds = true,
                Height = 0,
                Background = new SolidColorBrush(Color.FromArgb(0xCC, 0x0D, 0x0D, 0x0D)),
                Child = contentPresenter
            };

            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(headerBorder);
            stackPanel.Children.Add(contentClipper);

            outerBorder = new Border
            {
                Background = GetBrush("CardBrush", new SolidColorBrush(Color.FromRgb(37, 40, 48))),
                CornerRadius = new CornerRadius(6),
                BorderBrush = GetBrush("SubtleBorderBrush", new SolidColorBrush(Color.FromRgb(58, 61, 69))),
                BorderThickness = new Thickness(1),
                Child = stackPanel
            };

            Content = outerBorder;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshColors();

            headerTextBlock.Text = Header;
            contentPresenter.Content = ExpanderContent;

            if (IsExpanded)
            {
                contentClipper.Height = GetContentHeight();
                contentScaleTransform.ScaleY = 1;
                contentPresenter.Opacity = 1;
                arrowRotateTransform.Angle = 0;
            }
            else
            {
                contentClipper.Height = 0;
                contentScaleTransform.ScaleY = 0;
                contentPresenter.Opacity = 0;
                arrowRotateTransform.Angle = -90;
            }
        }

        private ControlTemplate CreateHeaderButton()
        {
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.Name = "ButtonBorder";
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(0));
            border.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Button.PaddingProperty));

            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, new TemplateBindingExtension(Button.HorizontalContentAlignmentProperty));
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, new TemplateBindingExtension(Button.VerticalContentAlignmentProperty));

            border.AppendChild(contentPresenter);

            ControlTemplate template = new ControlTemplate(typeof(Button));
            template.VisualTree = border;

            return template;
        }

        private void OnHeaderMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            headerBorder.Background = new SolidColorBrush(Color.FromRgb(26, 26, 26));
        }

        private void OnHeaderMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            headerBorder.Background = new SolidColorBrush(Color.FromArgb(0xCC, 0x0D, 0x0D, 0x0D));
        }

        private void OnHeaderMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            headerBorder.Background = new SolidColorBrush(Color.FromRgb(34, 34, 34));
        }

        private void OnHeaderMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            headerBorder.Background = new SolidColorBrush(Color.FromRgb(26, 26, 26));
        }

        private void OnHeaderClicked(object sender, RoutedEventArgs e)
        {
            if (isAnimating)
                return;

            IsExpanded = !IsExpanded;
        }

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnimatedExpander expander)
            {
                expander.headerTextBlock.Text = e.NewValue?.ToString() ?? "Section";
            }
        }

        private static void OnExpanderContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnimatedExpander expander)
            {
                expander.contentPresenter.Content = e.NewValue;
                expander.RefreshHeight();
            }
        }

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnimatedExpander expander && expander.IsLoaded)
            {
                expander.AnimateExpansion((bool)e.NewValue);
            }
        }

        private void AnimateExpansion(bool expand)
        {
            if (isAnimating)
                return;

            isAnimating = true;

            contentClipper.BeginAnimation(HeightProperty, null);
            contentScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            contentPresenter.BeginAnimation(OpacityProperty, null);

            double fromHeight = contentClipper.ActualHeight;
            double toHeight = expand ? GetContentHeight() : 0;

            var heightAnimation = new DoubleAnimation
            {
                From = fromHeight,
                To = toHeight,
                Duration = TimeSpan.FromMilliseconds(expand ? 280 : 380),
                EasingFunction = new QuadraticEase
                {
                    EasingMode = expand ? EasingMode.EaseOut : EasingMode.EaseIn
                }
            };

            var scaleAnimation = new DoubleAnimation
            {
                To = expand ? 1 : 0,
                Duration = TimeSpan.FromMilliseconds(expand ? 240 : 360),
                EasingFunction = new QuadraticEase
                {
                    EasingMode = expand ? EasingMode.EaseOut : EasingMode.EaseIn
                }
            };

            var opacityAnimation = new DoubleAnimation
            {
                To = expand ? 1 : 0,
                Duration = TimeSpan.FromMilliseconds(expand ? 180 : 280)
            };

            heightAnimation.Completed += (s, e) =>
            {
                contentClipper.Height = toHeight;
                isAnimating = false;
            };

            contentClipper.BeginAnimation(HeightProperty, heightAnimation);
            contentScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            contentPresenter.BeginAnimation(OpacityProperty, opacityAnimation);

            var arrowAnimation = new DoubleAnimation
            {
                To = expand ? 0 : -90,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            arrowRotateTransform.BeginAnimation(RotateTransform.AngleProperty, arrowAnimation);
        }

        public void RefreshHeight()
        {
            if (!IsExpanded || isAnimating)
                return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                contentClipper.BeginAnimation(HeightProperty, null);
                contentClipper.Height = GetContentHeight();
            }));
        }

        private double GetContentHeight()
        {
            contentPresenter.Measure(new Size(ActualWidth > 0 ? ActualWidth : 350, double.PositiveInfinity));
            return contentPresenter.DesiredSize.Height;
        }

        public void RefreshLayout()
        {
            if (!IsExpanded || isAnimating) return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                contentPresenter.Measure(new Size(ActualWidth > 0 ? ActualWidth : 350, double.PositiveInfinity));

                double newHeight = contentPresenter.DesiredSize.Height;

                contentClipper.BeginAnimation(HeightProperty, null);
                contentClipper.Height = newHeight;

            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void RefreshColors()
        {
            Brush normalBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0x0D, 0x0D, 0x0D));

            outerBorder.Background = normalBrush;
            headerBorder.Background = normalBrush;
            contentClipper.Background = normalBrush;

            outerBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(45, 45, 45));

            headerTextBlock.Foreground = GetBrush("TextPrimaryBrush", Brushes.White);
            arrowTextBlock.Foreground = GetBrush("TextPrimaryBrush", Brushes.White);
        }

        private Brush GetBrush(string key, Brush fallback)
        {
            return TryFindResource(key) as Brush ?? fallback;
        }
    }
}