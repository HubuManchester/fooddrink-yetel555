namespace Fooddrink;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Respect system dark mode setting
        UserAppTheme = AppTheme.Unspecified;

        MainPage = new AppShell();
    }
}
