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

namespace VinZ.Common.Logging;


public record MessageItem(Level level, string text, DateTime time);

public abstract class AbstractLogs : ILogs
{
    protected abstract void AddMessage(MessageItem message);
    public abstract IEnumerable<MessageItem> AllMessages();
    public abstract void Clear();
    protected abstract void HandlePrintArtifact(string formattedMessage);
    protected abstract void HandleAlwaysPrintArtifact(string text);



    public Level LogLevel { get; init; }
    public bool PrintTime { get; set; } = false;


    public AbstractLogs(Level logLevel)
    {
        LogLevel = logLevel;
    }


    public bool IsEnabled(Level level) => level >= LogLevel || level == Level.None;


    private void Add(bool add, Level level, string text, DateTime time)
    {
        if (!add)
        {
            return;
        }

        AddMessage(new(level, text, time));
    }


    public void Trace(string text) => Add(LogLevel <= Level.Trace, Level.Trace, text, DateTime.Now);
    public void Debug(string text) => Add(LogLevel <= Level.Debug, Level.Debug, text, DateTime.Now);
    public void Info(string text) => Add(LogLevel <= Level.Info, Level.Info, text, DateTime.Now);
    public void Warn(string text) => Add(LogLevel <= Level.Warn, Level.Warn, text, DateTime.Now);
    public void Always(string text)
    {
        Add(true, Level.None, text, DateTime.Now);

        HandleAlwaysPrintArtifact(text);
    }


    public void Error(string text) => Add(LogLevel <= Level.Error, Level.Error, text, DateTime.Now);
    public void Error(Exception ex) =>
        Add(LogLevel <= Level.Error, Level.Error, @$"{ex.Message}

{ex.StackTrace}
", DateTime.Now);

    public void Error(string message, Exception ex) => Add(LogLevel <= Level.Error, Level.Error, $"{message}: {ex.Message}", DateTime.Now);
    
    
    public void Critical(string text) => Add(LogLevel <= Level.Critical, Level.Critical, text, DateTime.Now);
    public void Critical(Exception ex) =>
        Add(LogLevel <= Level.Critical, Level.Critical, @$"{ex.Message}

{ex.StackTrace}
", DateTime.Now);
    public void Critical(string message, Exception ex) => Add(LogLevel <= Level.Critical, Level.Critical, $"{message}: {ex.Message}", DateTime.Now);


    public override string ToString() => Print(LogLevel);

    public string Print(Level level)
    {
        return string.Join($"{Environment.NewLine}", AllMessages()
            .Where(message => message.level >= level || message.level == Level.None)
            .Select(PrintMessage));
    }

    private string FormatMessage(MessageItem? message)
    {
        return message == default
            ? ""
            : (!PrintTime ? "" : $"[{message.time:HH:mm:ss.fff}] ")
              + (message.level == Level.None ? "" : $"{message.level}: ")
              + message.text;
    }


    protected string PrintMessage(MessageItem? message)
    {
        var formattedMessage = FormatMessage(message);

        HandlePrintArtifact(formattedMessage);

        return formattedMessage;
    }

    public string PrintLastMessage()
        => PrintMessage(AllMessages().LastOrDefault());



    public AbstractLogs Import(AbstractLogs logs2)
    {
        var concatLogLevel = LogLevel < logs2.LogLevel ? LogLevel : logs2.LogLevel;

        var concatMessages = AllMessages()
            .Concat(logs2.AllMessages())
            .OrderBy(message => message.time)
            .ToArray();

        Clear();

        foreach (var message in concatMessages)
        {
            AddMessage(message);
        }

        return this;
    }
}