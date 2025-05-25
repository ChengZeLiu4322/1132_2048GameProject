using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1132_2048GameProject
{
    public class GameRecord
    {
        public int Score { get; set; }
        public List<List<int>> Board { get; set; }
        public DateTime Time { get; set; }
    }
}
