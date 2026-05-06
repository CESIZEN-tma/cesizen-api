# Conformité RGPD — CesiZen

## 1. Responsable du traitement

| Champ | Valeur |
|---|---|
| Organisation | CESI (projet étudiant) |
| Contexte | Application de bien-être mental / gestion du stress |
| Finalité principale | Permettre aux utilisateurs de pratiquer des exercices de respiration guidée |

---

## 2. Inventaire des données personnelles collectées

### 2.1 Données de compte

| Donnée | Obligatoire | Finalité | Base légale |
|---|---|---|---|
| Adresse email | Oui | Identifiant de connexion, communication | Exécution du contrat |
| Mot de passe (haché) | Oui | Authentification | Exécution du contrat |
| Rôle utilisateur | Oui | Gestion des accès (User / Administrator) | Exécution du contrat |

### 2.2 Données de session

| Donnée | Obligatoire | Finalité | Base légale | Durée de conservation |
|---|---|---|---|---|
| Access token JWT | Oui | Authentification des requêtes | Intérêt légitime | 15 minutes (expiration automatique) |
| Refresh token | Oui | Renouvellement de session | Intérêt légitime | 7 jours (ou jusqu'à déconnexion) |

### 2.3 Données d'usage

| Donnée | Obligatoire | Finalité | Base légale |
|---|---|---|---|
| Configurations de respiration créées | Non | Personnalisation de l'expérience | Exécution du contrat |
| Favoris (bookmarks) | Non | Accès rapide aux configurations | Consentement implicite |

### 2.4 Données NON collectées

- Localisation géographique
- Données de santé (les exercices de respiration ne constituent pas des données médicales)
- Données de paiement
- Contacts / carnet d'adresses
- Identifiants de publicité mobile

---

## 3. Durées de conservation

| Catégorie | Durée | Déclencheur de suppression |
|---|---|---|
| Données de compte | Jusqu'à suppression du compte | Demande utilisateur ou 2 ans d'inactivité |
| Access token | 15 minutes | Expiration automatique |
| Refresh token | 7 jours | Expiration ou déconnexion |
| Configurations personnelles | Jusqu'à suppression du compte | Liées au compte utilisateur |
| Logs applicatifs | 6 mois | Rotation automatique |

---

## 4. Droits des utilisateurs

Conformément au RGPD (articles 15 à 22), les utilisateurs disposent des droits suivants :

| Droit | Description | Mise en œuvre |
|---|---|---|
| **Accès** | Obtenir une copie de ses données | Export via l'API (`GET /user/me`) |
| **Rectification** | Corriger des données inexactes | Modification du profil dans l'application |
| **Effacement** | Supprimer son compte et toutes ses données | `DELETE /user/me` — suppression en cascade |
| **Portabilité** | Recevoir ses données dans un format structuré | Export JSON via l'API |
| **Opposition** | S'opposer à un traitement | Contact direct — pas de traitement à des fins marketing |
| **Limitation** | Suspendre un traitement | Désactivation temporaire du compte |

---

## 5. Sécurité des données

| Mesure | Description |
|---|---|
| Hachage des mots de passe | bcrypt avec salt — les mots de passe ne sont jamais stockés en clair |
| Chiffrement des tokens | JWT signé avec clé secrète — infalsifiable sans la clé |
| Stockage mobile sécurisé | expo-secure-store (Keychain iOS / Keystore Android) |
| Accès base de données | Restreint au réseau Docker interne, jamais exposé publiquement |
| HTTPS | À configurer en production (certificat TLS requis) |
| Secrets d'infrastructure | Gérés via GitHub Secrets, jamais dans le code source |

---

## 6. Sous-traitants (processeurs de données)

| Sous-traitant | Rôle | Localisation | Garanties |
|---|---|---|---|
| GitHub (Microsoft) | Hébergement du code, CI/CD, registry Docker | USA / UE | SCC (Standard Contractual Clauses) |
| Hébergeur serveur | Exécution des conteneurs en production | À définir | Contrat de sous-traitance RGPD requis |

---

## 7. Transferts hors UE

Le code source et les images Docker sont hébergés sur GitHub (Microsoft), avec des garanties contractuelles via les Standard Contractual Clauses (SCC). Les données utilisateurs sont hébergées sur le serveur de production dont la localisation doit être documentée.

---

## 8. Procédure en cas de violation de données

En cas de violation (accès non autorisé à des données personnelles) :

1. **Détection** — Identifier le périmètre et le type de données affectées
2. **Qualification** — Évaluer le risque pour les personnes concernées
3. **Notification CNIL** — Si risque élevé : notifier la CNIL sous **72 heures**
4. **Notification des personnes** — Si risque très élevé : informer directement les utilisateurs concernés
5. **Documentation** — Tenir un registre des violations (article 33.5 RGPD)

**Contact :** En l'absence de DPO désigné (non obligatoire pour ce projet), le responsable technique fait office de point de contact.
