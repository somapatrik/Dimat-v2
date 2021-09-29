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
        public string DisplayName;

        DBGlobal dbglob = new DBGlobal();

        public event EventHandler CloseClicked;
        public event EventHandler NameClicked;

        public UserControl userControl;

        public Bookmark(int ID)
        {
            InitializeComponent();
            this.ID = ID;
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
            DisplayName = dbglob.GetPlcName(ID);
            lblName.Content = DisplayName;
        }
        private void Close_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseClicked?.Invoke(this, null);
        }

        private void Name_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NameClicked?.Invoke(this, null);
        }
    }
}
