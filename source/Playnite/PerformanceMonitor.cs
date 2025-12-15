using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    /// <summary>
    /// Performance monitoring utility for tracking UI performance improvements.
    /// </summary>
    public static class PerformanceMonitor
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly Dictionary<string, List<long>> performanceMetrics = new Dictionary<string, List<long>>();

        /// <summary>
        /// Measures execution time of an operation and logs the result.
        /// </summary>
        /// <param name="operationName">Name of the operation being measured.</param>
        /// <param name="operation">The operation to measure.</param>
        /// <param name="logThresholdMs">Threshold in milliseconds above which the operation should be logged.</param>
        /// <returns>The execution time in milliseconds.</returns>
        public static long MeasureOperation(string operationName, Action operation, int logThresholdMs = 50)
        {
            var stopwatch = Stopwatch.StartNew();
            var startMemory = GC.GetTotalMemory(false);

            try
            {
                operation();
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                var endMemory = GC.GetTotalMemory(false);
                var memoryDelta = endMemory - startMemory;

                // Store metrics for analysis
                lock (performanceMetrics)
                {
                    if (!performanceMetrics.ContainsKey(operationName))
                    {
                        performanceMetrics[operationName] = new List<long>();
                    }
                    performanceMetrics[operationName].Add(elapsedMs);

                    // Keep only last 100 measurements per operation
                    if (performanceMetrics[operationName].Count > 100)
                    {
                        performanceMetrics[operationName].RemoveAt(0);
                    }
                }

                // Log slow operations
                if (elapsedMs > logThresholdMs)
                {
                    logger.Info($"[PERF] {operationName}: {elapsedMs}ms (Memory delta: {memoryDelta:N0} bytes)");
                }
            }

            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Measures execution time of an async operation and logs the result.
        /// </summary>
        /// <param name="operationName">Name of the operation being measured.</param>
        /// <param name="operation">The async operation to measure.</param>
        /// <param name="logThresholdMs">Threshold in milliseconds above which the operation should be logged.</param>
        /// <returns>The execution time in milliseconds.</returns>
        public static async Task<long> MeasureOperationAsync(string operationName, Func<Task> operation, int logThresholdMs = 50)
        {
            var stopwatch = Stopwatch.StartNew();
            var startMemory = GC.GetTotalMemory(false);

            try
            {
                await operation();
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                var endMemory = GC.GetTotalMemory(false);
                var memoryDelta = endMemory - startMemory;

                // Store metrics for analysis
                lock (performanceMetrics)
                {
                    if (!performanceMetrics.ContainsKey(operationName))
                    {
                        performanceMetrics[operationName] = new List<long>();
                    }
                    performanceMetrics[operationName].Add(elapsedMs);

                    // Keep only last 100 measurements per operation
                    if (performanceMetrics[operationName].Count > 100)
                    {
                        performanceMetrics[operationName].RemoveAt(0);
                    }
                }

                // Log slow operations
                if (elapsedMs > logThresholdMs)
                {
                    logger.Info($"[PERF] {operationName}: {elapsedMs}ms (Memory delta: {memoryDelta:N0} bytes)");
                }
            }

            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Gets performance statistics for a specific operation.
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns>Performance statistics or null if no data available.</returns>
        public static PerformanceStats GetStats(string operationName)
        {
            lock (performanceMetrics)
            {
                if (!performanceMetrics.TryGetValue(operationName, out var measurements))
                {
                    return null;
                }

                if (!measurements.Any())
                {
                    return new PerformanceStats
                    {
                        OperationName = operationName,
                        SampleCount = 0,
                        AverageMs = 0,
                        MinMs = 0,
                        MaxMs = 0,
                        MedianMs = 0
                    };
                }

                var sorted = measurements.OrderBy(x => x).ToList();
                return new PerformanceStats
                {
                    OperationName = operationName,
                    SampleCount = measurements.Count,
                    AverageMs = (long)measurements.Average(),
                    MinMs = measurements.Min(),
                    MaxMs = measurements.Max(),
                    MedianMs = sorted[sorted.Count / 2]
                };
            }
        }

        /// <summary>
        /// Gets performance statistics for all operations.
        /// </summary>
        /// <returns>Dictionary of operation names to their statistics.</returns>
        public static Dictionary<string, PerformanceStats> GetAllStats()
        {
            lock (performanceMetrics)
            {
                return performanceMetrics.Keys.ToDictionary(
                    key => key,
                    key => GetStats(key));
            }
        }

        /// <summary>
        /// Clears all performance metrics.
        /// </summary>
        public static void ClearMetrics()
        {
            lock (performanceMetrics)
            {
                performanceMetrics.Clear();
            }
        }

        /// <summary>
        /// Logs a performance summary for all tracked operations.
        /// </summary>
        public static void LogPerformanceSummary()
        {
            lock (performanceMetrics)
            {
                logger.Info("[PERF] Performance Summary:");
                foreach (var operationName in performanceMetrics.Keys)
                {
                    var stats = GetStats(operationName);
                    if (stats != null && stats.SampleCount > 0)
                    {
                        logger.Info($"[PERF] {operationName}: Avg={stats.AverageMs}ms, Min={stats.MinMs}ms, Max={stats.MaxMs}ms, Samples={stats.SampleCount}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Performance statistics for an operation.
    /// </summary>
    public class PerformanceStats
    {
        public string OperationName { get; set; }
        public int SampleCount { get; set; }
        public long AverageMs { get; set; }
        public long MinMs { get; set; }
        public long MaxMs { get; set; }
        public long MedianMs { get; set; }
    }
}