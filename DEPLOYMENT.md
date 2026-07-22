# Plan de déploiement — cesizen-api

## 1. Architecture

cesizen-api est une API REST en **.NET 10** (C#) exposée via un conteneur Docker. Elle utilise une base de données PostgreSQL accessible via PgBouncer (pooling de connexions).

```
Développeur → GitHub (push) → GitHub Actions (CI/CD) → GHCR (registry) → Serveur (docker-compose)
                                                                                    │
                                                                             PostgreSQL + PgBouncer

Client (Web / Mobile) → Cloudflare (edge : TLS, WAF, DDoS) → Cloudflare Tunnel (Zero Trust) → Serveur (docker-compose)
```

En production, l'entrée réseau ne se fait plus par une exposition directe des ports du serveur mais par un **Cloudflare Tunnel** (`cloudflared`), qui établit une connexion sortante depuis le serveur vers Cloudflare : aucun port entrant n'est ouvert publiquement, et la terminaison TLS est assurée en amont par Cloudflare (voir `RGPD.md` section 7 bis pour le détail).

## 2. Environnements

| Environnement | Branche source | Image Docker | Port API | Port DB |
|---|---|---|---|---|
| Développement local | `dev` | — (`dotnet run`) | 3000 | 5432 |
| Préprod | merge sur `dev` | `ghcr.io/cesizen-tma/cesizen-api:preprod` | 3001 | 5433 |
| Production | merge sur `main` | `ghcr.io/cesizen-tma/cesizen-api:prod` | 3000 | 5432 |

### Flux de branches

```
feature/* ──► dev ──► main
                │         │
             preprod     prod
```

## 3. Pipeline CI/CD

### 3.1 CI — Non-régression (`.github/workflows/ci.yml`)

**Déclencheur :** push sur toute branche sauf `main`, pull request vers `dev` ou `main`.

| Étape | Commande | Rôle |
|---|---|---|
| Configuration NuGet | Ajout source privée GitHub Packages | Accès aux packages internes |
| Restore | `dotnet restore` | Récupération des dépendances |
| Build | `dotnet build --no-restore` | Compilation et vérification des types |
| Tests | `dotnet test --no-build` | Tests unitaires et d'intégration |

La pipeline échoue et bloque le merge si l'une des étapes ne passe pas.

> **Analyse statique (SonarCloud)** : ne tourne pas ici. Le dépôt étant privé, le plan gratuit de SonarCloud n'analyse pas les branches/PR (fonctionnalité payante) — le check restait bloqué indéfiniment sur les Pull Requests sans jamais pouvoir passer. L'analyse a donc été déplacée dans `deploy-prod.yml`, qui ne se déclenche que sur push vers `main` : c'est le seul contexte où le plan gratuit affiche réellement un résultat exploitable.

### 3.2 Déploiement préprod (`.github/workflows/deploy-preprod.yml`)

**Déclencheur :** push sur `dev`.

| Étape | Description |
|---|---|
| 1. Tests | Rejeu complet de la CI |
| 2. Build Docker | `docker build` — image .NET 10 multi-stage |
| 3. Push GHCR | `docker push ghcr.io/cesizen-tma/cesizen-api:preprod` |
| 4. *(Manuel)* Déploiement | `docker compose -f docker-compose.preprod.yml pull && up -d` |

### 3.3 Déploiement production (`.github/workflows/deploy-prod.yml`)

**Déclencheur :** push sur `main`.

| Étape | Description |
|---|---|
| 1. Tests + analyse SonarCloud | Rejeu complet de la CI, plus `dotnet-sonarscanner` (bugs, code smells, vulnérabilités, couverture Cobertura) — non bloquant (`sonar.qualitygate.wait` non activé) |
| 2. Build Docker | Image optimisée (multi-stage, runtime only) |
| 3. Push GHCR | Tags `:prod` et `:latest` |
| 4. Tag sémantique | Création automatique d'un tag `vX.Y.Z` (workflow `release.yml`) |
| 5. *(Manuel)* Déploiement | `docker compose -f docker-compose.prod.yml pull && up -d` |

## 4. Versioning sémantique

| Mention dans le message de commit | Effet |
|---|---|
| `#major` | Bump majeur : `1.0.0 → 2.0.0` (changement breaking de l'API) |
| `#minor` | Bump mineur : `1.0.0 → 1.1.0` (nouvelle fonctionnalité) |
| *(aucune mention)* | Bump patch : `1.0.0 → 1.0.1` (correction de bug) |

## 5. Ressources nécessaires

### 5.1 Secrets GitHub

#### Secrets d'organisation (`CESIZEN-tma`)

| Secret | Usage |
|---|---|
| `GH_NUGET_USERNAME` | Authentification NuGet privé GitHub Packages |
| `GH_NUGET_TOKEN` | Token PAT avec scope `read:packages` |
| `GH_SONAR_TOKEN` | Jeton d'authentification SonarCloud (analyse statique + Quality Gate) |

#### Secrets d'environnement `preprod`

| Secret | Description |
|---|---|
| `DATABASE_URL` | Chaîne de connexion PostgreSQL préprod |
| `JWT_SECRET` | Clé de signature JWT préprod |
| `API_KEY` | Clé d'API préprod |

#### Secrets d'environnement `prod`

| Secret | Description |
|---|---|
| `DATABASE_URL` | Chaîne de connexion PostgreSQL production |
| `JWT_SECRET` | Clé de signature JWT production |
| `API_KEY` | Clé d'API production |

### 5.2 Infrastructure serveur

- Docker Engine sur le serveur cible
- Accès GHCR : `docker login ghcr.io`
- Fichiers `.env.preprod` / `.env.prod` (templates dans `cesizen-infra`)
- PostgreSQL + PgBouncer déployés via `cesizen-infra`

### 5.3 Dépendances

| Dépendance | Version | Rôle |
|---|---|---|
| .NET SDK | 10 | Compilation et runtime |
| PostgreSQL | 16 | Base de données |
| PgBouncer | 1.22 | Pooling de connexions |
| Docker | 24+ | Conteneurisation |

## 6. Migrations de base de données

Les migrations Entity Framework sont appliquées **au démarrage du conteneur** :

```csharp
// Program.cs
app.MigrateDatabase(); // applique les migrations en attente
```

En cas de migration bloquante, il est possible de revenir en arrière :

```bash
dotnet ef database update <NomDeLaMigrationPrécédente>
```

## 7. Procédure de rollback

```bash
# Revenir à une version précédente de l'image
docker pull ghcr.io/cesizen-tma/cesizen-api:vX.Y.Z
docker tag ghcr.io/cesizen-tma/cesizen-api:vX.Y.Z ghcr.io/cesizen-tma/cesizen-api:prod
docker compose -f docker-compose.prod.yml up -d cesizen-api

# Rollback de la migration si nécessaire
dotnet ef database update <MigrationCible>
```

## 8. Cohérence avec le projet

| Contrainte | Solution retenue |
|---|---|
| Packages NuGet privés | Source GitHub Packages configurée dans la CI avec token dédié |
| Base de données partagée | PgBouncer limite les connexions simultanées, évite la saturation |
| Équipe réduite | Déploiement manuel post-push pour garder le contrôle des mises en production |
| Conformité CESI | Séparation stricte preprod/prod avec secrets d'environnement distincts |
