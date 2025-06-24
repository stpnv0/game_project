using ConnectDotsGame.ViewModels;
using NUnit.Framework;

namespace ConnectDotsGame.Tests;

[TestFixture]
public class MainPageViewModelTests
{
    private DummyNavigation _navigation;
    private MainPageViewModel _vm;

    [SetUp]
    public void Setup()
    {
        _navigation = new DummyNavigation();
        _vm = new MainPageViewModel(_navigation);
    }

    [Test]
    public void PlayCommand_NavigatesToLevelSelect()
    {
        _vm.PlayCommand.Execute(null);
        Assert.That(_navigation.NavigatedToLevelSelect, Is.True);
    }

    [Test]
    public void AboutCommand_NavigatesToAboutPage()
    {
        _vm.AboutCommand.Execute(null);
        Assert.That(_navigation.NavigatedToAbout, Is.True);
    }

    // Dummy navigation for test
    private class DummyNavigation : Services.INavigation
    {
        public bool NavigatedToLevelSelect;
        public bool NavigatedToAbout;
        public void RegisterView<TViewModel, TView>() { }
        public void NavigateTo<TViewModel>(object? parameter = null)
        {
            if (typeof(TViewModel).Name == "LevelSelectViewModel")
                NavigatedToLevelSelect = true;
            if (typeof(TViewModel).Name == "AboutPageViewModel")
                NavigatedToAbout = true;
        }
    }
}

