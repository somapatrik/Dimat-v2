using Dimat_WPF.Class;
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
    public partial class PlcList : UserControl
    {

        DBGlobal dbglobal = new DBGlobal();

        private bool ValidGroup;

        public event EventHandler AddPlc_Clicked;
        public event EventHandler Plc_DoubleClicked;
        public event EventHandler DeleteClicked;
        public event EventHandler EditClicked;


        public PlcList()
        {
            InitializeComponent();
            // Create PLC list
            PopulatePlcList();

            btnExpand.Visibility = Visibility.Collapsed;
        }
        private void PopulatePlcList()
        {
            GroupButtons nogroup = new GroupButtons();
            AddGroup(nogroup);

            foreach (PlcGroup grp in dbglobal.GetGroupList())
            {
                GroupButtons group = new GroupButtons(grp.ID);
                AddGroup(group);
            }
        }

        private void AddGroup(GroupButtons group)
        {
            group.AddPlc_Clicked += AddPlc_Group_Clicked;
            group.Plc_DoubleClicked += Group_Plc_DoubleClicked;
            group.DeleteClicked += Group_DeleteClicked;
            group.EditClicked += Group_EditClicked;
            PlcStack.Children.Add(group);
        }

        public void RefreshPlcList()
        {
            foreach (GroupButtons group in PlcStack.Children)
                group.Refresh();
        }

        #region Button events

        // Add PLC
        private void AddPlc_Group_Clicked(object sender, EventArgs e)
        {
            AddPlc_Clicked?.Invoke(sender, e);
        }

        // Open PLC detail
        private void Group_Plc_DoubleClicked(object sender, EventArgs e)
        {
            Plc_DoubleClicked?.Invoke(sender, e);
        }

        // Edit PLC
        private void Group_EditClicked(object sender, EventArgs e)
        {
            EditClicked?.Invoke(sender, e);
        }

        // Delete PLC
        private void Group_DeleteClicked(object sender, EventArgs e)
        {
            DeleteClicked?.Invoke(sender, e);
        }

        #endregion

        private void BtnShowAddGroup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GridAddGroup.Visibility = GridAddGroup.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtGroupName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtGroupName.Text.Length < 4 | !dbglobal.IsGroupNameAvailable(txtGroupName.Text))
            {
                txtGroupName.Style = (Style)Resources["DarkBoxNOK"];
                btnAddGroup.IsEnabled = false;
                ValidGroup = false;
            }
            else
            {
                txtGroupName.Style = (Style)Resources["DarkBox"];
                btnAddGroup.IsEnabled = true;
                ValidGroup = true;
            }
        }

        private void btnAddGroup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ValidGroup)
            {
                dbglobal.CreateGroup(txtGroupName.Text);
                txtGroupName.Text = "";
                txtGroupName.Style = (Style)Resources["DarkBox"];
                GroupButtons grp = new GroupButtons(dbglobal.GetLastGroupID());
                AddGroup(grp);
            }
        }

        private void btnCollapse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (GroupButtons group in PlcStack.Children)
                group.Collapse();
            btnCollapse.Visibility = Visibility.Collapsed;
            btnExpand.Visibility = Visibility.Visible;
        }

        private void btnExpand_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (GroupButtons group in PlcStack.Children)
                group.Expand();
            btnCollapse.Visibility = Visibility.Visible;
            btnExpand.Visibility = Visibility.Collapsed;
        }
    }
}
