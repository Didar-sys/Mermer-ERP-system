using MvvmCross.Wpf.Views;
using System.Diagnostics;
using System.Reflection;

namespace Mermer.Ui.Pc.Views;

public partial class AboutView : MvxWpfView
{
    public AboutView()
    {
        InitializeComponent();
        ShowVersionInfo();
    }

    private void ShowVersionInfo()
    {
        VersionText.Text = ": " + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
    }
}