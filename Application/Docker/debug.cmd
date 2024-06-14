@echo off
CHCP 65001
setlocal
setlocal EnableDelayedExpansion
call %~dp0\colordef


set COMPOSE_FILE=^
docker-compose.yml^
;docker-compose.override.yml^
;docker-compose.debug.yml

set DockerProjectStr=%_Bold%Debug%_Reset% %_Underline%BookingSolution%_Reset% %_fGreen%▸%_Reset% %_Dim%Docker Compose%_Reset%


cls
echo %DockerProjectStr%  %_fGreen%build%_Reset%...
echo.
echo ------------------------------------------------------------
echo.


echo %_Dim%Creating certificates...%_Reset%
set DockerSshDir=.\.ssh
if not exist "%DockerSshDir%" (mkdir "%DockerSshDir%")
if exist "%DockerSshDir%\id_rsa_debug" (del "%DockerSshDir%\id_rsa_debug")
ssh-keygen -f "%DockerSshDir%\id_rsa_debug" -t rsa -b 4096 -q -N ""
:: more "%DockerSshDir%\id_rsa_debug.pub" | ssh-keygen -l -f - > "%DockerSshDir%\authorized_keys"
more "%DockerSshDir%\id_rsa_debug.pub" > "%DockerSshDir%\authorized_keys"
dir /b "%DockerSshDir%"

echo.
echo %_Dim%Building...%_Reset%
docker compose build

:: only show config and stop
:: docker compose config
:: exit /b


cls
echo %DockerProjectStr%  %_fGreen%up%_Reset%...
echo.
echo ------------------------------------------------------------
echo.

docker compose up  --quiet-pull


call Application\Docker\sshdebug
