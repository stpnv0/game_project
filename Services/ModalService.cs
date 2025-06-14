using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.Platform;

namespace ConnectDotsGame.Services
{
    public class ModalService : IModalService
    {
        private readonly ContentControl _contentControl;
        private static readonly Color PrimaryColor = Color.Parse("#E106D9");
        private static readonly Color BackgroundColor = Color.Parse("#1A1A1A");
        private static readonly Color BorderColor = Color.Parse("#2A2A2A");

        public ModalService(ContentControl contentControl)
        {
            _contentControl = contentControl ?? throw new ArgumentNullException(nameof(contentControl));
        }

        public void ShowModal(string title, string message, string buttonText, Action onButtonClick)
        {
            var modalWindow = new Window
            {
                Title = title,
                Width = 420,
                Height = 220,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SystemDecorations = SystemDecorations.None,
                Background = new SolidColorBrush(BackgroundColor),
                Content = new Border
                {
                    BorderBrush = new SolidColorBrush(BorderColor),
                    BorderThickness = new Thickness(2),
                    Child = new StackPanel
                    {
                        Spacing = 25,
                        Margin = new Thickness(25),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = message,
                                FontSize = 22,
                                FontWeight = FontWeight.Medium,
                                Foreground = Brushes.White,
                                TextAlignment = TextAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                TextWrapping = TextWrapping.Wrap,
                                MaxWidth = 350
                            },
                            new Button
                            {
                                Content = new TextBlock
                                {
                                    Text = buttonText,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    FontSize = 18,
                                    FontWeight = FontWeight.SemiBold,
                                    Foreground = Brushes.White
                                },
                                MinWidth = 180,
                                Height = 45,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Padding = new Thickness(25, 0),
                                Cursor = new Cursor(StandardCursorType.Hand),
                                Background = new SolidColorBrush(PrimaryColor)
                            }
                        }
                    }
                }
            };

            var button = ((StackPanel)((Border)modalWindow.Content).Child).Children.OfType<Button>().First();
            
            button.PointerEntered += (_, _) =>
            {
                button.Background = new SolidColorBrush(Color.Parse("#F516E6"));
            };
            
            button.PointerExited += (_, _) =>
            {
                button.Background = new SolidColorBrush(PrimaryColor);
            };

            button.Click += (_, _) =>
            {
                modalWindow.Close();
                onButtonClick();
            };

            if (_contentControl.Parent is Window parentWindow)
            {
                modalWindow.ShowDialog(parentWindow);
            }
            else
            {
                modalWindow.Show();
            }
        }
    }
} 