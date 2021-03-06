NxtCommunicatorはETロボコンの開発ツールです。  

■機能  
1. NXTから送信されるログデータの受信  
2. NXTへのデータ送信  
3. 受信したログのGrid表示  
4. 受信したログのChart表示  
5. ログのCSV Export、Import  
6. ログの再生によるコース画像へのプロット  

■Movie  
http://www.youtube.com/watch?v=OKdR9NgGhBQ&feature=youtu.be  

■Screenshot  
![screenshot](https://raw.github.com/teammaru/NxtCommunicator/master/readme-image/screen-shot.png)
  
■使い方  
1. ログの受信  
  ※Bluetoothでのログ受信方法は http://lejos-osek.sourceforge.net/jp/nxtgamepad.htm を参照  
  受信ポートを選択して「接続」クリックだけです。  
  その後、NXTからログデータが流れてくると自動でログを取集します。  
  注）大量のログを貯めると動作が遅くなります。適当にLogタブ右下の「Xxx行まで保持」を設定して保持行数を調整してください。  
  
  ecrobot_send_bt_packet() でのログ送信を前提としています。  
  以下の感じのデータ型、データ長を前提にしています。  
    
        void BtLogger::logSend(S8 data1, S8 data2, S16 adc1, S16 adc2, S16 adc3, S16 adc4)
        {
            U8 data_log_buffer[32];

            *((U32 *)(&data_log_buffer[0]))  = (U32)systick_get_ms();
            *(( S8 *)(&data_log_buffer[4]))  =  (S8)data1;
            *(( S8 *)(&data_log_buffer[5]))  =  (S8)data2;
            *((U16 *)(&data_log_buffer[6]))  = (U16)val of light sensor;
            *((S32 *)(&data_log_buffer[8]))  = (S32)nxt_motor_get_count(0);
            *((S32 *)(&data_log_buffer[12])) = (S32)nxt_motor_get_count(1);
            *((S32 *)(&data_log_buffer[16])) = (S32)nxt_motor_get_count(2);
            *((S16 *)(&data_log_buffer[20])) = (S16)adc1;
            *((S16 *)(&data_log_buffer[22])) = (S16)adc2;
            *((S16 *)(&data_log_buffer[24])) = (S16)adc3;
            *((S16 *)(&data_log_buffer[26])) = (S16)adc4;
            *((S32 *)(&data_log_buffer[28])) = (S32)val of distance;
            
            ecrobot_send_bt_packet(data_log_buffer, 32);
        }
  
  
2. データ送信  
  送信の箇所に値を入力して「送信」クリックだけです。  
  別途NXT側で受信処理が必要になります。  
    
  
3. Chart  
  Chart上をドラッグする事により拡大可能です。  
  
4. ログの再生とコースへのプロット  
  ・Settingタブの車軸とタイヤ半径をご自分のNXTに合わせて設定してください。  
      この数値がおかしいと上手にプロットされません。  
      私のNXTだと車軸：16.8、タイヤ半径：4.17が最適値でした。  
    
  ・コース上のオレンジの四角がスタート位置となります。  
      ドラッグして位置を決定してください。  
    
  ・NXTの向き  
      コース上のオレンジの四角にカーソルを合わせると上下左右に矢印が表示されるので、クリックしてスタート時のNXT方向と設定します。  
      現状は上下左右の４方向だけです。  
    
  ・「軌跡の縮小率」の値で、走行位置を画面上のコースに縮尺を合わせてプロットされます。  
      現状のプログラムだと1.2位がちょうど良いです。  
  
■動作環境  
  .NetFramework4 が必要です。  
  Windows7でのみ動作確認しています。  
  Visual C# 2010 Express で作成。  
  
■既知の問題  
・「ログ再生スピード」を大きくしてログ再生すると、再生中はボタンなどが無反応になります。  
・ログ再生時にChartにガイド線を表示しているが、ちゃんと追従しない。  
  
■Version  
v1.0 初回  
v1.1 Gridの表示件数の制御対応  
v2.0 走行位置のプロットに対応  


