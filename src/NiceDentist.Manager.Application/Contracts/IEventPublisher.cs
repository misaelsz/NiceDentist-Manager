namespace NiceDentist.Manager.Application.Contracts;

/// <summary>
/// Interface for publishing events to message broker
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to the message broker
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="eventObject">Event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if published successfully</returns>
    Task<bool> PublishAsync<T>(T eventObject, CancellationToken cancellationToken = default) where T : class;
}
