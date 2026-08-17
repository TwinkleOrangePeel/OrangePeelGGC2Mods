using System;

namespace OrangePeelGGC2Mods.CharacterTraits
{
    public static class CriticalHitCalculator
    {
        private static readonly Random random = new Random();

        public static bool CheckCriticalHit()
        {
            // 25% 概率
            return random.NextDouble() < 0.25;
        }
    }
}
