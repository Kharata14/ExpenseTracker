FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY
RUN dotnet restore "./ExpenseTrackerApi.csproj"
COPY..
WORKDIR "/src/."
RUN dotnet build "ExpenseTrackerApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ExpenseTrackerApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish.
ENTRYPOINT