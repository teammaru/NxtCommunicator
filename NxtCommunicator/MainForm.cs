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

namespace NxtCommunicator
{
    /// <summary>
    /// ログ受信時のハンドラ用
    /// </summary>
    /// <param name="logData"></param>
    public delegate void LogReceiveDelegate(LogData logData);

    /// <summary>
    /// 通信先開始時向け
    /// </summary>
    public delegate void ConnectDelegate();

    /// <summary>
    /// メイン操作、表示フォーム
    /// </summary>
    public partial class MainForm : Form
    {
        private const int SEND_TEXTBOX_COUNT = 7;
        private const int CHART_COUNT = 3;

        private const string PROP_KEY_PORT = "port";
        private string[] propKeyNameComboTarget = { "comboTargetChart0", "comboTargetChart1", "comboTargetChart2" };
        private string[] propKeyNameSendVal = { "textBoxVal1", "textBoxVal2", "textBoxVal3",
                                                "textBoxVal4", "textBoxVal5", "textBoxVal6",
                                                "textBoxVal7" };

        private delegate void DelegateDrawChart(Chart targetChart, int targetColPos);

        private LogReceiveDelegate logReceiveDelegate;
        private DataTransceiver dataTransceiver;

        // ログデータ
        private List<DataGridViewRow> logBufferList = new List<DataGridViewRow>();

        // ストップウォッチ（経過時間計測）
        private Stopwatch stopwatch = new Stopwatch();
        private Stopwatch stopwatch4Grid = new Stopwatch();

        // コントロール部品の配列
        private TextBox[] textBoxSendVals = new TextBox[SEND_TEXTBOX_COUNT];
        private Button[] buttonSendVals = new Button[SEND_TEXTBOX_COUNT];
        private Chart[] charts = new Chart[CHART_COUNT];
        private ComboBox[] comboBoxTargetCharts = new ComboBox[CHART_COUNT];

        public MainForm()
        {
            InitializeComponent();

            logReceiveDelegate = new LogReceiveDelegate(receiveLog);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            init();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            terminate();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void init()
        {
            // コントロール部品を配列化
            textBoxSendVals[0] = textBoxVal1;
            textBoxSendVals[1] = textBoxVal2;
            textBoxSendVals[2] = textBoxVal3;
            textBoxSendVals[3] = textBoxVal4;
            textBoxSendVals[4] = textBoxVal5;
            textBoxSendVals[5] = textBoxVal6;
            textBoxSendVals[6] = textBoxVal7;

            buttonSendVals[0] = val1SendButton;
            buttonSendVals[1] = val2SendButton;
            buttonSendVals[2] = val3SendButton;
            buttonSendVals[3] = val4SendButton;
            buttonSendVals[4] = val5SendButton;
            buttonSendVals[5] = val6SendButton;
            buttonSendVals[6] = val7SendButton;

            charts[0] = chart0;
            charts[1] = chart1;
            charts[2] = chart2;

            comboBoxTargetCharts[0] = comboBoxChartingTarget0;
            comboBoxTargetCharts[1] = comboBoxChartingTarget1;
            comboBoxTargetCharts[2] = comboBoxChartingTarget2;

            // 送信値の初期値とイベント設定
            for (int ii = 0; ii < SEND_TEXTBOX_COUNT; ii++)
            {
                textBoxSendVals[ii].Text = CommonUtil.nvl(PropertyUtil.getProp(propKeyNameSendVal[ii]), "");
                buttonSendVals[ii].Click += new EventHandler(sendButton_Click);
            }

            // 表示チャートComboの初期値とイベント設定
            for (int ii = 0; ii < CHART_COUNT; ii++)
            {
                comboBoxTargetCharts[ii].SelectedIndex
                    = int.Parse(CommonUtil.nvl(PropertyUtil.getProp(propKeyNameComboTarget[ii]), "0"));
                comboBoxTargetCharts[ii].SelectedIndexChanged += new EventHandler(comboBoxChartingTarget_SelectedIndexChanged);
            }

            // 選択可能なポートをSelBoxへ
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBoxPort.Items.Add(port);
            }

            // 選択ポートが記憶されていない場合は、先頭を選択
            comboBoxPort.SelectedIndex = int.Parse(CommonUtil.nvl(PropertyUtil.getProp(PROP_KEY_PORT), "0"));

            dataTransceiver = new DataTransceiver(logReceiveDelegate);
        }

        /// <summary>
        /// 接続・切断ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (dataTransceiver.IsOpen == true)
            {
                try
                {
                    ConnectDelegate disconnectDelegate = new ConnectDelegate(disconnectSerial);
                    this.BeginInvoke(disconnectDelegate);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                return;
            }

            try
            {
                ConnectDelegate connectDelegate = new ConnectDelegate(connectSerial);
                this.BeginInvoke(connectDelegate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// シリアル切断
        /// </summary>
        private void disconnectSerial()
        {
            if (!dataTransceiver.disconnect())
            {
                MessageBox.Show("切断できませんでした");
                return;
            }
            this.serialDisabled();
        }

        /// <summary>
        /// シリアル接続
        /// </summary>
        private void connectSerial()
        {
            if (!this.connect())
            {
                MessageBox.Show("接続できませんでした");
                return;
            }
            this.serialEnabled();
            stopwatch4Grid.Start();
        }

        /// <summary>
        /// シリアル接続後のUI制御
        /// </summary>
        private void serialEnabled()
        {
            buttonConnect.Text = "切断";
            val1SendButton.Enabled = true;
            val2SendButton.Enabled = true;
            val3SendButton.Enabled = true;
            val4SendButton.Enabled = true;
            val5SendButton.Enabled = true;
            val6SendButton.Enabled = true;
            val7SendButton.Enabled = true;
        }

        /// <summary>
        /// シリアル切断後のUI制御
        /// </summary>
        private void serialDisabled()
        {
            buttonConnect.Text = "接続";
            val1SendButton.Enabled = false;
            val2SendButton.Enabled = false;
            val3SendButton.Enabled = false;
            val4SendButton.Enabled = false;
            val5SendButton.Enabled = false;
            val6SendButton.Enabled = false;
            val7SendButton.Enabled = false;
        }

        /// <summary>
        /// シリアル接続
        /// </summary>
        private bool connect()
        {
            PropertyUtil.saveProp(PROP_KEY_PORT, comboBoxPort.SelectedIndex.ToString());
            return dataTransceiver.connect(comboBoxPort.SelectedItem.ToString());
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        private void terminate()
        {
            dataTransceiver.disconnect();
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
            if (textBoxMaxLine.Text != "")
            {
                int maxLine = int.Parse(textBoxMaxLine.Text);
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

        /// <summary>
        /// 送信ボタンクリック.
        /// NXTへテキストを送信する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendButton_Click(object sender, EventArgs e)
        {
            for (int ii = 0; ii < SEND_TEXTBOX_COUNT; ii++)
            {
                if(sender == buttonSendVals[ii])
                {
                    dataTransceiver.Write(textBoxSendVals[ii].Text);
                    PropertyUtil.saveProp(propKeyNameSendVal[ii], textBoxSendVals[ii].Text);
                    return;
                }
            }
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
            if(dataGridView.Rows[e.RowIndex].Cells[0].Value == null)
            {
                return;
            }
            if (dataGridView.Rows[e.RowIndex].Cells[0].Value.ToString() == "")
            {
                return;
            }

            // ガイド線をクリアしてから表示
            for (int ii = 0; ii < CHART_COUNT; ii++)
            {
                clearStripLine(charts[ii]);
            }
            long xVal = long.Parse(dataGridView.Rows[e.RowIndex].Cells[0].Value.ToString());
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
            targetChart.ChartAreas[0].AxisX.StripLines.Add(new StripLine());
            targetChart.ChartAreas[0].AxisX.StripLines[0].BackColor = Color.Red;
            targetChart.ChartAreas[0].AxisX.StripLines[0].StripWidth = 1;
            targetChart.ChartAreas[0].AxisX.StripLines[0].Interval = 999999999;
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
