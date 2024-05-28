# todo

 - if not Production-Admin
	injection of IAdminRepo should be a proxy to ask on the bus
	same for GeoIndexing
	etc...
	- 
 - POST /bus/subscribe not working





# build image

A:\Kata\BookingKata> `cls&&docker build -f Application\BookingApi\Dockerfile --force-rm -t bookingapi .`

# run api -single container
A:\Kata\BookingKata> `docker run -e ASPNETCORE_ENVIRONMENT=Production-Admin -it bookingapi /bin/bash`

# run solution - multiple containers (no rebuild)
A:\Kata\BookingKata> `cls&&docker compose up`
A:\Kata\BookingKata> `cls&&docker compose -f "docker-compose.yml" -f "docker-compose.override.yml" -f "docker-compose.debug.yml" build`
A:\Kata\BookingKata> `cls&&docker compose -f "docker-compose.yml" -f "docker-compose.override.yml" -f "docker-compose.debug.yml" up`

# rebuild+run solution oneliner
A:\Kata\BookingKata> `cls&&docker compose build&&cls&&docker compose up`

# cleanup
list containers
`docker ps --filter "status=running" --filter "ancestor=bookingapi:latest" --filter "label=com.docker.compose.service" --format {{.ID}};{{.Names}}`
list images
`docker images -f "dangling=true" -q`

remove images
 - linux
   `docker rmi $(docker images -f "dangling=true" -q)`
 - windows
   `echo off&for /f "delims=" %A in ('docker images -f "dangling=true" -q ') do docker rmi %A & echo on`

# inspect
A:\Kata\BookingKata> `dive bookingapi`

# troubleshoot

/pgadmin4 # `apk add curl`

/pgadmin4 # `hostname -i`
192.168.128.6

/pgadmin4 # `ping 192.168.128.9`
/pgadmin4 # `ping app001.booking.local`

/pgadmin4 # `curl http://192.168.128.9:8080`


`netstat -anop`
TCPView from SysInternals--

# investigate

??? A:\Kata\BookingKata> `docker stack deploy -c docker-compose.yml my-db-stack`
https://aws.plainenglish.io/deploying-services-on-aws-with-docker-swarm-using-a-docker-stack-for-efficient-management-5cf1830909a8


## nuget repo --> baget
https://netcoregenesis.com/documentation/docs/3.22.0/BaGet_Installation/
`docker compose -f "Application/Docker/nuget/docker-compose.nuget.yml" up`
RUN dotnet nuget push -k welcome -s http://localhost:5555/v3/index.json .\CoreSvc.1.0.2.nupkg


dotnet add package EntityFramework