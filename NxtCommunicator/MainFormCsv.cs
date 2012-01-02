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
    /// MainForm（CSV関係）
    /// </summary>
    public partial class MainForm
    {
        /// <summary>
        /// CSV出力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void csvOutputButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();

            fileDialog.FileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
            fileDialog.InitialDirectory = @".";
            fileDialog.Filter = "CSV (*.csv)|*.csv|すべてのファイル (*.*)|*.*";
            fileDialog.Title = "保存先のファイルを入力、選択してください";
            fileDialog.RestoreDirectory = true;
            fileDialog.CheckFileExists = false;
            fileDialog.CheckPathExists = true;

            // 開くボタンが押されたらテキストボックスに表示
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                CsvHelper csvHelper = new CsvHelper();
                csvHelper.save(dataGridView, fileDialog.FileName);
            }
        }

        /// <summary>
        /// CSVインポート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void csvImportButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.FileName = "";
            fileDialog.InitialDirectory = @".";
            fileDialog.Filter = "CSV (*.csv)|*.csv|すべてのファイル (*.*)|*.*";
            fileDialog.Title = "ファイルを選択";
            fileDialog.RestoreDirectory = true;
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;

            // 開くボタンが押されたらテキストボックスに表示
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                CsvHelper csvHelper = new CsvHelper();
                csvHelper.read(dataGridView, fileDialog.FileName);

                for (int ii = 0; ii < CHART_COUNT; ii++)
                {
                    this.drawChart(charts[ii], getTarget4Chart(ii));
                }
            }
        }
    }
}
