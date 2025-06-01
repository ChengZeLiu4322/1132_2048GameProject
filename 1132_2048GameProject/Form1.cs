using System.Drawing.Text;
using static System.Formats.Asn1.AsnWriter;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace _1132_2048GameProject
{
    public partial class Form1 : Form
    {
        private bool goalReached = false;
        private int score = 0;
        private int highScore = 0;
        private const string HighScoreFilePath = "highscore.json";
        private const string HistoryFilePath = "history.json";
        private bool gameStarted = false;
        private GamePanel2048 gamePanel;
        public Form1()
        {
            InitializeComponent();
            gamePanel = new GamePanel2048(tableLayoutPanel1);
            gamePanel.AddRandomTile();
            gamePanel.AddRandomTile();
            gamePanel.UpdateUI();
            label4.Text = $"{gamePanel.Score}";

            if (gamePanel.Score > highScore)
            {
                highScore = gamePanel.Score;
                label3.Text = $"{highScore}";
                SaveHighScore();
            }
            gamePanel.InitializeBoard();
            LoadHighScore();
            this.KeyDown += MainForm_KeyDown;
            this.KeyPreview = true;

        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!gameStarted) return base.ProcessCmdKey(ref msg, keyData);// 若未開始，忽略方向鍵
            // 模擬呼叫 KeyDown 方法
            if (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down)
            {
                MainForm_KeyDown(this, new KeyEventArgs(keyData));
                return true; // 已處理，防止 base 再處理一次
            }

            // 繼續處理預設事件
            return base.ProcessCmdKey(ref msg, keyData);
        }
        //按鍵輸入
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // 判斷按鍵並移動數字
            bool moved = false;
            switch (e.KeyCode)
            {
                case Keys.Left:
                    moved = gamePanel.MoveLeft();
                    break;
                case Keys.Right:
                    moved = gamePanel.MoveRight();
                    break;
                case Keys.Up:
                    moved = gamePanel.MoveUp();
                    break;
                case Keys.Down:
                    moved = gamePanel.MoveDown();
                    break;
            }


            if (moved)
            {
                gamePanel.AddRandomTile();
                gamePanel.UpdateUI();
                label4.Text = $"{gamePanel.Score}";
                if (gamePanel.Score > highScore)
                {
                    highScore = gamePanel.Score;
                    label3.Text = $"{highScore}";
                    SaveHighScore();
                }
                if (CheckGameOver())
                {
                    SaveHistory(); // 儲存遊戲紀錄
                    DialogResult result = MessageBox.Show("沒有可以合成的數字了，要重新來過嗎", "你輸了", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        ResetGame();
                    }
                    if (result == DialogResult.No)
                    {
                        gameStarted = false; // 停止遊戲
                        button2.Enabled = true; // 啟用開始按鈕
                        button1.Enabled = true; // 啟用歷史紀錄按鈕
                    }
                }
            }

            // 判斷是否達成 2048 且尚未提示過
            if (!goalReached && Goal())
            {
                goalReached = true; // 記得已經達標，避免再次提示
                DialogResult result = MessageBox.Show("你達到 2048！是否繼續遊戲？", "恭喜！", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    button2.Enabled = true;
                    button1.Enabled = true;
                    return;
                }
            }
        }
        //確認遊戲結束
        private bool CheckGameOver()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (gamePanel.Board[i, j] == 0 || // 判斷是否有空格
                        (j < 3 && gamePanel.Board[i, j] == gamePanel.Board[i, j + 1]) || // 還可水平合併
                        (i < 3 && gamePanel.Board[i, j] == gamePanel.Board[i + 1, j])) // 還可垂直合併
                        return false;
            return true;
        }
        //達到2048，停止遊戲
        private bool Goal()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    if (gamePanel.Board[i, j] == 2048)
                    {
                        return true;
                    }
                }
            return false;
        }
        // 重設遊戲邏輯
        private void ResetGame()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    gamePanel.Board[i, j] = 0;

            gamePanel.AddRandomTile();
            gamePanel.AddRandomTile();
            gamePanel.UpdateUI();
            score = 0;
            label4.Text = $"{score}";
            goalReached = false;
        }
        // 儲存一筆紀錄
        private void SaveHistory()
        {
            List<GameRecord> records = LoadHistory();
            var boardList = new List<List<int>>();
            records.Add(new GameRecord
            {
                Score = gamePanel.Score,
                Board = boardList,
                Time = DateTime.Now
            });

            if (records.Count > 10)
                records = records.Skip(records.Count - 9).ToList();

            string json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(HistoryFilePath, json);
        }
        // 載入所有紀錄
        private List<GameRecord> LoadHistory()
        {
            if (!File.Exists(HistoryFilePath))
                return new List<GameRecord>();

            string json = File.ReadAllText(HistoryFilePath);
            return JsonSerializer.Deserialize<List<GameRecord>>(json) ?? new List<GameRecord>();
        }
        private void SaveHighScore()
        {
            GameRecord best = new GameRecord
            {
                Score = gamePanel.Score,
                Board = gamePanel.Board.Cast<int>().Select((v, i) => new { v, i })
                      .GroupBy(x => x.i / 4)
                      .Select(g => g.Select(x => x.v).ToList())
                      .ToList(),
                Time = DateTime.Now
            };

            string bestJson = JsonSerializer.Serialize(best, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(HighScoreFilePath, bestJson);
        }
        private void LoadHighScore()
        {
            if (File.Exists(HighScoreFilePath))
            {
                try
                {
                    string json = File.ReadAllText(HighScoreFilePath);
                    GameRecord best = JsonSerializer.Deserialize<GameRecord>(json);
                    if (best != null)
                    {
                        highScore = best.Score;
                        label3.Text = $"{highScore}";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("讀取高分紀錄失敗：" + ex.Message);
                }
            }
        }
        private void ShowHistory()
        {
            var records = LoadHistory();
            string message = string.Join("\n", records.Select(r =>
                $"時間：{r.Time:G}，分數：{r.Score}"));

            MessageBox.Show(message, "歷史紀錄");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShowHistory();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ResetGame();          // 重設遊戲
            gameStarted = true;   // 啟用鍵盤控制
            button2.Enabled = false; // 禁用開始按鈕，避免重複啟動
            button1.Enabled = false;
        }
    }
}
