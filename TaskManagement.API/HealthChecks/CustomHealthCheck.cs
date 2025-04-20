using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace TaskManagement.API.HealthChecks;

public class CustomHealthCheck : IHealthCheck
{
    private readonly ILogger<CustomHealthCheck> _logger;

    public CustomHealthCheck(ILogger<CustomHealthCheck> logger)
    {
        _logger = logger;
    }
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(CheckHealth(context, cancellationToken));
    }

    private HealthCheckResult CheckHealth(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var memoryUsage = process.WorkingSet64 / 1024.0 / 1024.0;
            var cpuTime = process.TotalProcessorTime.TotalMilliseconds;

            // Check if memory usage is within acceptable limits (e.g., 80% of available memory)
            var maxMemoryMB = 1024; // Example threshold
            if (memoryUsage > maxMemoryMB)
            {
                return HealthCheckResult.Degraded(
                    description: $"High memory usage: {memoryUsage:F2}MB",
                    data: new Dictionary<string, object>
                    {
                        { "MemoryUsageMB", memoryUsage },
                        { "CpuTimeMs", cpuTime }
                    });
            }

            // Check if CPU time is within acceptable limits
            var maxCpuTimeMs = 1000; // Example threshold
            if (cpuTime > maxCpuTimeMs)
            {
                return HealthCheckResult.Degraded(
                    description: $"High CPU usage: {cpuTime:F2}ms",
                    data: new Dictionary<string, object>
                    {
                        { "MemoryUsageMB", memoryUsage },
                        { "CpuTimeMs", cpuTime }
                    });
            }

            // If all checks pass, return healthy
            return HealthCheckResult.Healthy(
                description: "System resources are within acceptable limits",
                data: new Dictionary<string, object>
                {
                    { "MemoryUsageMB", memoryUsage },
                    { "CpuTimeMs", cpuTime }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Custom health check failed");
            return HealthCheckResult.Unhealthy(
                description: "Custom health check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "Error", ex.Message }
                });
        }
    }

} 