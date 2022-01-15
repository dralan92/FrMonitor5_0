using FrMonitor4_0.Models;
using System.Collections.Generic;

namespace FrMonitor4_0.Services
{
    public interface IBoxService
    {
        void AddPlacedTrade(PlacedTrade pt);
        void DeletePlacedTrade(PlacedTrade placedTrade);
        List<PlacedTrade> GetAllPlacedTrade();
        List<string> GetBoxRegistrations();
        List<PlacedTrade> GetPlacedTrade(string instrumentName, int timeframe);
        bool IsBoxDataInFile();

        HarmonicPatternDPoint GetHarmonicPatternDPoint(string name);
        void AddHarmonicPatternDPoint(HarmonicPatternDPoint hpdp);
        void DeleteHarmonicPatternDPoint(string name);
    }
}