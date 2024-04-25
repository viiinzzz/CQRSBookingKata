namespace VinZ.MessageQueue;

public interface IClientNotificationDeserialized :
    IHaveDeliveryOptions,
    IHaveCorrelation,
    IHaveDestination,

    IHaveDeserializedMessage
;