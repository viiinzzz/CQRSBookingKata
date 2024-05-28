﻿/*
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

namespace BookingKata.API.Demo;

public class DemoContextService(IServerContextService serverContext) : IDemoContext
{
    public int[] FakeStaffIds { get; set; } = [];
    public int[] FakeManagerIds { get; set; } = [];
    public int[] FakeHotelsIds { get; set; } = [];
    public int[] FakeCustomerIds { get; set; } = [];
    public ConcurrentDictionary<int, FakeHelper.FakeCustomer> FakeCustomersDictionary { get; } = new();

    public (int, FakeHelper.FakeCustomer)[] FakeCustomers
        => FakeCustomersDictionary.Select(kv => (kv.Key, kv.Value)).ToArray();

    public bool SeedComplete { get; set; } = false;

    public int SimulationDay { get; set; } = 0;
    public Hotel[] Hotels { get; set; } = [];

    public long ServerId { get; set; } = serverContext.Id;
    public long SessionId { get; set; } = serverContext.SessionId;
}
