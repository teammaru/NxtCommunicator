using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.Ports;
using System.Collections.Generic;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using NxtCommunicator.util;
using NxtCommunicator.io;
using NxtCommunicator.log;
using System.Drawing.Imaging;
using NxtCommunicator.replay;
using System.Threading;
using NxtCommunicator.analyze;

namespace NxtCommunicator
{
    /// <summary>
    /// ログ受信時のハンドラ用
    /// </summary>
    /// <param name="logData"></param>
    public delegate void LogReceiveDelegate(LogData logData);

    /// <summary>
    /// メイン操作、表示フォーム
    /// </summary>
    public partial class MainForm : Form
    {
        private LogReceiveDelegate logReceiveDelegate;
        private DataTransceiver dataTransceiver;

        public MainForm()
        {
            InitializeComponent();

            logReceiveDelegate = new LogReceiveDelegate(receiveLog);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            init();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            terminate();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void init()
        {
            initGps();
            initSerial();
            initGrid();

            dataTransceiver = new DataTransceiver(logReceiveDelegate);
        }
    }
}
