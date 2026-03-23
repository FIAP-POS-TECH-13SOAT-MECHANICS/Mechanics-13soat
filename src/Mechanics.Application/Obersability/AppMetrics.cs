using System.Diagnostics.Metrics;

namespace Mechanics.Application.Observability;

public class AppMetrics
{
    public static readonly Meter Meter = new("Mechanics.Api");
    public static readonly Counter<long> WorkOrdersCreated =
        Meter.CreateCounter<long>("work_orders.created");

    public static readonly Counter<double> TimeInStatusTotalSeconds =
        Meter.CreateCounter<double>("work_orders.time_in_status.total_seconds", unit: "s");

    public static readonly Counter<long> TimeInStatusSamples =
        Meter.CreateCounter<long>("work_orders.time_in_status.samples");
}
