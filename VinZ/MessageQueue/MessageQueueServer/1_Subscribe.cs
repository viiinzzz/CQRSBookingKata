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

public partial class MqServer
{
    public string[] GetSubscribeUrls()
    {
        return _subscribers_0.Values
            .Concat(_subscribers_R.Values.SelectMany(s => s))
            .Concat(_subscribers_V.Values.SelectMany(s => s))
            .Concat(_subscribers_RV.Values.SelectMany(s => s))
            .Distinct()
            .Select(url =>
            {
                _url2name.TryGetValue(url, out var name);

                return $"{url}{(name !=null ? " ": string.Empty)}{name ?? string.Empty}";
            })
            .ToArray();
    }

    private readonly ConcurrentDictionary<int, string> _subscribers_0 = new(); //recipient* verb*
    private readonly ConcurrentDictionary<int, HashSet<string>> _subscribers_R = new(); //recipient verb*
    private readonly ConcurrentDictionary<int, HashSet<string>> _subscribers_V = new(); //recipient* verb
    private readonly ConcurrentDictionary<int, HashSet<string>> _subscribers_RV = new(); //recipient verb
    
    private readonly ConcurrentDictionary<string, string> _url2name = new(); //recipient verb

    public void Subscribe(SubscriptionRequest sub, int busId)
    {
        if (busId != 0)
        {
            throw new ArgumentException("Only value 0 allowed", nameof(busId));
        }

        var name = sub.name;
        var url = sub.url;
        var recipient = sub.recipient;
        var verb = sub.verb;

        _url2name[url] = name;

        var hash0 = url.GetHashCode();

        if (recipient == Omni && verb == AnyVerb)
        {
            _subscribers_0[hash0] = url;

            if (_isTrace) log.LogInformation(
                $"<<<{name}:{hash0.xby4()}>>> Subscribe recipient={nameof(Omni)}, verb=Any (0+{hash0.xby4()})");

            // hash = hash0;
            return;
        }

        if (recipient != Omni && verb == AnyVerb)
        {
            var hash1 = recipient.GetHashCode();

            _subscribers_R.AddOrUpdate(hash1,
                new HashSet<string>(new[] { url }),
                (i, subscribers) =>
                {
                    subscribers.Add(url);
                    return subscribers;
                });

            if (_isTrace) log.LogInformation(
                $"<<<{name}:{hash0.xby4()}>>> Subscribe recipient={recipient}, verb=Any (R+{hash1.xby4()})");

            // hash = hash1;
            return;
        }

        if (recipient == Omni && verb != AnyVerb)
        {
            var hash1 = verb.GetHashCode();

            _subscribers_V.AddOrUpdate(hash1,
                new HashSet<string>(new[] { url }),
                (i, subscribers) =>
                {
                    subscribers.Add(url);
                    return subscribers;
                });

            if (_isTrace) log.LogInformation(
                $"<<<{name}:{hash0.xby4()}>>> Subscribe recipient={nameof(Omni)}, verb={verb} (V+{hash1.xby4()})");

            // hash = hash1;
            return;
        }

        var hash2 = (recipient, verb).GetHashCode();

        _subscribers_RV.AddOrUpdate(hash2,
            new HashSet<string>(new[] { url }),
            (i, subscribers) =>
            {
                subscribers.Add(url);
                return subscribers;
            });

        if (_isTrace) log.LogInformation(
            $"<<<{name}:{hash0.xby4()}>>> Subscribe recipient={recipient}, verb={verb} (RV+{hash2.xby4()})");

        // hash = hash2;
        return;
    }

    public bool Unsubscribe(SubscriptionRequest sub, int busId)
    {
        if (busId != 0)
        {
            throw new ArgumentException("Only value 0 allowed", nameof(busId));
        }

        var name = sub.name;
        var url = sub.url;
        var recipient = sub.recipient;
        var verb = sub.verb;

        var hash0 = sub.url.GetHashCode();

        if (_isTrace) log.LogInformation(
            $"<<<{name}:{hash0.xby4()}>>> Unsubscribe recipient=Any, verb=Any (0+{hash0.xby4()})");

        if (recipient == Omni && verb == AnyVerb)
        {
            return _subscribers_0.Remove(url.GetHashCode(), out _);
        }

        if (recipient != Omni && verb == AnyVerb)
        {
            return !_subscribers_R.AddOrUpdate(recipient.GetHashCode(),
                    new HashSet<string>(),
                    (i, subscribers) =>
                    {
                        subscribers.Remove(url);
                        return subscribers;
                    })
                .Contains(url);
        }

        if (recipient == Omni && verb != AnyVerb)
        {
            return !_subscribers_V.AddOrUpdate(verb.GetHashCode(),
                    new HashSet<string>(),
                    (i, subscribers) =>
                    {
                        subscribers.Remove(url);
                        return subscribers;
                    })
                .Contains(url);
        }

        return !_subscribers_RV.AddOrUpdate((recipient, verb).GetHashCode(),
                new HashSet<string>(),
                (i, subscribers) =>
                {
                    subscribers.Remove(url);
                    return subscribers;
                })
            .Contains(url);
    }
}