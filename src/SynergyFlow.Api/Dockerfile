FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

USER app
WORKDIR /src

COPY --chown=app:app ["Directory.Build.props", "."]
COPY --chown=app:app ["Directory.Packages.props", "."]
COPY --chown=app:app ["SynergyFlow.slnx", "."]

COPY --chown=app:app ["src/SynergyFlow.Api/SynergyFlow.Api.csproj", "src/SynergyFlow.Api/"]
COPY --chown=app:app ["src/SynergyFlow.Application/SynergyFlow.Application.csproj", "src/SynergyFlow.Application/"]
COPY --chown=app:app ["src/SynergyFlow.Domain/SynergyFlow.Domain.csproj", "src/SynergyFlow.Domain/"]
COPY --chown=app:app ["src/SynergyFlow.Infrastructure/SynergyFlow.Infrastructure.csproj", "src/SynergyFlow.Infrastructure/"]

RUN dotnet restore "src/SynergyFlow.Api/SynergyFlow.Api.csproj"

COPY --chown=app:app . .

RUN dotnet publish "src/SynergyFlow.Api/SynergyFlow.Api.csproj" \
  -c Release \
  -o ./publish \
  /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER root
RUN apt-get update \
  && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
  && rm -rf /var/lib/apt/lists/*

USER app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /src/publish .

ENTRYPOINT ["dotnet", "SynergyFlow.Api.dll"]
