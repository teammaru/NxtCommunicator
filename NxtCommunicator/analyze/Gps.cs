using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace NxtCommunicator.analyze
{
    /// <summary>
    /// 走行位置算出クラス
    /// </summary>
    class Gps
    {
        /// <summary>
        /// タイヤ半径
        /// </summary>
        private double tireRadius = 0;

        /// <summary>
        /// 車軸長
        /// </summary>
        private double shajikuLen = 0;

        // 前回計測値関係
        private double saveMotorCountL = 0;
        private double saveMotorCountR = 0;
        private double saveX = 0;
        private double saveY = 0;
        private double saveSenkaiKakudo = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="tireRadius"></param>
        /// <param name="shajikuLen"></param>
        /// <param name="initKakado"></param>
        public Gps(double tireRadius, double shajikuLen)
        {
            this.tireRadius = tireRadius;
            this.shajikuLen = shajikuLen;
        }

        /// <summary>
        /// 走行位置を計測する
        /// </summary>
        /// <param name="motorCountL"></param>
        /// <param name="motorCountR"></param>
        /// <returns></returns>
        public Position getPosition(int motorCountL, int motorCountR)
        {
            // タイヤの回転角度 「回転角度 = 今回エンコーダ値 - 前回エンコーダ値」
            double thL = motorCountL - saveMotorCountL;
            double thR = motorCountR - saveMotorCountR;

            // タイヤの進んだ距離 「距離 = 2πr * (θ / 360)    (r:タイヤ半径)
            double distanceL = 2 * Math.PI * tireRadius * (thL / 360);
            double distanceR = 2 * Math.PI * tireRadius * (thR / 360);

            // NXTの「移動した距離」 「距離 = (右タイヤ移動距離 + 左タイヤ移動距離) / 2
            double distance = (distanceR + distanceL) / 2;

            // NXTの「旋回角度」
            // 孤長 ＝ 右タイヤ移動距離 - 左タイヤ移動距離
            // 旋回角度 ＝ 孤長 / 車軸長
            double kocho = distanceR - distanceL;
            double senkaiKakudo = kocho / shajikuLen;

            double saX = 0;
            double saY = 0;
            double kakudo = saveSenkaiKakudo + senkaiKakudo;

            // cosは引数がゼロの場合（直進した場合）に１を返すので、ゼロ以外の場合にのみcos演算する
            // （ゼロの場合はsaXはゼロ）
            // X座標 ＝ 移動した距離 * cos(トータルの旋回角度)
            if (kakudo != 0)
            {
                saX = distance * Math.Cos(kakudo);
            }

            // Y座標 ＝ 移動した距離 * sin(トータルの旋回角度)
            saY = distance * Math.Sin(kakudo);

            Position pos = new Position();
            pos.X = saveX + saX;
            pos.Y = saveY + saY;
            pos.Kakudo = kakudo;

            //Debug.WriteLine(
            //        "motorCountL:" + motorCountL
            //        + ",thL:" + thL
            //        + ",thR:" + thR
            //        + ",distL:" + distanceL
            //        + ",distR:" + distanceR
            //        + ",kocho:" + kocho
            //        + ",dist:" + distance
            //        + ", senkaiKakudo:" + senkaiKakudo
            //        + ", saX:" + saX
            //        + ", saY:" + saY
            //        + ", X:" + pos.X
            //        + ", Y:" + pos.Y
            //        + ", saveSenkaiKakudo:" + saveSenkaiKakudo
            //        );

            saveMotorCountL = motorCountL;
            saveMotorCountR = motorCountR;
            saveX = pos.X;
            saveY = pos.Y;
            saveSenkaiKakudo = pos.Kakudo;

            return pos;
        }
    }

    class Position
    {
        public Position()
        {
            this.X = 0;
            this.Y = 0;
        }

        public Position(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public double X
        {
            get;
            set;
        }

        public double Y
        {
            get;
            set;
        }

        public double Kakudo
        {
            get;
            set;
        }
    }
}
