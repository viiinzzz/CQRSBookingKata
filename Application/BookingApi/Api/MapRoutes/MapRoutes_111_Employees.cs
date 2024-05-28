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

using static BookingKata.Services.Verb.Admin;

namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string EmployeesTag = "Employees";

    private static void MapRoutes_11_Employees(RouteGroupBuilder admin)
    {
        var employees = admin.MapGroup("/employees"
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);

        employees.MapListMq<Employee>("/", "/admin/employees", filter: null,
            Recipient.Admin, RequestPage, originator, 
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, EmployeesTag]);

        employees.MapPostMq<CreateEmployeeRequest>("/",
            Recipient.Admin, RequestCreateEmployee, originator, 
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, EmployeesTag]);

        employees.MapGetMq<Employee>("/{id}",
            Recipient.Admin, RequestFetchEmployee, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, EmployeesTag]);

        employees.MapPatchMq<UpdateEmployee>("/{id}",
            Recipient.Admin, RequestModifyEmployee, originator, 
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, EmployeesTag]);

        employees.MapDisableMq<Employee>("/{id}",
            Recipient.Admin, RequestDisableEmployee, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, EmployeesTag]);
    }

}