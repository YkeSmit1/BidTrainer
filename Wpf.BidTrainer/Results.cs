using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf.BidTrainer
{
    public class Result
    {
        public TimeSpan TimeElapsed { get; set; }
        public bool UsedHint { get; set; } = false;
        public bool AnsweredCorrectly { get; set; } = true;

        public string GetReport()
        {
            return $"Time:{TimeElapsed:mm\\:ss}. UsedHint:{UsedHint}. Correct:{AnsweredCorrectly}";
        }
    }
    public class Results
    {
        class ResultsPerLesson
        {
            [JsonProperty]
            public readonly Dictionary<int, Result> results = new();
            public void AddResult(int board, Result result)
            {
                results[board] = result;
            }
            public string GetReport()
            {
                var sb = new StringBuilder();
                foreach (var result in results)
                {
                    sb.AppendLine($"*** Board {result.Key + 1} ***");
                    sb.AppendLine(result.Value.GetReport());
                    sb.AppendLine();
                }
                sb.AppendLine();
                sb.AppendLine("Results for lesson");
                sb.AppendLine();
                GetOverview(sb, results);
                sb.AppendLine();
                sb.AppendLine("****************************************");
                return sb.ToString();
            }
        }

        [JsonProperty]
        readonly Dictionary<int, ResultsPerLesson> allResults = new();
        public void AddResult(int lesson, int board, Result result)
        {
            if (!allResults.ContainsKey(lesson))
                allResults[lesson] = new();
            allResults[lesson].AddResult(board, result);
        }

        public string GetReport()
        {
            var sb = new StringBuilder();
            foreach (var resultPerLesson in allResults)
            {
                sb.AppendLine($"************ Lesson {resultPerLesson.Key} ************");
                sb.AppendLine();
                sb.AppendLine(resultPerLesson.Value.GetReport());
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine("****************************************");
            sb.AppendLine("****************************************");
            sb.AppendLine();
            sb.AppendLine("Results for all lessons");
            sb.AppendLine();
            var results = allResults.Values.SelectMany(x => x.results);
            GetOverview(sb, results);

            return sb.ToString();
        }

        private static void GetOverview(StringBuilder sb, IEnumerable<KeyValuePair<int, Result>> results)
        {
            sb.AppendLine($"Boards: {results.Count()} Time: {new TimeSpan(results.Sum(r => r.Value.TimeElapsed.Ticks)):mm\\:ss} " +
                $"Correct: {results.Count(x => x.Value.AnsweredCorrectly)} " +
                $"Incorrect: {results.Count(x => !x.Value.AnsweredCorrectly)} " +
                $"Hints used: {results.Count(x => x.Value.UsedHint)}");
        }
    }
}
