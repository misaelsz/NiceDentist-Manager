namespace NiceDentist.Manager.Application.Contracts;

/// <summary>
/// Interface for consuming events from message broker
/// </summary>
public interface IEventConsumer
{
    /// <summary>
    /// Starts consuming events from the message broker
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops consuming events from the message broker
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StopAsync(CancellationToken cancellationToken = default);
}
