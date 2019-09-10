FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
LABEL vendor="AiLECS Lab"
LABEL status="Beta"

MAINTAINER = Janis Dalins 'janis.dalins@afp.gov.au'

# Build runtime image
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "netPDQContainer.dll"]
