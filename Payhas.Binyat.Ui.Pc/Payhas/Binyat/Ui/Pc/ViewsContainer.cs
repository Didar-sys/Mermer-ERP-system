// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.ViewsContainer
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using Humanizer;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.Exceptions;
using MvvmCross.Wpf.Views;
using Payhas.Binyat.Ui.Pc.ViewModels;
using Payhas.Binyat.Ui.Pc.Views.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

#nullable disable
namespace Payhas.Binyat.Ui.Pc;

public class ViewsContainer : MvxWpfViewsContainer
{
  public override FrameworkElement CreateView(MvxViewModelRequest request)
  {
    Type viewType = this.GetViewType(request.ViewModelType);
    object obj = !(viewType == (Type) null) ? Activator.CreateInstance(viewType) : throw new MvxException("View Type not found for " + request.ViewModelType?.ToString());
    if (obj == null)
      throw new MvxException("View not loaded for " + viewType?.ToString());
    if (!(obj is IMvxWpfView mvxWpfView))
      throw new MvxException("Loaded View does not have IMvxWpfView interface " + viewType?.ToString());
    if (!(obj is FrameworkElement view))
      throw new MvxException("Loaded View is not a FrameworkElement " + viewType?.ToString());
    if (request is MvxViewModelInstanceRequest modelInstanceRequest)
    {
      mvxWpfView.ViewModel = modelInstanceRequest.ViewModelInstance;
      return view;
    }
    IMvxViewModelLoader mvxViewModelLoader = Mvx.Resolve<IMvxViewModelLoader>();
    mvxWpfView.ViewModel = mvxViewModelLoader.LoadViewModel(request, (IMvxBundle) null);
    return view;
  }

  protected new Type GetViewType(Type viewModelType)
  {
    Type type1 = (Type) null;
    if (viewModelType.IsGenericType)
    {
      Type genericTypeDefinition = viewModelType.GetGenericTypeDefinition();
      string genericSuffix = genericTypeDefinition?.Name.Replace($"ViewModel`{genericTypeDefinition.GetGenericArguments().Length}", "View");
      string viewName = viewModelType.GetGenericArguments()[0].Name;
      type1 = this.ViewTypes.FirstOrDefault<Type>((Func<Type, bool>) (t => t.Name == viewName + genericSuffix || t.Name == viewName.Pluralize() + genericSuffix));
    }
    else if (viewModelType == typeof (ReportsListViewModel))
      type1 = typeof (ReportsListView);
    Type type2 = type1;
    return (object) type2 != null ? type2 : base.GetViewType(viewModelType);
  }

  public IEnumerable<Type> ViewTypes
  {
    get
    {
      return ((IEnumerable<Type>) this.GetType().Assembly.GetTypes()).Where<Type>((Func<Type, bool>) (t => t.Name.EndsWith("View")));
    }
  }
}
