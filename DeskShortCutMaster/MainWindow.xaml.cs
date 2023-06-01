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
    public partial class MainWindow : Window
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
            foreach(var child in MainMenu.ROOTNode.children)
            {
                TreeViewItem childItem = new TreeViewItem();
                childItem.Header = child.DisplayName;
                childItem.Tag = child;
                MenuTreeView.Items.Add(childItem);
                DisplayTreeView(childItem,child);
            }
        }
        public MainWindow()
        {
            InitializeComponent();
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
                    Console.WriteLine(SelectedNode.ToString());
                }
            }
        }
    }
}
