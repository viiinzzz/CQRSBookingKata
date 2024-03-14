namespace CQRSBookingKata.Sales;

public class PaymentFailureException() : Exception
{

}

public class ServerErrorException : Exception
{
    public ServerErrorException(Exception innerException) : base("server error", innerException) {}
}