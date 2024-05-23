global using System.Collections.Concurrent;
global using System.Dynamic;
global using System.Net;
global using System.Net.Mime;
global using System.Text.RegularExpressions;
global using System.Transactions;
global using System.Text;
global using Booking.API;
global using Booking.API.Infrastructure;
global using Microsoft.AspNetCore.Mvc;
global using Newtonsoft.Json.Linq;
global using cons = AnsiVtConsole.NetCore;

global using VinZ.Common;
global using VinZ.MessageQueue;
global using VinZ.GeoIndexing;


global using BookingKata.Admin;
global using BookingKata.API;
global using BookingKata.Billing;
global using BookingKata.Planning;
global using BookingKata.Sales;
global using BookingKata.ThirdParty;
global using Business.Common;


global using BookingKata.Infrastructure.Network;
global using BookingKata.Infrastructure.Storage;

global using static BookingKata.API.ApiMethods;
global using BookingKata.API.Demo;
global using BookingKata.API.Infrastructure;
global using BookingKata.Infrastructure;
global using BookingKata.Infrastructure.Bus.Sales;
global using BookingKata.Services;
global using BookingKata.Shared;
global using Microsoft.AspNetCore.Diagnostics;
global using Microsoft.AspNetCore.Mvc.Infrastructure;
global using OpenTelemetry.Metrics;
global using Support.Infrastructure.Network;
global using Support.Infrastructure.Storage;
global using static Business.Common.Exceptions;
global using static VinZ.MessageQueue.Const;
