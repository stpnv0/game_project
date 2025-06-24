using System;
using Avalonia.Controls;
using ConnectDotsGame.Services;
using NUnit.Framework;

namespace ConnectDotsGame.Tests;

[TestFixture]
public class ModalServiceTests
{
    private class DummyWindow : Window
    {
        public bool DialogShown { get; private set; }
        public object? LastDataContext { get; private set; }
        public void ShowDialog(Window owner)
        {
            DialogShown = true;
        }
        public new object? DataContext
        {
            get => base.DataContext;
            set { base.DataContext = value; LastDataContext = value; }
        }
    }

    [Test]
    public void ShowModal_SetsDataContextAndShowsDialog()
    {
        var window = new DummyWindow();
        var service = new ModalService(window);
        bool buttonClicked = false;
        service.ShowModal("Заголовок", "Сообщение", "Кнопка", () => buttonClicked = true);
        Assert.That(window.DialogShown, Is.True);
        Assert.That(window.LastDataContext, Is.Not.Null);
    }

    [Test]
    public void ShowModal_CallsOnButtonClick()
    {
        var window = new DummyWindow();
        var service = new ModalService(window);
        bool buttonClicked = false;
        // Имитация вызова делегата через ViewModel
        service.ShowModal("Заголовок", "Сообщение", "Кнопка", () => buttonClicked = true);
        // Обычно делегат вызывается из VM, здесь вызываем вручную
        dynamic vm = window.LastDataContext!;
        vm.OnButtonClick();
        Assert.That(buttonClicked, Is.True);
    }

    [Test]
    public void Constructor_ThrowsIfWindowNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ModalService(null!));
    }
}
