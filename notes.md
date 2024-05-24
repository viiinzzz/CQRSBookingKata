
# build

A:\Kata\BookingKata> cls&&docker build -f Application\BookingApi\Dockerfile --force-rm -t bookingapi .
-or-
A:\Kata\BookingKata> cls&&docker compose build&&cls&&docker compose up

A:\Kata\BookingKata> dive bookingapi

A:\Kata\BookingKata> docker run -e ASPNETCORE_ENVIRONMENT=Production-Admin -it bookingapi /bin/bash 

A:\Kata\BookingKata>


# troubleshoot

/pgadmin4 # apk add curl

/pgadmin4 # hostname -i
192.168.128.6

/pgadmin4 # ping 192.168.128.9
/pgadmin4 # ping app001.booking.local

/pgadmin4 # curl http://192.168.128.9:8080


netstat -anop
TCPView from SysInternals--

# investigate

??? A:\Kata\BookingKata> docker stack deploy -c docker-compose.yml my-db-stack
https://aws.plainenglish.io/deploying-services-on-aws-with-docker-swarm-using-a-docker-stack-for-efficient-management-5cf1830909a8
