using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NxtCommunicator.log;
using NxtCommunicator.util;
using System.Diagnostics;

namespace NxtCommunicator.replay
{
    /// <summary>
    /// ログ再生クラス
    /// </summary>
    class Player
    {
        private DataGridView dataGridView;
        private GPGUpdateDelegate gpsUpdateDelegate;
        private int currentRow = 0;
        private decimal speed = 0;
        private uint saveTick = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="gpsUpdateDelegate"></param>
        /// <param name="speed"></param>
        public Player(DataGridView dataGridView, GPGUpdateDelegate gpsUpdateDelegate, decimal speed)
        {
            this.dataGridView = dataGridView;
            this.gpsUpdateDelegate = gpsUpdateDelegate;
            this.speed = speed;
            saveTick = 0;
            currentRow = 0;
        }

        /// <summary>
        /// 再生する
        /// </summary>
        public void play()
        {
            int count = dataGridView.Rows.Count;
            uint wait = 0;
            for(int ii = 0; ii < count; ii++)
            {
                LogData logData = new LogData();
                logData.SysTick = ConvUtil.toUInt32(dataGridView.Rows[ii].Cells[0].Value);
                logData.MotorCnt1 = ConvUtil.toInt32(dataGridView.Rows[ii].Cells[5].Value);
                logData.MotorCnt2 = ConvUtil.toInt32(dataGridView.Rows[ii].Cells[6].Value);

                gpsUpdateDelegate.Invoke(logData);
                currentRow++;

                // 再生スピード制御（Tickの差分だけWait）
                // 初回は4ms固定とする
                wait = logData.SysTick - saveTick;
                if (wait == logData.SysTick || logData.SysTick == 0)
                {
                    wait = 4;
                }

                saveTick = logData.SysTick;
                System.Threading.Thread.Sleep((int)(wait / speed));
            }
        }
    }
}
