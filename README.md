## Запуск

1. Убедитесь, что установлен .NET 9 SDK.
2. В корне репозитория выполните:

   dotnet restore
   dotnet build
   dotnet run --project yp.csproj

3. По умолчанию приложение слушает:
   - http://localhost:5256
   - https://localhost:7149

4. Swagger UI доступен по адресу: http://localhost:5256/swagger (или https://localhost:7149/swagger)

## Endpoints

- GET /events — получить список событий
- GET /events/{id} — получить событие по id
- POST /events — создать событие
- PUT /events/{id} — обновить событие
- DELETE /events/{id} — удалить событие
