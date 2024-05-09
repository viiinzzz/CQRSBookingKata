namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Seed()
    {
        //admin setup

        Fake_Employees();
        Fake_Hotels();
        Fake_Vacancies();

        //sales setup

        Fake_Customers();


        // DateTime.Unfreeze();
    }
}