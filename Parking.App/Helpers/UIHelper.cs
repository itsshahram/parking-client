using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Helpers;

public static class UIHelper
{
    public static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T)
                return (T)current;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
