using System;
using System.IO.Ports;
using System.Diagnostics;

namespace NxtCommunicator.io
{
    /// <summary>
    /// シリアル送受信クラス
    /// </summary>
    class BasicSerialPort : SerialPort
    {
        private DataReceiveDelegate dataReceiveDelegate;

        public BasicSerialPort(DataReceiveDelegate dataReceiveDelegate)
        {
            init();
            this.dataReceiveDelegate = dataReceiveDelegate;
        }

        /// <summary>
        /// 各種初期化処理
        /// </summary>
        public void init()
        {
            // データ受信時のハンドラ
            base.DataReceived += new SerialDataReceivedEventHandler(this.serialDataReceived);
        }

        /// <summary>
        /// 接続を行う
        /// </summary>
        /// <param name="portName"></param>
        public bool connect(string portName)
        {
            if (base.IsOpen == true)
            {
                return false;
            }

            base.PortName = portName;
            base.BaudRate = 9600;
            base.DataBits = 8;
            base.StopBits = StopBits.One;
            base.Parity = Parity.None;

            base.Open();
            base.DtrEnable = true;
            base.RtsEnable = true;
            base.NewLine = "\r\n";

            // 接続できていない場合
            if (base.IsOpen == false)
            {
                return false;
            }

            Debug.WriteLine("connect!");
            return true;
        }

        /// <summary>
        /// 切断
        /// </summary>
        public bool disconnect()
        {
            if (base.IsOpen == false)
            {
                return false;
            }

            base.Close();
            base.Dispose();

            // 切断できていない場合
            if (base.IsOpen == true)
            {
                return false;
            }

            Debug.WriteLine("disconnect!");
            return true;
        }

        /// <summary>
        /// シリアルデータ受信ハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // データ受信バッファ
            Byte[] buf;
            try
            {
                buf = new byte[this.BytesToRead];
            }
            catch
            {
                return;
            }

            if (buf.Length > 0)
            {
                try
                {
                    // シリアルポートより受信
                    base.Read(buf, 0, buf.Length);

                    dataReceiveDelegate.Invoke(buf);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }

        }
    }
}
