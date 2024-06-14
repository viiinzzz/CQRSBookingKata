#todo
notification history. detect loop

# scripts

A:\Kata\BookingKata>
 - `Application\Docker\clean&&Application\Docker\debug`
 - `Application\Docker\clean`
 - `Application\Docker\debug`
 - `Application\Docker\build`
 - `Application\Docker\run`

# build image
 - image
   A:\Kata\BookingKata> `cls&&docker build -f Application\BookingApi\Dockerfile --force-rm -t bookingapi .`
 - compose
   A:\Kata\BookingKata> `cls&&docker compose build`

# run api -single container

A:\Kata\BookingKata> `docker run -e ASPNETCORE_ENVIRONMENT=Production-Admin -it bookingapi /bin/bash`

# run solution - multiple containers (no rebuild)

 - compose release
 
A:\Kata\BookingKata> `cls&&docker compose up`
	
 - compose debug
 
A:\Kata\BookingKata> `cls&&docker compose -f "docker-compose.yml" -f "docker-compose.override.yml" -f "docker-compose.debug.yml" build`

A:\Kata\BookingKata> `cls&&docker compose -f "docker-compose.yml" -f "docker-compose.override.yml" -f "docker-compose.debug.yml" up`


A:\Kata\BookingKata> `cls&&docker compose -f "docker-compose.yml" -f "docker-compose.override.yml" -f "docker-compose.debug.yml" build&&cls&&docker compose -f "docker-compose.yml" -f "docker-compose.override.yml" -f "docker-compose.debug.yml" up`

# rebuild+run solution oneliner
A:\Kata\BookingKata> `cls&&docker compose build&&cls&&docker compose up`

# cleanup

 - list containers
  `docker ps --filter "status=running" --filter "ancestor=bookingapi:latest" --filter "label=com.docker.compose.service" --format {{.ID}};{{.Names}}`
 
 - list images
  `docker images booking*`
  `docker images -f "dangling=true" -q`
 
 - list volumes
  `docker volume ls  --filter "name=bookingsolution"`

## remove dangling images

 - linux
   `docker rmi $(docker images -f "dangling=true" -q)`
 
 - windows
   `echo off&for /f "delims=" %A in ('docker images -f "dangling=true" -q') do docker rmi %A & echo on`

## remove api images

 - linux
   `docker rmi $(docker images bookingapi* -q)`
 
 - windows
   `echo off&for /f "delims=" %A in ('docker images bookingapi* -q') do docker rmi %A & echo on`

## compose stack cleanup

  `docker-compose -p "bookingsolution_release" down -v`
  `docker-compose -p "bookingsolution_debug" down -v`

## volumes cleanup

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

# split screen
Windows Terminal has split pane in its command palette ctrl+shift+p
a:
cd Kata\BookingKata
docker compose logs -f app
docker compose logs -f demo

\\wsl.localhost\docker-desktop-data\version-pack-data\community\docker\containers

for /R "\\wsl.localhost\docker-desktop-data\version-pack-data\community\docker\containers" %%f in (*-json.log) do echo "%%f"

\\wsl$\docker-desktop-data\data\docker\containers
 %PROGRAMDATA%\docker\containers\[container_ID]\[container_ID]-json.log

A:\Kata\BookingKata> `cls&&docker compose up`
A:\Kata\BookingKata> `wt -d A:\Kata\BookingKata -p "dos" docker compose logs -f app ; split-pane -H -d A:\Kata\BookingKata -p "dos" cmd /k docker compose logs -f demo`
A:\Kata\BookingKata> `wt -d A:\Kata\BookingKata -p "dos" docker compose up ; split-pane -H -d A:\Kata\BookingKata -p "dos" docker compose logs -f app ; split-pane -d A:\Kata\BookingKata -p "dos" cmd /k docker compose logs -f demo`

WSL
tmux
ctr+b "  split active pane vertical
ctrl+b %  split active pane horizontal
ctrl+b x  kill active pane
ctrl+b &  kill tmux
ctrl+b arrow  move active pane
ctrl+b [ updown pageupdown  scroll active pane
cd /mnt/a/Kata/BookingKata&&tmux new-session  'docker compose logs -f app ; bash' \; split-window -h 'docker compose logs -f demo ; bash' 


# Database
db schema compatibility
DbUp
EFCore.TestSupport
Scaffolding

event on messagequeue table to replace check loop

hop calc rather than updated

# dockerfile syntax

https://docs.docker.com/reference/dockerfile/

The ${variable_name} syntax also supports a few of the standard bash modifiers as specified below:

    ${variable:-word} indicates that if variable is set then the result will be that value. If variable is not set then word will be the result.

    ${variable:+word} indicates that if variable is set then word will be the result, otherwise the result is the empty string.

# Debug vs Release

    https://www.kenmuse.com/blog/understanding-dotnet-debug-vs-release/

# Docker bind issue

Symptom: the VHD drive was mapped after docker desktop started the WSL distribution hosting the engine, resulting in a bind failure
Solution: restart the engine, click on the bug icon (troubleshoot) in docker desktop and restart

# logs
Docker Desktop for Windows
\\wsl$\docker-desktop-data\version-pack-data\community\docker\containers\xxx\xxx-json.log

Open-source log management tools
https://signoz.io/blog/open-source-log-management/#signoz

SigNoz
Logstash
Graylog
Fluentd
Syslog-ng
Logwatch
Grafana Loki


https://docs.docker.com/config/containers/logging/fluentd/

https://signoz.io/blog/open-source-log-management/#grafana-loki

https://prometheus.io/docs/introduction/overview/
prometheus good for metrics (numercia time series and microservices monitoring)


# troubleshooting ssh in container

bash
grep Port /etc/ssh/sshd_config
netstat -tpln | egrep '(Proto|ssh)'

cd /etc/init.d
ssh-keygen -A

/usr/sbin/sshd -D

service ssh start

ssh-keygen -q -t rsa -N '' -f ~/.ssh/id_rsa <<<y >/dev/null 2>&1

ls /etc/ssh
cat /etc/ssh/ssh_host_rsa_key

ls ~/.ssh
cat ~/.ssh/id_rsa

/usr/sbin/sshd -D -h ~/.ssh/id_rsa

cat /etc/ssh/sshd_config



Windows
more C:\Users\vinz_\.ssh\known_hosts
ssh-keygen -R '[localhost]:2291'
  REM notworking edit by hand

ssh app@localhost -p 2291 -o "StrictHostKeyChecking no" -o "UserKnownHostsFile nul"

ssh app@localhost -p 2291 -o "StrictHostKeyChecking no" -o "UserKnownHostsFile nul" -o "IdentitiesOnly yes" -i "%DockerSshDir%\id_rsa_debug"

 "pipeProgram": "ssh",
        "pipeArgs": [ "app@localhost -p 2291 -o \" StrictHostKeyChecking no\" -o \"UserKnownHostsFile nul\"" ],
 
https://www.chiark.greenend.org.uk/~sgtatham/putty/latest.htm
C:\Program Files\PuTTY\puttygen.exe

plink app@localhost -P 2291 -pw welcome -batch

asking to store.....


http://www.9bis.net/kitty/index.html#!index.md
choco install kitty
klink app@localhost -P 2291 -pw welcome -auto_store_sshkey -batch

 "pipeProgram": "klink",
        "pipeArgs": [ "app@localhost -P 2291 -pw welcome -auto_store_sshkey -batch" ],

https://phoenixnap.com/kb/ssh-permission-denied-publickey
https://www.cyberciti.biz/faq/linux-bash-exit-status-set-exit-statusin-bash/
 host.docker.internal

 https://github.com/microsoft/MIEngine/wiki/Offroad-Debugging-of-.NET-Core-on-Linux---OSX-from-Visual-Studio


save:
id_rsa
id_rsa.pub
bind to /root/.ssh

ssh-keygen -t rsa -b 4096

~/.ssh/authorized_keys
/etc/ssh/authorized_keys



# Pour installer les composants OpenSSH sur les appareils Windows 11 :

 - Ouvrez Paramètres, sélectionnez Système, puis Fonctionnalités facultatives.

 - Parcourez la liste pour voir si OpenSSH est déjà installé. Si ce n’est pas le cas, sélectionnez Voir les fonctionnalités, puis :
    Recherchez OpenSSH Client, sélectionnez Suivant, puis Installer
    Recherchez Serveur OpenSSH, sélectionnez Suivant, puis Installer

 - Ouvrez l’application de bureau Services. (Sélectionnez Démarrer, tapez services.msc dans la zone de recherche, puis sélectionnez l’application de service ou appuyez sur Entrée.)

 - Dans le volet d’informations, double-cliquez sur OpenSSH SSH Server.

 - Sous l’onglet Général, dans le menu déroulant Type de démarrage, sélectionnez Automatique, puis OK.

 - Pour démarrer le service, sélectionnez Démarrer.


 volumes:
      - type: bind
        source: ./Application/Docker/ssh/windows/authorized_keys
        target: /root/.ssh/authorized_keys
        read_only: true
      - type: bind
        source: ./Application/Docker/ssh/api/id_rsa_api.pub
        target: /root/.ssh/id_rsa.pub
        read_only: true
      - type: bind
        source: ./Application/Docker/ssh/api/id_rsa_api
        target: /root/.ssh/id_rsa
        read_only: true


        https://github.com/dotnet/vscode-csharp/wiki/Attaching-to-remote-processes

        ps -aux

        https://code.visualstudio.com/docs/csharp/debugger-settings

        https://dotnettutorials.net/lesson/asp-net-core-launchsettings-json-file/