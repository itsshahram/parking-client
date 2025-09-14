using System.ComponentModel;

namespace Parking.App.Models.Tickets;


public class TicketSummaryReportModel
{
    [DisplayName("تعداد کل بلیت‌ها")]
    public int TotalTickets { get; set; }

    [DisplayName("مبلغ کل")]
    public string TotalAmount { get; set; }

    [DisplayName("مبلغ پرداخت شده")]
    public string TotalPaidAmount { get; set; }

    [DisplayName("مبلغ تخفیف")]
    public string TotalDiscountAmount { get; set; }

    [DisplayName("تعداد پرداخت نقدی")]
    public int TotalCreditPaid { get; set; }

    [DisplayName("تعداد پرداخت کارت")]
    public int TotalPosPaid { get; set; }

    [DisplayName("مبلغ پرداخت نقدی")]
    public string TotalCreditPaidAmount { get; set; }

    [DisplayName("مبلغ پرداخت کارت")]
    public string TotalPosPaidAmount { get; set; }

    [DisplayName("ورودها")]
    public int TotalEntries { get; set; }

    [DisplayName("خروج‌ها")]
    public int TotalExits { get; set; }

    [DisplayName("در پارکینگ")]
    public int CurrentlyInside { get; set; }
}