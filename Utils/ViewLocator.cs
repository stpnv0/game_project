using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ConnectDotsGame.ViewModels;

namespace ConnectDotsGame.Utils
{
    public class ViewLocator : IDataTemplate
    {
        public Control Build(object? data)
        {
            if (data == null)
                return new TextBlock { Text = "Data is null" };

            var name = data.GetType().FullName!.Replace("ViewModel", "");
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            
            return new TextBlock { Text = $"View not found: {name}" };
        }

        public bool Match(object? data)
        {
            return data is INotifyPropertyChanged;
        }
    }
} 