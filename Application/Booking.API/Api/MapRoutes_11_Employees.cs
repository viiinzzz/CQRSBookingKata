using static BookingKata.Services.Verb.Admin;

namespace BookingKata.API;

public static partial class ApiMethods
{
    private static void MapRoutes_11_Employees(RouteGroupBuilder admin)
    {
        var employees = admin.MapGroup("/employees"
            ).WithOpenApi();

        employees.MapListMq<Employee>("/", "/admin/employees",
            Recipient.Admin, RequestPage, responseTimeoutSeconds
            ).WithOpenApi();

        employees.MapPostMq<NewEmployee>("/",
            Recipient.Admin, RequestCreateEmployee, responseTimeoutSeconds
            ).WithOpenApi();

        employees.MapGetMq<Employee>("/{id}", 
            Recipient.Admin, RequestFetchEmployee, responseTimeoutSeconds
            ).WithOpenApi();

        employees.MapPatchMq<UpdateEmployee>("/{id}", 
            Recipient.Admin, RequestModifyEmployee, responseTimeoutSeconds
            ).WithOpenApi();

        employees.MapDisableMq<Employee>("/{id}",
            Recipient.Admin, RequestDisableEmployee, responseTimeoutSeconds
            ).WithOpenApi();
    }

}