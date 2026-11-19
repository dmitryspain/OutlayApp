# 002 · Перехід бекенду з .NET 8 на .NET 10 LTS

- **Статус:** accepted 2026-10-19. Власник обрав варіант **A**: переходимо на .NET 10 зараз.
- **Дата:** запропоновано й прийнято 2026-10-19. Дані перевірено того ж дня: release-metadata .NET, NuGet, документація Microsoft.
- **Автор:** project-manager
- **Картки:**
  - OUT-012 (власник): оновити SDK і рантайми. Потрібна за будь-якого варіанта, можна робити вже зараз;
  - OUT-011 (coder): міграція, лише за варіанта A.
- **Привід:** зауваження tester до OUT-008 (2026-10-19).

## 1. Контекст

- **Стан бекенду.** Усі 5 проєктів на `net8.0`. `global.json` має `8.0.0` + `rollForward: latestMajor` + `allowPrerelease: true`, тобто збирає найновіший SDK, навіть preview.
- **Строки підтримки** (офіційні release-metadata):
  - .NET 8 — до **2026-11-10**, тобто ~3 тижні від дати рішення. Після цього патчів безпеки не буде;
  - .NET 9 закінчується тоді ж;
  - .NET 10 — LTS, підтримка до **2028-11-14**.
- **Локальні рантайми застарілі вже зараз.**
  - Рантайм .NET 8 і ASP.NET Core 8 — **8.0.13** (лютий 2025), а актуальний **8.0.32** (2026-10-13, security). Тобто бекенд працює без ~19 патчів.
  - `dotnet list package --vulnerable` shared framework не перевіряє, тож аудит цього не показує.
  - SDK 10 встановлено як 10.0.100 з рантаймом 10.0.0. Актуальний SDK — 10.0.402 з рантаймом 10.0.13.
- **Експозиція.** Бекенд працює лише на Маку власника і доступний через приватний tailnet. Ризик реальний, але не P0.
- **Що дає net10 «безкоштовно»:**
  - `NuGetAuditMode` = `all` за замовчуванням (Microsoft, breaking change SDK 10), тож транзитивні вразливості на кшталт Pkcs в OUT-008 будуть видні в звичайній збірці;
  - package pruning відсікає пакети, які постачає рантайм, тож хибних спрацювань менше.
- **Обсяг коду невеликий:**
  - `Database.Migrate()` не викликається, міграції застосовуються вручну через `dotnet ef database update`;
  - `Microsoft.AspNetCore.OpenApi` 8.0.11 у коді не використовується (немає `AddOpenApi` / `WithOpenApi`), тож його можна прибрати;
  - Swagger іде через Swashbuckle 6.9.0.

## 2. Варіанти

| Варіант | Що робимо | Плюси | Мінуси |
|---|---|---|---|
| **A. .NET 10 зараз** (рекомендую) | OUT-012 + OUT-011 до 10.11.2026, далі смоук власника на реальних даних | підтримка до 2028; аудит транзитивних пакетів; бекенд-репо зараз вільне, тож iOS-робота в UI-репо не гальмує | breaking changes EF Core 9/10 і Npgsql 9/10; тестів на хендлери мало, тож потрібен смоук власника (~15 хв) |
| **B. .NET 8 до кінця MVP iOS** | OUT-012 (рантайм 8.0.32) + `NuGetAuditMode=all` у `Directory.Build.props`; перехід пізніше | мінімум роботи зараз | з 10.11.2026 рантайм без патчів; міграція однаково знадобиться, тільки на непідтримуваному стеку |
| **C. Нічого не робити** | — | — | рантайм 8.0.13 уже без 19 патчів; не рекомендую |

## 3. Рекомендація: A

Бекенд-репо зараз вільне: OUT-008 закрито, а OUT-007 чекає на Tailscale і зачіпає лише README. Робота переважно механічна: TFM, `global.json`, пакети Microsoft.* 8 → 10. Встигаємо до EOL із запасом на перевірку. Контракт API не змінюється, і це доводимо тестами серіалізації DTO.

## 4. Обсяг і план (варіант A)

1. **OUT-012 (власник, зараз).** SDK 10.0.402 (рантайм 10.0.13) + ASP.NET Core Runtime 8.0.32. Другий потрібен, щоб до злиття міграції бекенд працював на пропатченому рантаймі.
2. **OUT-011 (coder):**
   - TFM `net10.0`;
   - `global.json`: `10.0.402`, `rollForward: latestFeature`, `allowPrerelease: false`;
   - Microsoft.* → 10.0.x, Npgsql EF → 10.x;
   - прибрати `Microsoft.AspNetCore.OpenApi`;
   - переглянути піни `System.Security.Cryptography.Xml` і `Pkcs` з урахуванням pruning;
   - тести серіалізації DTO, від яких залежить UI;
   - `dotnet ef migrations has-pending-model-changes`.
3. **Власник:** смоук на реальних даних після OUT-011. У вебі перевірити вхід, синхронізацію, транзакції за період, live (SSE), завантаження історії.

**Свідомо не оновлюємо:**
- **MediatR.** Лишаємо 12.x: лінійка ≤ 12.5.0 під Apache 2.0, а з 13.0 подвійна ліцензія RPL-1.5 або комерційна з ключем.
- **Quartz.** Лишаємо 3.13.x: 4.x — мажорна версія з breaking changes.
- **Swashbuckle.** Лишаємо 6.9.0, якщо Swagger UI працює на net10. Версія 10.x означає Microsoft.OpenApi 2 і зміну коду SecurityScheme в `Program.cs`, це окрема картка за потреби.
- **`Dockerfile`** — поза скоупом, це окрема ідея.

## 5. Ризики варіанта A

| Ризик | Пом'якшення |
|---|---|
| Breaking changes EF Core 9/10 і Npgsql 9/10 | тести, `has-pending-model-changes`, смоук власника |
| Тихі зміни JSON у System.Text.Json | тести серіалізації DTO проти очікувань `api.service.ts` (`toCard`, `toTxns`, `toBackfill`) |
| Мале покриття хендлерів (32 тести, переважно імпорт, безпека, CORS, шифрування) | смоук власника обов'язковий |
| Swashbuckle 6.9 на net10 | перевірити `/swagger` у Development; Swagger — лише dev-інструмент |
| Перетин з OUT-007 у бекенд-репо | послідовно: OUT-011 і OUT-007 не паралелити |

## 6. Питання до власника

Відповідено 2026-10-19: **A**. Порядок такий: OUT-012 (власник оновлює SDK і рантайми) → OUT-011 (міграція) → смоук власника.

## Джерела (перевірено 2026-10-19)

- Release-metadata .NET:
  - https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/8.0/releases.json (`eol-date: 2026-11-10`, latest 8.0.32, SDK 8.0.426);
  - …/9.0/releases.json (`eol-date: 2026-11-10`);
  - …/10.0/releases.json (`eol-date: 2028-11-14`, latest 10.0.13, SDK 10.0.402).
- NuGetAuditMode = all для net10: https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/10.0/nugetaudit-transitive-packages
- Package pruning у .NET 10: https://devblogs.microsoft.com/dotnet/nuget-package-pruning-in-dotnet-10/
- Ліцензія MediatR 13+: https://www.jimmybogard.com/automapper-and-mediatr-commercial-editions-launch-today/
- Актуальні версії на NuGet: EF Core 10.0.13, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3, DataProtection.EntityFrameworkCore 10.0.13, Microsoft.Extensions.Http.Resilience 10.11.0, Azure.Extensions.AspNetCore.Configuration.Secrets 1.5.2, Swashbuckle.AspNetCore 10.3.0, Quartz 4.5.1, MediatR 14.2.0.
