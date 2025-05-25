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
        private int[,] board = new int[4, 4];
        private Label[,] labels = new Label[4, 4];
        private Random rand = new Random();
        private bool goalReached = false;
        private int score = 0;
        private int highScore = 0;
        private const string HighScoreFilePath = "highscore.json";
        private const string HistoryFilePath = "history.json";
        private bool gameStarted = false;

        public Form1()
        {
            InitializeComponent();
            InitializeBoard();
            LoadHighScore();
            AddRandomTile();
            AddRandomTile();
            UpdateUI();
            this.KeyDown += MainForm_KeyDown;
            this.KeyPreview = true;

        }
        private void InitializeBoard()
        {
            // 將 labels[,] 對應到 TableLayoutPanel 中的 Label 控制項
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.ColumnCount = 4;

            // 設定 TableLayoutPanel 的行和列的大小
            for (int i = 0; i < 4; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            }

            // 設定 Label 控制項的屬性
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    Label lbl = new Label();
                    lbl.Dock = DockStyle.Fill;
                    lbl.TextAlign = ContentAlignment.MiddleCenter; // 設定文字置中
                    lbl.Font = new Font("Microsoft JhengHei", 12, FontStyle.Bold);
                    lbl.BackColor = Color.LightGray;
                    lbl.Margin = new Padding(5);
                    tableLayoutPanel1.Controls.Add(lbl, col, row);
                    labels[row, col] = lbl;
                }
            }
        }
        private void AddRandomTile()
        {
            // 找到空格子隨機放 2 或 4
            var empty = new System.Collections.Generic.List<(int, int)>();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (board[i, j] == 0)
                        empty.Add((i, j));

            if (empty.Count == 0) return;

            var (x, y) = empty[rand.Next(empty.Count)];
            board[x, y] = rand.Next(10) == 0 ? 4 : 2;
        }
        private void UpdateUI()
        {
            // 把 board 裡的數字更新到 Label
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int val = board[i, j];
                    labels[i, j].Text = val == 0 ? "" : val.ToString();
                    labels[i, j].BackColor = GetTileColor(val);
                    labels[i, j].ForeColor = val <= 4 ? Color.Black : Color.White;
                }
            }
            //紀錄分數
            label4.Text = $"{score}";
            if (score > highScore)
            {
                highScore = score;
                label3.Text = $"{highScore}";
                SaveHighScore();
            }
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
        //上色
        private Color GetTileColor(int value)
        {
            return value switch
            {
                0 => Color.LightGray,
                2 => Color.Beige,
                4 => Color.BurlyWood,
                8 => Color.Orange,
                16 => Color.DarkOrange,
                32 => Color.OrangeRed,
                64 => Color.Red,
                128 => Color.YellowGreen,
                256 => Color.Green,
                512 => Color.Teal,
                1024 => Color.MediumBlue,
                2048 => Color.Gold,
                _ => Color.Black
            };
        }
        //按鍵輸入
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // 判斷按鍵並移動數字
            bool moved = false;
            switch (e.KeyCode)
            {
                case Keys.Left:
                    moved = MoveLeft();
                    break;
                case Keys.Right:
                    moved = MoveRight();
                    break;
                case Keys.Up:
                    moved = MoveUp();
                    break;
                case Keys.Down:
                    moved = MoveDown();
                    break;
            }


            if (moved)
            {
                AddRandomTile();
                UpdateUI();
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
        // 移動數字
        private bool MoveLeft()
        {
            bool moved = false;// 是否移動過
            // 合併數字
            for (int i = 0; i < 4; i++)
            {
                int[] row = new int[4];
                int index = 0;
                bool merged = false;
                // 將行的數字移到左邊
                for (int j = 0; j < 4; j++)
                {
                    if (board[i, j] != 0)
                    {
                        if (index > 0 && row[index - 1] == board[i, j] && !merged)
                        {
                            row[index - 1] *= 2;
                            score += row[index - 1];
                            merged = true;
                            moved = true;
                        }
                        else
                        {
                            row[index++] = board[i, j];
                            if (j != index - 1) moved = true;
                            merged = false;
                        }
                    }
                }

                for (int j = 0; j < 4; j++)
                    board[i, j] = row[j];
            }
            return moved;
        }
        private bool MoveRight()
        {
            ReverseRows();
            bool moved = MoveLeft();
            ReverseRows();
            return moved;
        }
        private bool MoveUp()
        {
            Transpose();
            bool moved = MoveLeft();
            Transpose();
            return moved;
        }
        private bool MoveDown()
        {
            Transpose();
            ReverseRows();
            bool moved = MoveLeft();
            ReverseRows();
            Transpose();
            return moved;
        }
        // 反轉行
        private void ReverseRows()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    (board[i, j], board[i, 3 - j]) = (board[i, 3 - j], board[i, j]);
                }
            }
        }
        // 轉置矩陣
        private void Transpose()
        {
            for (int i = 0; i < 4; i++)
                for (int j = i + 1; j < 4; j++)
                    (board[i, j], board[j, i]) = (board[j, i], board[i, j]);
        }
        //確認遊戲結束
        private bool CheckGameOver()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (board[i, j] == 0 || // 判斷是否有空格
                        (j < 3 && board[i, j] == board[i, j + 1]) || // 還可水平合併
                        (i < 3 && board[i, j] == board[i + 1, j])) // 還可垂直合併
                        return false;
            return true;
        }
        //達到2048，停止遊戲
        private bool Goal()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    if (board[i, j] == 2048)
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
                    board[i, j] = 0;

            AddRandomTile();
            AddRandomTile();
            UpdateUI();
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
                Score = score,
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
                Score = score,
                Board = board.Cast<int>().Select((v, i) => new { v, i })
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
