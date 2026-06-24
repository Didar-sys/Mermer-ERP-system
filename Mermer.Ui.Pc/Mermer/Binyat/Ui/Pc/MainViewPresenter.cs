using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using Mermer.Mvvm.Messages;
using Mermer.Mvvm.ViewModels;
using Mermer.Ui.Core.ViewModels;
using MvvmCross.Core.ViewModels;
using MvvmCross.Wpf.Views.Presenters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mermer.Ui.Pc
{
    public class MainViewPresenter : MvxBaseWpfViewPresenter
    {
        private readonly ContentControl _contentControl;
        private static DXTabControl _tabControl;
        private static readonly ObservableCollection<FrameworkElement> TabItems = new ObservableCollection<FrameworkElement>();
        private static readonly List<WinUIDialogWindow> Dialogs = new List<WinUIDialogWindow>();

        public MainViewPresenter(ContentControl contentControl) => this._contentControl = contentControl;

        public static void SetTabControl(DXTabControl tabControl)
        {
            MainViewPresenter._tabControl = tabControl;
            MainViewPresenter._tabControl.ItemsSource = MainViewPresenter.TabItems;
        }

        public override void Present(FrameworkElement frameworkElement)
        {
            object dataContext = frameworkElement.DataContext;
            if (dataContext is IDialogViewModel dialogViewModel)
            {
                frameworkElement.SetValue(ThemeManager.ThemeNameProperty, "HybridApp");
                WinUIDialogWindow dialog = new WinUIDialogWindow(dialogViewModel.Caption ?? dataContext.GetType().Name);
                dialog.Content = frameworkElement;
                dialog.SetValue(ThemeManager.ThemeNameProperty, "Office2013DarkGray");
                MainViewPresenter.Dialogs.Add(dialog);
                Task.Run(() => Application.Current.Dispatcher.Invoke(() => dialog.ShowDialog()));
            }
            else
            {
                if (MainViewPresenter._tabControl != null)
                {
                    if (dataContext is LoginViewModel || dataContext is MainViewModel) return;
                    MainViewPresenter.TabItems.Insert(MainViewPresenter._tabControl.SelectedIndex + 1, frameworkElement);
                    MainViewPresenter._tabControl.SelectedIndex = MainViewPresenter._tabControl.SelectedIndex + 1;
                    return;
                }
                this._contentControl.Content = frameworkElement;
            }
        }

        // ==========================================
        // РАДАР: Шукає конкретну форму на всьому екрані
        // ==========================================
        private static FrameworkElement FindElementByDataContext(DependencyObject root, object dataContext)
        {
            if (root == null || dataContext == null) return null;

            if (root is FrameworkElement fe && fe.DataContext == dataContext)
            {
                // Нам потрібна саме форма (UserControl), а не дрібні елементи типу кнопок
                if (fe is UserControl || fe is Page || fe.GetType().Name.Contains("View"))
                    return fe;
            }

            // Шукаємо у візуальному дереві
            int childrenCount = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var result = FindElementByDataContext(child, dataContext);
                if (result != null) return result;
            }

            // Шукаємо у логічному дереві (іноді DevExpress ховає форми там)
            foreach (var logicalChild in LogicalTreeHelper.GetChildren(root))
            {
                if (logicalChild is DependencyObject depChild)
                {
                    var result = FindElementByDataContext(depChild, dataContext);
                    if (result != null) return result;
                }
            }

            return null;
        }

        public override void ChangePresentation(MvxPresentationHint hint)
        {
            if (hint is MvxClosePresentationHint closeHint)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // 1. ДІАЛОГИ
                        if (MainViewPresenter.Dialogs.Count > 0)
                        {
                            var dialog = MainViewPresenter.Dialogs.Last();
                            dialog.Close();
                            MainViewPresenter.Dialogs.Remove(dialog);
                            return;
                        }

                        // 2. ІДЕАЛЬНИЙ КІЛЕР ВКЛАДОК (Без складного пошуку по дереву)
                        // Шукаємо форму в нашій колекції вкладок за її ViewModel
                        var viewToKill = MainViewPresenter.TabItems.FirstOrDefault(v => v.DataContext == closeHint.ViewModelToClose);

                        if (viewToKill != null)
                        {
                            // Просто видаляємо з колекції - TabControl сам знищить вкладку!
                            MainViewPresenter.TabItems.Remove(viewToKill);

                            // Очищаємо пам'ять і ресурси
                            viewToKill.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                            if (viewToKill.DataContext is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Помилка закриття вкладки: " + ex.Message);
                    }
                });
            }
            else if (hint is MvxCloseAppPresentationHint)
            {
                Application.Current.MainWindow?.Close();
            }
            else
            {
                base.ChangePresentation(hint);
            }
        }

        public override void Close(IMvxViewModel toClose)
        {
            this.ChangePresentation(new MvxClosePresentationHint(toClose));
        }

        public bool CloseAll(MvxCloseAllPresentationHint hint)
        {
            return true;
        }
    }
}