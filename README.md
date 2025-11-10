# 🗨️ Chattibus

## 🔍 Descrizione
**Chattibus** è un'app di messaggistica realtime ispirata a **Discord**, **Slack** e **WhatsApp**, sviluppata con un focus su **sicurezza**, **architettura pulita** e **comprensione profonda dei sistemi WebSocket**.

## 🧩 Stack Tecnologico
- **Frontend:** Angular 19 + TailwindCSS  
- **Backend:** .NET 8.0 (C#) + SQLite (espandibile a SQL Server) + EF Core
- **Comunicazione:** WebSocket nativo (senza SignalR)  
- **Autenticazione:** JWT + PBKDF2 con salt & pepper  

## ⚙️ Architettura
Il backend gestisce connessioni WebSocket dirette per la comunicazione in tempo reale.  
Il sistema include:
- Gestione utenti con autenticazione JWT  
- Sistema di amicizie (Users, Friends, FriendRequests)  
- Mapping personalizzato senza dipendenze esterne  
- Logica **DTO**, **Service**, **Clean Architecture** senza over-engineering  
- Middleware centralizzato per la gestione delle eccezioni  

## 🛡️ Sicurezza
Sono state adottate misure per prevenire i principali attacchi web:
- **XSS**, **CSRF**, **IDOR**, **SQL Injection**  
- Limitazione delle dipendenze esterne non necessarie  
- Header HTTP di sicurezza configurati nel backend  
- ho fatto dei penetration test utiizzando OWASP ZAP e il sistema back-end delle mie api è sicuro

## 🎨 Frontend
Il client Angular è organizzato in:
- **Services:** per la comunicazione con l’API e il WebSocket  
- **Componenti modulari** e stilizzazione con **Tailwind CSS**  

## 🚀 Motivazione
Ho scelto di implementare un WebSocket manualmente per comprendere in profondità i meccanismi dietro i sistemi realtime come Discord o Slack, e per ridurre la dipendenza da framework esterni.

---

## 🧰 Come provarlo in locale
```bash
# 🖥️ BACKEND (.NET 8 + EF Core + SQLite)
dotnet user-secrets init
# (consulta usersSecretsExample.json per un esempio completo)
dotnet user-secrets set "JwtSettings:SecretKey" "tua_chiave_segreta_qui"

dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run

# 💻 FRONTEND (Angular 19 + TailwindCSS)
npm install
ng serve
