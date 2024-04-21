namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus
{
    private void RequestEmployeesPage(PageRequest request, out object? page)
    {
        using var scope = sp.GetScope<IAdminRepository>(out var adminRepository);

        page = adminRepository
            .Employees
            .Page(request.Path, request.Page, request.PageSize);
    }
}