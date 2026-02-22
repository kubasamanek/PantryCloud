using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace PantryCloud.SharedKernel.Logging;

/// <summary>
/// Serilog enricher that adds OpenTelemetry TraceId and SpanId from the current Activity.
/// Enables Loki → Tempo linking when log lines contain TraceId.
/// </summary>
public sealed class ActivityEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity is null)
            return;

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("TraceId", activity.TraceId.ToHexString()));
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("SpanId", activity.SpanId.ToHexString()));
    }
}
