﻿using System;
using System.Threading;
using Metrics.MetricData;
using Metrics.Utils;

namespace Metrics.Reporters
{
    public sealed class ScheduledReporter : IDisposable
    {
        private readonly Scheduler scheduler;
        private readonly TimeSpan interval;

        private readonly Func<MetricsReport> reporter;
        private readonly MetricsDataProvider metricsDataProvider;
        private readonly Func<HealthStatus> healthStatus;

        public ScheduledReporter(Func<MetricsReport> reporter, MetricsDataProvider metricsDataProvider, Func<HealthStatus> healthStatus, TimeSpan interval)
            : this(reporter, metricsDataProvider, healthStatus, interval, new ActionScheduler()) { }

        public ScheduledReporter(Func<MetricsReport> reporter, MetricsDataProvider metricsDataProvider, Func<HealthStatus> healthStatus, TimeSpan interval, Scheduler scheduler)
        {
            this.reporter = reporter;
            this.metricsDataProvider = metricsDataProvider;
            this.healthStatus = healthStatus;
            this.interval = interval;
            this.scheduler = scheduler;
        }

        private void RunReport(CancellationToken token)
        {
            reporter().RunReport(this.metricsDataProvider.CurrentMetricsData, this.healthStatus, token);
        }

        public void Start()
        {
            this.scheduler.Start(this.interval, t => RunReport(t));
        }

        public void Stop()
        {
            this.scheduler.Stop();
        }

        public void Dispose()
        {
            this.Stop();
            this.scheduler.Dispose();
        }
    }
}
