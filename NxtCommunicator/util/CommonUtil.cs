using System;

namespace NxtCommunicator.util
{
    /// <summary>
    /// 共通ユーティリティクラス
    /// </summary>
    class CommonUtil
    {
        /// <summary>
        /// NVL2
        /// </summary>
        /// <param name="checkVal">対象文字列</param>
        /// <param name="initVal">初期値</param>
        /// <returns>null、またはブランクの場合に初期値を返す</returns>
        public static String nvl(String checkVal, String initVal)
        {
            if (checkVal == null || checkVal == "")
            {
                return initVal;
            }
            return checkVal;
        }
    }
}
