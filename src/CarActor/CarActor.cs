using CarActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CarActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class CarActor : Actor, ICarActor, IRemindable
    {
        /// <summary>
        /// Initializes a new instance of CarActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public CarActor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            return Task.CompletedTask;
        }

        public async Task<VehicleStatus> GetStatusAsync(CancellationToken cancellationToken)
        {
            var status = new VehicleStatus
            {
                FromGeoPosition = await StateManager.GetOrAddStateAsync<int>("from-geo-position", 0),
                CurrentGeoPosition = await StateManager.GetOrAddStateAsync<int>("current-geo-position", 0),
                ToGeoPosition = await StateManager.GetOrAddStateAsync<int>("to-geo-position", 0)
            };

            return status;
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            var currentGeoPosition = await StateManager.GetOrAddStateAsync("current-geo-position", 0);
            var toGeoPosition = await StateManager.GetOrAddStateAsync("to-geo-position", 0);

            await StateManager.AddOrUpdateStateAsync("current-geo-position", ++currentGeoPosition, (k, v) => v);

            if (currentGeoPosition >= toGeoPosition)
            {
                await StopAsync(CancellationToken.None);
            }
        }

        public async Task StartAsync(int from, int to, CancellationToken cancellationToken)
        {
            await StateManager.AddOrUpdateStateAsync("from-geo-position", from, (k, v) => v);
            await StateManager.AddOrUpdateStateAsync("current-geo-position", from, (k, v) => v);
            await StateManager.AddOrUpdateStateAsync("to-geo-position", to, (k, v) => v);

            await RegisterReminderAsync(
                "update-reminder", null,
                TimeSpan.FromSeconds(5),    //The amount of time to delay before firing the reminder
                TimeSpan.FromSeconds(5));    //The time interval between firing of reminders
        }

        public async Task StopAsync(CancellationToken cancellation)
        {
            var reminder = GetReminder("update-reminder");
            await UnregisterReminderAsync(reminder);
        }
    }
}
