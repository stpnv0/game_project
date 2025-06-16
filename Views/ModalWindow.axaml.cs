using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ConnectDotsGame.ViewModels;
using Avalonia.Interactivity;

namespace ConnectDotsGame.Views
{
    public partial class ModalWindow : Window
    {
        public ModalWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            // Находим кнопку и подписываемся на её событие Click
            if (this.FindControl<Button>("CloseButton") is Button closeButton)
            {
                closeButton.Click += CloseButton_Click;
            }
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is ModalWindowViewModel viewModel)
            {
                viewModel.CloseCommand.Execute(null);
                Close();
            }
        }
    }
} 