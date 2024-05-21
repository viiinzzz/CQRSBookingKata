namespace Booking.API;

internal class Dependencies
{
    private Dependencies()
    {
        //for type dependency diagram, establish dependency
        _ = nameof(EnterpriseStorage);
        _ = nameof(EnterpriseNetwork);
    }


    public static readonly Type[] AvailableDbContextTypes = Types.From
    <
        BookingAdminContext, BookingSalesContext, BookingPlanningContext,
        MessageQueueContext, MoneyContext, GazeteerContext
    >();


    public static readonly (Type interfaceType, Type implementationType)[] AvailableRepositories = Types.From
    <
        IMessageBus,
        IAdminRepository, ISalesRepository, IPlanningRepository,
        IMessageQueueRepository, IMoneyRepository

    >().Zip(Types.From<

        MqServer,
        AdminRepository, SalesRepository, PlanningRepository,
        MessageQueueRepository, MoneyRepository

    >(), (typeInterface, typeImplementation) => (typeInterface, typeImplementation)).ToArray();


    public static readonly (Type interfaceType, Type implementationType)[] AvailableServices = Types.From
    <
        AdminQueryService, SalesQueryService, BookingCommandService, PlanningQueryService, PlanningCommandService, KpiQueryService,
        IBillingCommandService, IGazetteerService, IPaymentCommandService, IPricingQueryService

    >().Zip(Types.From<

        AdminQueryService, SalesQueryService, BookingCommandService, PlanningQueryService, PlanningCommandService, KpiQueryService,
        BillingCommandService, GazetteerService, PaymentCommandService, PricingQueryService

    >(), (typeInterface, typeImplementation) => (typeInterface, typeImplementation)).ToArray();


    public static readonly Type[] AvailableBusTypes = Types.From
    <
        AdminBus, SalesBus, PlanningBus,
        BillingBus, ThirdPartyBus, ConsoleAuditBus,
        DemoBus
    >();
}