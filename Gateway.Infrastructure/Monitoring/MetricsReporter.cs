using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Monitoring
{
    public class MetricsReporter : IDisposable
    {
        private readonly ILogger<MetricsReporter> _logger;
        private readonly ConcurrentDictionary<string, long> _counters = new();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<double>> _timers = new();
        private readonly Timer _reportingTimer;
        private bool _disposed;

        public MetricsReporter(ILogger<MetricsReporter> logger)
        {
            _logger = logger;
            _reportingTimer = new Timer(ReportMetrics, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public void IncrementCounter(string name, long amount = 1)
        {
            _counters.AddOrUpdate(name, amount, (_, oldValue) => oldValue + amount);
        }

        public void RecordTime(string name, double milliseconds)
        {
            _timers.GetOrAdd(name, _ => new ConcurrentQueue<double>())
                .Enqueue(milliseconds);
        }

        private void ReportMetrics(object? state)
        {
            foreach (var counter in _counters)
            {
                _logger.LogInformation("Metric Counter: {Name} = {Value}", counter.Key, counter.Value);
            }

            foreach (var timer in _timers)
            {
                if (timer.Value.IsEmpty)
                    continue;

                double sum = 0;
                int count = 0;
                double min = double.MaxValue;
                double max = double.MinValue;

                foreach (var value in timer.Value)
                {
                    sum += value;
                    count++;
                    min = Math.Min(min, value);
                    max = Math.Max(max, value);
                }

                double avg = count > 0 ? sum / count : 0;

                _logger.LogInformation(
                    "Metric Timer: {Name} = Avg: {Avg}ms, Min: {Min}ms, Max: {Max}ms, Count: {Count}",
                    timer.Key, Math.Round(avg, 2), Math.Round(min, 2), Math.Round(max, 2), count);

                timer.Value.Clear();
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _reportingTimer?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
