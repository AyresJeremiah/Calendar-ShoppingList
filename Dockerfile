FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY GParents.sln .
COPY src/GParents.Server/GParents.Server.csproj src/GParents.Server/
COPY src/GParents.Client/GParents.Client.csproj src/GParents.Client/
COPY src/GParents.Shared/GParents.Shared.csproj src/GParents.Shared/
RUN dotnet restore

COPY src/ src/
RUN dotnet publish src/GParents.Server/GParents.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "GParents.Server.dll"]
