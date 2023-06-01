using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace DeskShortCutMaster
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public Menu MainMenu;

        public void DisplayTreeView(TreeViewItem Parent,MenuTree Node)
        {
            foreach (var child in Node.children)
            {
                TreeViewItem childItem = new TreeViewItem();
                childItem.Header = child.DisplayName;
                childItem.Tag = child;
                Parent.Items.Add(childItem);
                DisplayTreeView(childItem, child);
            }
        }
        public void DisplayInitTreeView(Menu MainMenu)
        {
            MenuTreeView.Items.Clear();
            foreach(var child in MainMenu.ROOTNode.children)
            {
                TreeViewItem childItem = new TreeViewItem();
                childItem.Header = child.DisplayName;
                childItem.Tag = child;
                MenuTreeView.Items.Add(childItem);
                DisplayTreeView(childItem,child);
            }
        }

        public void InitComboBox()
        {
            List<string> DisplayPositionList = new List<string>
            {
                "0",
                "1",
                "2",
                "3",
                "4",
                "5",
                "8",
                "9",
                "10",
                "11",
                "12",
                "13",
                "14",
                "15",
            };

            List<string> NodeTypeList = new List<string>
            {
                "List",
            };

            List<string> NodeCommandList = new List<string>
            {
                "",
            };
            foreach (string item in DisplayPositionList)
            {
                FormDisplayPosition.Items.Add(item);
            }

            foreach (string item in NodeTypeList)
            {
                FormNodeType.Items.Add(item);
            }
            foreach (string item in NodeCommandList)
            {
                FormNodeCommand.Items.Add(item);
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            InitComboBox();

            string filePath = @"./MenuData";

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            string fileContent;
            using (StreamReader reader = new StreamReader(@"./MenuData", Encoding.UTF8))
            {
                fileContent = reader.ReadToEnd();
            }
            MainMenu = new Menu();
            MainMenu.GenerateTree(fileContent);
            DisplayInitTreeView(MainMenu);
            Console.WriteLine(MainMenu.GetSaveString());
        }

        private void MenuTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedItem = e.NewValue as TreeViewItem;
            if (selectedItem != null)
            {
                MenuTree SelectedNode = selectedItem.Tag as MenuTree;
                if (SelectedNode != null)
                {
                    FormDisplayPosition.Text = SelectedNode.DisplayPosition.ToString();
                    FormNodeCommand.Text = SelectedNode.NodeCommand;
                    FormNodeType.Text = SelectedNode.NodeType;
                    FormNodeData.Text = SelectedNode.NodeData;
                    FormDisplayName.Text = SelectedNode.DisplayName;
                }
            }
        }

        private void FormSave_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = MenuTreeView.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                MenuTree SelectedNode = selectedItem.Tag as MenuTree;
                if (SelectedNode != null)
                {
                    SelectedNode.DisplayPosition = UInt16.Parse(FormDisplayPosition.Text);
                    SelectedNode.NodeCommand = FormNodeCommand.Text;
                    SelectedNode.NodeType = FormNodeType.Text;
                    SelectedNode.NodeData = FormNodeData.Text;
                    SelectedNode.DisplayName = FormDisplayName.Text;
                }
            }
            MainMenu.Save();
            DisplayInitTreeView(MainMenu);
        }
    }
}
