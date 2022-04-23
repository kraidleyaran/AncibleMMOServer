using System;

namespace AncibleCoreServer.Services
{
    public class RNGService : WorldService
    {
        public static Random RANDOM => _instance._random;

        public override string Name => "Random Number Generator Service";
        

        private static RNGService _instance = null;

        private Random _random = null;

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                _random = new Random();
                base.Start();
            }
            
        }

        public static bool Roll(float chance = .5f)
        {
            var chanceToSucceed = chance > 1f ? 1f : chance;
            if (chanceToSucceed < 1f)
            {
                return (_instance._random.Next(0, 101) / 100f) <= chanceToSucceed;
            }
            else
            {
                return true;
            }
        }

        public static float GenerateRoll()
        {
            return _instance._random.Next(0, 101) / 100f;
        }

        public static int RollRange(int min, int max)
        {
            return _instance._random.Next(min, max);
        }

        public override void Stop()
        {
            _random = null;
            base.Stop();
        }
    }
}