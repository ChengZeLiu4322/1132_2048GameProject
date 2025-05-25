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
            // �N labels[,] ������ TableLayoutPanel ���� Label ���
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.ColumnCount = 4;

            // �]�w TableLayoutPanel ����M�C���j�p
            for (int i = 0; i < 4; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            }

            // �]�w Label ������ݩ�
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    Label lbl = new Label();
                    lbl.Dock = DockStyle.Fill;
                    lbl.TextAlign = ContentAlignment.MiddleCenter; // �]�w��r�m��
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
            // ���Ů�l�H���� 2 �� 4
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
            // �� board �̪��Ʀr��s�� Label
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
            //��������
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
        //�W��
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
        //�����J
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // �P�_����ò��ʼƦr
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
        // ���ʼƦr
        private bool MoveLeft()
        {
            bool moved = false;// �O�_���ʹL
            // �X�ּƦr
            for (int i = 0; i < 4; i++)
            {
                int[] row = new int[4];
                int index = 0;
                bool merged = false;
                // �N�檺�Ʀr���쥪��
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
        // �����
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
        // ��m�x�}
        private void Transpose()
        {
            for (int i = 0; i < 4; i++)
                for (int j = i + 1; j < 4; j++)
                    (board[i, j], board[j, i]) = (board[j, i], board[i, j]);
        }
        //�T�{�C������
        private bool CheckGameOver()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (board[i, j] == 0 || // �P�_�O�_���Ů�
                        (j < 3 && board[i, j] == board[i, j + 1]) || // �٥i�����X��
                        (i < 3 && board[i, j] == board[i + 1, j])) // �٥i�����X��
                        return false;
            return true;
        }
        //�F��2048�A����C��
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
        // ���]�C���޿�
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
        // �x�s�@������
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
