FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env


# Build runtime image
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
LABEL vendor="AiLECS Lab"
LABEL status="Beta"

MAINTAINER = Janis Dalins 'janis.dalins@afp.gov.au'
# Prep for PDQ install - imagemagick is dependency for PDQ. 
# Remainder requierd for make
RUN mkdir /facebook
# RUN apt-get update && apt-get -y install \
# build-essential \
# git \
# imagemagick
RUN apt-get update && apt-get -y install \
g++ \
make \
git \
imagemagick

#Install PDQ
RUN git clone https://github.com/facebook/ThreatExchange.git /facebook
WORKDIR /facebook/hashing/pdq/cpp
RUN make

RUN apt-get -y remove \
g++ \
make \
git 

WORKDIR /app
COPY --from=build-env /app/out .

# ENV PATH="/facebook/hashing/pdq/cpp:${PATH}"

ENTRYPOINT ["dotnet", "netPDQContainer.dll"]
