FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY /src/ ./
RUN dotnet restore

# RUN dotnet publish -c Release -o out
RUN dotnet publish -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# --- Установка dockerize ---
ENV DOCKERIZE_VERSION v0.6.1
RUN apt-get update && apt-get install -y wget \
    && wget https://github.com/jwilder/dockerize/releases/download/$DOCKERIZE_VERSION/dockerize-linux-amd64-$DOCKERIZE_VERSION.tar.gz \
    && tar -C /usr/local/bin -xzvf dockerize-linux-amd64-$DOCKERIZE_VERSION.tar.gz \
    && rm dockerize-linux-amd64-$DOCKERIZE_VERSION.tar.gz

WORKDIR /app

# Копируем опубликованные файлы из этапа build
COPY --from=build /app/out .

# Открываем порт, который будет слушать приложение
EXPOSE 8080

# Указываем команду для запуска приложения
ENTRYPOINT ["dotnet", "Potratim.dll"]