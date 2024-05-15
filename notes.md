A:\Kata\BookingKata>

docker build -f Application\BookingApi\Dockerfile --force-rm -t bookingapi .
docker run -it bookingapi /bin/bash -e ASPNETCORE_ENVIRONMENT=backoffice
docker compose up
