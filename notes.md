A:\Kata\BookingKata>

docker build -f Application\BookingApi\Dockerfile --force-rm -t bookingapi .

docker run -e ASPNETCORE_ENVIRONMENT=Production-Admin -it bookingapi /bin/bash 

docker compose up

docker stack deploy -c docker-compose.yml my-db-stack

services:

  booking_admin:
    container_name: booking_admin
    networks:
      - booking_backend
    ports:
      - 5291:8080
    environment:
      - ASPNETCORE_HTTP_PORTS=8080
      - DEMO_MODE=true

