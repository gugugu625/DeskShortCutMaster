using AudioSwitcher.AudioApi.CoreAudio;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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
        CoreAudioDevice defaultPlaybackDevice;

        /// <summary>
        /// 向串口发送一个字符串
        /// </summary>
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

        /// <summary>
        /// 用TreeView显示字符串（递归部分）
        /// </summary>
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

        /// <summary>
        /// 向TreeView显示字符串（初始启动）
        /// </summary>
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

        /// <summary>
        /// 初始化列表
        /// </summary>
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
        /// <summary>
        /// 新增节点的回调函数
        /// </summary>
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
            PortSendData("SetMenuStart" + MainMenu.GetSaveString() + "SetMenuEnd");
            MainMenu.Save();
            DisplayInitTreeView(MainMenu);

        }
        /// <summary>
        /// 新增根节点的回调函数
        /// </summary>
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
            PortSendData("SetMenuStart" + MainMenu.GetSaveString() + "SetMenuEnd");
            MainMenu.Save();
            DisplayInitTreeView(MainMenu);

        }
        /// <summary>
        /// 初始化托盘图标
        /// </summary>
        public void InitNotifyIcon()
        {
            StackPanel subpanel = new StackPanel();
            subpanel.Background = new SolidColorBrush(Colors.White);
            RowDefinition subgridrow1 = new RowDefinition();
            Button exitbutton = new Button { Content = "   退出   " };
            exitbutton.Click += ExitButton_Click;
            Button restartbutton = new Button { Content = "重新启动" };
            restartbutton.Click += RestartButton_Click;
            subpanel.Children.Add(exitbutton);
            subpanel.Children.Add(restartbutton);
            notifyicon.ContextContent = subpanel;
        }
        public void InitAudioDevice()
        {
            defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
        }
        /// <summary>
        /// 主函数
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            //defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
            InitComboBox();
            InitNotifyIcon();

            COMInit init = new COMInit();
            init.ShowDialog();//显示串口初始化（自动选择）窗口

            //读取由自动选择保存的串口
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
            ThreadStart childref = new ThreadStart(InitAudioDevice);
            Thread childThread = new Thread(childref);
            childThread.Start();
            
            //检测菜单文件是否存在，否则创建
            string filePath = @"./MenuData";

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            //读取整个菜单文件，之后生成树并显示
            string fileContent;
            using (StreamReader reader = new StreamReader(@"./MenuData", Encoding.UTF8))
            {
                fileContent = reader.ReadToEnd();
            }
            MainMenu = new Menu();
            MainMenu.GenerateTree(fileContent);
            DisplayInitTreeView(MainMenu);

            //每一秒发送心跳包的计时器
            timer = new System.Timers.Timer(1000);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(SendData);
            timer.Start();
            
            //Console.WriteLine(MainMenu.GetSaveString());
        }
        /// <summary>
        /// 发送心跳包
        /// </summary>
        private void SendData(object sender, System.Timers.ElapsedEventArgs e)
        {
            PortSendData("HeartBeat");
        }
        /// <summary>
        /// 设备串口接收回调
        /// </summary>
        private void DevicePort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[DevicePort.BytesToRead];
            DevicePort.Read(buffer, 0, buffer.Length); 
            string str = Encoding.UTF8.GetString(buffer);
            if (str.StartsWith("OpenFile"))
            {
                try
                {
                    string res = Encoding.UTF8.GetString(Convert.FromBase64String(str.Replace("OpenFile", "").Trim()));
                    Console.WriteLine(res);
                    System.Diagnostics.Process.Start(res);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
                
            }else if (str.StartsWith("IncreaseVolume"))
            {
                int VolumeChange = Convert.ToInt32(str.Replace("IncreaseVolume", "").Trim());
                defaultPlaybackDevice.Volume += VolumeChange;
            }
            else if (str.StartsWith("DecreaseVolume"))
            {
                int VolumeChange = Convert.ToInt32(str.Replace("DecreaseVolume", "").Trim());
                defaultPlaybackDevice.Volume -= VolumeChange;
            }
            else if (str.StartsWith("Mute"))
            {
                defaultPlaybackDevice.Mute(true);
            }
            else if (str.StartsWith("CancelMute"))
            {
                defaultPlaybackDevice.Mute(false);
            }
            Console.WriteLine("[REC]"+str);
        }
        /// <summary>
        /// TreeView选项更改事件
        /// </summary>
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
        /// <summary>
        /// 保存按键按下事件
        /// </summary>
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
            PortSendData("SetMenuStart" + MainMenu.GetSaveString() + "SetMenuEnd");
            MainMenu.Save();
            DisplayInitTreeView(MainMenu);
        }
        /// <summary>
        /// 新增子节点按下事件
        /// </summary>
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
        /// <summary>
        /// 复制保存字符串（调试功能
        /// </summary>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(MainMenu.GetSaveString());
        }
        /// <summary>
        /// 删除节点按下事件
        /// </summary>
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
            PortSendData("SetMenuStart" + MainMenu.GetSaveString() + "SetMenuEnd");
            MainMenu.Save();
            DisplayInitTreeView(MainMenu);
        }

        /// <summary>
        /// 新增根节点按下事件
        /// </summary>
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            AddNode form = new AddNode();
            form.TransfEvent += ReturnAddROOTNode;
            form.ShowDialog();
        }

        /// <summary>
        /// 托盘气泡双击显示主菜单
        /// </summary>
        private void notifyicon_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        /// <summary>
        /// 关闭窗口时改为隐藏主界面
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        /// <summary>
        /// 托盘气泡退出
        /// </summary>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            DevicePort.Close();
            Environment.Exit(0);
        }
        /// <summary>
        /// 托盘气泡重启
        /// </summary>
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Application.Current.Shutdown();
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            About form = new About();
            form.ShowDialog();
        }
    }
}
