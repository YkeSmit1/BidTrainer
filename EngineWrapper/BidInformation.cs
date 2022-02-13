using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineWrapper
{
    public class BidInformation
    {
        public bool HasInformation { get; set; }
        public Dictionary<string, int> minRecords;
        public Dictionary<string, int> maxRecords;
        public List<int> ids;
        public List<bool?> controls;
        public List<int> possibleKeyCards;
        public bool? trumpQueen;

        public BidInformation(List<Dictionary<string, string>> records)
        {
            HasInformation = records.Any();
            minRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Min")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Min());
            maxRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Max")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Max());
            ids = records.SelectMany(x => x).Where(x => x.Key == "Id").Select(x => Convert.ToInt32(x.Value)).ToList();
            controls = records.Any() ? new List<bool?> { GetHasProperty("SpadeControl"), GetHasProperty("HeartControl"), GetHasProperty("DiamondControl"), GetHasProperty("ClubControl") } :
                new List<bool?> { null, null, null, null };
            possibleKeyCards = records.SelectMany(x => x).Where(x => x.Key == "KeyCards" && !string.IsNullOrWhiteSpace(x.Value)).Select(x => int.Parse(x.Value)).ToList();
            trumpQueen = records.Any() ? GetHasProperty("TrumpQueen") : null;

            bool? GetHasProperty(string fieldName)
            {
                var recordsWithValue = records.SelectMany(x => x).Where(x => x.Key == fieldName && !string.IsNullOrWhiteSpace(x.Value));
                bool? p = recordsWithValue.Any() ? int.Parse(recordsWithValue.First().Value) == 1 : null;
                return p;
            }
        }

        public string GetInformation()
        {
            return !HasInformation ? "No information" :
                GetMinMaxAsText("Spades") + GetMinMaxAsText("Hearts") + GetMinMaxAsText("Diamonds") + GetMinMaxAsText("Clubs") + GetMinMaxAsText("Hcp") +
                $"{GetControlAsText()}" + $"{GetKeyCardsAsText()}" + $"{GetTrumpQueen()}";

            string GetMinMaxAsText(string suit)
            {
                return minRecords.ContainsKey($"Min{suit}") ? $"\n{suit}: {minRecords[$"Min{suit}"]} - {maxRecords[$"Max{suit}"]}" : "";
            }

            string GetControlAsText()
            {
                if (controls.All(x => x is null))
                    return "";
                var stringbuilder = new StringBuilder();
                stringbuilder.Append($"\nControls: ");
                foreach (var suit in Enum.GetValues<Suit>().Except(new[] { Suit.NoTrump }))
                    if (controls[3 - (int)suit].GetValueOrDefault())
                        stringbuilder.Append(suit);
                return stringbuilder.ToString();
            }

            string GetKeyCardsAsText()
            {
                if (!possibleKeyCards.Any())
                    return "";
                return $"\nKeyCards: {string.Join(",", possibleKeyCards)}";
            }

            string GetTrumpQueen()
            {
                return trumpQueen.HasValue ? $"\nTrumpQueen: {trumpQueen}" : "";
            }
        }
    }
}
