FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
EXPOSE 80

ENV ASPNETCORE_HTTP_PORTS=80

COPY ./src/Aslanta.Idgen.Api/Aslanta.Idgen.Api.csproj ./Aslanta.Idgen.Api/
RUN dotnet restore ./Aslanta.Idgen.Api/Aslanta.Idgen.Api.csproj

COPY ./src/Aslanta.Idgen.Api ./Aslanta.Idgen.Api
RUN dotnet publish ./Aslanta.Idgen.Api/Aslanta.Idgen.Api.csproj -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

EXPOSE 80
ENV ASPNETCORE_HTTP_PORTS=80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Aslanta.Idgen.Api.dll"]