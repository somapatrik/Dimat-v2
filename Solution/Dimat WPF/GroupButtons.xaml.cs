using Dimat_WPF.Class;
using System;
using System.Collections.Generic;
using System.Data;
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
    public partial class GroupButtons : UserControl
    {
        DBGlobal dbglob = new DBGlobal();
        List<PlcButton> plcbuttons = new List<PlcButton>();

        int ID_GROUP;

        public event EventHandler AddPlc_Clicked;
        public event EventHandler Plc_DoubleClicked;
        Point StartPosition;
        private void Inicialization()
        {
            InitializeComponent();
        }

        public GroupButtons(int GROUP_ID = 0)
        {
            Inicialization();

            ID_GROUP = GROUP_ID;

            CreateGroupName();
            LoadGroup();
        }

        public void Refresh()
        {
            plcbuttons.Clear();
            ButtonStack.Children.Clear();
            LoadGroup();
        }

        private void CreateGroupName()
        {
            if (ID_GROUP == 0)
            {
                lbl_Groupname.Content = "";
            } else
            {
                lbl_Groupname.Content = dbglob.GetGroupName(ID_GROUP);
            }
        }

        private void GroupName_clicked(object sender, MouseButtonEventArgs e)
        {
            ButtonStack.Visibility = ButtonStack.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LoadGroup()
        {
            DataTable dt = dbglob.GetPLClist(ID_GROUP);
            foreach(DataRow row in dt.Rows)
            {
                int ID;
                if (int.TryParse(row["ID"].ToString(), out ID))
                {
                    S7PLC plc = new S7PLC(ID);
                    PlcButton button = new PlcButton(plc);
                    button.PreviewMouseMove += PLC_PreviewMouseMove;
                    button.PreviewMouseLeftButtonDown += PLC_PreviewMouseLeftButtonDown;
                    button.MouseDoubleClick += Button_MouseDoubleClick;
                    plcbuttons.Add(button);
                }
            }
            GenerateButtons();
        }

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlcButton plc = (PlcButton)sender;
            Plc_DoubleClicked?.Invoke(plc, null);
        }

        private void GenerateButtons()
        {
            ButtonStack.Children.Clear();

            foreach (PlcButton button in plcbuttons)
            {
                ButtonStack.Children.Add(button);
            }
        }

        private void AddPLC_clicked(object sender, MouseButtonEventArgs e)
        {
            AddPlc_Clicked?.Invoke(ID_GROUP, e);
        }

        #region Drag&drop

        private void UserControl_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("DragPlcFormat"))
            {
                //
            }
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DragPlcFormat"))
            {
                S7PLC draged = (S7PLC)e.Data.GetData("DragPlcFormat");
                draged.UpdateGroup(ID_GROUP);
                Refresh();
            }
        }

        private void PLC_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = StartPosition - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    PlcButton butt = (PlcButton)sender;
                    DataObject dragData = new DataObject("DragPlcFormat", butt.s7plc);
                    DragDrop.DoDragDrop(this, dragData, DragDropEffects.Move);
                    Refresh();
                }

            }
        }

        private void PLC_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartPosition = e.GetPosition(null);
        }

        #endregion
    }
}
