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

using System.Text.Json;

namespace BookingKata.Sales;

public partial class SalesQueryService
{
    public IQueryable<StayMatch> FindStay(StayRequest request, int customerId)
    {
        if (
            !request.ArrivalFlexBeforeDays.HasValue &&
            !request.ArrivalFlexAfterDays.HasValue &&
            !request.DepartureFlexBeforeDays.HasValue &&
            !request.DepartureFlexAfterDays.HasValue &&
            !request.NightsCountMin.HasValue &&
            !request.NightsCountMax.HasValue
        )
        {
            //fixed bounds

            return FindStay2(request, customerId);
        }

        //flex bounds

        var flexRequests = new List<StayRequest>();

        for (int arrivalFlexDays = -request.ArrivalFlexBeforeDays ?? 0;
             arrivalFlexDays <= (request.ArrivalFlexAfterDays ?? 0);
             arrivalFlexDays++)
        {
            for (int departureFlexDays = -request.DepartureFlexBeforeDays ?? 0;
                 departureFlexDays <= (request.DepartureFlexAfterDays ?? 0);
                 departureFlexDays++)
            {
                try
                {
                    var flexRequest = request with
                    {
                        ArrivalFlexBeforeDays = null,
                        ArrivalFlexAfterDays = null,
                        DepartureFlexBeforeDays = null,
                        DepartureFlexAfterDays = null,
                        NightsCountMin = null,
                        NightsCountMax = null,
                        ArrivalDate = request.ArrivalDate.AddDays(arrivalFlexDays),
                        DepartureDate = request.DepartureDate.AddDays(departureFlexDays)
                    };
                    //may not pass validation and throw

                    flexRequests.Add(flexRequest);
                }
                catch (Exception ex)
                {
                    //invalid bounds dismissed
                }
            }
        }

        //lengthiest stays first
        flexRequests.Sort((r1, r2) => (r1.Nights - r2.Nights));


        var ret = flexRequests.Aggregate(
            seed: Array.Empty<StayMatch>().AsQueryable(),
            (accumulator, flexRequest) => accumulator.Union(FindStay2(flexRequest, customerId))
            );

        return ret;
    }

    private static readonly string[] StepsFindStay = [$"{nameof(SalesQueryService)}.{nameof(FindStay)}"];

    private IQueryable<StayMatch> FindStay2(StayRequest request, int customerId)
    {
        var firstNight = OvernightStay.From(request.ArrivalDate);
        var lastNight = OvernightStay.FromCheckOutDate(request.DepartureDate);


        var maxKm =
            !request.MaxKm.HasValue ? FindMaxKm
            : request.MaxKm < FindMinKm ? FindMinKm
            : request.MaxKm > FindMaxKm ? FindMaxKm
            : request.MaxKm.Value;


        var nightsCount = firstNight.To(lastNight);


        var requestCells = new List<IGeoIndexCell>();

        if (request is { Latitude: not null, Longitude: not null })
        {
            var positionCells = geo.NewGeoIndex(request, bconf.PrecisionMaxKm, maxKm);

            requestCells.AddRange(positionCells);
        }

        ;

        if (request.CityName != default)
        {
            var citiesCells = geo
                .QueryCities(request.CityName, request.ApproximateNameMatch, request.CountryCode)
                .Where(city => city.Position != default)
                .SelectMany(city => geo.CacheGeoIndex(city, bconf.PrecisionMaxKm))
                .AsEnumerable();

            requestCells.AddRange(citiesCells);
        }

        if (requestCells.Count == 0)
        {
            throw new ArgumentException(
                $"must specify either known {request.CityName} or {request.Latitude} and {request.Longitude}",
                nameof(request.CityName));
        }

        var matchingVacancyIds = geo.GetMatchingRefererLongIds<Vacancy>(requestCells).ToHashSet();


        var vacancies =
            from vacancy in sales.Vacancies
            where vacancy.PersonMaxCount >= request.PersonCount &&
                  vacancy.DayNum >= firstNight.DayNum &&
                  vacancy.DayNum <= lastNight.DayNum
            select vacancy;


        if (request.HotelName != default)
        {
            vacancies =
                from vacancy in vacancies
                where !(request.ApproximateNameMatch ?? false)
                    ? vacancy.HotelName.EqualsIgnoreCaseAndAccents(request.HotelName)
                    : vacancy.HotelName.EqualsApprox(request.HotelName)
                select vacancy;
        }


        {
            vacancies = vacancies
                .Where(vacancy => matchingVacancyIds.Contains(vacancy.VacancyId));
        }


        var urids =
            from stay in
                from vacancy in vacancies
                group vacancy by vacancy.UniqueRoomId
                into stay
                select new
                {
                    urid = stay.Key,
                    nightsCount = stay.Count(),
                    personMaxCount = stay.First().PersonMaxCount
                }
            where stay.nightsCount == nightsCount
            orderby stay.personMaxCount
            select stay.urid;

        var ids = urids.ToArray(); //fetch into db only good room size, geo-localization and availability timing

        if (ids.Length == 0)
        {
            throw new StayNotFoundException(request);
        }

        var originator = GetType().FullName
                         ?? throw new Exception("invalid originator");

        var roomDetails = bus.AskResult<RoomDetails[]>(
            Recipient.Admin, Verb.Admin.RequestManyRoomDetails,
            new RoomDetailsRequest
            {
                onlyRoomNumbers = ids
            },
            originator, StepsFindStay);

        if (roomDetails is null or { Length: 0 })
        {
            throw new RoomNotFoundException();
        }


        var customerProfile = GetCustomerProfile(customerId);


        var stays = roomDetails
            .Select(roomDetail =>
            {


                var price = bus.AskResult<Price>(
                    Support.Services.ThirdParty.Recipient, Support.Services.ThirdParty.Verb.RequestPricing,
                    new PricingRequest
                    {
                        //room
                        personMaxCount = roomDetail.PersonMaxCount,
                        floorNum = roomDetail.FloorNum,
                        floorNumMax = roomDetail.FloorNumMax,
                        hotelRank = roomDetail.HotelRank,
                        latitude = roomDetail.Latitude,
                        longitude = roomDetail.Longitude,

                        //booking
                        personCount = request.PersonCount,
                        arrivalDateUtc = request.ArrivalDate.SerializeUniversal(),
                        departureDateUtc = request.DepartureDate.SerializeUniversal(),
                        currency = request.Currency,
                        customerProfileJson = JsonSerializer.Serialize(customerProfile)
                    },
                    originator, StepsFindStay);

                var match = new StayMatch
                (
                    request.PersonCount,
                    price.Amount,
                    price.Currency,
                    request.ArrivalDate,
                    request.DepartureDate,
                    roomDetail.Urid
                );

                return match;
            })
            .Where(stay => //now filter with dynamic pricing

                (!request.PriceMax.HasValue || stay.Price <= request.PriceMax) &&

                (!request.PriceMin.HasValue || request.PriceMax.HasValue && request.PriceMax <= request.PriceMin ||
                 stay.Price >= request.PriceMin) &&

                (request.Currency == null || stay.Currency.EqualsIgnoreCase(request.Currency))
            )
            .AsQueryable();

        if (request.PriceMax is > 0)
        {
            return stays.OrderBy(stay => stay.Price);
        }

        if (request.PriceMin is > 0)
        {
            return stays.OrderByDescending(stay => stay.Price);
        }

        return stays;
    }
}