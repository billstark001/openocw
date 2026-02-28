# ── Stage 1: Build the Vue SPA ───────────────────────────────────────────────
FROM node:22-alpine AS frontend-build
WORKDIR /src/Oocw.Frontend
COPY Oocw.Frontend/package*.json ./
RUN npm ci
COPY Oocw.Frontend/ .
RUN npm run build   # emits to dist/

# ── Stage 2: Build the .NET backend ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
COPY Oocw.Base/           Oocw.Base/
COPY Oocw.Database/       Oocw.Database/
COPY Oocw.Backend/        Oocw.Backend/
WORKDIR /src/Oocw.Backend
RUN dotnet publish -c Release -o /app/publish

# ── Stage 3: Runtime image ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=backend-build /app/publish .
# Copy the compiled SPA into wwwroot so the backend can serve it
COPY --from=frontend-build /src/Oocw.Frontend/dist ./wwwroot

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Oocw.Backend.dll"]
