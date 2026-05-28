using Mermer.Commerce.Models;
using Mermer.Ui.Pc.Reports.Helpers;
using System.Threading.Tasks;

namespace Mermer.Ui.Pc.Reports.Models.Mappers;

public class InvoiceReportMapper(NameHelper nameHelper, InvoiceReportLineMapper lineMapper) :
    StockTransactionReportMapper<Invoice, InvoiceLine, InvoiceReport, InvoiceReportLine>(nameHelper, lineMapper)
{
    private decimal _prevBalance;
    public void SetPartnerPrevBalance(decimal prevBalance) => _prevBalance = prevBalance;

    public override async Task<InvoiceReport> Map(Invoice source, string localizedType)
    {
        InvoiceReport destination = await base.Map(source, localizedType);

        destination.DueDate = source.DueDate;
        destination.Depository = await NameHelper.GetDepositoryName(source.DepositoryId);
        destination.IsCash = source.IsCash;
        destination.DiscountsTotal = source.DisplayDiscountsTotal;
        destination.PaymentsTotal = source.DisplayPaymentsTotal;
        destination.ChangesTotal = source.DisplayChangesTotal;

        if (!string.IsNullOrEmpty(source.PartnerId))
        {
            destination.Partner = await NameHelper.GetPartnerName(source.PartnerId);
            destination.PartnerPrevBalance = _prevBalance;
            destination.PartnerDebitEffect = source.DisplayDebitTotal;
            destination.PartnerCreditEffect = source.DisplayCreditTotal;
        }

        return destination;
    }
}