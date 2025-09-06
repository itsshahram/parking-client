using Parking.App.Models;
using System.Printing;
using System.Windows.Markup;
using System.Windows.Xps;
using Border = System.Windows.Controls.Border;
using Brushes = System.Windows.Media.Brushes;
using FlowDirection = System.Windows.FlowDirection;
using Grid = System.Windows.Controls.Grid;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Windows.Controls.Image;
using PrintDialog = System.Windows.Controls.PrintDialog;
using StackPanel = System.Windows.Controls.StackPanel;
using TextBlock = System.Windows.Controls.TextBlock;

namespace Parking.App.Helpers;

public class ReceiptPrinter
{
    public static UIElement GenerateReceiptContent(ReceiptModel receipt, double width)
    {
        var outerBorder = new Border
        {
            Width = width - 10,
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(5),
            Margin = new Thickness(5),
            Background = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var stackPanel = new StackPanel
        {
            Width = width - 30, 
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top
        };

        if (receipt.QueueNumber != null)
        {
            var queueBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6),
                Margin = new Thickness(0, 5, 0, 5),
                Background = Brushes.White,
                Child = new TextBlock
                {
                    Text = $"{receipt.QueueNumber}",
                    FontSize = 25,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };

            stackPanel.Children.Add(queueBorder);
        }

        stackPanel.Children.Add(new TextBlock
        {
            Text = $"{receipt.ParkingName}",
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Width = width - 50,
            Margin = new Thickness(0, 0, 0, 5)
        });


        var infoGrid = new Grid
        {
            Margin = new Thickness(0, 0, 0, 10),
            FlowDirection = FlowDirection.RightToLeft
        };

        infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        void AddRow(string label, string value, bool? valueInNewRow = false, bool? justValue = false)
        {
            if ((bool)justValue)
            {
                // فقط value نمایش داده شود
                int valueRowIndex = infoGrid.RowDefinitions.Count;
                infoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var valueText = new TextBlock
                {
                    Text = value,
                    FontSize = 15,
                    Margin = new Thickness(2),
                    TextWrapping = TextWrapping.Wrap,
                    FlowDirection = FlowDirection.RightToLeft , 
                    HorizontalAlignment = HorizontalAlignment.Center , VerticalAlignment= VerticalAlignment.Bottom
                };
                Grid.SetRow(valueText, valueRowIndex);
                Grid.SetColumn(valueText, 0);
                Grid.SetColumnSpan(valueText, 2);
                infoGrid.Children.Add(valueText);
                return;
            }

            if (valueInNewRow == true)
            {
                // ردیف اول: فقط label
                int labelRowIndex = infoGrid.RowDefinitions.Count;
                infoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var labelText = new TextBlock
                {
                    Text = label,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(2),
                    FlowDirection = FlowDirection.RightToLeft
                };
                Grid.SetRow(labelText, labelRowIndex);
                Grid.SetColumn(labelText, 0);
                infoGrid.Children.Add(labelText);

                // ردیف دوم: فقط value با colspan = 2
                int valueRowIndex = infoGrid.RowDefinitions.Count;
                infoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var valueText = new TextBlock
                {
                    Text = value,
                    FontSize = 15,
                    Margin = new Thickness(2),
                    TextWrapping = TextWrapping.Wrap,
                    FlowDirection = FlowDirection.RightToLeft
                };
                Grid.SetRow(valueText, valueRowIndex);
                Grid.SetColumn(valueText, 0);
                Grid.SetColumnSpan(valueText, 2);
                infoGrid.Children.Add(valueText);
            }
            else
            {
                // ردیف واحد: label و value کنار هم
                int rowIndex = infoGrid.RowDefinitions.Count;
                infoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var labelText = new TextBlock
                {
                    Text = label,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(2),
                    FlowDirection = FlowDirection.RightToLeft
                };
                Grid.SetRow(labelText, rowIndex);
                Grid.SetColumn(labelText, 0);

                var valueText = new TextBlock
                {
                    Text = value,
                    FontSize = 15,
                    Margin = new Thickness(2),
                    TextWrapping = TextWrapping.Wrap,
                    FlowDirection = FlowDirection.RightToLeft
                };
                Grid.SetRow(valueText, rowIndex);
                Grid.SetColumn(valueText, 1);

                infoGrid.Children.Add(labelText);
                infoGrid.Children.Add(valueText);
            }
        }

        //AddRow("زمان:", receipt.Description);
        AddRow("صف:", receipt.DriverDescription, false,false);
        AddRow("نوع:", receipt.VehicleSegmentName);
        AddRow("ورود:", receipt.StartTime);


        AddRow("پلاک:", receipt.LicensePlate);

        stackPanel.Children.Add(infoGrid);

        var barcodeImage = BarcodeHelper.GenerateBarcode(receipt.BarcodeId.ToString());
        stackPanel.Children.Add(new Image
        {
            Source = barcodeImage,
            Width = width - 40,
            Stretch = Stretch.Uniform,
            Margin = new Thickness(0, 10, 0, 0)
        });

        outerBorder.Child = stackPanel;
        return outerBorder;
    }
    public static UIElement GenerateInvoiceContent(InvoiceModel receipt, double width)
    {
        var outerBorder = new Border
        {
            Width = width -10,
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(5),
            Margin = new Thickness(5),
            Background = Brushes.White , HorizontalAlignment = HorizontalAlignment.Center
        };

        var stackPanel = new StackPanel
        {
            Width = width - 30, // برای در نظر گرفتن Padding و Border
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top
        };

        stackPanel.Children.Add(new TextBlock
        {
            Text = $"قبض پارکینگ {receipt.ParkingName}",
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap,
            Width = width - 50 ,
            Margin = new Thickness(0, 0, 0, 5)
        });

        if (receipt.QueueNumber != null)
        {
            var queueBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6),
                Margin = new Thickness(0, 5, 0, 5),
                Background = Brushes.White,
                Child = new TextBlock
                {
                    Text = $"نوبت: {receipt.QueueNumber}",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };

            stackPanel.Children.Add(queueBorder);
        }

        var infoGrid = new Grid
        {
            Margin = new Thickness(0, 0, 0, 10),
            FlowDirection = FlowDirection.RightToLeft
        };

        infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        void AddRow(string label, string value, bool? valueInNewRow = false)
        {
            if ((bool)valueInNewRow)
            {
                // ردیف اول: فقط label
                int labelRowIndex = infoGrid.RowDefinitions.Count;
                infoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var labelText = new TextBlock
                {
                    Text = label,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(2),
                    FlowDirection = FlowDirection.RightToLeft
                };
                Grid.SetRow(labelText, labelRowIndex);
                Grid.SetColumn(labelText, 0);
                infoGrid.Children.Add(labelText);

                // ردیف دوم: فقط value با colspan = 2
                int valueRowIndex = infoGrid.RowDefinitions.Count;
                infoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var valueText = new TextBlock
                {
                    Text = value,
                    FontSize = 15,
                    Margin = new Thickness(2),
                    TextWrapping = TextWrapping.Wrap,
                    FlowDirection = FlowDirection.RightToLeft
                };
                Grid.SetRow(valueText, valueRowIndex);
                Grid.SetColumn(valueText, 0);
                Grid.SetColumnSpan(valueText, 2);
                infoGrid.Children.Add(valueText);
            }
            else
            {
                // ردیف واحد: label و value کنار هم
                int rowIndex = infoGrid.RowDefinitions.Count;
                infoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var labelText = new TextBlock
                {
                    Text = label,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(2),
                    FlowDirection = FlowDirection.RightToLeft
                };
                Grid.SetRow(labelText, rowIndex);
                Grid.SetColumn(labelText, 0);

                var valueText = new TextBlock
                {
                    Text = value,
                    FontSize = 15,
                    Margin = new Thickness(2),
                    TextWrapping = TextWrapping.Wrap,
                    FlowDirection = FlowDirection.RightToLeft
                };
                Grid.SetRow(valueText, rowIndex);
                Grid.SetColumn(valueText, 1);

                infoGrid.Children.Add(labelText);
                infoGrid.Children.Add(valueText);
            }
        }

        //AddRow("زمان:", receipt.Description);
        AddRow("صف:", receipt.DriverDescription, false);
        AddRow("نوع:", receipt.VehicleSegmentName);
        AddRow(" ورود:", receipt.StartTime);
        if (receipt.EndTime!=null && receipt.EndTime.Length>2)
        {
            AddRow(" خروج:", receipt.EndTime);
        }
        
       
        AddRow("پلاک:", receipt.LicensePlate);
        AddRow("مبلغ کل:", $"{receipt.TotalAmount:N0} ریال");
        AddRow("تخفیف:", $"{receipt.TotalDiscount:N0} ریال");
        AddRow("پرداخت شده:", $"{receipt.PaidAmount:N0} ریال");

        stackPanel.Children.Add(infoGrid);

        var barcodeImage = BarcodeHelper.GenerateBarcode(receipt.BarcodeId.ToString());
        stackPanel.Children.Add(new Image
        {
            Source = barcodeImage,
            Width = width - 40,
            Stretch = Stretch.Uniform,
            Margin = new Thickness(0, 10, 0, 0)
        });

        outerBorder.Child = stackPanel;
        return outerBorder;
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


            double dpi = Settings.Default.Application_Print_dpi;
            double widthMm = Settings.Default.Application_Print_widthMm;
            double widthInches = widthMm / 25.4;
            double widthPixels = dpi * widthInches;


            contentToPrint.Measure(new System.Windows.Size(widthPixels, double.PositiveInfinity));
            contentToPrint.Arrange(new Rect(new System.Windows.Point(0, 0), contentToPrint.DesiredSize));
            double contentHeight = contentToPrint.DesiredSize.Height;

            // تنظیم اندازه صفحه بر اساس محتوای واقعی
            System.Windows.Size pageSize = new System.Windows.Size(widthPixels, contentHeight);

            FixedDocument fixedDoc = new FixedDocument();
            fixedDoc.DocumentPaginator.PageSize = pageSize;

            FixedPage fixedPage = new FixedPage
            {
                Width = pageSize.Width,
                Height = pageSize.Height
            };

            FixedPage.SetLeft(contentToPrint, 0);
            FixedPage.SetTop(contentToPrint, 0);
            fixedPage.Children.Add(contentToPrint);

            PageContent pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(fixedPage);
            fixedDoc.Pages.Add(pageContent);

            // ارسال به پرینتر
            XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(printQueue);
            writer.Write(fixedDoc, printTicket);
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
