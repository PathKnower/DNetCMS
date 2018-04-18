# Jessie version with huge weight (Develop)
# First (build) stage
FROM microsoft/aspnetcore-build:2.0.6-2.1.104-jessie AS build
MAINTAINER wardef

WORKDIR /app
EXPOSE 80

WORKDIR /src
COPY DNetCMS.sln ./
COPY DNetCMS/DNetCMS.csproj DNetCMS/
RUN dotnet restore
COPY . .
WORKDIR /src/DNetCMS
RUN dotnet build -c Release -o /app

#Second (deploy) stage
FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM microsoft/aspnetcore:2.0.6-jessie AS base
FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN [ "mkdir", "Files" ]

ENTRYPOINT ["dotnet", "DNetCMS.dll"]