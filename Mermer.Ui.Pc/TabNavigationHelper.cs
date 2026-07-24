using DevExpress.Xpf.Core;
using Mermer.Ui.Core.ViewModels;
using MvvmCross.Core.Navigation;
using MvvmCross.Platform;
using System;
using System.Windows;
using System.Windows.Media;

namespace Mermer.Ui.Pc
{
    class TabNavigationHelper
    {
        // ========================================================
        // ГЛАВНЫЙ МЕТОД ЗАКРЫТИЯ
        // ========================================================
        public static void ForceCloseTab(FrameworkElement currentView)
        {
            if (currentView == null) return;

            // ========================================================
            // 2. СТАНДАРТНОЕ ЗАКРЫТИЕ ВКЛАДКИ
            // ========================================================
            var tabControl = FindParent<DXTabControl>(currentView);

            if (tabControl != null)
            {
                // Удаляем вкладку
                if (tabControl.ItemsSource is System.Collections.IList list)
                    list.Remove(currentView);
                else
                    tabControl.Items.Remove(currentView);

                // АНТИ-СЕРЫЙ ЭКРАН: Если вкладок больше нет, перерисовываем меню
                int count = (tabControl.ItemsSource as System.Collections.IList)?.Count ?? tabControl.Items.Count;
                if (count == 0)
                {
                    RestoreMainMenu();
                }
            }
            else
            {
                RestoreMainMenu();
            }

            // Всегда скрываем саму форму
            currentView.Visibility = Visibility.Collapsed;

            // Очищаем ресурсы накладной
            if (currentView.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        // ========================================================
        // ДЕФИБРИЛЛЯТОР МЕНЮ
        // ========================================================
        private static void RestoreMainMenu()
        {
            try
            {
                // Сбрасываем кэш Презентера
                var field = typeof(Mermer.Ui.Pc.MainViewPresenter).GetField("_tabControl", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (field != null) field.SetValue(null, null);

                // Вызываем Главное меню
                Mvx.Resolve<IMvxNavigationService>().Navigate<MainViewModel>();
            }
            catch { }
        }

        // ========================================================
        // РАДАР
        // ========================================================
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;

            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) parentObject = LogicalTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;

            return FindParent<T>(parentObject);
        }
    }
}