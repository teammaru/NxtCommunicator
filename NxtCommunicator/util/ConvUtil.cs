using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NxtCommunicator.util
{
    /// <summary>
    /// 変換系ユーティリティ
    /// </summary>
    class ConvUtil
    {
        public static UInt32 toUInt32(object val)
        {
            if (!checkNum(val))
            {
                return 0;
            }

            return Convert.ToUInt32(val);
        }

        public static Int32 toInt32(object val)
        {
            if (!checkNum(val))
            {
                return 0;
            }

            return Convert.ToInt32(val);
        }

        private static bool checkNum(object val)
        {
            if (val == null)
            {
                return false;
            }

            if (val.ToString() == "")
            {
                return false;
            }

            return true;
        }
    }
}
