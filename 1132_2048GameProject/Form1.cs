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
            if (!gameStarted) return base.ProcessCmdKey(ref msg, keyData);// �Y���}�l�A������V��
            // �����I�s KeyDown ��k
            if (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down)
            {
                MainForm_KeyDown(this, new KeyEventArgs(keyData));
                return true; // �w�B�z�A���� base �A�B�z�@��
            }

            // �~��B�z�w�]�ƥ�
            return base.ProcessCmdKey(ref msg, keyData);
        }
        //�����J
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // �P�_����ò��ʼƦr
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
                    SaveHistory(); // �x�s�C������
                    DialogResult result = MessageBox.Show("�S���i�H�X�����Ʀr�F�A�n���s�ӹL��", "�A��F", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        ResetGame();
                    }
                    if (result == DialogResult.No)
                    {
                        gameStarted = false; // ����C��
                        button2.Enabled = true; // �ҥζ}�l���s
                        button1.Enabled = true; // �ҥξ��v�������s
                    }
                }
            }

            // �P�_�O�_�F�� 2048 �B�|�����ܹL
            if (!goalReached && Goal())
            {
                goalReached = true; // �O�o�w�g�F�СA�קK�A������
                DialogResult result = MessageBox.Show("�A�F�� 2048�I�O�_�~��C���H", "���ߡI", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    button2.Enabled = true;
                    button1.Enabled = true;
                    return;
                }
            }
        }
        //�T�{�C������
        private bool CheckGameOver()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (gamePanel.Board[i, j] == 0 || // �P�_�O�_���Ů�
                        (j < 3 && gamePanel.Board[i, j] == gamePanel.Board[i, j + 1]) || // �٥i�����X��
                        (i < 3 && gamePanel.Board[i, j] == gamePanel.Board[i + 1, j])) // �٥i�����X��
                        return false;
            return true;
        }
        //�F��2048�A����C��
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
        // ���]�C���޿�
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
        // �x�s�@������
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
        // ���J�Ҧ�����
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
                    MessageBox.Show("Ū�������������ѡG" + ex.Message);
                }
            }
        }
        private void ShowHistory()
        {
            var records = LoadHistory();
            string message = string.Join("\n", records.Select(r =>
                $"�ɶ��G{r.Time:G}�A���ơG{r.Score}"));

            MessageBox.Show(message, "���v����");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShowHistory();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ResetGame();          // ���]�C��
            gameStarted = true;   // �ҥ���L����
            button2.Enabled = false; // �T�ζ}�l���s�A�קK���ƱҰ�
            button1.Enabled = false;
        }
    }
}
