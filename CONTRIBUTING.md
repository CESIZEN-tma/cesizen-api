# Guide de contribution — cesizen-api

## 1. Stratégie de branches

```
main          ← production (protégée, merge via PR uniquement)
  └── dev     ← intégration / préprod (protégée, merge via PR uniquement)
        └── feature/<nom>   ← développement d'une fonctionnalité
        └── fix/<nom>        ← correction de bug
        └── chore/<nom>      ← maintenance, dépendances, config
```

**Règles :**
- On ne pousse jamais directement sur `main` ou `dev`.
- Toute modification passe par une Pull Request.
- Une PR doit être approuvée avant d'être mergée.
- La CI doit passer (build + tests) avant tout merge.

## 2. Conventions de commits

Format : `type: description courte`

| Type | Usage |
|---|---|
| `feat` | Nouvelle fonctionnalité ou endpoint |
| `fix` | Correction de bug |
| `chore` | Maintenance (packages, migrations de config) |
| `docs` | Documentation uniquement |
| `refactor` | Refactoring sans changement de comportement |
| `test` | Ajout ou modification de tests |
| `migration` | Ajout ou modification d'une migration EF Core |

**Exemples :**
```
feat: add breathing configuration CRUD endpoints
fix: correct JWT expiration validation
migration: add bookmark table
```

**Versioning sémantique :** pour contrôler le bump de version lors du merge sur `main` :
- `#major` dans le message → v1.0.0 → v2.0.0 (breaking change API)
- `#minor` dans le message → v1.0.0 → v1.1.0 (nouveau endpoint)
- *(défaut)* → bump patch automatique

## 3. Gestion des tickets (GitHub Issues)

### Créer un ticket

Tout bug, évolution ou tâche est tracé dans **GitHub Issues** du repo `cesizen-api`.

**Labels disponibles :**

| Label | Couleur | Usage |
|---|---|---|
| `bug` | Rouge | Comportement incorrect de l'API |
| `enhancement` | Bleu | Nouveau endpoint ou amélioration |
| `chore` | Gris | Maintenance technique |
| `breaking change` | Rouge foncé | Modification incompatible avec les clients |
| `migration` | Orange | Nécessite une migration de base de données |
| `critical` | Rouge foncé | Bloquant, à traiter en priorité |

**Structure d'un ticket :**
```
Titre : [BUG] POST /configurations retourne 500 avec un body vide

Description :
- Endpoint : POST /configurations
- Environnement : preprod / prod / local
- Body envoyé : { ... }
- Réponse reçue : { "status": 500, ... }
- Comportement attendu : 400 Bad Request avec message d'erreur
```

### Workflow d'un ticket

```
Open → In Progress → In Review → Done
```

| Statut | Signification |
|---|---|
| `Open` | Ticket créé, non assigné ou en attente |
| `In Progress` | Assigné à un développeur, branche créée |
| `In Review` | Pull Request ouverte, en attente de relecture |
| `Done` | PR mergée, ticket fermé |

### Lier un ticket à une PR

Dans le corps de la Pull Request :
```
Closes #42
```

## 4. Processus de Pull Request

1. Créer une branche depuis `dev` : `git checkout -b feature/mon-feature`
2. Développer, ajouter les migrations EF si nécessaire
3. Committer et pousser
4. Ouvrir une PR vers `dev`
5. La CI valide : restore → build → tests
6. Revue de code + approbation
7. Merger

> **Attention aux migrations :** toujours vérifier qu'une migration n'est pas destructive (suppression de colonne avec données) avant de merger.

## 5. Commandes utiles

```bash
# Restaurer les dépendances
dotnet restore

# Compiler
dotnet build

# Lancer les tests
dotnet test

# Lancer l'API en développement
dotnet run

# Ajouter une migration EF Core
dotnet ef migrations add NomDeLaMigration

# Appliquer les migrations
dotnet ef database update

# Revenir à une migration précédente
dotnet ef database update NomDeLaMigrationPrécédente
```
