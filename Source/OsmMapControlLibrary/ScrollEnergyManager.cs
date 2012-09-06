using System;

namespace OsmMapControlLibrary
{
    public class ScrollEnergyManager
    {
        private const double maxEnergy = 2;
        private const double rechargeRate = 0.03;
        private const double requestRate = 0.2;
        public double CurrentEnergy
        {
            get;
            set;
        }
        /// <summary>
        /// Requests the energy. Not the complete request will be returned
        /// </summary>
        /// <param name="requiredEnergy"></param>
        /// <returns></returns>
        public double RequestEnergy(double requiredEnergy)
        {
            if (requiredEnergy < 0)
            {
                return -this.RequestEnergy(-requiredEnergy);
            }
            var available = this.CurrentEnergy * requestRate;
            if (available < requiredEnergy)
            {
                requiredEnergy = available;
            }
            this.CurrentEnergy -= available;
            return available;
        }
        /// <summary>
        /// Recharges the energy (done during every frame)
        /// </summary>
        public void Recharge()
        {
            var diff = maxEnergy - this.CurrentEnergy;
            diff *= rechargeRate;
            this.CurrentEnergy += diff;
        }
        public override string ToString()
        {
            return string.Format("{0:n2} of {1}", this.CurrentEnergy, maxEnergy);
        }
    }
}

