using System;

namespace Lattirune.Core
{
    /// <summary>
    /// Decoupled interface for deterministic pseudorandom number generation.
    /// Enables seed injection and reproducible testing without UnityEngine.Random coupling.
    /// </summary>
    public interface IRandomSource
    {
        int Next();
        int Next(int min, int max);
        double NextDouble();
    }

    /// <summary>
    /// Deterministic System.Random wrapper implementing IRandomSource.
    /// </summary>
    public class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;

        public SystemRandomSource()
        {
            _random = new Random();
        }

        public SystemRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        public int Next()
        {
            return _random.Next();
        }

        public int Next(int min, int max)
        {
            return _random.Next(min, max);
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }
    }
}
