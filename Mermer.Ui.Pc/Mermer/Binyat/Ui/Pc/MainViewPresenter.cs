// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.MainViewPresenter
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using Mermer.Mvvm.Messages;
using Mermer.Mvvm.ViewModels;
using Mermer.Ui.Core.ViewModels;
using MvvmCross.Core.ViewModels;
using MvvmCross.Wpf.Views.Presenters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

#nullable disable
namespace Mermer.Ui.Pc;

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
    MainViewPresenter._tabControl.ItemsSource = (IEnumerable) MainViewPresenter.TabItems;
  }

  public override void Present(FrameworkElement frameworkElement)
  {
    object dataContext = frameworkElement.DataContext;
    if (dataContext is IDialogViewModel dialogViewModel)
    {
      frameworkElement.SetValue(ThemeManager.ThemeNameProperty, (object) "HybridApp");
      WinUIDialogWindow winUiDialogWindow = new WinUIDialogWindow(dialogViewModel.Caption ?? dataContext.GetType().Name);
      winUiDialogWindow.Content = (object) frameworkElement;
      WinUIDialogWindow dialog = winUiDialogWindow;
      dialog.SetValue(ThemeManager.ThemeNameProperty, (object) "Office2013DarkGray;Touch");
      MainViewPresenter.Dialogs.Add(dialog);
      Task.Run<bool?>((Func<bool?>) (() => System.Windows.Application.Current.Dispatcher.Invoke<bool?>((Func<bool?>) (() => dialog.ShowDialog()))));
    }
    else
    {
      if (MainViewPresenter._tabControl != null)
      {
        switch (dataContext)
        {
          case LoginViewModel _:
          case MainViewModel _:
            break;
          default:
            MainViewPresenter.TabItems.Insert(MainViewPresenter._tabControl.SelectedIndex + 1, frameworkElement);
            MainViewPresenter._tabControl.SelectNext();
            return;
        }
      }
      this._contentControl.Content = (object) frameworkElement;
    }
  }

    public override void ChangePresentation(MvxPresentationHint hint)
    {
        if (hint is MvxClosePresentationHint closeHint)
        {
            FrameworkElement frameworkElement = null;

            // 1. Якщо відкрите якесь спливаюче вікно - закриваємо його
            if (MainViewPresenter.Dialogs.Count > 0)
            {
                var dialog = MainViewPresenter.Dialogs.FirstOrDefault(d => ((FrameworkElement)d.Content).DataContext == closeHint.ViewModelToClose)
                             ?? MainViewPresenter.Dialogs.Last();
                dialog.Close();
                MainViewPresenter.Dialogs.Remove(dialog);
                frameworkElement = dialog.Content as FrameworkElement;
            }
            // 2. Якщо вікон немає, закриваємо вкладку
            else if (MainViewPresenter._tabControl != null)
            {
                // Спроба 1: Шукаємо за точним збігом ViewModel
                frameworkElement = MainViewPresenter.TabItems.FirstOrDefault(t => t.DataContext == closeHint.ViewModelToClose);

                // Спроба 2 (Бронебійна): Беремо вкладку чисто за ІНДЕКСОМ, уникаючи хитрощів DevExpress
                if (frameworkElement == null)
                {
                    int activeIndex = MainViewPresenter._tabControl.SelectedIndex;
                    if (activeIndex >= 0 && activeIndex < MainViewPresenter.TabItems.Count)
                    {
                        frameworkElement = MainViewPresenter.TabItems[activeIndex];
                    }
                }

                // Видаляємо вкладку з екрану
                if (frameworkElement != null)
                {
                    MainViewPresenter.TabItems.Remove(frameworkElement);
                }
            }

            // 3. Коректно очищаємо пам'ять за закритою вкладкою
            if (frameworkElement != null)
            {
                frameworkElement.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                if (frameworkElement.DataContext is IDisposable dataContext)
                {
                    dataContext.Dispose();
                }
            }
        }
        else if (hint is MvxCloseAppPresentationHint)
        {
            MainWindow.Instance.Close();
        }
        else
        {
            base.ChangePresentation(hint);
        }
    }

    public override void Close(IMvxViewModel toClose)
  {
    this.ChangePresentation((MvxPresentationHint) new MvxClosePresentationHint(toClose));
  }

  public bool CloseAll(MvxCloseAllPresentationHint hint)
  {
    for (int index = MainViewPresenter.TabItems.Count - 1; index >= 0; --index)
    {
      if (MainViewPresenter.TabItems[index].DataContext is BaseViewModel dataContext && !dataContext.OnCloseAsync().GetAwaiter().GetResult())
        throw new Exception("Operation canceled by user!");
    }
    return true;
  }
}
