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

namespace DeskShortCutMaster
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public Menu MainMenu;
        public MainWindow()
        {
            InitializeComponent();

            MainMenu = new Menu();
            MainMenu.GenerateTree("1/0/cd1/List///0/\r\n5/0/菜单5/List///1/\r\n6/1/菜单6/List///1/\r\n2/5/菜单2/List///0/\r\n3/8/菜单3/List///0/\r\n4/13/菜单4/List///0/");
            Console.WriteLine(MainMenu.GetSaveString());
        }
    }
}
