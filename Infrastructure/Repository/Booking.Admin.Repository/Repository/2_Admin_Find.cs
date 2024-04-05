namespace BookingKata.Infrastructure.Storage;

public partial class AdminRepository
{
    public int? FindHotel(string hotelName, bool approx)
    {
        var query = _admin.Hotels
            .AsNoTracking();

        var exactMatch = query
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

        var approxMatch = query
            .FirstOrDefault(hotel => hotel.HotelName.EqualsApprox(hotelName))
            ?.HotelId;

        return approxMatch;
    }


    public int? FindEmployee(string? lastName, string? firstName, bool? isManager, bool approx)
    {
        var query = _admin.Employees
            .AsNoTracking()
            .AsQueryable();

        if (isManager.HasValue)
        {
            query = query
                .Select(employee => new
                {
                    employee,
                    isManager = _admin.Hotels.Where(hotel => hotel.ManagerId == employee.EmployeeId).Any()
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




    private static int[] ForbiddenRoomNumbers = GenForbiddenRoomNumbers();

    private static int[] AuthorizedRoomNumbers = Enumerable.Range(1, 9999)
        .Where(roomNum => ForbiddenRoomNumbers
            .All(forbiddenRoomNum => forbiddenRoomNum != roomNum))
        .ToArray();

    private static int[] GenForbiddenRoomNumbers()
    {
        var ret = new List<int>();

        for (var roomNum = 13; roomNum < 1_0000; roomNum += 100)
        {
            ret.Add(roomNum);
        }

        return ret.ToArray();
    }

    public int[] GetFloorNextRoomNumbers(int hotelId, int floorNum, int roomCount)
    {
        var hotel = GetHotel(hotelId);

        if (hotel == default)
        {
            throw new ArgumentException(Common.Exceptions.ReferenceInvalid, nameof(hotelId));
        }

        var newRoomNums = AuthorizedRoomNumbers
            .Where(authorizedRoomNum => authorizedRoomNum / 100 == floorNum &&
                                        !Rooms(hotelId).Select(room => room.RoomNum).Contains(authorizedRoomNum))
            .Order()
            .Take(roomCount)
            .ToArray();

        return newRoomNums;
    }

}