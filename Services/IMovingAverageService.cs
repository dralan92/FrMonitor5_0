using System.Collections.Generic;

namespace FrMonitor4_0.Services
{
    public interface IMovingAverageService
    {
        double Average { get; }
        bool HasFullPeriod { get; }
        int N { get; }
        IEnumerable<double> Observations { get; }
        double StandardDeviation { get; }
        double Variance { get; }

        void AddObservation(double observation);
        void MovingAverageCalculator(int period);
    }
}