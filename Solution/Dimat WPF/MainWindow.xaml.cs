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

    public partial class MainWindow : Window
    {
        DBGlobal dbglobal = new DBGlobal();
        bool ValidGroup;
        List<Bookmark> UsedBookmarks = new List<Bookmark>();
        List<S7PlcDetail> UsedS7Details = new List<S7PlcDetail>();

        private enum MenuTag { None, PLC, Settings};
        private MenuTag OpenedMenu;

        public MainWindow()
        {
            InitializeComponent();

            // Close left menu
            ToogleLeftMenu(MenuTag.None);
            // Create PLC list
            PopulatePlcList();
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

        // Edit PLC
        private void Group_EditClicked(object sender, EventArgs e)
        {
            S7PLC plc = (S7PLC)sender;
            AddPlc edit = new AddPlc(plc);
            edit.CloseClicked += AddPlc_CloseClicked;
            ShowPopup(edit);
        }

        // Delete PLC
        private void Group_DeleteClicked(object sender, EventArgs e)
        {
            S7PLC plc = (S7PLC)sender;
            DeletePlc delete = new DeletePlc(plc.ID);
            delete.Close += AddPlc_CloseClicked;
            ShowPopup(delete);
        }

        // Open PLC detail
        private void Group_Plc_DoubleClicked(object sender, EventArgs e)
        {
            S7PLC plc = ((PlcButton)sender).s7plc;
            AddBookmark(plc.ID);
        }

        #region Bookmark

        private void AddBookmark(int ID)
        {
            Bookmark old = UsedBookmarks.Find(o => o.ID == ID);
            if (old == null)
            {
                Bookmark bookmark = new Bookmark(ID);
                bookmark.CloseClicked += Bookmark_CloseClicked;
                bookmark.NameClicked += Bookmark_NameClicked;
                BookmarkStack.Children.Add(bookmark);
                UsedBookmarks.Add(bookmark);
                AddPlcDetail(ID);
            } else
            {
                FocusDetail(ID);
            }
        }

        private void RemoveBookmark(int ID)
        {
            Bookmark bookmark = UsedBookmarks.Find(o => o.ID == ID);
            BookmarkStack.Children.Remove(bookmark);
            UsedBookmarks.Remove(bookmark);
        }

        private void Bookmark_NameClicked(object sender, EventArgs e)
        {
            FocusDetail((int)sender);
        }

        private void Bookmark_CloseClicked(object sender, EventArgs e)
        {
            RemoveBookmark((int)sender);
            RemoveDetail((int)sender);
        }

        private void SelectBookmark(int ID)
        {
            foreach (Bookmark bookmark in BookmarkStack.Children)
            {
                if (bookmark.ID == ID)
                    bookmark.Select();
                else
                    bookmark.Unselect();
            }
        }

        #endregion

        #region Plc detail

        private void AddPlcDetail(int ID)
        {
            S7PlcDetail detail = new S7PlcDetail(ID);
            UsedS7Details.Add(detail);
            FocusDetail(detail);
        }

        private void RemoveDetail(int ID)
        {
            S7PlcDetail detail = UsedS7Details.Find(o => o.ID == ID);
            GridPlcDetail.Children.Remove(detail);
            UsedS7Details.Remove(detail);
        }

        private void FocusDetail(int ID)
        {
            S7PlcDetail detail = UsedS7Details.Find(o => o.ID == ID);
            if (detail != null)
                FocusDetail(detail);
        }

        private void FocusDetail(S7PlcDetail detail)
        {
            GridPlcDetail.Children.Clear();
            GridPlcDetail.Children.Add(detail);
            SelectBookmark(detail.ID);
        }

        #endregion

        private void AddPlc_Group_Clicked(object sender, EventArgs e)
        {
            int id_group = (int)sender;
            GridPopup.Children.Clear();
            AddPlc addPlc = new AddPlc(id_group);
            addPlc.CloseClicked += AddPlc_CloseClicked;
            ShowPopup(addPlc);
        }

        private void AddPlc_CloseClicked(object sender, EventArgs e)
        {
            HidePopup();
            RefreshPlcList();
        }

        private void RefreshPlcList()
        {
            foreach (GroupButtons group in PlcStack.Children)
            {
                group.Refresh();
            }
        }

        #region Popup

        private void ShowPopup(UserControl control)
        {
            GridPopup.Children.Clear();
            GridPopup.Children.Add(control);
            Grid.SetColumn(control, 1);
            Grid.SetRow(control, 1);
            GridPopup.Visibility = Visibility.Visible;
        }

        private void HidePopup()
        {
            GridPopup.Children.Clear();
            GridPopup.Visibility = Visibility.Collapsed;
        }

        #endregion

        private void BtnShowAddGroup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtGroupName.Visibility = txtGroupName.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            btnAddGroup.Visibility = btnAddGroup.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
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

        // PLC left clicked
        private void btnMenuPLC_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RefreshPlcList();
            ToogleLeftMenu(MenuTag.PLC, sender);
        }

        private void ToogleLeftMenu(MenuTag tag, object clickedbutton = null)
        {
            DeselectSideMenu();

            // Compare required menu with actual one
            if (OpenedMenu == tag)
                // Same menu => Toogle
                GridLeftMenu.Visibility = GridLeftMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            else
                // Just open
                GridLeftMenu.Visibility = Visibility.Visible;


            if (GridLeftMenu.Visibility == Visibility.Visible)
            {
                col_LeftMenu.Width = new GridLength(250, GridUnitType.Pixel);
                col_LeftMenuGripper.Width = new GridLength(2, GridUnitType.Pixel);

                if (clickedbutton != null)
                    ((Label)clickedbutton).Style = (Style)Resources["DetailSideButtonActive"];
            } 
            else
            {
                col_LeftMenu.Width = new GridLength(0, GridUnitType.Pixel);
                col_LeftMenuGripper.Width = new GridLength(0, GridUnitType.Pixel);
            }

            OpenedMenu = tag;
        }

        private void DeselectSideMenu()
        {
            foreach (Label button in StackLeftSideMenu.Children)
                button.Style = (Style)Resources["DetailSideButton"];
        }

    }
}
