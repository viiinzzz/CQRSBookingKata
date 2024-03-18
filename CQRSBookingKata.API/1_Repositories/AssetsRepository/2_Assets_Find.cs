
namespace CQRSBookingKata.API;

public partial class AssetsRepository
{
    public int? FindHotel(string hotelName, bool approx)
    {
        var exactMatch = _back.Hotels
            .FirstOrDefault(hotel => hotel.HotelName.EqualsIgnoreCaseAndAccents(hotelName))
            ?.HotelId;

        if (exactMatch != default)
        {
            return exactMatch;
        }

        if (!approx)
        {
            return default;
        }

        var approxMatch = _back.Hotels
            .FirstOrDefault(hotel => hotel.HotelName.EqualsApprox(hotelName))
            ?.HotelId;

        return approxMatch;
    }


    public int? FindEmployee(string? lastName, string? firstName, bool? isManager, bool approx)
    {
        var query = _back.Employees.AsQueryable();

        if (isManager.HasValue)
        {
            query = query
                .Select(employee => new
                {
                    employee,
                    isManager = _back.Hotels.Where(hotel => hotel.ManagerId == employee.EmployeeId).Any()
                })
                .Where(q => q.isManager == isManager)
                .Select(q => q.employee);
        }

        var exactMatch = query
            .FirstOrDefault(employee =>
                (lastName == default || employee.LastName.EqualsIgnoreCaseAndAccents(lastName)) &&
                (firstName == default || employee.FirstName.EqualsIgnoreCaseAndAccents(firstName))
                )
            ?.EmployeeId;

        if (exactMatch != default)
        {
            return exactMatch;
        }

        if (!approx)
        {
            return default;
        }

        var approxMatch = query
            .FirstOrDefault(employee =>
                (lastName == default || employee.LastName.EqualsApprox(lastName)) &&
                (firstName == default || employee.FirstName.EqualsApprox(firstName))
            )
            ?.EmployeeId;

        return approxMatch;
    }

}