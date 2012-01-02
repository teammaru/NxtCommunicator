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
    public delegate void GPGUpdateDelegate(LogData logData);

    /// <summary>
    /// MainForm（Player関係）
    /// </summary>
    public partial class MainForm
    {
        private Gps gps = null;
        private Thread playerThread = null;

        /// <summary>
        /// 再生クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPlay_Click(object sender, EventArgs e)
        {
            gps = new Gps(Convert.ToDouble(numUpDownTireRadius.Text),
                            Convert.ToDouble(numUpDownShajikuLen.Text));

            this.startPlayer();
        }

        /// <summary>
        /// ログを再生する
        /// </summary>
        private void startPlayer()
        {
            GPGUpdateDelegate gpsUpdateDelegate = new GPGUpdateDelegate(gpsUpdate);
            Player player = new Player(dataGridView, gpsUpdateDelegate, numericUpDownPlaySpeed.Value);

            playerThread = new Thread(new ThreadStart(player.play));
            playerThread.IsBackground = true;
            playerThread.Start();
        }

        private delegate void MovePointDelegate(LogData logData);

        /// <summary>
        /// １行分のログデータから走行位置を演算する
        /// </summary>
        /// <param name="logData"></param>
        private void gpsUpdate(LogData logData)
        {
            MovePointDelegate movePointDelegate = new MovePointDelegate(movePoint);
            this.BeginInvoke(movePointDelegate, logData);
        }

        /// <summary>
        /// １行分のログデータから現在位置を演算し、位置を再表示する
        /// </summary>
        /// <param name="logData"></param>
        private void movePoint(LogData logData)
        {
            Position pos = gps.getPosition(logData.MotorCnt2, logData.MotorCnt1);
            Position currentNxtPoint = convScaleAndMuki(pos.X, pos.Y);

            Graphics graphics = panelCourse.CreateGraphics();
            Rectangle rectangle = new Rectangle(
                 Convert.ToInt32(currentNxtPoint.X),
                 Convert.ToInt32(currentNxtPoint.Y), 1, 1);
            graphics.DrawRectangle(Pens.Red, rectangle);

            redrawStripLine(logData.SysTick);
        }

        /// <summary>
        /// 位置情報から表示座標へ変換を行う
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Position convScaleAndMuki(double x, double y)
        {
            double scallX = x * double.Parse(numUpDownCourseScale.Text);
            double scallY = y * double.Parse(numUpDownCourseScale.Text);

            Position position = new Position();

            int kakudo = Convert.ToInt32(textBoxKakudo.Text);
            if (kakudo == 90)
            {
                position.X = scallY * -1;
                position.Y = scallX;
            }
            else if (kakudo == 180)
            {
                position.X = scallX * -1;
                position.Y = scallY * -1;
            }
            else if (kakudo == 270)
            {
                position.X = scallY;
                position.Y = scallX * -1;
            }
            else
            {
                position.X = scallX;
                position.Y = scallY;
            }

            position.X = position.X + gpsOffsetX;
            position.Y = position.Y + gpsOffsetY;

            return position;
        }

        /// <summary>
        /// ログ再生を停止する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPlayerStop_Click(object sender, EventArgs e)
        {
            if (playerThread == null)
            {
                return;
            }

            playerThread.Abort();
        }
    }
}
