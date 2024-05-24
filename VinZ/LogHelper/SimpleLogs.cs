/*
 * LoggingHelper
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

using System.Diagnostics;

namespace VinZ.Common.Logging;


//non thread-safe logging backed by a list


[DebuggerDisplay("LastMessage = {PrintLastMessage()}")]
public class SimpleLogs : AbstractLogs
{
    private readonly List<MessageItem> _messages = new();

    protected override void AddMessage(MessageItem message)
    {
        _messages.Add(message);
    }

    public override IEnumerable<MessageItem> AllMessages()
    {
        return _messages.ToArray();
    }

    public override void Clear()
    {
        _messages.Clear();
    }


    protected override void HandlePrintArtifact(string formattedMessage)
    {
        ;
    }

    protected override void HandleAlwaysPrintArtifact(string text)
    {
        Console.WriteLine(text);
    }

    public SimpleLogs(Level logLevel) : base(logLevel) {}


    public static SimpleLogs operator +(SimpleLogs logs1, SimpleLogs logs2)
    {
        var concatLogLevel = logs1.LogLevel < logs2.LogLevel ? logs1.LogLevel : logs2.LogLevel;

        var concatLogs = new SimpleLogs(concatLogLevel);

        var concatMessages = logs1.AllMessages()
            .Concat(logs2.AllMessages())
            .OrderBy(message => message.time);

        foreach (var message in concatMessages)
        {
            concatLogs.AddMessage(message);
        }

        return concatLogs;
    }
}