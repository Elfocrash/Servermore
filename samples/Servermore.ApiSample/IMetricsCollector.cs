using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Servermore.ApiSample
{
    public interface IMetricsCollector
    {
        void Increment(string metricName);

        int GetValue(string metricName);
    }

    public class MetricsCollector : IMetricsCollector
    {
        private readonly ConcurrentDictionary<string, int> _metrics = new ConcurrentDictionary<string, int>();

        public void Increment(string metricName)
        {
            if (!_metrics.ContainsKey(metricName))
            {
                _metrics.TryAdd(metricName, 1);
                return;
            }

            _metrics[metricName]++;
        }

        public int GetValue(string metricName)
        {
            return _metrics.GetValueOrDefault(metricName, 0);
        }
    }
}
