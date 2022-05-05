using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wpf.BidTrainer
{
    public class Result
    {
        public TimeSpan TimeElapsed { get; set; }
        public bool UsedHint { get; set; }
        public bool AnsweredCorrectly { get; set; } = true;
    }
    public class Results
    {
        public class ResultsPerLesson
        {
            [JsonProperty]
            public Dictionary<int, Result> Results { get; set; } = new();
            public void AddResult(int board, Result result)
            {
                Results[board] = result;
            }

            public string Title => GetOverview(Results);
        }

        [JsonProperty]
        public SortedDictionary<int, ResultsPerLesson> AllResults { get; set; } = new();
        public void AddResult(int lesson, int board, Result result)
        {
            if (!AllResults.ContainsKey(lesson))
                AllResults[lesson] = new ResultsPerLesson();
            AllResults[lesson].AddResult(board + 1, result);
        }
        public string Title => GetOverview(AllResults.SelectMany(x => x.Value.Results).ToList());

        private static string GetOverview(IReadOnlyCollection<KeyValuePair<int, Result>> results)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{results.Count(x => x.Value.AnsweredCorrectly)} out of {results.Count} are correct");
            sb.AppendLine($"Time spent: {new TimeSpan(results.Sum(r => r.Value.TimeElapsed.Ticks)):mm\\:ss} ");
            sb.AppendLine($"Hints used: {results.Count(x => x.Value.UsedHint)}");
            return sb.ToString();
        }

    }
}
