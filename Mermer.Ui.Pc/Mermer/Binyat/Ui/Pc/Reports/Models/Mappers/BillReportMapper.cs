using Mermer.Commerce.Models;
using Mermer.Ui.Pc.Reports.Helpers;
using System;
using System.Threading.Tasks;

namespace Mermer.Ui.Pc.Reports.Models.Mappers;

public class BillReportMapper(NameHelper nameHelper, BillReportLineMapper lineMapper) :
    FundsTransactionReportMapper<Bill, BillLine, BillReport, BillReportLine>(nameHelper, lineMapper)
{
    private decimal _prevBalance;

    public void SetPartnerPrevBalance(decimal prevBalance) => _prevBalance = prevBalance;

    public override async Task<BillReport> Map(Bill source, string localizedType)
    {
        BillReport destination = await base.Map(source, localizedType);

        destination.Partner = await NameHelper.GetPartnerName(source.PartnerId);
        destination.PartnerPrevBalance = _prevBalance;
        destination.PartnerDebitEffect = source.IsPartnerDebit ? source.DisplayTotal : 0M;
        destination.PartnerCreditEffect = source.IsPartnerDebit ? 0M : source.DisplayTotal;

        return destination;
    }
}