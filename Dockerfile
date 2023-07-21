# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env
WORKDIR /src

COPY src/Crash.Server.csproj /src
RUN dotnet restore /src/Crash.Server.csproj
COPY src /src/

RUN dotnet publish /src/Crash.Server.csproj -c Release -f net7.0 --no-restore -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime

WORKDIR /publish
COPY --from=build-env /publish .

EXPOSE 80
EXPOSE 5000
EXPOSE 5001

# https://stackoverflow.com/questions/40272341/how-to-pass-parameters-to-a-net-core-project-with-dockerfile
# /publish/Crash.Server.dll
# /publish/Crash.Server.exe
# /publish/Crash.Server
ENTRYPOINT ["dotnet", "/publish/Crash.Server.dll"]