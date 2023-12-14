FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["FileLister/FileLister.csproj", "FileLister/"]
RUN dotnet restore "FileLister/FileLister.csproj"
COPY . .
WORKDIR "/src/FileLister"

ARG TITLE_SUFFIX

ENV TITLE_SUFFIX $TITLE_SUFFIX

RUN sed -i "s/TITLE_SUFFIX/${TITLE_SUFFIX}/" Program.cs

RUN dotnet build "FileLister.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FileLister.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileLister.dll"]
