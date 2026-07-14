# Plan de sécurisation — cesizen-api

> Plan de sécurité global disponible dans [cesizen-infra/SECURITY.md](https://github.com/CESIZEN-tma/cesizen-infra/blob/main/SECURITY.md).
> Ce document couvre les vulnérabilités spécifiques à l'API REST.

---

## 1. Contexte

cesizen-api est une API REST .NET 10 exposant des données personnelles (comptes utilisateurs, historique d'exercices de respiration). Elle constitue le point d'entrée unique de toutes les données de la solution et représente la surface d'attaque la plus critique.

---

## 2. Vulnérabilités spécifiques — API

| ID  | Vulnérabilité                     | P | I | Criticité | Statut         |
|-----|-----------------------------------|---|---|-----------|----------------|
| A01 | Brute force sur `/user/login`     | 3 | 3 | **9**     | ❌ À corriger  |
| A02 | Refresh token non révocable       | 2 | 3 | **6**     | ⚠️ Partiel     |
| A03 | Absence de rate limiting          | 2 | 2 | **4**     | ❌ À corriger  |
| A04 | Secrets potentiels dans les logs  | 2 | 2 | **4**     | ⚠️ À auditer   |
| A05 | Validation insuffisante des DTOs  | 1 | 2 | **2**     | ✅ Mitigé      |
| A06 | Injection SQL                     | 1 | 3 | **3**     | ✅ Mitigé (EF Core) |
| A07 | CORS mal configuré                | 1 | 2 | **2**     | ✅ Configuré   |

---

## 3. Mesures en place

### Authentification
- **JWT** avec access token courte durée + refresh token
- **Clé API** obligatoire sur toutes les requêtes (`x-api-key` header)
- Vérification des rôles (`User`, `Administrator`) sur chaque endpoint protégé

### Protection des données
- Mots de passe hachés avec **bcrypt** (salted)
- Aucune donnée sensible retournée dans les réponses (pas de mot de passe en clair)

### Validation
- Data annotations .NET sur tous les DTOs entrants
- Retour de `400 Bad Request` sur données invalides

### Infrastructure
- Base de données accessible uniquement via le réseau Docker interne (pas exposée publiquement)
- PgBouncer limite le nombre de connexions simultanées
- Conteneur exécuté avec un utilisateur non privilégié (`USER $APP_UID` dans le `Dockerfile`, image de base `mcr.microsoft.com/dotnet/aspnet`) — aucun processus applicatif ne tourne en `root`

### Analyse statique du code (SAST)
- **SonarCloud** intégré dans `.github/workflows/ci.yml` à chaque push et Pull Request : détection des bugs, code smells, vulnérabilités et secrets potentiellement codés en dur, calcul du taux de couverture de tests (Cobertura via `coverlet.collector`)
- Complémentaire à **Trivy** (`deploy-preprod.yml` / `deploy-prod.yml`) qui scanne les CVE connues des dépendances et de l'image Docker : Sonar couvre les vulnérabilités *introduites dans le code métier* (ex. injection, gestion des secrets, mauvaises pratiques cryptographiques) que Trivy ne détecte pas
- Quality Gate actuellement non bloquant (rapport + commentaire de PR), le temps de stabiliser la dette technique existante

---

## 4. Actions correctives prioritaires

### Rate limiting (A01, A03) — CRITIQUE
```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.AddFixedWindowLimiter("global", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
    });
});

// Endpoint login
app.MapPost("/user/login", ...).RequireRateLimiting("login");
```

### Révocation des refresh tokens (A02) — ÉLEVÉ
Stocker les refresh tokens en base de données avec champ `RevokedAt`. Invalider lors du logout.

### Audit des logs (A04) — ÉLEVÉ
S'assurer que les middlewares de logging ne capturent pas les headers `Authorization` ou `x-api-key`.

---

## 5. Procédure de gestion de crise

En cas d'incident sur l'API, se référer à la procédure complète dans [cesizen-infra/SECURITY.md](https://github.com/CESIZEN-tma/cesizen-infra/blob/main/SECURITY.md).

**Actions immédiates spécifiques à l'API :**
```bash
# Arrêter l'API sans affecter la DB
docker compose -f docker-compose.prod.yml stop cesizen-api

# Rollback vers la version stable
docker pull ghcr.io/cesizen-tma/cesizen-api:vX.Y.Z
docker compose -f docker-compose.prod.yml up -d cesizen-api
```
