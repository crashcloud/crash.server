FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env
WORKDIR /src

COPY src/Crash.Server.csproj /src
RUN dotnet restore /src/Crash.Server.csproj
COPY src /src/

RUN dotnet publish /src/Crash.Server.csproj -c Release -f net9.0 --no-restore -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime

WORKDIR /src
COPY --from=build-env /src/out .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Crash.Server.dll"]