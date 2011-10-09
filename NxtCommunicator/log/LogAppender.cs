using System;

namespace NxtCommunicator.log
{
    /// <summary>
    /// ログ受信制御クラス
    /// </summary>
    class LogAppender
    {
        /// <summary>
        /// パケット先頭からの番号
        /// </summary>
        private int currentBytePos = 0;

        /// <summary>
        /// SPP(Bluetooth)のパケット長定義
        /// </summary>
        private const UInt16 PACKET_HEADER_LENGTH = 2;
        private const UInt16 PACKET_BODY_LENGTH = 32;
        private const UInt16 PACKET_LENGTH = PACKET_HEADER_LENGTH + PACKET_BODY_LENGTH;

        /// <summary>
        /// パケット格納配列・ヘッダ
        /// </summary>
        private Byte[] packetHeader = new Byte[PACKET_HEADER_LENGTH];

        /// <summary>
        /// パケット格納配列・本体部
        /// </summary>
        private Byte[] packetBody = new Byte[PACKET_BODY_LENGTH];

        private UInt32? offTick;    // 時刻オフセット Nullable

        /// <summary>
        /// ログデータが１パケット（１行）分受信したことを通知するためのデリゲート
        /// </summary>
        private LogReceiveDelegate logReceiveDelegate;

        public LogAppender(LogReceiveDelegate logReceiveDelegate)
        {
            this.logReceiveDelegate = logReceiveDelegate;
        }

        /// <summary>
        /// ログ追加
        /// </summary>
        /// <param name="data"></param>
        public void append(Byte data)
        {
            // ヘッダー部
            if (currentBytePos < PACKET_HEADER_LENGTH)
            {
                readHeader(data);
            }

            // ペイロード 本体部
            else if (currentBytePos < PACKET_LENGTH)
            {
                readBody(data);
            }

            // こないはず
            else
            {
                currentBytePos = 0;
            }
        }

        /// <summary>
        /// ヘッダ部読み込み
        /// </summary>
        /// <param name="data"></param>
        private void readHeader(Byte data)
        {
            // 順送りでパケットヘッダー配列へ保存
            packetHeader[currentBytePos++] = data;

            // ヘッダ終端に達していなかったら次の受信待ち
            if (currentBytePos != PACKET_HEADER_LENGTH)
            {
                return;
            }

            // NXTから送信されるパケットサイズにはヘッダの２バイト分は含まれない   とのこと
            UInt16 len = BitConverter.ToUInt16(packetHeader, 0);
            if (len != PACKET_BODY_LENGTH)
            {
                // パケット仕様： ヘッダー値 ＝ ペイロードサイズ
                // 想定したヘッダー値でなければ１バイト分を読み捨てる
                packetHeader[0] = packetHeader[1];
                currentBytePos = 1;
            }
        }

        /// <summary>
        /// 本体部読み込み
        /// </summary>
        /// <param name="data"></param>
        private void readBody(Byte data)
        {
            packetBody[currentBytePos++ - PACKET_HEADER_LENGTH] = data;

            // １パケット分に達していない場合は次の受信待ち
            if (currentBytePos != PACKET_LENGTH)
            {
                return;
            }

            LogData logData = new LogData();

            // パケットをフィールドに変換
            logData.SysTick = BitConverter.ToUInt32(packetBody, 0);
            logData.DataLeft = (SByte)packetBody[4];
            logData.DataRight = (SByte)packetBody[5];
            logData.Light = BitConverter.ToUInt16(packetBody, 6);
            logData.MotorCnt0 = BitConverter.ToInt32(packetBody, 8);
            logData.MotorCnt1 = BitConverter.ToInt32(packetBody, 12);
            logData.MotorCnt2 = BitConverter.ToInt32(packetBody, 16);
            logData.SensorAdc0 = BitConverter.ToInt16(packetBody, 20);
            logData.SensorAdc1 = BitConverter.ToInt16(packetBody, 22);
            logData.SensorAdc2 = BitConverter.ToInt16(packetBody, 24);
            logData.SensorAdc3 = BitConverter.ToInt16(packetBody, 26);
            logData.I2c = BitConverter.ToInt32(packetBody, 28);
            logData.RelTick = getTick(logData.SysTick);

            // データ受信を通知
            this.logReceiveDelegate.Invoke(logData);

            // 全部読み終わったので、読み込み位置を先頭に戻す
            currentBytePos = 0;
        }

        /// <summary>
        /// Tickを返す
        /// </summary>
        /// <returns></returns>
        private UInt32 getTick(UInt32 sysTick)
        {
            // 初回にオフセット確定
            if (offTick == null)
            {
                offTick = sysTick;
            }

            // 相対時間（ログ開始時からの時刻）計算
            if (sysTick >= offTick)
            {
                return sysTick - (UInt32)offTick;
            }

            // システム時刻が最大値を越えて一周した場合
            else
            {
                return sysTick + UInt32.MaxValue - (UInt32)offTick;
            }
        }
    }
}
