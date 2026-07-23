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
        // ГОЛОВНИЙ МЕТОД ЗАКРИТТЯ
        // ========================================================
        public static void ForceCloseTab(FrameworkElement currentView)
        {
            if (currentView == null) return;

            // ========================================================
            // 2. СТАНДАРТНЕ ЗАКРИТТЯ ВКЛАДКИ
            // ========================================================
            var tabControl = FindParent<DXTabControl>(currentView);

            if (tabControl != null)
            {
                // Видаляємо вкладку
                if (tabControl.ItemsSource is System.Collections.IList list)
                    list.Remove(currentView);
                else
                    tabControl.Items.Remove(currentView);

                // АНТИ-СІРИЙ ЕКРАН: Якщо вкладок більше немає, перемальовуємо меню
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

            // Завжди ховаємо саму форму
            currentView.Visibility = Visibility.Collapsed;

            // Очищаємо ресурси накладної
            if (currentView.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        // ========================================================
        // ДЕФІБРИЛЯТОР МЕНЮ
        // ========================================================
        private static void RestoreMainMenu()
        {
            try
            {
                // Скидаємо кеш Презентера
                var field = typeof(Mermer.Ui.Pc.MainViewPresenter).GetField("_tabControl", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (field != null) field.SetValue(null, null);

                // Викликаємо Головне меню
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