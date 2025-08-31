using Parking.App.Models;
using System.Printing;
using System.Windows.Markup;
using System.Windows.Xps;
using FlowDirection = System.Windows.FlowDirection;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using PrintDialog = System.Windows.Controls.PrintDialog;
using StackPanel = System.Windows.Controls.StackPanel;
using TextBlock = System.Windows.Controls.TextBlock;

namespace Parking.App.Helpers;

public class ReceiptPrinter
{
    public static UIElement GenerateReceiptContent(ReceiptModel receipt)
    {
        var stackPanel = new StackPanel();
        stackPanel.Margin = new Thickness { Top=0, Bottom=20, Left=20, Right=20 };
        stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
        stackPanel.VerticalAlignment = VerticalAlignment.Top;
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"قبض پارکینگ " + receipt.ParkingName,
            FontSize = 17,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center
        });
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"توضیحات: {receipt.Description}",
            FontSize = 12,
            TextAlignment = TextAlignment.Center
        });
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"زمان ورود: {receipt.StartTime}",
            FontSize = 12,
            TextAlignment = TextAlignment.Center
        });

        stackPanel.Children.Add(new Separator());

        stackPanel.Children.Add(new TextBlock
        {
            Text = $"نوع وسیله نقلیه: {receipt.VehicleSegmentName}",
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            FlowDirection = FlowDirection.RightToLeft
        });
        stackPanel.Children.Add(new Separator());

        stackPanel.Children.Add(new TextBlock
        {
            Text = $"نام پارکینگ: {receipt.ParkingName}",
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            FlowDirection = FlowDirection.RightToLeft
        });
        stackPanel.Children.Add(new Separator());
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"پلاک: {receipt.LicensePlate}",
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            FlowDirection = FlowDirection.RightToLeft
        });
        stackPanel.Children.Add(new Separator());
        stackPanel.Children.Add(new Separator());
        // افزودن بارکد به رسید
        var barcodeImage = BarcodeHelper.GenerateBarcode(receipt.BarcodeId.ToString());
        stackPanel.Children.Add(new System.Windows.Controls.Image
        {
            Source = barcodeImage,
            Height = 70,
            Width = 200,
            Stretch = Stretch.Uniform
        });
        stackPanel.Children.Add(new Separator());


        return stackPanel;
    }
    public static UIElement GenerateInvoiceContent(InvoiceModel receipt)
    {
        var stackPanel = new StackPanel();
        stackPanel.Margin = new Thickness { Top = 0, Bottom = 20, Left = 20, Right = 20 };
        stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
        stackPanel.VerticalAlignment = VerticalAlignment.Top;
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"قبض پارکینگ " + receipt.ParkingName,
            FontSize = 17,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center
        });
        stackPanel.Children.Add(new Separator());
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"نام پارکینگ: {receipt.ParkingName}",
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            FlowDirection = System.Windows.FlowDirection.RightToLeft
        });
        stackPanel.Children.Add(new Separator());
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"زمان: {receipt.Description}",
            FontSize = 12,
            TextAlignment = TextAlignment.Center , Width = 170, TextWrapping = TextWrapping.Wrap,
        });
        stackPanel.Children.Add(new Separator());
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"توضیحات: {receipt.DriverDescription}",
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            Width = 170,
            TextWrapping = TextWrapping.Wrap,
        });
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"زمان ورود: {receipt.StartTime}",
            FontSize = 12,
            TextAlignment = TextAlignment.Center
        });
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"زمان خروج: {receipt.EndTime}",
            FontSize = 12,
            TextAlignment = TextAlignment.Center
        });

        stackPanel.Children.Add(new Separator());

        stackPanel.Children.Add(new TextBlock
        {
            Text = $"نوع وسیله نقلیه: {receipt.VehicleSegmentName}",
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            FlowDirection = FlowDirection.RightToLeft
        });
        stackPanel.Children.Add(new Separator());

        stackPanel.Children.Add(new TextBlock
        {
            Text = $"پلاک: {receipt.LicensePlate}",
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            FlowDirection = FlowDirection.RightToLeft
        });
        stackPanel.Children.Add(new Separator());
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"مبلغ کل: {receipt.TotalAmount} ريال",
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            FlowDirection = FlowDirection.RightToLeft
        });
        //stackPanel.Children.Add(new Separator() { Margin = new Thickness { Bottom = 5, Top = 5, Left = 0, Right = 0 } });
        stackPanel.Children.Add(new Separator());
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"تخفیف: {receipt.TotalDiscount} ريال",
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            FlowDirection = FlowDirection.RightToLeft
        });
        stackPanel.Children.Add(new Separator());
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"مبلغ پرداخت شده: {receipt.PaidAmount} ريال",
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            FlowDirection = FlowDirection.RightToLeft
        });
        stackPanel.Children.Add(new Separator());
        stackPanel.Children.Add(new Separator());
        // افزودن بارکد به رسید
        var barcodeImage = BarcodeHelper.GenerateBarcode(receipt.BarcodeId.ToString());
        stackPanel.Children.Add(new System.Windows.Controls.Image
        {
            Source = barcodeImage,
            Height = 70,
            Width = 200,
            Stretch = Stretch.Uniform
        });
        stackPanel.Children.Add(new Separator());


        return stackPanel;
    }
}


public class PrintHelper
{
    public static void Print(UIElement contentToPrint)
    {
        try
        {
            PrintQueue printQueue = LocalPrintServer.GetDefaultPrintQueue();
            PrintTicket printTicket = printQueue.DefaultPrintTicket;

            FixedDocument fixedDoc = new FixedDocument();
            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage();

            fixedPage.Children.Add(contentToPrint);
            ((IAddChild)pageContent).AddChild(fixedPage);
            fixedDoc.Pages.Add(pageContent);

            XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(printQueue);
            writer.Write(fixedDoc, printTicket);

            Console.WriteLine("Printing completed successfully.");
        }
        catch
        {
            Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
            ms.Title = "خطا در چاپ";
            ms.Content = "پرینتر یافت نشد";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }
    }


    private void DirectPrint(UIElement contentToPrint)
    {
        try
        {
            PrintQueue printQueue = LocalPrintServer.GetDefaultPrintQueue();
            PrintTicket printTicket = printQueue.DefaultPrintTicket;

            FixedDocument fixedDoc = new FixedDocument();
            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage();

            fixedPage.Children.Add(contentToPrint);
            ((IAddChild)pageContent).AddChild(fixedPage);
            fixedDoc.Pages.Add(pageContent);

            XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(printQueue);
            writer.Write(fixedDoc, printTicket);

            Console.WriteLine("Printing completed successfully.");
        }
        catch
        {
            Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
            ms.Title = "خطا در چاپ";
            ms.Content = "پرینتر یافت نشد";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }

    }
}
