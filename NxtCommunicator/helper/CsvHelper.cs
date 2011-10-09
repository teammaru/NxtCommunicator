using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NxtCommunicator
{
    /// <summary>
    /// CSV関係のヘルパ（DataGrid依存）
    /// </summary>
    class CsvHelper
    {
        /// <summary>
        /// グリッドからCSVデータを取得してファイルに出力する
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="filePath"></param>
        public void save(DataGridView dataGridView, String filePath)
        {
            // CSVファイルオープン
            StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.GetEncoding("UTF-8"));
            for (int rowPos = 0; rowPos < dataGridView.Rows.Count; rowPos++)
            {
                for (int colPos = 0; colPos < dataGridView.Columns.Count; colPos++)
                {
                    // DataGridViewのセルのデータ取得
                    String dt = "";
                    if (dataGridView.Rows[rowPos].Cells[colPos].Value != null)
                    {
                        dt = dataGridView.Rows[rowPos].Cells[colPos].Value.
                            ToString();
                    }
                    if (colPos < dataGridView.Columns.Count - 1)
                    {
                        dt = dt + ",";
                    }
                    // CSVファイル書込
                    sw.Write(dt);
                }
                sw.Write("\n");
            }

            sw.Close();
        }

        /// <summary>
        /// CSVからデータを取得し、グリッドに設定する
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="filePath"></param>
        public void read(DataGridView dataGridView, String filePath)
        {
            // CSVファイルオープン
            StreamReader sr = new StreamReader(filePath, System.Text.Encoding.GetEncoding("UTF-8"));

            // CSVファイルの各セルをDataGridViewに表示
            dataGridView.Rows.Clear();

            int row = 0;
            String line = "";
            do
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    dataGridView.Rows.Add();
                    String[] csv = line.Split(',');
                    for (int col = 0; col <= csv.GetLength(0) - 1; col++)
                    {
                        if (col < dataGridView.Columns.Count)
                        {
                            dataGridView.Rows[row].Cells[col].Value = csv[col];
                        }
                    }
                    row += 1;
                }
            } while (line != null);

            sr.Close();
        }
    }
}
