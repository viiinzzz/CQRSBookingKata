@echo off
CHCP 65001
setlocal
setlocal EnableDelayedExpansion
call %~dp0\colordef


set CONFIGURATION=Release

set DockerProjectStr=%_Bold%%CONFIGURATION%%_Reset% %_Underline%BookingSolution%_Reset% %_fGreen%▸%_Reset% %_Dim%Docker Compose%_Reset%


cls
echo %DockerProjectStr%  %_fGreen%up%_Reset%...
echo.
echo ------------------------------------------------------------
echo.


set COMPOSE_FILE=
docker compose up
