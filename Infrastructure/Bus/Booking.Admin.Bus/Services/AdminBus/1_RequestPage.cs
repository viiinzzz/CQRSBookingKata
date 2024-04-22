﻿using System;

namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus
{
    private void Verb_Is_RequestPage(IClientNotification notification)
    {
        var request = notification.MessageAs<PageRequest>();

        object? page;

        switch (request.Path)
        {
            case "/admin/hotels":
            {
                RequestHotelsPage(request, out page);
                break;
            }

            case "/admin/employees":
            {
                RequestEmployeesPage(request, out page);
                break;
            }

            case "/admin/geo/indexes":
            {
                var originator = GetType().FullName
                                 ?? throw new ArgumentException("invalid originator");

                //pass request to third-party
                page = AskResult<PageRequest>(originator, Support.Services.ThirdParty.Recipient, RequestPage, request);

                break;
            }

            default:
            {
                throw new NotImplementedException($"page request for path not supported: {request.Path}");
            }
        }

        Notify(new ResponseNotification(notification.Originator)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = page
        });
    }
}