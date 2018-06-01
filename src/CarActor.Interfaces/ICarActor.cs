using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;
using Models;

[assembly: FabricTransportActorRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace CarActor.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface ICarActor : IActor
    {
        /// <summary>
        /// Returns the Vehicle Status
        /// </summary>
        /// <returns></returns>
        Task<VehicleStatus> GetStatusAsync();

        /// <summary>
        /// Get the status of the rule
        /// </summary>
        /// <returns>If rule is active, it returns whether the rule has hit or not. Otherwise it return null</returns>
        Task<bool?> GetRuleStatusAsync();

        /// <summary>
        /// Starts the Vehicle, simulating driving from one point to another
        /// </summary>
        /// <param name="latitude">Starting position</param>
        /// <param name="longitude">Starting position</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(string vechicleId, double latitude, double longitude, CancellationToken cancellationToken);

        /// <summary>
        /// Stops the Vehicle simulation
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellation);

        /// <summary>
        /// Sets an optional rule that monitors the driving
        /// </summary>
        /// <param name="maxSpeed">Maximum speed that the driver can drive</param>
        /// <param name="geoBoundaryJson">Geographic boundaris that the driver should not exceed. Expressed in GeoJSON</param>
        /// <returns></returns>
        Task SetRuleAsync(double maxSpeed, string geoBoundaryJson);
    }
}
