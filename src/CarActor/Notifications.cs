using Microsoft.Azure.NotificationHubs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarActor
{
    public class Notifications
    {
        public static Notifications Instance = new Notifications();

        public NotificationHubClient Hub { get; set; }

        private Notifications()
        {
            Hub = NotificationHubClient.CreateClientFromConnectionString("Endpoint=sb://monitormycar.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=UxEf4uxXmVmdPLDgFmXmkvnIjLHHw90Yr259nyZCZRM=", "AZ-LC-OIP-MonitorMyCar");
        }
    }
}
