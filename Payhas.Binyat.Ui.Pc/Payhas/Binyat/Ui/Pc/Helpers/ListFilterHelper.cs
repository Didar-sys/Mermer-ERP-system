// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Helpers.ListFilterHelper
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Helpers;

public class ListFilterHelper : DependencyObject, INotifyPropertyChanged
{
  private bool _isCustomFilter;
  private string _filterString;
  private string _title;
  private ImageSource _imageSource;
  private bool _isVisible = true;
  public static readonly DependencyProperty ItemsCountProperty = DependencyProperty.Register(nameof (ItemsCount), typeof (int), typeof (ListFilterHelper), new PropertyMetadata((PropertyChangedCallback) null));
  public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof (Command), typeof (ICommand), typeof (ListFilterHelper), new PropertyMetadata((PropertyChangedCallback) null));
  public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof (CommandParameter), typeof (object), typeof (ListFilterHelper), new PropertyMetadata((PropertyChangedCallback) null));

  public bool IsCustomFilter
  {
    get => this._isCustomFilter;
    set
    {
      this.SetProperty<bool>(ref this._isCustomFilter, value, (Expression<Func<bool>>) (() => this.IsCustomFilter));
    }
  }

  public string FilterString
  {
    get => this._filterString;
    set
    {
      this.SetProperty<string>(ref this._filterString, value, (Expression<Func<string>>) (() => this.FilterString));
    }
  }

  public string Title
  {
    get => this._title;
    set
    {
      if (!this.SetProperty<string>(ref this._title, value, (Expression<Func<string>>) (() => this.Title)))
        return;
      this.RaisePropertyChanged<string>((Expression<Func<string>>) (() => this.Initials));
    }
  }

  public string Initials
  {
    get
    {
      return string.Concat(((IEnumerable<string>) this.Title.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries)).Select<string, string>((Func<string, string>) (t => t.Substring(0, 1))));
    }
  }

  public ImageSource ImageSource
  {
    get => this._imageSource;
    set
    {
      if (!this.SetProperty<ImageSource>(ref this._imageSource, value, (Expression<Func<ImageSource>>) (() => this.ImageSource)))
        return;
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasNoImage));
    }
  }

  public bool HasNoImage => this.ImageSource == null;

  public bool IsVisible
  {
    get => this._isVisible;
    set
    {
      this.SetProperty<bool>(ref this._isVisible, value, (Expression<Func<bool>>) (() => this.IsVisible));
    }
  }

  public int? ItemsCount
  {
    get => (int?) this.GetValue(ListFilterHelper.ItemsCountProperty);
    set => this.SetValue(ListFilterHelper.ItemsCountProperty, (object) value);
  }

  public ICommand Command
  {
    get => (ICommand) this.GetValue(ListFilterHelper.CommandProperty);
    set => this.SetValue(ListFilterHelper.CommandProperty, (object) value);
  }

  public object CommandParameter
  {
    get => this.GetValue(ListFilterHelper.CommandParameterProperty);
    set => this.SetValue(ListFilterHelper.CommandParameterProperty, value);
  }

  protected virtual bool SetProperty<T>(
    ref T property,
    T value,
    Expression<Func<T>> propertyExpression)
  {
    if ((object) property != null && property.Equals((object) value))
      return false;
    property = value;
    this.RaisePropertyChanged((object) this, new PropertyChangedEventArgs(propertyExpression.Body is MemberExpression body ? body.Member.Name : (string) null));
    return true;
  }

  public event PropertyChangedEventHandler PropertyChanged;

  protected virtual void RaisePropertyChanged(string propertyName)
  {
    this.RaisePropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }

  protected virtual void RaisePropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
    if (propertyChanged == null)
      return;
    propertyChanged(sender, e);
  }

  public void RaisePropertyChanged<TProperty>(Expression<Func<TProperty>> property)
  {
    LambdaExpression lambdaExpression = (LambdaExpression) property;
    this.RaisePropertyChanged((!(lambdaExpression.Body is UnaryExpression body) ? (MemberExpression) lambdaExpression.Body : (MemberExpression) body.Operand).Member.Name);
  }
}
