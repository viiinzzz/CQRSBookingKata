FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

ARG BUILD_CONFIGURATION=release

#must have basic network troubleshooting tools
RUN \
  apt-get update -yq \
&& \
  apt-get upgrade -yq \
&& \
  apt-get install -yq \
    curl \
    unzip \
    inetutils-ping \
    net-tools \
    nano \
    sudo \
#    git \
&& true

#in debug configuration we need ssh
RUN \
#  [ "$BUILD_CONFIGURATION" = "debug" ] && \
  apt-get install -yq \
    openssh-server \
&& \
    mkdir -p /var/run/sshd \
&& \
    chmod 755 /var/run/sshd \
&& true

RUN	\
  curl -sSL https://aka.ms/getvsdbgsh | \
    /bin/bash /dev/stdin -v latest -l /vsdbg

RUN \
#  addgroup -S app \
#&& \
#  adduser -S -D app -G app \
#&& \
  echo "root:notwelcome" | chpasswd \
&& \
  echo "app:welcome" | chpasswd \
&& \
  echo "app ALL=(root) NOPASSWD:ALL" > /etc/sudoers.d/user \
&& \
  true

RUN \
  sed -i \
    's/#PasswordAuthentication.*/PasswordAuthentication\ yes/' \
    /etc/ssh/sshd_config \
&& \
  echo "ChallengeResponseAuthentication no" >> /etc/ssh/sshd_config \
&& \
  sed -i \
    's/#PermitRootLogin.*/PermitRootLogin\ no/' \
    /etc/ssh/sshd_config \
&& \
  sed -i \
    's/#PubkeyAuthentication.*/PubkeyAuthentication\ yes/' \
    /etc/ssh/sshd_config \
&& \
  sed -i \
    's/#GSSAPIAuthentication.*/GSSAPIAuthentication\ yes/' \
    /etc/ssh/sshd_config \
&& \
  sed -i \
    's/#GSSAPICleanupCredentials.*/GSSAPICleanupCredentials\ no/' \
    /etc/ssh/sshd_config \
&& \
  true

USER app
WORKDIR /app



FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore1
WORKDIR /src
COPY ["Business/ThirdParty/ThirdParty.csproj", "Business/ThirdParty/"]
COPY ["Business/Business.Common/Business.Common.csproj", "Business/Business.Common/"]
COPY ["Business/ThirdParty.Contract/ThirdParty.Contract.csproj", "Business/ThirdParty.Contract/"]
COPY ["Business/Booking.Shared/Booking.Shared.Contract.csproj", "Business/Booking.Shared/"]
COPY ["Business/Billing/Billing.csproj", "Business/Billing/"]
COPY ["Business/Business.Service/Booking.Contract.csproj", "Business/Business.Service/"]
COPY ["Business/Booking.Admin/Booking.Admin.csproj", "Business/Booking.Admin/"]
COPY ["Business/Booking.Planning/Booking.Planning.csproj", "Business/Booking.Planning/"]
COPY ["Business/Booking.Sales/Booking.Sales.csproj", "Business/Booking.Sales/"]

FROM restore1 AS restore2
COPY ["VinZ/DateTimeHelper/VinZ.Common.DateTimeHelper.csproj", "VinZ/DateTimeHelper/"]
COPY ["VinZ/FakeTime.Contracts/VinZ.Common.FakeTime.Contract.csproj", "VinZ/FakeTime.Contracts/"]
COPY ["VinZ/GeoIndexing.Contract/VinZ.GeoIndexing.Contract.csproj", "VinZ/GeoIndexing.Contract/"]
COPY ["VinZ/PositionHelper/VinZ.Common.PositionHelper.csproj", "VinZ/PositionHelper/"]
COPY ["VinZ/RecordHelper/VinZ.Common.RecordHelper.csproj", "VinZ/RecordHelper/"]
COPY ["VinZ/NumberHelper/VinZ.Common.NumberHelper.csproj", "VinZ/NumberHelper/"]
COPY ["VinZ/RetryHelper/VinZ.Common.RetryHelper.csproj", "VinZ/RetryHelper/"]
COPY ["VinZ/ServerContext.Contract/VinZ.Common.ServerContext.Contract.csproj", "VinZ/ServerContext.Contract/"]
COPY ["VinZ/AnotherConsole/VinZ.Common.AnotherConsole.csproj", "VinZ/AnotherConsole/"]
COPY ["VinZ/StringHelper/VinZ.Common.StringHelper.csproj", "VinZ/StringHelper/"]
COPY ["VinZ/MessageQueue.Client/VinZ.MessageQueue.Client.csproj", "VinZ/MessageQueue.Client/"]
COPY ["VinZ/Paging/VinZ.Common.Paging.csproj", "VinZ/Paging/"]
COPY ["VinZ/Paging.Contract/VinZ.Common.Paging.Contract.csproj", "VinZ/Paging.Contract/"]
COPY ["VinZ/ToolBox/VinZ.Common.ToolBox.csproj", "VinZ/ToolBox/"]
COPY ["VinZ/TransactionHelper/VinZ.Common.DbContext.csproj", "VinZ/TransactionHelper/"]
COPY ["VinZ/DbContext.Contract/VinZ.Common.DbTransaction.csproj", "VinZ/DbContext.Contract/"]
COPY ["VinZ/GeoIndexing/VinZ.GeoIndexing.csproj", "VinZ/GeoIndexing/"]
COPY ["VinZ/HashHelper/VinZ.Common.HashHelper.csproj", "VinZ/HashHelper/"]
COPY ["VinZ/ApiHelper/VinZ.Common.ApiHelper.csproj", "VinZ/ApiHelper/"]
COPY ["VinZ/ProgramHelper/VinZ.Common.ProgramHelper.csproj", "VinZ/ProgramHelper/"]
COPY ["VinZ/ServerContext/VinZ.Common.ServerContext.csproj", "VinZ/ServerContext/"]
COPY ["VinZ/UUIDHelper/VinZ.Common.UUIDHelper.csproj", "VinZ/UUIDHelper/"]
COPY ["VinZ/FakeTime/VinZ.Common.FakeTime.csproj", "VinZ/FakeTime/"]
COPY ["VinZ/MessageQueue/VinZ.MessageQueue.csproj", "VinZ/MessageQueue/"]
COPY ["VinZ/ParsableHelper/VinZ.Common.ParsableHelper.csproj", "VinZ/ParsableHelper/"]
COPY ["VinZ/MessageQueue.Contracts/VinZ.MessageQueue.Contract.csproj", "VinZ/MessageQueue.Contracts/"]
COPY ["VinZ/JsonHelper/VinZ.Common.JsonHelper.csproj", "VinZ/JsonHelper/"]
COPY ["VinZ/MiniAnsi/VinZ.Common.MiniAnsi.csproj", "VinZ/MiniAnsi/"]

FROM restore2 AS restore3
COPY ["Infrastructure/Repository/Booking.Admin.Repository/Booking.Admin.Repository.csproj", "Infrastructure/Repository/Booking.Admin.Repository/"]
COPY ["Infrastructure/Repository/Infrastructure.Repository.Common/Infrastructure.Storage.Common.csproj", "Infrastructure/Repository/Infrastructure.Repository.Common/"]
COPY ["Infrastructure/Repository/Booking.Planning.Repository/Booking.Planning.Repository.csproj", "Infrastructure/Repository/Booking.Planning.Repository/"]
COPY ["Infrastructure/Repository/Booking.Sales.Repository/Booking.Sales.Repository.csproj", "Infrastructure/Repository/Booking.Sales.Repository/"]
COPY ["Infrastructure/Repository/Bus.Repository/Bus.Repository.csproj", "Infrastructure/Repository/Bus.Repository/"]
COPY ["Infrastructure/Repository/GeoIndexing.Repository/GeoIndexing.Repository.csproj", "Infrastructure/Repository/GeoIndexing.Repository/"]
COPY ["Infrastructure/Repository/Money.Repository/Money.Repository.csproj", "Infrastructure/Repository/Money.Repository/"]

COPY ["Infrastructure/Bus/ThirdParty.Bus.Contract/ThirdParty.Bus.Contract.csproj", "Infrastructure/Bus/ThirdParty.Bus.Contract/"]
COPY ["Infrastructure/Bus/ThirdParty.Bus/ThirdParty.Bus.csproj", "Infrastructure/Bus/ThirdParty.Bus/"]
COPY ["Infrastructure/Bus/Billing.Bus.Contract/Billing.Bus.Contract.csproj", "Infrastructure/Bus/Billing.Bus.Contract/"]
COPY ["Infrastructure/Bus/Billing.Bus/Billing.Bus.csproj", "Infrastructure/Bus/Billing.Bus/"]
COPY ["Infrastructure/Bus/Booking.Admin.Bus/Booking.Admin.Bus.csproj", "Infrastructure/Bus/Booking.Admin.Bus/"]
COPY ["Infrastructure/Bus/Booking.Planning.Bus/Booking.Planning.Bus.csproj", "Infrastructure/Bus/Booking.Planning.Bus/"]
COPY ["Infrastructure/Bus/Booking.Sales.Bus/Booking.Sales.bus.csproj", "Infrastructure/Bus/Booking.Sales.Bus/"]

COPY ["Infrastructure/Infrastructure.Common/Bus.Common.csproj", "Infrastructure/Infrastructure.Common/"]
COPY ["Infrastructure/BookingKata.Enterprise/Enterprise.Repository/Booking.Enterprise.Storage.csproj", "Infrastructure/BookingKata.Enterprise/Enterprise.Repository/"]
COPY ["Infrastructure/BookingKata.Enterprise/Enterprise.Bus/Booking.Enterprise.Network.csproj", "Infrastructure/BookingKata.Enterprise/Enterprise.Bus/"]

FROM restore3 AS restore
COPY ["Application/BookingApi/BookingApi.csproj", "Application/BookingApi/"]
RUN dotnet restore "./Application/BookingApi/BookingApi.csproj"



FROM restore AS build
ARG BUILD_CONFIGURATION=release
ARG DOTNET_CLI_TELEMETRY_OPTOUT=1
COPY . .
WORKDIR "/src/Application/BookingApi"
RUN dotnet build "./BookingApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "./BookingApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR  "/etc/ssh/"
RUN ssh-keygen -A

RUN \
  /bin/bash -c \
    "ssh-keygen -q -t rsa -N '' -f ~/.ssh/id_rsa <<<y >/dev/null 2>&1" \
&& \
  (([ ! -f /home/app/.ssh/id_rsa ] && exit 2) || true)

EXPOSE 22

WORKDIR "/app"
COPY --from=publish /app/publish .
WORKDIR "/src"
#COPY ["./Application/Docker/bookingapi/config/appsettings.*.json", "/app"]
WORKDIR "/app"
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080

#RUN \
#  apt-get clean \
#&& \
#  apt-get autoremove --yes \
#&& \
#  rm -rf /var/lib/{apt,dpkg,cache,log}/ \
#&& true

ENTRYPOINT \
  /usr/sbin/sshd -D -h ~/.ssh/id_rsa \
& \
  dotnet BookingApi.dll
