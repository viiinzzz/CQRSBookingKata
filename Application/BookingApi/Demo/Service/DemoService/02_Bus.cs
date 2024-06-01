/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace BookingKata.API.Demo;

public partial class DemoBus : MessageBusClientBase
{
    private readonly string originator = nameof(DemoBus);

    public override async Task Configure()
    {
        Subscribe(Recipient.Demo, Verb.Demo.RequestDemoContext);

        Notified += (sender, notification) =>
        {
            try
            {
                switch (notification.Verb)
                {
                    case Verb.Demo.RequestDemoContext:
                    {
                        Verb_Is_RequestDemoContext(notification);
                        break;
                    }

                    case Verb.Demo.RequestDemoHotels:
                    {
                        Verb_Is_RequestDemoHotels(notification);
                        break;
                    }

                    default:
                    {
                        throw new VerbInvalidException(notification.Verb);
                    }
                }
            }
            catch (Exception ex)
            {
                Notify(notification.Response(new ResponseOptions
                {
                    ex = ex
                }));
            }
        };
    }

    private void Verb_Is_RequestDemoContext(IClientNotificationSerialized notification)
    {
        System.Console.WriteLine(@$"=================================Verb_Is_RequestDemoContext
{demoContextService.ToJson(true)}");

        throw new Exception("no go further!!!!!!!!!!!!!!!!!!!");

        Notify(notification.Response(new ResponseOptions {
            Recipient = Omni,
            Verb = Verb.Demo.RequestDemoContext,
            MessageObj = demoContextService
        }));
    }
    
    private void Verb_Is_RequestDemoHotels(IClientNotificationSerialized notification)
    {

        var list = bus.Ask<IdCollection<Hotel>>(
                nameof(DemoBus), notification._steps,
                Recipient.Admin, Verb.Admin.RequestFetchHotelList,
                null, CancellationToken.None)
            ?.Result ?? throw new HotelNotFoundException();

        var hotels = list.ids

            .Select(hotelId => bus.Ask<Hotel>(
                    nameof(DemoBus), notification._steps,
                    Recipient.Admin, Verb.Admin.RequestFetchHotel,
                    new HotelRef(hotelId), CancellationToken.None)
                ?.Result ?? throw new HotelNotFoundException())

            .ToArray();


        Notify(notification.Response(new ResponseOptions {
            Recipient = Omni,
            Verb = Verb.Demo.RequestDemoHotels,
            MessageObj = hotels
        }));
    }

}