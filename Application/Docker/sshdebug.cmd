@echo off
CHCP 65001
setlocal
setlocal EnableDelayedExpansion
call %~dp0\colordef



set COMPOSE_FILE=^
docker-compose.yml^
;docker-compose.override.yml^
;docker-compose.debug.yml

set CONFIGURATION=Debug

set DockerProjectStr=%_Bold%%CONFIGURATION%%_Reset% %_Underline%BookingSolution%_Reset% %_fGreen%▸%_Reset% %_Dim%Docker Compose%_Reset%

set DockerSshDir=.\.ssh

set DockerSshConnectionOptions=^
-o "StrictHostKeyChecking no" ^
-o "UserKnownHostsFile nul" ^
-o "IdentitiesOnly yes" ^
-i "%DockerSshDir%\id_rsa_debug"


set commandA1=docker compose logs -f app
set commandA2=ssh app@localhost -p 2291 %DockerSshConnectionOptions%

set commandB1=docker compose logs -f demo
set commandB2=ssh app@localhost -p 2292 %DockerSshConnectionOptions%


set WtOptions=-d A:\Kata\BookingKata -p "dos"

set command=wt ^
             %WtOptions%    cmd /k ^
  ^
    %commandA1% ^
  ^
; split-pane %WtOptions% -H cmd /k ^
  ^
    %commandA2% ^
  ^
; move-focus up ^
; split-pane %WtOptions% -V cmd /k ^
  ^
    %commandB1% ^
  ^
; move-focus down ^
; split-pane %WtOptions% -V cmd /k^
  ^
    %commandB2% ^
  ^
;


cls
echo %DockerProjectStr%  %_fGreen%ssh%_Reset%...
echo.
echo ------------------------------------------------------------
echo.
echo %_Dim%command=%command%%_Reset%
echo.

:: docker compose config
:: exit /b

%command%

rem app demo admin planning sales support