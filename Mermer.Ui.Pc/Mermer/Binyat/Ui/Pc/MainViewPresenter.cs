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
using MvvmCross.Core.Views;


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

            // ИСПРАВЛЕНО: Подписываемся на событие крестика (закрытие вкладки пользователем)
            MainViewPresenter._tabControl.TabHiding += TabControl_TabHiding;
        }

        // =========================================================================
        // ОБРАБОТЧИК НАЖАТИЯ НА КРЕСТИК В ВКЛАДКЕ
        // =========================================================================
        private static void TabControl_TabHiding(object sender, TabControlTabHidingEventArgs e)
        {
            // Отменяем стандартное скрытие DevExpress (оно сломано кастомным шаблоном)
            e.Cancel = true;

            // e.Item содержит сам FrameworkElement (View), который лежит в нашей коллекции TabItems
            if (e.Item is FrameworkElement viewToKill)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Удаляем из коллекции — TabControl полностью уничтожит вкладку из интерфейса
                    MainViewPresenter.TabItems.Remove(viewToKill);

                    // Очищаем память и ресурсы (копия вашей логики из ChangePresentation)
                    viewToKill.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                    if (viewToKill.DataContext is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                });
            }
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
                // ----------------------------------------------------
                // ЛОГИКА ВЫХОДА ИЗ ПРОГРАММЫ (LOGOUT)
                // ----------------------------------------------------
                if (dataContext != null && dataContext.GetType().Name.Contains("LoginViewModel"))
                {
                    // 1. Отвязываем TabControl и чистим память вкладок
                    MainViewPresenter._tabControl = null;
                    MainViewPresenter.TabItems.Clear();

                    // 2. Заменяем весь наш Mermer-интерфейс на окно Логина
                    this._contentControl.Content = frameworkElement;
                    return;
                }

                // ----------------------------------------------------
                // ЛОГИКА ОТКРЫТИЯ НОВЫХ ВКЛАДОК
                // ----------------------------------------------------
                if (MainViewPresenter._tabControl != null)
                {
                    // Игнорируем попытку открыть MainViewModel как обычную вкладку
                    if (dataContext != null && dataContext.GetType().Name.Contains("MainViewModel")) return;

                    // Добавляем новую вкладку сразу после текущей
                    MainViewPresenter.TabItems.Insert(MainViewPresenter._tabControl.SelectedIndex + 1, frameworkElement);

                    // Жестко переключаем фокус на только что созданную вкладку
                    MainViewPresenter._tabControl.SelectedItem = frameworkElement;
                    return;
                }

                // Первая загрузка (когда TabControl еще нет)
                this._contentControl.Content = frameworkElement;
            }
        }

        // ==========================================
        // РАДАР: Ищет конкретную форму на всем экране
        // ==========================================
        private static FrameworkElement FindElementByDataContext(DependencyObject root, object dataContext)
        {
            if (root == null || dataContext == null) return null;

            if (root is FrameworkElement fe && fe.DataContext == dataContext)
            {
                if (fe is UserControl || fe is Page || fe.GetType().Name.Contains("View"))
                    return fe;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var result = FindElementByDataContext(child, dataContext);
                if (result != null) return result;
            }

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
                        // 1. ДИАЛОГИ
                        if (MainViewPresenter.Dialogs.Count > 0)
                        {
                            var dialog = MainViewPresenter.Dialogs.Last();
                            dialog.Close();
                            MainViewPresenter.Dialogs.Remove(dialog);
                            return;
                        }

                        // 2. КИЛЛЕР ВКЛАДОК (Программное закрытие через ViewModel)
                        var viewToKill = MainViewPresenter.TabItems.FirstOrDefault(v => v.DataContext == closeHint.ViewModelToClose);

                        if (viewToKill != null)
                        {
                            MainViewPresenter.TabItems.Remove(viewToKill);

                            viewToKill.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                            if (viewToKill.DataContext is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка закрытия вкладки: " + ex.Message);
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