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

using System.Collections.Concurrent;
using System.Diagnostics;
using VinZ.Common.Async;

namespace VinZ.Common.Logging;


//thread-safe logging backed by a blocking collection


[DebuggerDisplay("LastMessage = {PrintLastMessage()}")]
public class Logs : AbstractLogs//, IDisposable
{
    //private readonly AsyncBuffer<MessageItem> _messages = new(default, "Logs");
    private readonly ConcurrentDictionary<int, MessageItem> _messages = new();
    private int dictId = 0;
    
    protected override void AddMessage(MessageItem message)
    {
        //_messages.Enqueue(message);
      
        _messages.AddOrUpdate(dictId++, message, (i, m) => m);
    }

    public override IEnumerable<MessageItem> AllMessages()
    {
        //return _messages.Snapshot();
        return _messages.Values.OrderBy(m => m.time);
    }

    public override void Clear()
    {
        //_messages.DequeueAll();
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

    //public void Dispose()
    //{
    //    _messages.Dispose();
    //}

    public Logs(Level logLevel) : base(logLevel) {}


    public static Logs operator +(Logs logs1, Logs logs2)
    {
        if (logs1 == null)
        {
            return logs2;
        }

        if (logs2 == null)
        {
            return null;
        }

        var concatLogLevel = logs1.LogLevel < logs2.LogLevel ? logs1.LogLevel : logs2.LogLevel;

        var concatLogs = new Logs(concatLogLevel);

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
