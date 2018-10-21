using System;
using System.Security.Cryptography;

namespace Kinoheld.Base.Utils
{
    // based from https://github.com/saidout/saidout-security-securerandom
    public class RandomGenerator : IRandomGenerator
    {
        public int Generate(int minValue, int maxValue)
        {
            if (maxValue <= minValue)
            {
                throw new ArgumentException("The lower bound has to be smaller than the upper bound", nameof(maxValue));
            }

            var elemInRange = (long)maxValue - minValue + 1;
            var randomData = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomData);
                var randomInt = BitConverter.ToUInt32(randomData, 0);
                var mod = randomInt % elemInRange;
                return (int)(minValue + mod);
            }
        }
    }
}