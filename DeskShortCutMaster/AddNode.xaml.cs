using HandyControl.Controls;
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
using System.Windows.Shapes;

namespace DeskShortCutMaster
{
    /// <summary>
    /// AddNode.xaml 的交互逻辑
    /// </summary>
    public partial class AddNode : System.Windows.Window
    {
        public delegate void TransfDelegate(MenuTree value);

        public event TransfDelegate TransfEvent;
        public void InitComboBox()
        {
            foreach (string item in ParameterList.DisplayPositionList)
            {
                FormDisplayPosition.Items.Add(item);
            }

            foreach (string item in ParameterList.NodeTypeList)
            {
                FormNodeType.Items.Add(item);
            }
            foreach (string item in ParameterList.NodeCommandList)
            {
                FormNodeCommand.Items.Add(item);
            }
        }
        public AddNode()
        {
            InitializeComponent();
            InitComboBox();
        }

        private void FormSave_Click(object sender, RoutedEventArgs e)
        {
            MenuTree res = new MenuTree(0, 0, "", "", "", "");
            res.DisplayPosition = UInt16.Parse(FormDisplayPosition.Text);
            res.NodeCommand = FormNodeCommand.Text;
            res.NodeType = FormNodeType.Text;
            res.NodeData = FormNodeData.Text;
            res.DisplayName = FormDisplayName.Text;
            TransfEvent(res);
            this.Close();
        }
    }
}
