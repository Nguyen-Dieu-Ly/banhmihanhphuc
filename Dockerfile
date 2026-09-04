# =========================================
# GIAI ĐOẠN BUILD
# =========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["banhmihanhphuc.csproj", "./"]

RUN dotnet restore "banhmihanhphuc.csproj"

COPY . .

RUN dotnet publish "banhmihanhphuc.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# =========================================
# GIAI ĐOẠN CHẠY
# =========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "banhmihanhphuc.dll"]