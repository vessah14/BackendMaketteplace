# Améliorations de Sécurité - Backend Mboa Green

## Résumé des Changements

Ce document résume les améliorations de sécurité apportées au backend de l'application Mboa Green.

---

## 1. ✅ Authentification & Autorisation Renforcées

### JWT Amélioré
- **Avant**: Validation minimale (Issuer/Audience désactivés)
- **Après**: 
  - Validation stricte de l'Issuer et Audience
  - Validation de la durée de vie du token
  - ClockSkew = 0 (pas de tolérance)
  - Gestion d'erreurs améliorée

### Configuration JWT
```json
"Jwt": {
  "Key": "Clé min 32 caractères (changez la clé en production!)",
  "Issuer": "MboaGreen",
  "Audience": "MboaGreenUsers",
  "ExpirationMinutes": 1440
}
```

**⚠️ Action Required**: Changez la clé JWT en production!

---

## 2. ✅ Fonction "Mot de Passe Oublié" Complètement Implémentée

### Endpoints Sécurisés

#### 1. POST `/api/password-reset/forgot-password`
```json
{
  "email": "user@example.com"
}
```
- ✅ Génère un token cryptographiquement sécurisé (32 bytes, Base64)
- ✅ Expiration: 30 minutes
- ✅ Ne révèle pas si l'email existe (sécurité)
- ✅ Envoie un email avec lien de réinitialisation

#### 2. POST `/api/password-reset/reset-password`
```json
{
  "email": "user@example.com",
  "token": "token_reçu_par_email",
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```
- ✅ Valide le token et l'expiration
- ✅ Hash le nouveau mot de passe avec PasswordHasher
- ✅ Supprime tous les tokens de l'utilisateur
- ✅ Validation du mot de passe strict:
  - Minimum 8 caractères
  - Contient majuscules, minuscules, chiffres et caractères spéciaux

#### 3. POST `/api/password-reset/verify-token` (Optionnel)
```json
{
  "email": "user@example.com",
  "token": "token_à_vérifier"
}
```
- Permet au frontend de vérifier la validité du token avant de soumettre le formulaire

---

## 3. ✅ Sécurité des Données Transmises

### CORS Sécurisé
- **Avant**: `AllowAnyOrigin()` - Accepte toutes les origines!
- **Après**: Liste blanche configurable
```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:3000",
    "https://yourdomain.com"
  ]
}
```
- ✅ AllowCredentials = true
- ✅ Headers spécifiques exposés

### Headers de Sécurité
Ajoutés automatiquement à chaque réponse:
- `X-Content-Type-Options: nosniff` - Prévient le MIME sniffing
- `X-Frame-Options: DENY` - Prévient le clickjacking
- `X-XSS-Protection: 1; mode=block` - Protection XSS
- `Strict-Transport-Security` - Force HTTPS

### HTTPS
- Redirection automatique HTTP → HTTPS en production
- HSTS activé (1 an)

---

## 4. ✅ Service Email Sécurisé

### Implémentation
Service `IEmailService` pour l'envoi d'emails sécurisé:

```csharp
// Injection dans les contrôleurs
public PasswordResetController(
    AppDbContext context,
    IEmailService emailService,
    ILogger<PasswordResetController> logger,
    IConfiguration config)
```

### Configuration Email (appsettings.json)
```json
"Email": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUsername": "votre-email@gmail.com",
  "SmtpPassword": "votre-mot-de-passe-app",
  "SenderEmail": "noreply@mboagreen.com",
  "SenderName": "Mboa Green"
}
```

### Configuration Gmail (Recommandé)
1. Activez l'authentification 2FA
2. Générez un mot de passe d'application: https://myaccount.google.com/apppasswords
3. Utilisez ce mot de passe dans `SmtpPassword`
4. Port: 587 avec StartTls

### Pour le Développement
Utilisez MailHog (serveur SMTP local):
```bash
docker run -p 1025:1025 -p 8025:8025 mailpit/mailpit
```

Configuration appsettings.Development.json:
```json
"Email": {
  "SmtpServer": "localhost",
  "SmtpPort": 1025,
  "SmtpUsername": "dev",
  "SmtpPassword": "dev",
  "SenderEmail": "dev@mboagreen.local"
}
```

---

## 5. ✅ Rate Limiting

Implémenté pour prévenir les attaques par force brute:
- Limite: 100 requêtes par minute
- S'applique à tous les endpoints

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "fixed", configure: options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
    });
});
```

---

## 6. ✅ Validation des Données

### UserController
- ✅ Validation ModelState
- ✅ Vérification email unique
- ✅ Validation UserType (vendeur/acheteur)
- ✅ Hash du mot de passe avec PasswordHasher

### ConnexionController
- ✅ Validation des champs obligatoires
- ✅ Messages d'erreur génériques (ne révèle pas les utilisateurs existants)
- ✅ Logging des tentatives échouées
- ✅ Gestion des exceptions

### PasswordResetController
- ✅ Validation stricte des mots de passe
- ✅ Tokens expirés supprimés
- ✅ Logging des actions sensibles

---

## 7. ✅ Docker Sécurisé

### Dockerfile Amélioré
- ✅ Utilisateur non-root (appuser, UID 1001)
- ✅ Port changé: 5000 → 8080
- ✅ Environment: Production
- ✅ Multi-stage build (réduit la taille)

```dockerfile
RUN useradd -m -u 1001 appuser
...
USER appuser
```

---

## 8. ✅ Gestion des Erreurs Sécurisée

### Avant
```
return BadRequest("Utilisateur introuvable");  // ❌ Révèle que l'email n'existe pas
return BadRequest("Mot de passe incorrect");  // ❌ Révèle que l'email existe
```

### Après
```csharp
return BadRequest(new { message = "Identifiants incorrects" });  // ✅ Générique
```

**Avantage**: Prévient l'énumération d'utilisateurs

---

## 9. ✅ Logging & Monitoring

### Niveaux de Log
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning"
  }
}
```

### Événements Loggés
- ✅ Tentatives de connexion échouées
- ✅ Demandes de réinitialisation (sans révéler les emails)
- ✅ Erreurs du service email
- ✅ Tokens expirés détectés
- ✅ Accès non autorisés

---

## 10. ✅ Mot de Passe Sécurisé

### Validation Stricte
```csharp
[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
    ErrorMessage = "Le mot de passe doit contenir...")]
```

**Critères**:
- ✅ Minimum 8 caractères
- ✅ Au moins une majuscule
- ✅ Au moins une minuscule
- ✅ Au moins un chiffre
- ✅ Au moins un caractère spécial (@$!%*?&)

### Hachage
- Utilise `PasswordHasher<UserModel>` (ASP.NET Core)
- Algorithme: PBKDF2 avec salt aléatoire
- Jamais stocker les mots de passe en clair

---

## 📋 Configuration Requise

### 1. NuGet Packages
Les packages suivants ont été ajoutés au csproj:
```xml
<PackageReference Include="MailKit" Version="4.7.1" />
<PackageReference Include="AspNetCore.RateLimiting" Version="1.0.0" />
```

### 2. Variables d'Environnement (Production)

```bash
# JWT
export Jwt__Key="your-secret-key-min-32-characters"
export Jwt__Issuer="MboaGreen"
export Jwt__Audience="MboaGreenUsers"

# Email
export Email__SmtpServer="smtp.gmail.com"
export Email__SmtpPort="587"
export Email__SmtpUsername="your-email@gmail.com"
export Email__SmtpPassword="your-app-password"

# CORS
export Cors__AllowedOrigins__0="https://yourdomain.com"

# Frontend URL
export App__FrontendUrl="https://yourdomain.com"
```

### 3. Secrets (Azure Key Vault ou similaire)

Pour la production, utilisez Azure Key Vault ou un gestionnaire de secrets:

```csharp
// Dans Program.cs (optionnel en production)
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = new Uri(builder.Configuration["KeyVault:VaultUri"]);
    var credential = new DefaultAzureCredential();
    builder.Configuration.AddAzureKeyVault(keyVaultUrl, credential);
}
```

---

## 🔐 Checklist de Sécurité

- [ ] Changez la clé JWT en production
- [ ] Configurez le serveur SMTP (Gmail ou autre)
- [ ] Changez les origines CORS autorisées
- [ ] Utilisez HTTPS en production
- [ ] Activez le logging et le monitoring
- [ ] Testez la fonction "mot de passe oublié"
- [ ] Validez les tokens email
- [ ] Mettez à jour les secrets en production
- [ ] Effectuez un audit de sécurité

---

## 🧪 Tests Recommandés

### Test du Flux "Mot de Passe Oublié"

```bash
# 1. Demander la réinitialisation
curl -X POST http://localhost:5000/api/password-reset/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com"}'

# 2. Récupérer le token du lien email
# (Dans MailHog ou la console de log)

# 3. Réinitialiser le mot de passe
curl -X POST http://localhost:5000/api/password-reset/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "email":"user@example.com",
    "token":"token_from_email",
    "newPassword":"NewSecurePass123!",
    "confirmPassword":"NewSecurePass123!"
  }'

# 4. Se connecter avec le nouveau mot de passe
curl -X POST http://localhost:5000/api/connexion/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"NewSecurePass123!"}'
```

---

## 📚 Références de Sécurité

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [Password Security](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

---

## 📞 Support

Pour des questions ou des améliorations de sécurité supplémentaires, contactez l'équipe de développement.

**Date**: 2026-07-09
**Version**: 1.0
