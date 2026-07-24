using DevExpress.Xpf.Core;
using DevExpress.Xpf.Reports.UserDesigner;
using DevExpress.XtraReports.UI;
using MvvmCross.Platform;
using Mermer.Ui.Pc.Services;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Mermer.Ui.Pc;

// ДОБАВЛЕНО: ключевое слово partial, удален IComponentConnector
public partial class ReportDesigner : ThemedWindow
{
    private readonly string _reportName;
    private IReportLayoutStorageService _layoutStorageService;

    public ReportDesigner(string reportName)
    {
        _reportName = reportName;

        // Этот метод теперь будет браться из автосгенерированной XAML-части
        InitializeComponent();

        // Подписываемся на события здесь, вместо автосгенерированного коннектора
        this.Loaded += OnLoaded;
        if (Designer != null)
        {
            Designer.DocumentSaved += OnDocumentSaved;
        }
    }

    public IReportLayoutStorageService LayoutStorageService
    {
        get => _layoutStorageService ?? (_layoutStorageService = Mvx.IocConstruct<IReportLayoutStorageService>());
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            string asyncText = await LayoutStorageService.GetAsync(_reportName);
            if (!string.IsNullOrEmpty(asyncText))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // leaveOpen: true, чтобы MemoryStream не закрылся раньше времени
                    using (StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true))
                    {
                        await streamWriter.WriteAsync(asyncText);
                        await streamWriter.FlushAsync();
                        memoryStream.Seek(0L, SeekOrigin.Begin);
                        Designer.OpenDocument(memoryStream);
                    }
                }
            }
            else
            {
                Type reportType = GetType().Assembly.GetTypes()
                    .Single(t => typeof(XtraReport).IsAssignableFrom(t) && t.Name == _reportName);

                XtraReport instance = Activator.CreateInstance(reportType) as XtraReport;
                Designer.OpenDocument(instance);
            }
        }
        catch (Exception ex)
        {
            DXMessageBox.Show(this, ex.ToString(), "Error loading report designer", MessageBoxButton.OK);
            Close();
        }
    }

    private async void OnDocumentSaved(object sender, ReportDesignerDocumentEventArgs e)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            e.Document.Report.SaveLayoutToXml(memoryStream);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            using (StreamReader streamReader = new StreamReader(memoryStream))
            {
                await LayoutStorageService.StoreAsync(_reportName, await streamReader.ReadToEndAsync());
            }
        }
    }
}