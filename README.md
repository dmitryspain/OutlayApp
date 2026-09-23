# Outlay
<img width="472" height="338" alt="image" src="https://github.com/user-attachments/assets/f943bfa8-1c3c-481c-83e8-d41b005997a3" />

<img width="3454" height="1910" alt="image" src="https://github.com/user-attachments/assets/38e35885-cfa2-469a-baeb-1556257e5dfb" />

<img width="3454" height="1908" alt="image" src="https://github.com/user-attachments/assets/1f17792b-beee-4657-b26c-6f5d246b4bdf" />

## Running locally

Secrets live in [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), never in `appsettings.json`:

```bash
cd OutlayApp.API
dotnet user-secrets set "ConnectionStrings:Database" "Host=localhost;Port=5432;Database=outlayappapi_db_1;Username=postgres;Password=postgres"
dotnet user-secrets set "GoogleApi:SearchKey" "<key>"          # optional: merchant logos
dotnet user-secrets set "GoogleApi:SearchEngineId" "<id>"
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --context OutlayContext --project ../OutlayApp.Infrastructure --startup-project .
dotnet run --launch-profile https
```

- **Sign-in.** The UI sends the Monobank token once to `POST /api/auth/session` and gets a session token
  (`Authorization: Bearer ots_…`). The Monobank token is stored encrypted (ASP.NET Data Protection, keys in the
  database) and looked up by its SHA-256. Every other endpoint needs the session and only sees the client's own cards.
- **CORS.** Allowed origins come from `Cors:Origins` (`appsettings.Development.json` allows `localhost:4200/4300`).
- **Live updates (optional).** Monobank must reach the API over public https — for development a tunnel works, e.g.
  `cloudflared tunnel --url https://localhost:7016 --no-tls-verify` — then set `Monobank:WebhookBaseUrl` to the tunnel
  URL and `Monobank:WebhookSecret` to any random string, and press "Turn on" in the UI settings. Events reach the
  browser as server-sent events and travel between instances through Postgres `LISTEN/NOTIFY`.
- **History.** `POST /api/transactions/backfill?cardId=…&months=6` loads older statements in the background
  (one request per minute, as the bank allows); `POST /api/transactions/resync?cardId=…&days=31` re-reads recent
  days. Jobs are stored in the database and survive restarts.
- **Time and money.** Dates are stored and returned in UTC (`timestamptz`); dates sent without an offset are read as
  Kyiv time. All amounts and balances are in whole currency units.

```bash
dotnet test   # OutlayApp.Tests
```
