namespace NiceDentist.Manager.Application.Contracts;

/// <summary>
/// Interface for handling events from message broker
/// </summary>
/// <typeparam name="T">Event type to handle</typeparam>
public interface IEventHandler<in T> where T : class
{
    /// <summary>
    /// Handles an event
    /// </summary>
    /// <param name="eventObject">Event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if handled successfully</returns>
    Task<bool> HandleAsync(T eventObject, CancellationToken cancellationToken = default);
}
