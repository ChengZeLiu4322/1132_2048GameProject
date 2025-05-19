using System.Drawing.Text;
using static System.Formats.Asn1.AsnWriter;

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

        public Form1()
        {
            InitializeComponent();
            InitializeBoard();
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
            }
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
                    DialogResult result = MessageBox.Show("�S���i�H�X�����Ʀr�F�A�n���s�ӹL��", "�A��F", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        ResetGame();
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
                    this.Close(); // �Y��ܤ��~��A�����C��
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
    }
}
