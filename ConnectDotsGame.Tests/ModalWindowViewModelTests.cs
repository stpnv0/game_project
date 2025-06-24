using System;
using ConnectDotsGame.ViewModels;
using NUnit.Framework;

namespace ConnectDotsGame.Tests;

[TestFixture]
public class ModalWindowViewModelTests
{
    [Test]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var vm = new ModalWindowViewModel("Заголовок", "Сообщение", "Кнопка", () => { });
        Assert.That(vm.Title, Is.EqualTo("Заголовок"));
        Assert.That(vm.Message, Is.EqualTo("Сообщение"));
        Assert.That(vm.ButtonText, Is.EqualTo("Кнопка"));
    }

    [Test]
    public void OnButtonClick_InvokesAction()
    {
        bool called = false;
        var vm = new ModalWindowViewModel("Заголовок", "Сообщение", "Кнопка", () => called = true);
        ((RelayCommand)vm.CloseCommand).Execute(null);
        Assert.That(called, Is.True);
    }

    [Test]
    public void Constructor_ThrowsIfActionNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ModalWindowViewModel("Заголовок", "Сообщение", "Кнопка", null));
    }
}
