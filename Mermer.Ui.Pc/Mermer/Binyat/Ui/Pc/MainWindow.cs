using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using MvvmCross.Localization;
using MvvmCross.Platform;
using Mermer.Common.Settings;
using Mermer.Ui.Pc.Helpers;
using Mermer.Ui.Pc.Services;
using Mermer.Services;
using Squirrel;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Orientation = System.Windows.Controls.Orientation;

namespace Mermer.Ui.Pc;

public partial class MainWindow : DXTabbedWindow
{
    public static MainWindow Instance { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        GridExtender.MainForm = this;
        ShortcutExtender.MainForm = this;
    }

    public ContentControl Root => ViewsRoot;

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!Mvx.Resolve<IConfigurator>().GetConfig<UpdateSettings>().CheckForUpdates)
            return;
        await CheckForUpdates(false);
    }

    public async Task CheckForUpdates(bool report = true)
    {
        try
        {
            MvxLanguageBinder localizer = new MvxLanguageBinder(GetType());
            bool? hasUpdates = await AppUpdaterService.CheckForUpdatesAsync();

            if (!hasUpdates.HasValue)
            {
                if (report)
                {
                    ShowMessage(localizer.GetText("Error Checking For Updates"), localizer.GetText("An error ocured while checking for updates, try again later!"));
                }
            }
            else
            {
                if (hasUpdates.Value)
                {
                    bool? doUpdate = ShowMessage(localizer.GetText("Update Available"), localizer.GetText($"An updated version of this application is available.{Environment.NewLine}Would you like to update now?{Environment.NewLine}{Environment.NewLine}New Version: {{0}}", new object[] { AppUpdaterService.UpdateVersion }), MessageBoxButton.YesNo);

                    if (doUpdate.HasValue && doUpdate.Value)
                    {
                        ReleaseEntry releaseEntry = await AppUpdaterService.UpdateAsync();
                        ShowMessage(localizer.GetText("Application Updated"), localizer.GetText($"Application has been updated to version: {{0}}{Environment.NewLine}You must restart application for changes to take effect.", new object[] { releaseEntry.Version }));
                    }
                }
                else if (report)
                {
                    ShowMessage(localizer.GetText("No Updates Available"), localizer.GetText("No updates found, Application is up to date!"));
                }
            }
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    private bool? ShowMessage(string caption, string message, MessageBoxButton button = MessageBoxButton.OK)
    {
        StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
        stackPanel.Children.Add(new TextBlock { Text = message });

        WinUIDialogWindow winUiDialogWindow = new WinUIDialogWindow(caption, button)
        {
            Content = stackPanel
        };
        ThemeManager.SetThemeName(winUiDialogWindow, "Office2013DarkGray;Touch");
        return winUiDialogWindow.ShowDialog();
    }
}