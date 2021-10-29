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

namespace Dimat_WPF
{
    /// <summary>
    /// Interaction logic for S7Console.xaml
    /// </summary>
    public partial class S7Console : UserControl
    {

        public double RowConsoleH;
        public GridUnitType RowConsoleType;

        public bool ConsoleVisible;

        public S7Console()
        {
            InitializeComponent();
            ConsoleVisible = true;
            RowConsoleType = GridUnitType.Star;
        }

        public async void Log(string msg)
        {
            await Task.Run(() =>
            {
                string raw = msg;
                string line = ">" + DateTime.Now.ToString("HH:mm:ss") + " -> " + raw + Environment.NewLine;

                LogConsole.Dispatcher.Invoke(() => 
                {
                    LogConsole.Text += line;
                    ScrollLog.ScrollToEnd();
                });

            });
        }
        private void btnShow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!ConsoleVisible)
                ShowConsole();
        }

        private void btnHide_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ConsoleVisible)
                HideConsole();
        }

        private void HideConsole()
        {
            RowConsoleH = RowConsole.Height.Value;
            RowConsoleType = RowConsole.Height.GridUnitType;
            RowConsole.Height = new GridLength(0);
            ConsoleVisible = false;
        }

        private void ShowConsole()
        {
            RowConsole.Height = new GridLength(RowConsoleH, RowConsoleType);
            ConsoleVisible = true;
        }


    }
}
