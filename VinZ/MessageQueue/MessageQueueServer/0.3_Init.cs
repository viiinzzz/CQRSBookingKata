/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace VinZ.MessageQueue;


public partial class MqServer : Initializable, IMessageBus
{
    private ConcurrentDictionary<IServiceScope, IMessageBusClient> _domainBuses = new();

    public override void Init()
    {
        // scope = scp.GetScope(out queue);

        DateTime.Notified += (sender, time) =>
        {
            var originator = GetType().Name;

            // var message = new
            // {
            //     // action = time.verb,
            //     time.state,
            //     time = $"{time.UtcNow.SerializeUniversal()}"
            // };

            // var message = $"Time {time.UtcNow.SerializeUniversal()} ({time.state})";
            var message = new
            {
                time = $"{time.UtcNow.SerializeUniversal()}",
                state = $"{time.state}"
            };


            var parentNotification = new ClientNotification(NotificationType.Request, nameof(DateTime), nameof(DateTime.Notified))
            {
                _steps = [],
                Originator = originator
            };

            Notify(new ResponseNotification(parentNotification, default, AuditMessage, message)
            {
                Immediate = true
            }, 0);
        };

        if (config.DomainBusTypes == default)
        {
            return;
        }


        var addClient = async (Type type) =>
        {
            if (typeof(IMessageBus).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Must implement {nameof(IMessageBus)}", nameof(config.DomainBusTypes));
            }

            var scope = scp.GetScope(type, out var domainBus);

            var client = (IMessageBusClient)domainBus;

            // client.ConnectToBus(this);
            client.ConnectToBus(scp);

            await client.Configure();

            client.Log = log;

            if (_isTrace) log.LogInformation(
                $"<<<{type.Name}:{client.GetHashCode().xby4()}>>> Connected.");

            _domainBuses[scope] = client;
        };


        var allAdded = Task
            .WhenAll(config.DomainBusTypes.Select(type => addClient(type)))
            .ContinueWith(prev =>
            {
                if (prev.IsCompletedSuccessfully)
                {
                    var master = $"<<<{nameof(MqServer)}:{GetHashCode().xby4()}>>>";
                    var clients = (string prepend) => string.Join(Environment.NewLine + prepend, _domainBuses.Values
                        .Select(client => $"<<<{client.GetType().Name}:{client.GetHashCode().xby4()}>>>"));

                    if (_isTrace) log.LogInformation(@$"+----------------------------------------
| Bus definition:
| {master}
| {clients("| ")}
+----------------------------------------");
                }
            });


    }
}