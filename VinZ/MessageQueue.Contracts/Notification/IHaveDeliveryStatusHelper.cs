namespace VinZ.MessageQueue;

public static class IHaveDeliveryStatusHelper
{
    public static bool IsErrorStatus(this IHaveDeliveryStatus notification)
    {

        if (notification.Status is < 400 or >= 600)
        {
            return false;
        }

        return true;
    }
}