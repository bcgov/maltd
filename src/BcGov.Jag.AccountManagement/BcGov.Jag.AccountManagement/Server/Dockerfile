#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BcGov.Jag.AccountManagement/Server/BcGov.Jag.AccountManagement.Server.csproj", "BcGov.Jag.AccountManagement/Server/"]
COPY ["BcGov.Jag.AccountManagement/Shared/BcGov.Jag.AccountManagement.Shared.csproj", "BcGov.Jag.AccountManagement/Shared/"]
COPY ["BcGov.Jag.AccountManagement/Client/BcGov.Jag.AccountManagement.Client.csproj", "BcGov.Jag.AccountManagement/Client/"]
RUN dotnet restore "BcGov.Jag.AccountManagement/Server/BcGov.Jag.AccountManagement.Server.csproj"
COPY . .
WORKDIR "/src/BcGov.Jag.AccountManagement/Server"
RUN dotnet build "BcGov.Jag.AccountManagement.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BcGov.Jag.AccountManagement.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BcGov.Jag.AccountManagement.Server.dll"]