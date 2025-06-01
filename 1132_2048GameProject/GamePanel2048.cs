using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace _1132_2048GameProject
{
    internal class GamePanel2048
    {
        public int score { get; private set; }
        public int[,] Board { get; private set; }
        public Label[,] Labels { get; private set; }
        private Random rand = new Random();
        private TableLayoutPanel panel;
        public int Score => score;

       

        public GamePanel2048(TableLayoutPanel table)
        {
            panel = table;
            Board = new int[4, 4];
            Labels = new Label[4, 4];
            InitializeBoard();
        }

        public void InitializeBoard()
        {
            panel.Controls.Clear();
            panel.RowCount = 4;
            panel.ColumnCount = 4;
            panel.RowStyles.Clear();
            panel.ColumnStyles.Clear();

            for (int i = 0; i < 4; i++)
            {
                panel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            }

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    Label lbl = new Label
                    {
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Microsoft JhengHei", 12, FontStyle.Bold),
                        BackColor = Color.LightGray,
                        Margin = new Padding(5)
                    };
                    panel.Controls.Add(lbl, col, row);
                    Labels[row, col] = lbl;
                    Board[row, col] = 0;
                }
            }
        }

        public void AddRandomTile()
        {
            var empty = new System.Collections.Generic.List<(int, int)>();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (Board[i, j] == 0)
                        empty.Add((i, j));

            if (empty.Count == 0) return;

            var (x, y) = empty[rand.Next(empty.Count)];
            Board[x, y] = rand.Next(10) == 0 ? 4 : 2;
        }

        public void UpdateUI()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    int val = Board[i, j];
                    Labels[i, j].Text = val == 0 ? "" : val.ToString();
                    Labels[i, j].BackColor = GetTileColor(val);
                    Labels[i, j].ForeColor = val <= 4 ? Color.Black : Color.White;
                }

        }

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
        public bool MoveLeft()
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
                    if (Board[i, j] != 0)
                    {
                        if (index > 0 && row[index - 1] == Board[i, j] && !merged)
                        {
                            row[index - 1] *= 2;
                            score += row[index - 1];
                            merged = true;
                            moved = true;
                        }
                        else
                        {
                            row[index++] = Board[i, j];
                            if (j != index - 1) moved = true;
                            merged = false;
                        }
                    }
                }

                for (int j = 0; j < 4; j++)
                    Board[i, j] = row[j];
            }
            return moved;
        }
        public bool MoveRight()
        {
            ReverseRows();
            bool moved = MoveLeft();
            ReverseRows();
            return moved;
        }
        public bool MoveUp()
        {
            Transpose();
            bool moved = MoveLeft();
            Transpose();
            return moved;
        }
        public bool MoveDown()
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
                    (Board[i, j], Board[i, 3 - j]) = (Board[i, 3 - j], Board[i, j]);
                }
            }
        }
        // 轉置矩陣
        private void Transpose()
        {
            for (int i = 0; i < 4; i++)
                for (int j = i + 1; j < 4; j++)
                    (Board[i, j], Board[j, i]) = (Board[j, i], Board[i, j]);
        }
    }
}
