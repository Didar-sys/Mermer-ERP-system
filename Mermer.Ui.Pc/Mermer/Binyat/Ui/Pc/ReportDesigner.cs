// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.ReportDesigner
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.Xpf.Core;
using DevExpress.Xpf.Reports.UserDesigner;
using DevExpress.XtraReports.UI;
using MvvmCross.Platform;
using Mermer.Ui.Pc.Services;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

#nullable disable
namespace Mermer.Ui.Pc;

public class ReportDesigner : ThemedWindow, IComponentConnector
{
  private readonly string _reportName;
  private IReportLayoutStorageService _layoutStorageService;
  internal DevExpress.Xpf.Reports.UserDesigner.ReportDesigner Designer;
  private bool _contentLoaded;

  public ReportDesigner(string reportName)
  {
    this._reportName = reportName;
    this.InitializeComponent();
  }

  public IReportLayoutStorageService LayoutStorageService
  {
    get
    {
      return this._layoutStorageService ?? (this._layoutStorageService = Mvx.IocConstruct<IReportLayoutStorageService>());
    }
  }

  private async void OnLoaded(object sender, RoutedEventArgs e)
  {
    ReportDesigner owner = this;
    try
    {
      string async = await owner.LayoutStorageService.GetAsync(owner._reportName);
      if (!string.IsNullOrEmpty(async))
      {
        using (MemoryStream memoryStream = new MemoryStream())
        {
          using (StreamWriter streamWriter = new StreamWriter((Stream) memoryStream, Encoding.UTF8))
          {
            await streamWriter.WriteAsync(async);
            await streamWriter.FlushAsync();
            memoryStream.Seek(0L, SeekOrigin.Begin);
            owner.Designer.OpenDocument((Stream) memoryStream);
          }
        }
      }
      else
      {
        // ISSUE: reference to a compiler-generated method
        XtraReport instance = Activator.CreateInstance(((IEnumerable<Type>) owner.GetType().Assembly.GetTypes()).Single<Type>(new Func<Type, bool>(owner.\u003COnLoaded\u003Eb__5_0))) as XtraReport;
        owner.Designer.OpenDocument(instance);
      }
    }
    catch (Exception ex)
    {
      int num = (int) DXMessageBox.Show((FrameworkElement) owner, ex.ToString(), "Error loading report designer", MessageBoxButton.OK);
      owner.Close();
    }
  }

  private async void OnDocumentSaved(object sender, ReportDesignerDocumentEventArgs e)
  {
    using (MemoryStream memoryStream = new MemoryStream())
    {
      e.Document.Report.SaveLayoutToXml((Stream) memoryStream);
      memoryStream.Seek(0L, SeekOrigin.Begin);
      using (StreamReader streamReader = new StreamReader((Stream) memoryStream))
        await this.LayoutStorageService.StoreAsync(this._reportName, await streamReader.ReadToEndAsync());
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Mermer.Ui.Pc;component/reportdesigner.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 1)
    {
      if (connectionId == 2)
      {
        this.Designer = (DevExpress.Xpf.Reports.UserDesigner.ReportDesigner) target;
        this.Designer.DocumentSaved += new EventHandler<ReportDesignerDocumentEventArgs>(this.OnDocumentSaved);
      }
      else
        this._contentLoaded = true;
    }
    else
      ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnLoaded);
  }
}
