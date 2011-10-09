using System;

namespace NxtCommunicator.log
{
    /// <summary>
    /// ログデータ
    /// </summary>
    public class LogData
    {
        public UInt32 SysTick
        {
            get;
            set;
        }

        public UInt32 RelTick
        {
            get;
            set;
        }

        public SByte DataLeft
        {
            get;
            set;
        }

        public SByte DataRight
        {
            get;
            set;
        }

        public UInt16 Light
        {
            get;
            set;
        }

        public Int32 MotorCnt0
        {
            get;
            set;
        }

        public Int32 MotorCnt1
        {
            get;
            set;
        }

        public Int32 MotorCnt2
        {
            get;
            set;
        }

        public Int16 SensorAdc0
        {
            get;
            set;
        }

        public Int16 SensorAdc1
        {
            get;
            set;
        }

        public Int16 SensorAdc2
        {
            get;
            set;
        }

        public Int16 SensorAdc3
        {
            get;
            set;
        }

        public Int32 I2c
        {
            get;
            set;
        }
    }
}
