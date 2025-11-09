# Chattibus Frontend

Interfaccia web moderna per il progetto **Chattibus**, sviluppata con Angular e pensata per integrarsi con il backend
ASP.NET Core esistente. Il focus è su un layout dark mode ispirato a Discord/Slack, modali per autenticazione e chat
realtime via WebSocket.

## Prerequisiti

- Node.js 18+
- npm 9+
- Accesso alle API REST di Chattibus (configurate in `src/environments/*`)

## Installazione e avvio

```bash
npm install
npm start
```

Il comando `npm start` avvia l'applicazione in modalità sviluppo su `http://localhost:4200/`.

Per una build di produzione:

```bash
npm run build
```

## Struttura principale

```
chattibus-frontend/
├── angular.json
├── package.json
├── src/
│   ├── app/
│   │   ├── app.module.ts
│   │   ├── app-routing.module.ts
│   │   ├── core/        # servizi condivisi (Auth, Chat, Friend, WebSocket, guard, interceptor)
│   │   ├── pages/       # pagine: landing, home, friends, chat
│   │   └── shared/      # componenti riutilizzabili: header, chat list/window, modali auth
│   ├── environments/    # configurazioni API/WS
│   ├── main.ts          # bootstrap Angular
│   └── styles.scss      # dark theme globale
└── README.md
```

## Servizi chiave

- **AuthService**: gestisce login, registrazione, logout e persistenza token JWT in `localStorage`.
- **AuthGuard**: protegge le route `/home`, `/friends` e `/chat/:id` reindirizzando gli utenti non autenticati.
- **JwtInterceptor**: aggiunge automaticamente l'header `Authorization: Bearer <token>` a tutte le chiamate HTTP.
- **FriendService**: wrapper per endpoint amici (lista, richieste, accetta/rifiuta).
- **ChatService**: gestione conversazioni e messaggi tramite REST API.
- **WebSocketService**: connessione RxJS a WebSocket per aggiornamenti realtime.

## UI e routing

- `/` landing page con call-to-action e modali login/register.
- `/home` dashboard autenticata con panoramica chat.
- `/friends` gestione lista amici e richieste.
- `/chat/:id` chat 1-to-1 con lista conversazioni, finestra messaggi e composer.

## Configurazione endpoint

Aggiorna i file in `src/environments/` per puntare agli endpoint reali del backend (REST e WebSocket).

## Script aggiuntivi

- `npm run test` – esegue la suite unit test (Karma + Jasmine).
- `npm run format` – formatta i file `.ts`, `.html` e `.scss` tramite Prettier.

## Note

Questo setup è pensato come base portfolio. Estendi pure con state management (NgRx, Akita), internationalization,
component library o test end-to-end in base alle esigenze del progetto.
