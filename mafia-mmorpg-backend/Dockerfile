FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["MafiaMMORPG.Web/MafiaMMORPG.Web.csproj", "MafiaMMORPG.Web/"]
COPY ["MafiaMMORPG.Application/MafiaMMORPG.Application.csproj", "MafiaMMORPG.Application/"]
COPY ["MafiaMMORPG.Infrastructure/MafiaMMORPG.Infrastructure.csproj", "MafiaMMORPG.Infrastructure/"]
COPY ["MafiaMMORPG.Domain/MafiaMMORPG.Domain.csproj", "MafiaMMORPG.Domain/"]
COPY ["Directory.Packages.props", "./"]

RUN dotnet restore "MafiaMMORPG.Web/MafiaMMORPG.Web.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MafiaMMORPG.Web"
RUN dotnet build "MafiaMMORPG.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MafiaMMORPG.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MafiaMMORPG.Web.dll"]
