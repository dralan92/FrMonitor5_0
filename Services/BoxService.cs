using FrMonitor4_0.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrMonitor4_0.Services
{
    public class BoxService : IBoxService
    {
        string jsonFile = @"alanDb.json";

        public bool IsBoxDataInFile()
        {

            try
            {
                var json = File.ReadAllText(jsonFile);
                var jsonObj = JObject.Parse(json);
                var instrumentsBoxes = jsonObj.GetValue("instruments") as JArray;
                if (0 < instrumentsBoxes.Count())
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;

        }

        public List<string> GetBoxRegistrations()
        {
            var registrations = new List<string>();
            try
            {

                var json = File.ReadAllText(jsonFile);
                var jsonObj = JObject.Parse(json);
                var instrumentsBoxes = jsonObj.GetValue("instruments") as JArray;
                if (0 < instrumentsBoxes.Count())
                {
                    registrations = instrumentsBoxes.Select(i => (string)i["name"]).ToList();
                }
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
            return registrations;

        }



        public void AddPlacedTrade(PlacedTrade pt)
        {
            try
            {
                var json = File.ReadAllText(jsonFile);
                var jsonObj = JObject.Parse(json);
                var instrumentsBoxes = jsonObj.GetValue("placedTrades") as JArray;

                var newHpdp = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(pt));
                instrumentsBoxes.Add(newHpdp);
                jsonObj["placedTrades"] = instrumentsBoxes;

                string newResut = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(jsonFile, newResut);

            }
            catch
            {

            }
        }

        public List<PlacedTrade> GetPlacedTrade(string instrumentName, int timeframe)
        {
            var json = File.ReadAllText(jsonFile);
            var jsonObj = JObject.Parse(json);
            var pts = jsonObj.GetValue("placedTrades") as JArray;
            var placedTradeList = new List<PlacedTrade>();

            foreach (var pt in pts)
            {
                var iName = pt["InstrumentName"].ToString();
                var sno = sbyte.Parse(pt["StrategyNo"].ToString());
                var tf = int.Parse(pt["Timeframe"].ToString());
                var tct = pt["TradeCandleTime"].ToString();
                var tid = pt["TradeId"].ToString();
                var a = pt["Action"].ToString();

                placedTradeList.Add(new PlacedTrade
                {
                    InstrumentName = iName,
                    StrategyNo = sno,
                    Timeframe = tf,
                    TradeCandleTime = tct,
                    TradeId = tid,
                    Action = a
                });
            }

            var ptl = placedTradeList.Where(t => t.InstrumentName == instrumentName)
                                        .Where(t => t.Timeframe == timeframe).ToList();


            return ptl;
        }

        public List<PlacedTrade> GetAllPlacedTrade()
        {
            var json = File.ReadAllText(jsonFile);
            var jsonObj = JObject.Parse(json);
            var pts = jsonObj.GetValue("placedTrades") as JArray;
            var placedTradeList = new List<PlacedTrade>();

            foreach (var pt in pts)
            {
                var iName = pt["InstrumentName"].ToString();
                var sno = sbyte.Parse(pt["StrategyNo"].ToString());
                var tf = int.Parse(pt["Timeframe"].ToString());
                var tct = pt["TradeCandleTime"].ToString();
                var tid = pt["TradeId"].ToString();
                var a = pt["Action"].ToString();

                placedTradeList.Add(new PlacedTrade
                {
                    InstrumentName = iName,
                    StrategyNo = sno,
                    Timeframe = tf,
                    TradeCandleTime = tct,
                    TradeId = tid,
                    Action = a
                });
            }

            return placedTradeList;
        }


        public void DeletePlacedTrade(PlacedTrade placedTrade)
        {
            try
            {
                var json = File.ReadAllText(jsonFile);
                var jsonObj = JObject.Parse(json);
                var instrumentsBoxes = jsonObj.GetValue("placedTrades") as JArray;

                var instBox = instrumentsBoxes
                                   .Where(i => (string)i["TradeId"] == placedTrade.TradeId)
                                   .FirstOrDefault();

                instrumentsBoxes.Remove(instBox);

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(jsonFile, output);
            }
            catch (Exception ex)
            {
                File.AppendAllText("AlanLog/Exception_PlacedTrade", "Could not delete closed trade from DB \n");
            }
        }


        public HarmonicPatternDPoint GetHarmonicPatternDPoint(string name)
        {
            try
            {

                var json = File.ReadAllText(jsonFile);
                var jsonObj = JObject.Parse(json);
                var instrumentsBoxes = jsonObj.GetValue("harmonicPatternDPoints") as JArray;

                var instBox = instrumentsBoxes
                                .Where(i => (string)i["Name"] == name)
                                .FirstOrDefault()
                                .ToObject<HarmonicPatternDPoint>();
                return instBox;

            }
            catch (Exception ex)
            {
                return new HarmonicPatternDPoint();
            }
        }

        public void AddHarmonicPatternDPoint(HarmonicPatternDPoint hpdp)
        {
            try
            {
                File.AppendAllText(@"AlanLog\Harmonic.txt", "Inside AddHarmonicPatternDPoint() writing pattern for " + hpdp.Name + Environment.NewLine);

               var json = File.ReadAllText(jsonFile);
                var jsonObj = JObject.Parse(json);
                var instrumentsBoxes = jsonObj.GetValue("harmonicPatternDPoints") as JArray;

                var newHpdp = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(hpdp));
                instrumentsBoxes.Add(newHpdp);
                jsonObj["harmonicPatternDPoints"] = instrumentsBoxes;

                string newResut = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(jsonFile, newResut);

            }
            catch
            {
                File.AppendAllText(@"AlanLog\HarmonicException.txt", "Cannot Write Pattern to File\n");

            }
        }

        public void DeleteHarmonicPatternDPoint(string name)
        {
            try
            {
                var json = File.ReadAllText(jsonFile);
                var jsonObj = JObject.Parse(json);
                var instrumentsBoxes = jsonObj.GetValue("harmonicPatternDPoints") as JArray;

                var instBox = instrumentsBoxes
                                   .Where(i => (string)i["Name"] == name)
                                   .FirstOrDefault();

                instrumentsBoxes.Remove(instBox);

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(jsonFile, output);
            }
            catch (Exception ex)
            {

            }
        }




    }
}
