using FrMonitor4_0.Models;
using System.Collections.Generic;

namespace FrMonitor4_0.Services
{
    public interface ICandleService
    {
        List<Candle> GetCandles(string instrumentName, int timeSlice, int count);
    }
}