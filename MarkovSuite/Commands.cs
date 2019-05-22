using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MarkovSuite
{
    public static class Commands
    {
        public static readonly RoutedUICommand EditEntry = new RoutedUICommand
            (
                "Edit Entry",
                "Edit Entry",
                typeof(Commands)
            );

        public static readonly RoutedUICommand DeleteEntry = new RoutedUICommand
            (
                "Delete Entry",
                "Delete Entry",
                typeof(Commands)
            );

        public static readonly RoutedUICommand DeleteChild = new RoutedUICommand
            (
                "Delete Child",
                "Delete Child",
                typeof(Commands)
            );
    }
}
