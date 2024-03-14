using CQRSBookingKata.Assets;
using CQRSBookingKata.Billing;

namespace CQRSBookingKata.Tests;

public class Demo
{
    public static int Main(string[] args)
    {
        // var bookings = new BookingCommandService();
        // var assets = new AssetService();




        const int staffPerHotel = 3;
        const int managerPerHotel = 1;
        const int hotelCount = 3;
        const int floorPerHotel = 2;
        const int roomPerFloor = 3;

        var staff = RandomHelper
            .GenerateFakeEmployees(hotelCount * staffPerHotel)
            .Select((fake, employeeId) =>
            {
                var employee = new Employee(
                    fake.LastName, fake.FirstName, fake.SocialSecurityNumber, employeeId + 1
                );

                var payrollId = employeeId;
                var payroll = new Payroll(fake.MonthlyIncome, fake.Currency, employeeId, payrollId);

                return employee;
            })
            .ToArray();
        
        var managers = RandomHelper
            .GenerateFakeEmployees(hotelCount * managerPerHotel)
            .Select((fake, employeeId) =>
            {
                var employee = new Employee(
                    fake.LastName, fake.FirstName, fake.SocialSecurityNumber, employeeId + 1 + hotelCount * staffPerHotel
                );

                var payrollId = employeeId;
                var payroll = new Payroll(fake.MonthlyIncome, fake.Currency, employeeId, payrollId);

                return employee;
            })
            .ToArray();

        var employees = staff.Concat(managers).ToArray();
        
        var chain = RandomHelper
            .GenerateFakeHotels(3)
            .Select((fake, hotelId) =>
            {
                var managerId = hotelId + 1 + hotelCount * staffPerHotel;

                var hotel =  new Hotel(
                    fake.HotelName,
                    fake.Latitude, fake.Longitude,
                    16_00, 10_00,
                    fake.LocationAddress, fake.ReceptionTelephoneNumber, fake.Url, 2,
                    managerId, hotelId + 1);

                //add rooms

                return hotel;
            })
            .ToArray();

        var fakeUsers = RandomHelper
            .GenerateFakeEmployees(9);


        return 0;
    }
}