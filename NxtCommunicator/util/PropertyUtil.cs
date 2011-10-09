using System;
using System.Diagnostics;

namespace NxtCommunicator
{
    /// <summary>
    /// Propertyに関するユーティリティ
    /// </summary>
    class PropertyUtil
    {

        /// <summary>
        /// 設定値を取得する
        /// </summary>
        /// <param name="key"></param>
        public static string getProp(string key)
        {
            try
            {
                return Properties.Settings.Default[key].ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return "";
            }
        }

        /// <summary>
        /// 設定値を保存する
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void saveProp(string key, string val)
        {
            Properties.Settings.Default[key] = val;
            Properties.Settings.Default.Save();
        }
    }
}
