using MvvmCross.Wpf.Views;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Mermer.Ui.Pc.Views;

public partial class LoginView : MvxWpfView
{
    public LoginView() => InitializeComponent();

    private void LoginView_OnLoaded(object sender, RoutedEventArgs e)
    {
        VersionText.Text = ": " + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
    }
}