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
    /// MainForm（Grid, Chart関係）
    /// </summary>
    public partial class MainForm
    {
        private const int CHART_COUNT = 3;
        private string[] propKeyNameComboTarget = { "comboTargetChart0", "comboTargetChart1", "comboTargetChart2" };

        private delegate void DelegateDrawChart(Chart targetChart, int targetColPos);

        // コントロール部品の配列
        private Chart[] charts = new Chart[CHART_COUNT];
        private ComboBox[] comboBoxTargetCharts = new ComboBox[CHART_COUNT];

        // ログデータ
        private List<DataGridViewRow> logBufferList = new List<DataGridViewRow>();

        // ストップウォッチ（経過時間計測）
        private Stopwatch stopwatch4Grid = new Stopwatch();

        /// <summary>
        /// 初期処理
        /// </summary>
        private void initGrid()
        {
            charts[0] = chart0;
            charts[1] = chart1;
            charts[2] = chart2;

            comboBoxTargetCharts[0] = comboBoxChartingTarget0;
            comboBoxTargetCharts[1] = comboBoxChartingTarget1;
            comboBoxTargetCharts[2] = comboBoxChartingTarget2;


            // 表示チャートComboの初期値とイベント設定
            for (int ii = 0; ii < CHART_COUNT; ii++)
            {
                comboBoxTargetCharts[ii].SelectedIndex
                    = int.Parse(CommonUtil.nvl(PropertyUtil.getProp(propKeyNameComboTarget[ii]), "0"));
                comboBoxTargetCharts[ii].SelectedIndexChanged += new EventHandler(comboBoxChartingTarget_SelectedIndexChanged);
            }
        }

        /// <summary>
        /// 取得したログをGrid、Chartに出力する
        /// </summary>
        /// <param name="logData"></param>
        private void receiveLog(LogData logData)
        {
            // SerialからはControl部品の操作はできないので、Formで再度Invoke
            LogReceiveDelegate bufferingLogDelegate = new LogReceiveDelegate(bufferingLog);
            this.BeginInvoke(bufferingLogDelegate, logData);
        }

        /// <summary>
        /// 取得したログをGrid、Chartに出力する
        /// </summary>
        /// <param name="logData"></param>
        private void bufferingLog(LogData logData)
        {
            DataGridViewRow row = new DataGridViewRow();

            row.CreateCells(dataGridView);
            row.Cells[0].Value = logData.RelTick;
            row.Cells[1].Value = logData.DataLeft;
            row.Cells[2].Value = logData.DataRight;
            row.Cells[3].Value = logData.Light;
            row.Cells[4].Value = logData.MotorCnt0;
            row.Cells[5].Value = logData.MotorCnt1;
            row.Cells[6].Value = logData.MotorCnt2;
            row.Cells[7].Value = logData.SensorAdc0;
            row.Cells[8].Value = logData.SensorAdc1;
            row.Cells[9].Value = logData.SensorAdc2;
            row.Cells[10].Value = logData.SensorAdc3;
            row.Cells[11].Value = logData.I2c;

            logBufferList.Add(row);

            if (stopwatch4Grid.ElapsedMilliseconds > 1000 || logBufferList.Count > 100)
            {
                outputGrid();
                stopwatch4Grid.Restart();
                logBufferList.Clear();
            }
        }

        /// <summary>
        /// グリッド更新
        /// </summary>
        private void outputGrid()
        {
            dataGridView.Rows.AddRange(logBufferList.ToArray());

            // グリッド表示件数が設定されている場合、設定値を超えたデータを削除
            int maxLine = (int)numUpDownMaxLine.Value;
            if (maxLine < dataGridView.Rows.Count)
            {
                int removeLineCount = dataGridView.Rows.Count - maxLine;
                for (int ii = 0; ii < removeLineCount; ii++)
                {
                    if (dataGridView.Rows.Count > 1)
                    {
                        dataGridView.Rows.RemoveAt(0);
                    }
                }
            }

            // 更新
            DelegateDrawChart delegateByteOut = new DelegateDrawChart(drawChart);
            for (int ii = 0; ii < CHART_COUNT; ii++)
            {
                this.BeginInvoke(delegateByteOut, charts[ii], getTarget4Chart(ii));
            }
        }

        /// <summary>
        /// ログクリアクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLogClear_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Clear();
            logBufferList.Clear();

            for (int ii = 0; ii < CHART_COUNT; ii++)
            {
                this.drawChart(charts[ii], getTarget4Chart(ii));
            }
        }

        /// <summary>
        /// 指定IndexのComboの値からチャートに表示するログ種別を返す
        /// </summary>
        /// <param name="comboIndex"></param>
        /// <returns></returns>
        private int getTarget4Chart(int comboIndex)
        {
            return comboBoxTargetCharts[comboIndex].SelectedIndex - 1;
        }

        /// <summary>
        /// チャート更新
        /// </summary>
        /// <param name="targetChart"></param>
        /// <param name="targetColPos"></param>
        private void drawChart(Chart targetChart, int targetColPos)
        {
            targetChart.Series.Clear();

            if (targetColPos < 0)
            {
                return;
            }

            Series series = new Series();
            series.ChartType = SeriesChartType.FastPoint;
            series.MarkerSize = 2;
            series.MarkerStyle = MarkerStyle.Circle;
            series.ToolTip = "#VALX{D}, #VAL{D}";

            for (int ii = 0; ii < dataGridView.Rows.Count; ii++)
            {
                if (dataGridView.Rows[ii].Cells[targetColPos].Value == null
                    || "".Equals(dataGridView.Rows[ii].Cells[targetColPos].Value.ToString()))
                {
                    break;
                }

                DataPoint point = series.Points.Add(Convert.ToDouble(dataGridView.Rows[ii].Cells[targetColPos].Value));
                point.XValue = Convert.ToDouble(dataGridView.Rows[ii].Cells[0].Value.ToString());
                point.AxisLabel = dataGridView.Rows[ii].Cells[0].Value.ToString();
            }
            targetChart.Series.Add(series);
            targetChart.ResetAutoValues();
        }

        /// <summary>
        /// チャート表示種別Combo変更イベントハンドラ.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxChartingTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int ii = 0; ii < CHART_COUNT; ii++)
            {
                if (sender == comboBoxTargetCharts[ii])
                {
                    PropertyUtil.saveProp(propKeyNameComboTarget[ii], comboBoxTargetCharts[ii].SelectedIndex.ToString());
                    this.drawChart(charts[ii], comboBoxTargetCharts[ii].SelectedIndex - 1);
                    return;
                }
            }
        }

        /// <summary>
        /// グリッドクリック.
        /// クリックしたTick位置にガイドを表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex <= 0)
            {
                return;
            }
            if (dataGridView.Rows[e.RowIndex].Cells[0].Value == null)
            {
                return;
            }
            if (dataGridView.Rows[e.RowIndex].Cells[0].Value.ToString() == "")
            {
                return;
            }

            // ガイド線の再表示
            redrawStripLine(long.Parse(dataGridView.Rows[e.RowIndex].Cells[0].Value.ToString()));
        }

        /// <summary>
        /// ガイド線の再表示
        /// </summary>
        /// <param name="rowIndex"></param>
        private void redrawStripLine(long xVal)
        {
            for (int ii = 0; ii < CHART_COUNT; ii++)
            {
                drawStripLine(charts[ii], xVal);
            }
        }

        /// <summary>
        /// ガイド線を表示
        /// </summary>
        /// <param name="targetChart"></param>
        /// <param name="xVal"></param>
        private void drawStripLine(Chart targetChart, long xVal)
        {
            if (targetChart.ChartAreas[0].AxisX.StripLines.Count == 0)
            {
                targetChart.ChartAreas[0].AxisX.StripLines.Add(new StripLine());
                targetChart.ChartAreas[0].AxisX.StripLines[0].BackColor = Color.Red;
                targetChart.ChartAreas[0].AxisX.StripLines[0].StripWidth = 1;
                targetChart.ChartAreas[0].AxisX.StripLines[0].Interval = 999999999;
            }

            targetChart.ChartAreas[0].AxisX.StripLines[0].IntervalOffset = xVal;
        }

        /// <summary>
        /// ガイド線を消す
        /// </summary>
        private void clearStripLine(Chart targetChart)
        {
            targetChart.ChartAreas[0].AxisX.StripLines.Clear();
        }
    }
}
