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

public partial class AdminBus
{
    private void Verb_Is_RequestDisableEmployee(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<IdDisable<Employee>>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var employee = repo.DisableEmployee(request.id, request.disable);

        Notify(new ResponseNotification(notification, notification.Originator, EmployeeDisabled, employee));
    }

    private void Verb_Is_RequestModifyEmployee(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<IdData<UpdateEmployee>>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var employee = repo.Update(request.id, request.data);

        Notify(new ResponseNotification(notification, notification.Originator, EmployeeModified, employee));
    }

    private void Verb_Is_RequestFetchEmployee(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<Id<Employee>>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var employee = repo.GetEmployee(request.id);

        Notify(new ResponseNotification(notification, notification.Originator, EmployeeFetched, employee));
    }

    private void Verb_Is_RequestCreateEmployee(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<CreateEmployeeRequest>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var employeeId = repo.Create(request);

        var id = new Id<Employee>(employeeId);

        Notify(new ResponseNotification(notification, notification.Originator, EmployeeCreated, id));
    }
}