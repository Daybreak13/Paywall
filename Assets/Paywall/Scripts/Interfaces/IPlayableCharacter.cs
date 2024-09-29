using UnityEngine;

namespace Paywall.Interfaces
{

    public interface IPlayableCharacter
    {
        /// <summary>
        /// Current EX charge
        /// </summary>
        public float CurrentEX { get; }
        /// <summary>
        /// Minimum possible EX charge
        /// </summary>
        public float MinEX { get; }
        /// <summary>
        /// Maximum possible EX charge
        /// </summary>
        public float MaxEX { get; }
        /// Value of a single EX bar
        public float OneEXBarValue { get; }
        /// Is EX currently draining?
        public bool EXDraining { get; }
        /// EX drain per second acceleration rate. The longer EX drains for, the faster it drains.
        public float EXDrainRateAcceleration { get; }
        /// Block EX gain while EX bar is draining
        public bool BlockEXGainWhileDraining { get; }
        /// EX gained on kill
        public float KillEXGain { get; }
        /// EX lost when a life is lost
        public float EXLifeLost { get; }
        /// Gain EX passively over time?
        public bool GainEXOverTime { get; }
        /// EX gain rate per second
        public float EXGainPerSecond { get; }

        GameObject CharGameObject { get; }

        public interface IFactory
        {
            IPlayableCharacter Create();
        }
    }
}
