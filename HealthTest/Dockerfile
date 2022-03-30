#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HealthTest/HealthTest.csproj", "HealthTest/"]
RUN dotnet restore "HealthTest/HealthTest.csproj"
COPY . .
WORKDIR "/src/HealthTest"
RUN dotnet build "HealthTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HealthTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthTest.dll"]

RUN export PATH=$PATH:/usr/sbin