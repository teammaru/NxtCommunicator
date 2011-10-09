using System;
using NxtCommunicator.log;

namespace NxtCommunicator.io
{
    /// <summary>
    /// データ受信時のハンドラ用
    /// </summary>
    /// <param name="message"></param>
    public delegate void DataReceiveDelegate(Byte[] message);

    /// <summary>
    /// データ送受信クラス
    /// </summary>
    class DataTransceiver
    {
        private LogReceiveDelegate logReceiveDelegate;
        private BasicSerialPort serialPort;
        private LogAppender logAppender;

        public DataTransceiver(LogReceiveDelegate logReceiveDelegate)
        {
            this.logReceiveDelegate = logReceiveDelegate;
            DataReceiveDelegate dataReceiveDelegate = new DataReceiveDelegate(dataReceive);

            serialPort = new BasicSerialPort(dataReceiveDelegate);
            logAppender = new LogAppender(logReceiveDelegate);
        }

        /// <summary>
        /// データ受信
        /// </summary>
        /// <param name="message"></param>
        private void dataReceive(Byte[] message)
        {
            for (int ii = 0; ii < message.Length; ii++)
            {
                logAppender.append(message[ii]);
            }
        }

        /// <summary>
        /// 通信先が接続済みか判定する
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return serialPort.IsOpen;
            }
        }

        /// <summary>
        /// 通信を切断する
        /// </summary>
        public bool disconnect()
        {
            return serialPort.disconnect();
        }

        /// <summary>
        /// 通信を開始する
        /// </summary>
        /// <param name="portName"></param>
        public bool connect(string portName)
        {
            logAppender = new LogAppender(logReceiveDelegate);
            return serialPort.connect(portName);
        }

        /// <summary>
        /// データを送信する
        /// </summary>
        /// <param name="text"></param>
        public void Write(string text)
        {
            serialPort.Write(text);
        }
    }
}
