@echo off
CHCP 65001
setlocal
setlocal EnableDelayedExpansion
call %~dp0\colordef


set CONFIGURATION=Debug+Release

set DockerProjectStr=%_Bold%%CONFIGURATION%%_Reset% %_Underline%BookingSolution%_Reset% %_fGreen%♺%_Reset% %_Dim%Docker Compose%_Reset%


cls
echo %DockerProjectStr%  %_fGreen%clean%_Reset%...
echo.
echo ------------------------------------------------------------


echo.
echo %_Dim%Removing dangling images...%_Reset%
for /f "delims=" %%a in ('docker images -f "dangling=true" -q') do docker rmi %%a


echo.
echo %_Dim%Removing BookingSolution images...%_Reset%
for /f "delims=" %%a in ('docker images bookingapi* -q') do docker rmi %%a


echo.
echo %_Dim%Removing BookingSolution Release containers, volumes, networks...%_Reset%
docker-compose -p "bookingsolution_release" down -v

echo.
echo %_Dim%Removing BookingSolution Debug containers, volumes, networks...%_Reset%
docker-compose -p "bookingsolution_debug" down -v
