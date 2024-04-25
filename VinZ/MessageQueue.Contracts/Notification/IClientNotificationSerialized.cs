namespace VinZ.MessageQueue;

public interface IClientNotificationSerialized :
    IHaveDeliveryOptions,
    IHaveCorrelation,
    IHaveDestination,
    IHaveOrigin,

    IHaveSerializedMessage
;