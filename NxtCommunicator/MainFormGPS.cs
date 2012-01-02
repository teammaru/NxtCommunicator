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
    /// MainForm（GPS関係）
    /// </summary>
    public partial class MainForm
    {
        private const int INIT_KAKUDO = 270;
        private const decimal INIT_PLAY_SPEED = 1;
        private const decimal INIT_COURSE_SCALE = 1.2M;
        private const decimal INIT_SHAJIKU_KEN = 16.8M;
        private const decimal INIT_TIRE_RADIUS = 4.17M;

        private int gpsOffsetX = 0;
        private int gpsOffsetY = 0;

        // マウス ボタンを押し込んだところ。
        private Point mouseDownPoint = Point.Empty;

        // ドラッグするコントロールの親コントロール座標での、コントロールの配置座標とマウス座標の差。
        private Point pickedPoint = Point.Empty;

        // ドラッグするコントロールの大きさ。
        private Size controlSize = Size.Empty;

        // トラッカー（ドラッグに追従して動くRect）
        private Rectangle beforeRect = Rectangle.Empty;

        /// <summary>
        /// 初期処理
        /// </summary>
        private void initGps()
        {
            // Nxtの初期向き設定
            hiddenPictureBoxNxtMuki();
            refreshPictureBoxNxtMuki(PropertyUtil.getIntProp("textBoxKakudo", INIT_KAKUDO));

            // その他の初期値設定
            numericUpDownPlaySpeed.Value = PropertyUtil.getDecimalProp("numericUpDownPlaySpeed", INIT_PLAY_SPEED);
            numUpDownCourseScale.Value = PropertyUtil.getDecimalProp("numUpDownCourseScale", INIT_COURSE_SCALE);
            numUpDownShajikuLen.Value = PropertyUtil.getDecimalProp("numUpDownShajikuLen", INIT_SHAJIKU_KEN);
            numUpDownTireRadius.Value = PropertyUtil.getDecimalProp("numUpDownTireRadius", INIT_TIRE_RADIUS);

            pictureBoxPoint.Left = PropertyUtil.getIntProp("pictureBoxPoint_Left", pictureBoxPoint.Left);
            pictureBoxPoint.Top = PropertyUtil.getIntProp("pictureBoxPoint_Top", pictureBoxPoint.Top);

            // 表示Offset（中心位置でOffset）
            gpsOffsetX = pictureBoxPoint.Left + (pictureBoxPoint.Width / 2);
            gpsOffsetY = pictureBoxPoint.Top + (pictureBoxPoint.Height / 2);
        }

        /// <summary>
        /// 走行開始位置のドラッグ開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPoint_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            if (pictureBox == null)
            {
                return;
            }

            // 右クリックでない場合は無視（ドラッグ終了処理は行う）
            if (e.Button != MouseButtons.Left)
            {
                endDrag();
                return;
            }

            // ドラッグ開始位置の保持
            mouseDownPoint = new Point(e.X, e.Y);
            Point sp = panelCourse.PointToClient(pictureBox.PointToScreen(new Point(e.X, e.Y)));
            pickedPoint = new Point(pictureBox.Left - sp.X, pictureBox.Top - sp.Y);
        }

        /// <summary>
        /// ドラッグ終了処理
        /// </summary>
        private void endDrag()
        {
            mouseDownPoint = Point.Empty;
            pickedPoint = Point.Empty;
            controlSize = Size.Empty;
        }

        /// <summary>
        /// 走行位置のドラッグ終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPoint_MouseUp(object sender, MouseEventArgs e)
        {
            endDrag();
        }

        /// <summary>
        /// 走行開始位置のドラッグ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPoint_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDownPoint == Point.Empty)
            {
                return;
            }

            // ドラッグを開始すべきか判定する（マウスがある距離以上動いたらドラッグ開始とする）
            Rectangle moveRect = new Rectangle(
                mouseDownPoint.X - SystemInformation.DragSize.Width / 2,
                mouseDownPoint.Y - SystemInformation.DragSize.Height / 2,
                SystemInformation.DragSize.Width,
                SystemInformation.DragSize.Height);
            if (moveRect.Contains(e.X, e.Y))
            {
                return;
            }

            // ドラッグ対象の大きさを取得
            mouseDownPoint = Point.Empty;
            PictureBox pictureBox = sender as PictureBox;
            controlSize = new Size(pictureBox.Width, pictureBox.Height);

            // ドラッグ開始
            DragDropEffects dde = pictureBox.DoDragDrop(pictureBox, DragDropEffects.Move);

            // （ドラッグ中の前回表示）トラッカーの消去
            if (beforeRect != Rectangle.Empty)
            {
                // フォームの外にドロップされたときも、トラッカーを消す
                ControlPaint.DrawReversibleFrame(beforeRect, Color.Black, FrameStyle.Dashed);
                beforeRect = Rectangle.Empty;
            }

            pictureBox.Refresh();
        }

        /// <summary>
        /// 走行開始位置のドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelCourse_DragDrop(object sender, DragEventArgs e)
        {
            PictureBox target = e.Data.GetData(typeof(PictureBox)) as PictureBox;
            if (target == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            // トラッカーの消去
            ControlPaint.DrawReversibleFrame(beforeRect, Color.White, FrameStyle.Dashed);
            beforeRect = Rectangle.Empty;

            // 移動先の取得（親Componentとの座標差を加味）
            Point newPoint = panelCourse.PointToClient(new Point(e.X, e.Y));
            target.Left = newPoint.X + pickedPoint.X;
            target.Top = newPoint.Y + pickedPoint.Y;

            // GPSのOffsetの取得（PictureBoxの中心座標で保持）
            gpsOffsetX = target.Left + (target.Width / 2);
            gpsOffsetY = target.Top + (target.Height / 2);

            // 位置を保存
            PropertyUtil.saveProp("pictureBoxPoint_Left", pictureBoxPoint.Left);
            PropertyUtil.saveProp("pictureBoxPoint_Top", pictureBoxPoint.Top);

            endDrag();
        }

        /// <summary>
        /// コース上にDrag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelCourse_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PictureBox)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// コース上にDragOver
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelCourse_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PictureBox)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }

        }

        /// <summary>
        /// 走行開始位置のドラッグ中イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPoint_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // マウス右ボタン
            if ((e.KeyState & 2) == 2)
            {
                e.Action = DragAction.Cancel;
            }

            if (controlSize == Size.Empty)
            {
                return;
            }

            // ドラッグ中の位置からRectを生成して、、、
            Point loc = new Point(Control.MousePosition.X + pickedPoint.X, Control.MousePosition.Y + pickedPoint.Y);
            Rectangle rect = new Rectangle(loc, controlSize);

            // 前回と同じ位置だったら何もしない
            if (beforeRect.Equals(rect))
            {
                return;
            }

            // 前回位置のトラッカーを消して、今回の位置を保持
            if (beforeRect != Rectangle.Empty)
            {
                ControlPaint.DrawReversibleFrame(beforeRect, Color.Black, FrameStyle.Dashed);
            }
            ControlPaint.DrawReversibleFrame(rect, Color.White, FrameStyle.Dashed);
            beforeRect = new Rectangle(rect.Location, rect.Size);
        }

        /// <summary>
        /// クリアボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClear_Click(object sender, EventArgs e)
        {
            // 再描画して走行ラインを削除
            panelCourse.Refresh();
        }

        /// <summary>
        /// タイヤ半径からフォーカスが離れた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numUpDownTireRadius_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(numUpDownTireRadius.Text))
            {
                numUpDownTireRadius.Text = "0";
            }

            PropertyUtil.saveProp("numUpDownTireRadius", numUpDownTireRadius.Value);
        }

        /// <summary>
        /// 車軸長からフォーカスが離れた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numUpDownShajikuLen_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(numUpDownShajikuLen.Text))
            {
                numUpDownShajikuLen.Text = "0";
            }

            PropertyUtil.saveProp("numUpDownShajikuLen", numUpDownShajikuLen.Value);
        }

        /// <summary>
        /// 縮小率からフォーカスが離れた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numUpDownCourseScale_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(numUpDownCourseScale.Text))
            {
                numUpDownCourseScale.Text = "0";
            }

            PropertyUtil.saveProp("numUpDownCourseScale", numUpDownCourseScale.Value);
        }

        /// <summary>
        /// Nxt位置画像のマウスホバー
        /// Nxt走行軌跡の初期向を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPoint_MouseHover(object sender, EventArgs e)
        {
            timerPictureBoxNxtMuki.Stop();

            pictureBoxNxtMukiNorth.Left = pictureBoxPoint.Left - 10;
            pictureBoxNxtMukiNorth.Top = pictureBoxPoint.Top - pictureBoxNxtMukiNorth.Height;

            pictureBoxNxtMukiSouth.Left = pictureBoxPoint.Left - 10;
            pictureBoxNxtMukiSouth.Top = pictureBoxPoint.Top + pictureBoxPoint.Height + 5;

            pictureBoxNxtMukiEast.Left = pictureBoxPoint.Left + pictureBoxPoint.Width + 5;
            pictureBoxNxtMukiEast.Top = pictureBoxPoint.Top - 10;

            pictureBoxNxtMukiWest.Left = pictureBoxPoint.Left - pictureBoxNxtMukiWest.Width - 5;
            pictureBoxNxtMukiWest.Top = pictureBoxPoint.Top - 10;

            pictureBoxNxtMukiNorth.Visible = true;
            pictureBoxNxtMukiSouth.Visible = true;
            pictureBoxNxtMukiEast.Visible = true;
            pictureBoxNxtMukiWest.Visible = true;
        }

        /// <summary>
        /// Nxtの軌跡の初期方向を消すTimer
        /// </summary>
        System.Windows.Forms.Timer timerPictureBoxNxtMuki = new System.Windows.Forms.Timer();

        private void pictureBoxPoint_MouseLeave(object sender, EventArgs e)
        {
            timerPictureBoxNxtMuki.Tick += new EventHandler(hiddenPictureBoxNxtMuki);
            timerPictureBoxNxtMuki.Interval = 1000;
            timerPictureBoxNxtMuki.Start();
        }

        /// <summary>
        /// Nxt走行軌跡の初期向きを隠す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hiddenPictureBoxNxtMuki(object sender, EventArgs e)
        {
            timerPictureBoxNxtMuki.Stop();
            hiddenPictureBoxNxtMuki();
        }

        /// <summary>
        /// Nxt走行軌跡の初期向きを隠す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hiddenPictureBoxNxtMuki()
        {
            pictureBoxNxtMukiNorth.Visible = false;
            pictureBoxNxtMukiSouth.Visible = false;
            pictureBoxNxtMukiEast.Visible = false;
            pictureBoxNxtMukiWest.Visible = false;
        }

        /// <summary>
        /// Nxt走行軌跡の初期向きクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxNxtMukiNorth_Click(object sender, EventArgs e)
        {
            refreshPictureBoxNxtMuki(270);
        }

        /// <summary>
        /// Nxt走行軌跡の初期向きクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxNxtMukiEast_Click(object sender, EventArgs e)
        {
            refreshPictureBoxNxtMuki(0);
        }

        /// <summary>
        /// Nxt走行軌跡の初期向きクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxNxtMukiWest_Click(object sender, EventArgs e)
        {
            refreshPictureBoxNxtMuki(180);
        }

        /// <summary>
        /// Nxt走行軌跡の初期向きクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxNxtMukiSouth_Click(object sender, EventArgs e)
        {
            refreshPictureBoxNxtMuki(90);
        }

        /// <summary>
        /// Nxt走行軌跡の初期向き画像を再設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshPictureBoxNxtMuki(int muki)
        {
            pictureBoxNxtMukiNorth.Image = Properties.Resources.muki_north;
            pictureBoxNxtMukiSouth.Image = Properties.Resources.muki_south;
            pictureBoxNxtMukiEast.Image = Properties.Resources.muki_east;
            pictureBoxNxtMukiWest.Image = Properties.Resources.muki_west;

            if (muki == 0)
            {
                pictureBoxNxtMukiEast.Image = Properties.Resources.muki_east_active;
            }
            else if (muki == 90)
            {
                pictureBoxNxtMukiSouth.Image = Properties.Resources.muki_south_active;
            }
            else if (muki == 180)
            {
                pictureBoxNxtMukiWest.Image = Properties.Resources.muki_west_active;
            }
            else if (muki == 270)
            {
                pictureBoxNxtMukiNorth.Image = Properties.Resources.muki_north_active;
            }

            textBoxKakudo.Text = muki.ToString();

            PropertyUtil.saveProp("textBoxKakudo", muki);
        }

        /// <summary>
        /// ログ再生スピード・ロストフォーカス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPlaySpeed_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(numericUpDownPlaySpeed.Text))
            {
                numericUpDownPlaySpeed.Value = 1;
            }
            PropertyUtil.saveProp("numericUpDownPlaySpeed", numericUpDownPlaySpeed.Value);
        }
    }
}
