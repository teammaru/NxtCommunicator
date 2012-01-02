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
    /// MainForm（Serial関係）
    /// </summary>
    public partial class MainForm
    {
        private const int SEND_TEXTBOX_COUNT = 7;
        private const string PROP_KEY_PORT = "port";
        private string[] propKeyNameSendVal = { "textBoxVal1", "textBoxVal2", "textBoxVal3",
                                                "textBoxVal4", "textBoxVal5", "textBoxVal6",
                                                "textBoxVal7" };
        
        // コントロール部品の配列
        private TextBox[] textBoxSendVals = new TextBox[SEND_TEXTBOX_COUNT];
        private Button[] buttonSendVals = new Button[SEND_TEXTBOX_COUNT];

        /// <summary>
        /// 通信先開始時向け
        /// </summary>
        public delegate void ConnectDelegate();

        private void initSerial()
        {
            // コントロール部品を配列化
            textBoxSendVals[0] = textBoxVal1;
            textBoxSendVals[1] = textBoxVal2;
            textBoxSendVals[2] = textBoxVal3;
            textBoxSendVals[3] = textBoxVal4;
            textBoxSendVals[4] = textBoxVal5;
            textBoxSendVals[5] = textBoxVal6;
            textBoxSendVals[6] = textBoxVal7;

            buttonSendVals[0] = val1SendButton;
            buttonSendVals[1] = val2SendButton;
            buttonSendVals[2] = val3SendButton;
            buttonSendVals[3] = val4SendButton;
            buttonSendVals[4] = val5SendButton;
            buttonSendVals[5] = val6SendButton;
            buttonSendVals[6] = val7SendButton;

            // 送信値の初期値とイベント設定
            for (int ii = 0; ii < SEND_TEXTBOX_COUNT; ii++)
            {
                textBoxSendVals[ii].Text = CommonUtil.nvl(PropertyUtil.getProp(propKeyNameSendVal[ii]), "");
                buttonSendVals[ii].Click += new EventHandler(sendButton_Click);
            }

            // 選択可能なポートをSelBoxへ
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBoxPort.Items.Add(port);
            }

            // 選択ポートが記憶されていない場合は、先頭を選択
            // 以前の選択位置が存在しない場合はスルー
            int selectedIndex = int.Parse(CommonUtil.nvl(PropertyUtil.getProp(PROP_KEY_PORT), "0"));
            if (comboBoxPort.Items.Count > selectedIndex)
            {
                comboBoxPort.SelectedIndex = selectedIndex;
            }
        }

        /// <summary>
        /// 接続・切断ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (dataTransceiver.IsOpen == true)
            {
                try
                {
                    ConnectDelegate disconnectDelegate = new ConnectDelegate(disconnectSerial);
                    this.BeginInvoke(disconnectDelegate);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                return;
            }

            try
            {
                ConnectDelegate connectDelegate = new ConnectDelegate(connectSerial);
                this.BeginInvoke(connectDelegate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// シリアル切断
        /// </summary>
        private void disconnectSerial()
        {
            if (!dataTransceiver.disconnect())
            {
                MessageBox.Show("切断できませんでした");
                return;
            }
            this.serialDisabled();
        }

        /// <summary>
        /// シリアル接続
        /// </summary>
        private void connectSerial()
        {
            if (!this.connect())
            {
                MessageBox.Show("接続できませんでした");
                return;
            }
            this.serialEnabled();
            stopwatch4Grid.Start();
        }

        /// <summary>
        /// シリアル接続後のUI制御
        /// </summary>
        private void serialEnabled()
        {
            buttonConnect.Text = "切断";
            val1SendButton.Enabled = true;
            val2SendButton.Enabled = true;
            val3SendButton.Enabled = true;
            val4SendButton.Enabled = true;
            val5SendButton.Enabled = true;
            val6SendButton.Enabled = true;
            val7SendButton.Enabled = true;
        }

        /// <summary>
        /// シリアル切断後のUI制御
        /// </summary>
        private void serialDisabled()
        {
            buttonConnect.Text = "接続";
            val1SendButton.Enabled = false;
            val2SendButton.Enabled = false;
            val3SendButton.Enabled = false;
            val4SendButton.Enabled = false;
            val5SendButton.Enabled = false;
            val6SendButton.Enabled = false;
            val7SendButton.Enabled = false;
        }

        /// <summary>
        /// シリアル接続
        /// </summary>
        private bool connect()
        {
            PropertyUtil.saveProp(PROP_KEY_PORT, comboBoxPort.SelectedIndex.ToString());
            return dataTransceiver.connect(comboBoxPort.SelectedItem.ToString());
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        private void terminate()
        {
            dataTransceiver.disconnect();
        }

        /// <summary>
        /// 送信ボタンクリック.
        /// NXTへテキストを送信する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendButton_Click(object sender, EventArgs e)
        {
            for (int ii = 0; ii < SEND_TEXTBOX_COUNT; ii++)
            {
                if (sender == buttonSendVals[ii])
                {
                    dataTransceiver.Write(textBoxSendVals[ii].Text);
                    PropertyUtil.saveProp(propKeyNameSendVal[ii], textBoxSendVals[ii].Text);
                    return;
                }
            }
        }
    }
}
