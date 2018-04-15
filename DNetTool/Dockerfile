# Jessie version with huge weight (Develop)
FROM microsoft/aspnetcore:2.0.6-jessie AS base
MAINTAINER wardef

WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0.6-2.1.104-jessie AS build
WORKDIR /src
COPY DNetCMS.sln ./
COPY DNetCMS/DNetCMS.csproj DNetCMS/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/DNetCMS
# RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

# RUN [ "mkdir", "Files" ]
# RUN [ "ln" , "-s", "./DNetSettings.json", "Files/DNetSettings.json"]

ENTRYPOINT ["dotnet", "DNetCMS.dll"]