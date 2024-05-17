A:\Kata\BookingKata>

docker build -f Application\BookingApi\Dockerfile --force-rm -t bookingapi .

docker run -e ASPNETCORE_ENVIRONMENT=Production-Admin -it bookingapi /bin/bash 

docker compose up
