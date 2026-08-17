using System;

namespace OrangePeelGGC2Mods.CharacterTraits
{
    public static class OneHitCalculator
    {
        private static readonly Random random = new Random();

        public static bool CheckOneHit()
        {
            // 5% 概率
            return random.NextDouble() < 0.05;
        }
    }
}
