using System;

namespace Kinoheld.Base.Utils
{
    public interface IRandomGenerator
    {
        /// <summary>Generate a random number betwen [<paramref name="minValue"/>, <paramref name="maxValue"/>].</summary>
        /// <param name="minValue">The lower bound of the range that the random value should be within.</param>
        /// <param name="maxValue">The upper bound of the range that the random value should be within.</param>
        /// <returns>A random number that is in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>].</returns>
        /// <exception cref="ArgumentException">If <paramref name="maxValue"/> is greater or equal to <paramref name="minValue"/>.</exception>
        int Generate(int minValue, int maxValue);
    }
}