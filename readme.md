# TP Azure Functions – HTTP → Queue → Table

## Présentation

Ce projet met en place une architecture serverless avec **Azure Functions** et **Azurite** pour simuler Azure Storage en local.

L’objectif est de créer un flux composé de deux fonctions Azure indépendantes :

```text
Client HTTP
    |
    | POST /api/messages
    v
+--------------------+
| PublishMessage     |
| HTTP Trigger       |
+--------------------+
    |
    | Queue Output Binding
    v
+--------------------+
| Queue Storage      |
| tp-messages        |
| Azurite            |
+--------------------+
    |
    | Queue Trigger
    v
+--------------------+
| PersistMessage     |
| Queue Trigger      |
+--------------------+
    |
    | Table Output Binding
    v
+--------------------+
| Table Storage      |
| Messages           |
| Azurite            |
+--------------------+
```

L’architecture respecte le principe de découplage entre les fonctions : la fonction HTTP ne contacte jamais directement la seconde fonction.

La communication entre les deux fonctions se fait uniquement par l’intermédiaire d’une **Azure Storage Queue**.

---

## Architecture

Le traitement se déroule en trois étapes.

### 1. Appel HTTP

Un client envoie une requête HTTP `POST` vers :

```text
http://localhost:7071/api/messages
```

avec un body JSON, par exemple :

```json
{
  "Name": "Alice",
  "Content": "Bonjour Azure Functions"
}
```

La fonction `PublishMessage` reçoit cette requête.

### 2. Publication dans la Queue Storage

`PublishMessage` transforme les données reçues en message puis l’envoie dans la queue :

```text
tp-messages
```

Cette étape utilise un **Queue Output Binding** Azure Functions.

La fonction HTTP reste donc stateless et ne contient pas de logique métier complexe.

### 3. Écriture dans Table Storage

Lorsqu’un message apparaît dans `tp-messages`, la fonction `PersistMessage` est automatiquement déclenchée.

Elle lit le contenu du message puis crée une entrée dans la table :

```text
Messages
```

L’écriture est effectuée grâce à un **Table Output Binding**.

---

# Rôle des fonctions

## PublishMessage

`PublishMessage` est une fonction déclenchée par une requête HTTP.

### Trigger

```text
HTTP Trigger
```

### Rôle

La fonction :

* reçoit une requête HTTP `POST` ;
* lit les données JSON envoyées par le client ;
* génère un identifiant unique pour le message ;
* ajoute une date de réception ;
* publie le message dans la queue `tp-messages`.

Exemple de données reçues :

```json
{
  "Name": "Alice",
  "Content": "Bonjour Azure Functions"
}
```

Le message publié dans la queue contient notamment :

```json
{
  "Id": "identifiant-unique",
  "Name": "Alice",
  "Content": "Bonjour Azure Functions",
  "ReceivedAtUtc": "date"
}
```

La fonction ne connaît pas `PersistMessage` et ne l’appelle jamais directement.

---

## PersistMessage

`PersistMessage` est une fonction déclenchée par Azure Queue Storage.

### Trigger

```text
Queue Trigger
```

Queue surveillée :

```text
tp-messages
```

### Rôle

La fonction :

* se déclenche automatiquement lorsqu’un message arrive dans la queue ;
* lit le contenu du message ;
* désérialise les données ;
* transforme le message en entité Table Storage ;
* écrit les données dans la table `Messages`.

Chaque élément enregistré possède notamment :

```text
PartitionKey
RowKey
Name
Content
ReceivedAtUtc
```

`RowKey` utilise l’identifiant unique généré lors de la réception de la requête HTTP.

---

# Technologies utilisées

Le projet utilise :

* C# ;
* .NET ;
* Azure Functions ;
* Azure Functions Core Tools ;
* Azure Storage Queue ;
* Azure Table Storage ;
* Azurite ;
* Visual Studio Code.

Azurite permet d’émuler Azure Storage localement sans avoir besoin d’utiliser directement des ressources Azure dans le cloud.

---

# Prérequis

Avant de lancer le projet, les outils suivants doivent être installés :

* Visual Studio Code ;
* .NET SDK ;
* Azure Functions Core Tools ;
* extension VS Code **Azure Functions** ;
* extension VS Code **Azurite**.

Pour vérifier .NET :

```bash
dotnet --version
```

Pour vérifier Azure Functions Core Tools :

```bash
func --version
```

---

# Configuration locale

Le projet utilise le fichier :

```text
local.settings.json
```

La configuration du stockage local doit contenir :

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

La valeur :

```text
UseDevelopmentStorage=true
```

indique à Azure Functions d’utiliser Azurite comme stockage local.

Le fichier `local.settings.json` peut être exclu du dépôt Git.

Un fichier `local.settings.example.json` peut être fourni dans le repository afin de montrer la configuration attendue.

---

# Lancer le projet en local

## 1. Ouvrir le projet

Ouvrir le dossier du projet dans Visual Studio Code.

Exemple :

```bash
cd Tp1-AzureFunctions
code .
```

---

## 2. Démarrer Azurite

Dans Visual Studio Code :

```text
Ctrl + Shift + P
```

Puis rechercher :

```text
Azurite: Start
```

Azurite démarre les services locaux de stockage :

```text
Blob Storage  : port 10000
Queue Storage : port 10001
Table Storage : port 10002
```

Les logs Azurite peuvent être consultés dans :

```text
View
→ Output
```

puis dans les sorties :

```text
Azurite Blob
Azurite Queue
Azurite Table
```

---

## 3. Restaurer les dépendances

Dans un terminal à la racine du projet :

```bash
dotnet restore
```

---

## 4. Compiler le projet

```bash
dotnet build
```

La compilation doit se terminer sans erreur.

Exemple :

```text
Build succeeded.
```

---

## 5. Lancer Azure Functions

Toujours à la racine du projet :

```bash
func start
```

Le runtime Azure Functions démarre alors en local.

La fonction HTTP doit être disponible à une adresse similaire à :

```text
http://localhost:7071/api/messages
```

---

# Tester l’application

Une requête HTTP `POST` peut être envoyée avec Postman, Thunder Client, curl ou PowerShell.

Exemple avec PowerShell :

```powershell
Invoke-WebRequest `
  -Method POST `
  -Uri "http://localhost:7071/api/messages" `
  -ContentType "application/json" `
  -Body '{"Name":"Alice","Content":"Bonjour Azure Functions"}'
```

Exemple de body JSON :

```json
{
  "Name": "Alice",
  "Content": "Bonjour Azure Functions"
}
```

---

# Résultat attendu

Après l’appel HTTP, le traitement attendu est :

```text
1. Requête HTTP POST
          ↓
2. PublishMessage
          ↓
3. Message envoyé dans tp-messages
          ↓
4. PersistMessage se déclenche
          ↓
5. Message lu depuis la queue
          ↓
6. Données enregistrées dans la table Messages
```

Les logs doivent afficher l’exécution des deux fonctions.

Exemple :

```text
Executing 'Functions.PublishMessage'
Requête HTTP reçue
Publication du message dans la queue
Executed 'Functions.PublishMessage' (Succeeded)

Executing 'Functions.PersistMessage'
Message reçu depuis la queue
Écriture dans Table Storage
Executed 'Functions.PersistMessage' (Succeeded)
```

---

# Vérification du stockage

Le contenu du stockage local peut être vérifié avec Azure Storage Explorer ou les outils Azure Storage de Visual Studio Code.

Après l’exécution, la table :

```text
Messages
```

doit contenir l’élément correspondant à la requête HTTP envoyée.

La queue `tp-messages` peut apparaître vide après le traitement.

C’est normal : `PersistMessage` récupère automatiquement les messages présents dans la queue et les traite rapidement.

---

# Structure du projet

Exemple de structure :

```text
Tp1-AzureFunctions/
│
├── Models/
│   ├── MessageRequest.cs
│   ├── QueuePayload.cs
│   └── MessageEntity.cs
│
├── PublishMessage.cs
├── PersistMessage.cs
├── Program.cs
├── host.json
├── local.settings.example.json
├── Tp1-AzureFunctions.csproj
├── .gitignore
└── README.md
```

---

# Résumé

Ce projet démontre une architecture événementielle simple avec Azure Functions :

```text
HTTP → Queue Storage → Table Storage
```

Les deux fonctions sont indépendantes.

`PublishMessage` reçoit les données depuis HTTP et les publie dans une queue.

`PersistMessage` est déclenchée automatiquement par cette queue et persiste les données dans Table Storage.

Cette architecture permet de découpler la réception des requêtes HTTP du traitement et de la persistance des données.
