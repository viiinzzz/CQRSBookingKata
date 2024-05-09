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