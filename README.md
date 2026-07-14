# CesiZen — API

[![CI](https://github.com/CESIZEN-tma/cesizen-api/actions/workflows/ci.yml/badge.svg)](https://github.com/CESIZEN-tma/cesizen-api/actions/workflows/ci.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=CESIZEN-tma_cesizen-api&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=CESIZEN-tma_cesizen-api)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=CESIZEN-tma_cesizen-api&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=CESIZEN-tma_cesizen-api)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=CESIZEN-tma_cesizen-api&metric=coverage)](https://sonarcloud.io/summary/new_code?id=CESIZEN-tma_cesizen-api)

API REST de l'application CesiZen, construite avec **ASP.NET Core** (.NET 10) et **PostgreSQL**.

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL accessible (local ou via Docker — voir `postgre-db-cesizen`)
- Serveur SMTP pour l'envoi d'emails

## Installation

```bash
git clone <repo-url>
cd cesizen-api
```

### Configuration

Copiez le fichier d'exemple et remplissez les valeurs :

```bash
cp .env.example .env
```

| Variable | Description |
|---|---|
| `JWT_SECRET` | Clé secrète pour la signature des tokens JWT |
| `DATABASE_CONNECTION_STRING` | Chaîne de connexion PostgreSQL |
| `API_KEY` | Clé d'API requise dans le header `x-api-key` |
| `SMTP_HOST` | Hôte du serveur SMTP |
| `SMTP_EMAIL_SENDER` | Adresse email expéditeur |
| `SMTP_PASSWORD` | Mot de passe SMTP |
| `URL_FRONT` | URL de l'application mobile (ex: `http://localhost:8081`) |
| `URL_BACKOFFICE` | URL du back-office (ex: `http://localhost:5173`) |

Exemple de `DATABASE_CONNECTION_STRING` :
```
Host=localhost;Port=6432;Database=cesizen;Username=postgres;Password=yourpassword
```

### Lancement

```bash
dotnet restore
dotnet run
```

L'API démarre par défaut sur `http://localhost:5027`.

### Base de données

Le schéma est géré par les scripts SQL du repo `postgre-db-cesizen`. Assurez-vous que la base est initialisée avant de lancer l'API.

## Structure

```
cesizen-api/
├── CZ.Core/          # Interfaces, DTOs, services partagés
├── CZ.Features/      # Modules métier (Administrators, Users, Quizzes…)
├── CZ.Common/        # Utilitaires (Result pattern, middlewares)
└── api.csproj
```

## Authentification

Toutes les routes sont protégées par :
- **Header `x-api-key`** : clé d'API statique
- **Bearer JWT** : token obtenu via `POST /api/auth/login`
