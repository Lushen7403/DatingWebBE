# -------- 1. Build Stage --------
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# 1.1 Copy chỉ file .csproj và restore (tận dụng cache)
COPY ["MatchLoveWeb.csproj", "./"]
RUN dotnet restore "MatchLoveWeb.csproj"

# 1.2 Copy toàn bộ mã nguồn và build + publish
COPY . .
RUN dotnet publish "MatchLoveWeb.csproj" -c Release -o /app/publish

# -------- 2. Runtime Stage --------
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# 2.1 Cho phép ứng dụng lắng nghe cổng mà Render cấp (env PORT)
ENV ASPNETCORE_URLS=http://*:${PORT}

# 2.2 Copy kết quả từ build stage
COPY --from=build /app/publish .

# 2.3 Khởi chạy ứng dụng
ENTRYPOINT ["dotnet", "MatchLoveWeb.dll"]
