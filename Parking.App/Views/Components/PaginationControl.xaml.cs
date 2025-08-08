using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Parking.App.Views.Components;

public sealed partial class PaginationControl : UserControl
{
    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(PaginationControl), new PropertyMetadata(1));

    public static readonly DependencyProperty TotalPagesProperty =
        DependencyProperty.Register(nameof(TotalPages), typeof(int), typeof(PaginationControl), new PropertyMetadata(1));

    public event EventHandler<int> PageChanged;

    
    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public PaginationControl()
    {
        this.InitializeComponent();
    }

    private void FirstPage_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPage != 1)
        {
            CurrentPage = 1;
            PageChanged?.Invoke(this, CurrentPage);
        }
    }

    private void PreviousPage_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            PageChanged?.Invoke(this, CurrentPage);
        }
    }

    private void NextPage_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            PageChanged?.Invoke(this, CurrentPage);
        }
    }

    private void LastPage_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPage != TotalPages)
        {
            CurrentPage = TotalPages;
            PageChanged?.Invoke(this, CurrentPage);
        }
    }
}