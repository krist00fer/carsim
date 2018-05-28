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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<VehicleStatus> GetStatusAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Starts the Vehicle, simulating driving from one point to another
        /// </summary>
        /// <param name="from">Drive from geo-position</param>
        /// <param name="to">Drive to geo-position</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(int from, int to, CancellationToken cancellationToken);

        /// <summary>
        /// Stops the Vehicle simulation
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellation);
    }
}
