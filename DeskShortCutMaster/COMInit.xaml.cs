﻿using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace DeskShortCutMaster
{
    /// <summary>
    /// COMInit.xaml 的交互逻辑
    /// 用于自动选择串口
    /// </summary>
    public partial class COMInit : Window
    {
        SerialPort DevicePort;
        /// <summary>
        /// 初始化界面及新进程
        /// </summary>
        public COMInit()
        {
            InitializeComponent();
            ThreadStart childref = new ThreadStart(ScanSerialPort);
            Thread childThread = new Thread(childref);
            childThread.Start();
        }
        /// <summary>
        /// 扫描所有串口，发送获取设备名称，如果超时没有回应也视为错误设备，与结果对应时保存串口名称
        /// </summary>
        private void ScanSerialPort()
        {
            int ScanCount = 0;
            foreach (string vPortName in SerialPort.GetPortNames())
            {
                DevicePort = new SerialPort(vPortName, 115200, Parity.None, 8, StopBits.One);
                Console.WriteLine(vPortName);
                try
                {
                    DevicePort.Open();
                    DevicePort.DtrEnable = true;
                    DevicePort.WriteLine("GetDeviceName");
                    DateTime start = DateTime.Now;
                    while (DevicePort.BytesToRead == 0)
                    {
                        //Console.WriteLine(DevicePort.BytesToRead);
                        TimeSpan pass = DateTime.Now - start;
                        if (pass.TotalMilliseconds >= 100)
                        {
                            Console.WriteLine("TIMEOUT");
                            break;
                        }
                        Thread.Sleep(1);
                    }
                    Thread.Sleep(50);
                    byte[] recData = new byte[DevicePort.BytesToRead];
                    DevicePort.Read(recData, 0, recData.Length);

                    if (System.Text.Encoding.UTF8.GetString(recData).Trim() == "DeskShortCut")
                    {
                        ScanCount++;
                        Console.WriteLine(vPortName);
                        File.WriteAllText(@"./SerialPort.ini", vPortName);
                    }

                    //timer.Start();
                    DevicePort.Close();
                }
                catch { }
            }
            if (ScanCount == 0)
            {
                //整个程序退出
                MessageBox.Show("无法检测到设备");
                Environment.Exit(0);
            }
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.DialogResult = true;
            }));
        }
    }
}
