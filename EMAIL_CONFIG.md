# Guide de Configuration Email et Mot de Passe Oublié

## Configuration Email Gmail (Recommandée)

### Étape 1: Activer l'authentification 2FA
1. Allez sur https://myaccount.google.com
2. Sécurité → Authentification à deux facteurs
3. Activez-la

### Étape 2: Générer un mot de passe d'application
1. Allez sur https://myaccount.google.com/apppasswords
2. Sélectionnez "Mail" et "Windows"
3. Google génère un mot de passe (16 caractères)
4. Copiez ce mot de passe

### Étape 3: Configuration
Dans `appsettings.json`:
```json
"Email": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUsername": "votre-email@gmail.com",
  "SmtpPassword": "xxxx xxxx xxxx xxxx",  // Le mot de passe généré
  "SenderEmail": "votre-email@gmail.com",
  "SenderName": "Mboa Green"
}
```

### Étape 4: Configuration Frontend URL
```json
"App": {
  "FrontendUrl": "http://localhost:3000"  // Changez en production
}
```

---

## Configuration pour le Développement Local (Recommandé)

Utilisez MailHog ou MailPit pour tester sans Gmail:

### Option 1: Avec Docker

```bash
docker run -d --name mailpit \
  -p 1025:1025 \
  -p 8025:8025 \
  mailpit/mailpit
```

Puis dans `appsettings.Development.json`:
```json
"Email": {
  "SmtpServer": "localhost",
  "SmtpPort": 1025,
  "SmtpUsername": "dev",
  "SmtpPassword": "dev",
  "SenderEmail": "dev@mboagreen.local",
  "SenderName": "Mboa Green Dev"
}
```

### Option 2: Installation Locale

Téléchargez MailPit depuis: https://github.com/axllent/mailpit/releases

Puis lancez:
```bash
./mailpit
# Accédez à http://localhost:8025 pour voir les emails
```

---

## Endpoints API

### 1. Demander une réinitialisation

**Requête**:
```bash
POST /api/password-reset/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

**Réponse** (200 OK):
```json
{
  "message": "Si cet email est enregistré, un lien de réinitialisation vous sera envoyé"
}
```

**Important**: Le message est générique pour des raisons de sécurité (ne révèle pas si l'email existe).

---

### 2. Réinitialiser le mot de passe

**Requête**:
```bash
POST /api/password-reset/reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "rFiHJ8...token...from...email",
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

**Réponse** (200 OK):
```json
{
  "message": "Mot de passe réinitialisé avec succès"
}
```

**Erreurs Possibles**:
- `400` - Token invalide ou expiré
- `400` - Mot de passe ne respecte pas les critères
- `500` - Erreur serveur

---

### 3. Vérifier la validité d'un token (Optionnel)

**Requête**:
```bash
POST /api/password-reset/verify-token
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "rFiHJ8...token..."
}
```

**Réponse** (200 OK):
```json
{
  "valid": true,
  "message": "Token valide"
}
```

---

## Flux Utilisateur Frontend

### 1. Page "Mot de Passe Oublié"
```jsx
async function requestPasswordReset(email) {
  const response = await fetch('/api/password-reset/forgot-password', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email })
  });
  
  if (response.ok) {
    alert('Vérifiez votre email pour le lien de réinitialisation');
  }
}
```

### 2. Page "Réinitialiser le Mot de Passe"
```jsx
// Récupérer le token et l'email depuis l'URL
const params = new URLSearchParams(window.location.search);
const token = params.get('token');
const email = params.get('email');

// Vérifier le token (optionnel)
async function verifyToken() {
  const response = await fetch('/api/password-reset/verify-token', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ token, email })
  });
  
  return response.ok;
}

// Soumettre le nouveau mot de passe
async function resetPassword(newPassword, confirmPassword) {
  const response = await fetch('/api/password-reset/reset-password', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      email,
      token,
      newPassword,
      confirmPassword
    })
  });
  
  if (response.ok) {
    alert('Mot de passe réinitialisé! Connectez-vous.');
    window.location.href = '/login';
  } else {
    const error = await response.json();
    alert(error.message);
  }
}
```

---

## Critères de Mot de Passe Valide

Le mot de passe doit:
- ✅ Avoir au moins 8 caractères
- ✅ Contenir au moins une majuscule (A-Z)
- ✅ Contenir au moins une minuscule (a-z)
- ✅ Contenir au moins un chiffre (0-9)
- ✅ Contenir au moins un caractère spécial (@$!%*?&)

**Exemples Valides**:
- `MyPassword123!`
- `Secure@Pass2026`
- `Test$ecure456`

**Exemples Invalides**:
- `password` ❌ (pas de majuscule, chiffre, caractère spécial)
- `Pass123` ❌ (pas de caractère spécial)
- `Pass@12` ❌ (7 caractères)

---

## Compilation et Exécution

### Développement

```bash
# Restaurer les dépendances
dotnet restore

# Compiler
dotnet build

# Exécuter
dotnet run
```

L'API sera disponible sur: `http://localhost:5000` (ou selon launchSettings.json)

### Production

```bash
# Publier
dotnet publish -c Release -o ./publish

# Ou avec Docker
docker build -t mboagreen-api:latest .
docker run -p 8080:8080 mboagreen-api:latest
```

---

## Dépannage

### Email non reçu
1. Vérifiez que le service SMTP est configuré
2. Vérifiez les logs pour voir le statut d'envoi
3. Vérifiez le dossier Spam
4. Pour Gmail, assurez-vous que le mot de passe d'application est correct

### Token expiré
Les tokens expirent après 30 minutes. L'utilisateur doit demander une nouvelle réinitialisation.

### Mot de passe non accepté
Vérifiez que le mot de passe respecte tous les critères (voir ci-dessus).

### Erreur CORS
Vérifiez que l'URL du frontend est dans `Cors:AllowedOrigins` dans appsettings.json.

---

## Sécurité

- Les tokens sont générés cryptographiquement avec 32 bytes de données aléatoires
- Les tokens expirent après 30 minutes
- Les mots de passe sont hachés avec PBKDF2 (ASP.NET Core PasswordHasher)
- Les messages d'erreur sont génériques pour ne pas révéler les utilisateurs existants
- Rate limiting: 100 requêtes/minute par client

---

**Version**: 1.0  
**Dernière mise à jour**: 2026-07-09
