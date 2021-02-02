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
    public partial class Bookmark : UserControl
    {
        public int ID;
        public string PlcName;

        DBGlobal dbglob = new DBGlobal();

        public event EventHandler CloseClicked;
        public event EventHandler NameClicked;

        public Bookmark(int ID)
        {
            InitializeComponent();
            this.ID = ID;
            LoadName();
        }

        public void Refresh(int ID)
        {
            this.ID=ID;
            LoadName();
        }

        public void Refresh()
        {
            LoadName();
        }

        public void Select()
        {
            GridBorder.Style = (Style)Resources["BookmarkSelected"];
        }

        public void Unselect()
        {
            GridBorder.Style = (Style)Resources["BookmarkNormal"];
        }

        private void LoadName()
        {
            PlcName = dbglob.GetPlcName(ID);
            lblName.Content = PlcName;
        }
        private void Close_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseClicked?.Invoke(ID, null);
        }

        private void Name_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NameClicked?.Invoke(ID, null);
        }
    }
}
