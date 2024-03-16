using System.Collections.Generic;

namespace CQRSBookingKata.Assets;

public record NewEmployee(string LastName, string FirstName, long SocialSecurityNumber);
public record UpdateEmployee(string? LastName, string? FirstName, bool? Disabled);

public record NewHotel(string HotelName, double Latitude, double Longitude);

public record UpdateHotel(
    string? HotelName, int? EarliestCheckInTime, int? LatestCheckOutTime,
    string? LocationAddress, string? ReceptionPhoneNumber,
    string? Url, int? Ranking,
    int? ManagerId, bool? Disabled
);

public record UpdateRoom(int? PersonMaxCount);


public interface IAssetsRepository
{
    IQueryable<Employee> Employees { get; }
    IQueryable<Hotel> Hotels { get; }
    IQueryable<Room> Rooms(int hotelId);


    int Create(NewEmployee employee);
    Employee? GetEmployee(int employeeId);
    void Update(int employeeId, UpdateEmployee update);
    void DisableEmployee(int employeeId, bool disable);


    int Create(NewHotel hotel);
    Hotel? GetHotel(int hotelId);
    void Update(int hotelId, UpdateHotel update);
    void DisableHotel(int hotelId, bool disable);

    void Create(Room room);
    Room? GetRoom(int uniqueRoomId);
    void Update(int roomId, UpdateRoom update);
    void DeleteRoom(int roomId);

}