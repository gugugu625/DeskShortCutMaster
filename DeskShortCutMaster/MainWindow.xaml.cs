using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
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
        SerialPort DevicePort;
        System.Timers.Timer timer;
        public void PortSendData(string str)
        {
            Encoding utf8 = Encoding.GetEncoding("UTF-8");
            byte[] bytes = utf8.GetBytes(str);
            if (DevicePort.IsOpen)
            {
                DevicePort.Write(bytes, 0, bytes.Length);
            }
            else
            {
                try
                {
                    FileStream fileStream = new FileStream("./SerialPort.ini", FileMode.Open);
                    StreamReader sr = new StreamReader(fileStream);
                    string line;
                    if ((line = sr.ReadLine()) != null)
                    {
                        foreach (string vPortName in SerialPort.GetPortNames())
                        {
                            if (vPortName == line.Trim())
                            {
                                DevicePort.Open();
                                DevicePort.Write(bytes, 0, bytes.Length);
                                return;
                            }
                        }
                    }
                }
                catch
                {
                    Environment.Exit(0);
                }
                System.Windows.MessageBox.Show("端口关闭");
                Environment.Exit(0);
            }

        }

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
        public void ReturnAddNode(MenuTree res)
        {
            uint maxid = 0;
            foreach(var item in MainMenu.MenuList)
            {
                if(item.id > maxid)
                {
                    maxid = item.id;
                }
            }
            res.id = maxid + 1;
            TreeViewItem selectedItem = MenuTreeView.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                MenuTree SelectedNode = selectedItem.Tag as MenuTree;
                if (SelectedNode != null)
                {
                    res.Parent = SelectedNode;
                    SelectedNode.children.Add(res);
                    MainMenu.MenuList.Add(res);
                }
            }
            PortSendData("SetMenu" + MainMenu.GetSaveString());
            MainMenu.Save();
            DisplayInitTreeView(MainMenu);

        }
        public void ReturnAddROOTNode(MenuTree res)
        {
            uint maxid = 0;
            foreach (var item in MainMenu.MenuList)
            {
                if (item.id > maxid)
                {
                    maxid = item.id;
                }
            }
            res.id = maxid + 1;
            res.Parent = MainMenu.ROOTNode;
            MainMenu.ROOTNode.children.Add(res);
            MainMenu.MenuList.Add(res);
            PortSendData("SetMenu" + MainMenu.GetSaveString());
            MainMenu.Save();
            DisplayInitTreeView(MainMenu);

        }
        public MainWindow()
        {
            InitializeComponent();
            InitComboBox();

            COMInit init = new COMInit();
            init.ShowDialog();

            FileStream fileStream = new FileStream("./SerialPort.ini", FileMode.Open);
            StreamReader sr = new StreamReader(fileStream);
            string line;
            if ((line = sr.ReadLine()) != null)
            {
                DevicePort = new SerialPort(line.Trim(), 115200, Parity.None, 8, StopBits.One);
                DevicePort.DtrEnable = true;
                DevicePort.DataReceived += new SerialDataReceivedEventHandler(DevicePort_DataReceived);
                DevicePort.Open();
            }
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

            timer = new System.Timers.Timer(1000);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(SendData);
            timer.Start();
            //Console.WriteLine(MainMenu.GetSaveString());
        }
        private void SendData(object sender, System.Timers.ElapsedEventArgs e)
        {
            PortSendData("HeartBeat");
        }
        private void DevicePort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[DevicePort.BytesToRead];
            DevicePort.Read(buffer, 0, buffer.Length);
            string str = System.Text.Encoding.UTF8.GetString(buffer);
            if (str.StartsWith("OpenFile"))
            {
                try
                {
                    System.Diagnostics.Process.Start(str.Replace("OpenFile", ""));
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
                
            }
            Console.WriteLine("[REC]"+str);
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
            PortSendData("SetMenu" + MainMenu.GetSaveString());
            MainMenu.Save();
            DisplayInitTreeView(MainMenu);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MenuTreeView.SelectedItem == null)
            {
                return;
            }
            AddNode form = new AddNode();
            form.TransfEvent += ReturnAddNode;
            form.ShowDialog();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(MainMenu.GetSaveString());
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = MenuTreeView.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                MenuTree SelectedNode = selectedItem.Tag as MenuTree;
                if (SelectedNode != null)
                {
                   SelectedNode.Parent.children.Remove(SelectedNode);
                }
            }
            PortSendData("SetMenu"+MainMenu.GetSaveString());
            MainMenu.Save();
            DisplayInitTreeView(MainMenu);
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            AddNode form = new AddNode();
            form.TransfEvent += ReturnAddROOTNode;
            form.ShowDialog();
        }
    }
}
