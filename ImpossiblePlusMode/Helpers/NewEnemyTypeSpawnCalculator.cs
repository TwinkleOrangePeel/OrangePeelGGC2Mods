using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpossiblePlusMode
{
    internal class NewEnemyTypeSpawnCalculator
    {
        private static readonly Random random = new Random();
        public static bool ShouldSpawnNewEnemyType()
        {
            // 25% 概率
            return random.NextDouble() < 0.25;
        }
    }
}
