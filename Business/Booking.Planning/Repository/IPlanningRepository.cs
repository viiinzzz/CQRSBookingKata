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

namespace BookingKata.Planning;

public interface IPlanningRepository
{
    IQueryable<ReceptionCheck> Checks { get; }
    IQueryable<RoomServiceDuty> Duties { get; }
    IQueryable<ServerContext> ServerContexts { get; }

    void Add(ReceptionCheck check);
    void Add(RoomServiceDuty duty);

    void DoneCheck(int CheckId, int employeeId, DateTime doneDate);
    ReceptionCheck CancelCheck(int CheckId, DateTime cancelDate);
    void DoneDuty(int dutyId, int employeeId, DateTime doneDate);
    RoomServiceDuty CancelDuty(int dutyId, DateTime cancelDate);
    void SetDutyBusyTime(int dutyId, DateTime busyTime, double busyDayNum);

    ServerContext? GetServerContext();
    void SetServerContext(ServerContext serverContext);
}