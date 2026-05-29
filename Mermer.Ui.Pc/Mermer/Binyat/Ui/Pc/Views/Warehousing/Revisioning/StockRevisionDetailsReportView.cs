using MvvmCross.Wpf.Views;
using Mermer.Ui.Core.ViewModels.Warehousing.Revisioning;
using System;

namespace Mermer.Ui.Pc.Views.Warehousing.Revisioning;

public partial class StockRevisionDetailsReportView : MvxWpfView
{
    public StockRevisionDetailsReportView() => InitializeComponent();

    private void ExceedsClick(object sender, EventArgs e)
    {
        GridControl.FilterString = "TotalDifference > 0";
        if (DataContext is StockRevisionDetailsReportViewModel dataContext)
        {
            dataContext.SubCaption = dataContext["Exceeds"];
        }
    }

    private void EqualsClick(object sender, EventArgs e)
    {
        GridControl.FilterString = "TotalDifference = 0";
        if (DataContext is StockRevisionDetailsReportViewModel dataContext)
        {
            dataContext.SubCaption = dataContext["Equals"];
        }
    }

    private void DeficitsClick(object sender, EventArgs e)
    {
        GridControl.FilterString = "TotalDifference < 0";
        if (DataContext is StockRevisionDetailsReportViewModel dataContext)
        {
            dataContext.SubCaption = dataContext["Deficits"];
        }
    }

    private void AllRecordsClick(object sender, EventArgs e)
    {
        GridControl.FilterString = string.Empty;
        if (DataContext is StockRevisionDetailsReportViewModel dataContext)
        {
            dataContext.SubCaption = dataContext["All Records"];
        }
    }
}