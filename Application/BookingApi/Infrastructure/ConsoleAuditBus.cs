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

namespace BookingKata.Infrastructure.Network;


public class ConsoleAuditBus
(
    ITimeService DateTime,
    IServerContextService server,
    // ILogger<ConsoleAuditBus> log
    ILoggerFactory logFactory
)
    : MessageBusClientBase
{
    ILogger<ConsoleAuditBus> log = logFactory.CreateLogger<ConsoleAuditBus>();


    private static Regex unquote = new Regex(@"^""(.*)""$");


    public override async Task Configure()
    {
        // await
        Subscribe(Omni, AuditMessage);

        Notified += (object? sender, IClientNotificationSerialized notification) =>
        {
            var serverLabel = sender == null ? "" : $"<<<Server:{server.Id.xby4()}>>>";
            var senderLabel = sender == null ? "" : $"<<<{sender.GetType().Name}:{sender.GetHashCode().xby4()}>>>";

            var correlation = new CorrelationId(notification.CorrelationId1, notification.CorrelationId2);
            var now = DateTime.UtcNow.ToString("O");

            var messageString = notification.Message.Replace("\\r", "").Replace("\\n", Environment.NewLine);
            messageString = unquote.Replace(Regex.Unescape(messageString), "$1");

            log.Log(LogLevel.Warning, @$"{serverLabel} {senderLabel} Notification{correlation.Guid}
===
{messageString}
===");
        };
    }
}