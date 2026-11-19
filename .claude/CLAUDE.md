# OutlayApp — бекенд Outlay

Особиста аналітика витрат за картками Monobank. Цей репозиторій — **бекенд** (ASP.NET Core 8 Web API, PostgreSQL).

## Пов'язаний репозиторій: OutlayUI-latest

Цей проєкт напряму працює з фронтендом **OutlayUI-latest**. На цьому Маку він лежить поруч:

- шлях: `/Users/dmitryspain/RiderProjects/OutlayUI-latest` (відносно цього репо: `../OutlayUI-latest`)
- стек: Angular 17 (standalone-компоненти, сигнали, NgRx), Karma/Jasmine
- адреса бекенду в UI: `src/environments/environment.ts` (веб, `https://localhost:7016/api`) і `environment.mobile.ts` (збірка `mobile` для iOS). Решта коду бере її лише через токен `API_BASE_URL` у `src/app/core/config.ts`
- усі HTTP-виклики й маппінг DTO → модель: `src/app/core/api.service.ts`; моделі: `src/app/core/models.ts`
- live-події (SSE): `src/app/core/live.service.ts` ← `GET /api/events`
- в UI є свій `README.md` з архітектурою (FeedService, `aggregate.ts`, теми, i18n). Читай його, коли задача зачіпає фронт.

**Правило контракту.** Будь-яка зміна ендпоінта, параметрів, DTO чи формату (дати, суми, назви полів у JSON) стосується обох репо.
Або оновлюй `api.service.ts` / `models.ts` в UI в межах тієї ж задачі, або явно пиши у звіті «потрібна зміна в UI». Мовчки ламати UI не можна.

Від чого UI залежить зараз:

| Ендпоінт | Де використовується в UI |
|---|---|
| `POST` / `DELETE /api/auth/session` | вхід за токеном Monobank / вихід |
| `GET /api/clients/cards` | список карток |
| `POST /api/clients/balance/refresh` | фонове оновлення балансу |
| `GET` / `POST /api/clients/webhook` | Налаштування → live-оновлення |
| `GET /api/transactions/by-period` | `FeedService`, єдине джерело даних для всіх екранів |
| `POST /api/transactions/latest` | синхронізація |
| `GET` / `POST /api/transactions/backfill` | завантаження історії |
| `GET /api/events?cardId=&access_token=` | SSE |

`grouped`, `weekly`, `by-description`, `resync` і `clients/me` на бекенді є, але UI їх зараз не викликає: агрегати він рахує сам із `by-period`.

### iOS-застосунок (Capacitor)

Рішення 001 (`.claude/pm/decisions/001-ios-stack.md`): застосунок для iPhone — це той самий Angular-код OutlayUI-latest у нативній оболонці Capacitor. Окремого мобільного репозиторію немає. Збірка `mobile` (OUT-003) і Xcode-проєкт на Capacitor 8.5 (OUT-004) готові. Статус-бар і безпечні зони — OUT-005 (`.claude/pm/backlog.md`).

- **Де лежить:** оболонка в `../OutlayUI-latest/ios/` (Xcode-проєкт, залежності через Swift Package Manager), конфіг у `../OutlayUI-latest/capacitor.config.ts`.
- **Збірка для iOS:** конфігурація Angular `mobile` (`src/environments/environment.mobile.ts`) зі своєю адресою API.
- **Підпис:** безкоштовна Personal Team власника. Профіль діє 7 днів. Push (APNs), TestFlight і App Store недоступні.
- **Зв'язок із бекендом:** iPhone досягає бекенду на Маку через Tailscale Serve (стабільна HTTPS-адреса `*.ts.net`). Quick-тунель cloudflared для телефона не годиться: адреса змінюється при кожному запуску, а SSE він не підтримує.
- **CORS:** WebView застосунку шле запити з origin `capacitor://localhost`. Він має бути в `Cors:Origins`, інакше WebView відкидає всі відповіді API, включно з 401 і SSE.
- **Один код:** будь-яка зміна Angular-UI змінює і веб, і iOS. Платформні відмінності робимо лише через `core/platform.ts` (DI-токени) і атрибут `html[data-native]`.
- **Згенероване:** `ios/App/App/public` і `ios/App/App/capacitor.config.json` створює `cap sync`. Їх не редагуємо й не комітимо.

## Архітектура бекенду

Clean Architecture + CQRS (MediatR), DI через Autofac-модулі.

- `OutlayApp.Domain`: сутності й агрегати (`Client`, `ClientCard`, `ClientTransaction`, `ClientSession`, `LogoReference`), `Result` / `Error`, інтерфейси репозиторіїв, доменні події. Від інфраструктури не залежить.
- `OutlayApp.Application`: команди, запити й хендлери (`ICommand`, `IQuery<T>`, `ICommandHandler`, `IQueryHandler` в `Abstractions/Messaging`), абстракції (`IMonobankClient`, `ITokenProtector`, `ILiveEvents`), `StatementImporter`, `KyivTime`.
- `OutlayApp.Infrastructure`: EF Core + Npgsql (`OutlayContext`, міграції в `Migrations/`), репозиторії, `MonobankClient` + `MonobankRateLimiter`, Quartz (outbox), backfill-воркер, live-події через Postgres `LISTEN/NOTIFY`, Data Protection.
- `OutlayApp.API`: тонкі контролери (`_mediator.Send(...)` → `Ok(result.Value)` / `BadRequest(result.Error)`), `SessionAuthenticationHandler`, `CardOwnershipFilter`, rate limiting, CORS.
- `OutlayApp.Tests`: xUnit. Мок-бібліотек немає, лише ручні фейки (`FakeTransactionRepository`).

## Конвенції

- Хендлер повертає `Result` / `Result<T>`. Помилка: `new Error("Area.Code", "Повідомлення українською.")`. Винятки лише для справді виняткових ситуацій.
- Нова фіча = `XxxCommand` / `XxxQuery` (record) + `XxxHandler` у папці фічі в Application + тонкий екшен у контролері.
- Залежності передаються через конструктор у поля `private readonly _name`.
- Коментарі й XML-doc пишемо англійською, коротко, пояснюємо «чому». Тексти для користувача українською.
- **Час:** зберігаємо й віддаємо в UTC (`timestamptz`). Дата від клієнта без офсету означає київський час (`KyivTime.ToUtc`). `ClientTransaction.Create` кидає виняток, якщо дата не в UTC.
- **Гроші:** у БД і API зберігаємо цілі одиниці валюти (`decimal`). Monobank надсилає копійки, тож при імпорті ділимо на 100.
- **Доступ:** усі ендпоінти за замовчуванням вимагають сесію (fallback policy). Анонімний доступ лише через явний `[AllowAnonymous]`. Клієнт бачить тільки свої картки (`CardOwnershipFilter`).
- **Monobank** дозволяє запит виписки раз на хвилину. Не додавай циклів чи ретраїв, які б'ють у банк в обхід `MonobankRateLimiter`.
- Секрети зберігаються лише в user-secrets або Key Vault, ніколи в `appsettings*.json`.
- Схему БД змінюємо тільки новою EF-міграцією. Старі міграції не редагуємо.

## Команди

```bash
dotnet build
dotnet test                                                   # OutlayApp.Tests, xUnit, кілька секунд
dotnet run --project OutlayApp.API --launch-profile https     # https://localhost:7016, Swagger у Development

# нова міграція
cd OutlayApp.API && ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations add <Name> \
  --context OutlayContext --project ../OutlayApp.Infrastructure --startup-project .
```

UI (у `../OutlayUI-latest`):

```bash
npm start                                               # http://localhost:4200
npx ng test --watch=false --browsers=ChromeHeadless     # юніт-тести
npm run build

# iOS
npm run build:mobile                                    # веб-збірка з адресою API для iOS
npm run ios:sync                                        # build:mobile + npx cap sync ios
npm run ios:run                                         # збірка й встановлення на симулятор або iPhone (див. примітку)
xcodebuild -project ios/App/App.xcodeproj -scheme App -configuration Debug -sdk iphonesimulator \
  -destination 'generic/platform=iOS Simulator' CODE_SIGNING_ALLOWED=NO build   # збірка під симулятор без підпису
xcrun simctl launch --console-pty <device-id> io.github.dmitryspain.outlay       # запуск на симуляторі з консоллю
```

Примітки для iOS:
- **Xcode 27.1 і `ios:run`.** На Xcode 27.1 `ios:run` встановлює застосунок, але завершується помилкою: `native-run` шукає `Simulator.app`, а симулятор тепер називається `DeviceHub.app` (`open -a DeviceHub`). Запускай через `xcrun simctl launch`.
- **Що потрапляє в застосунок.** `ios:run` кладе в застосунок те, що лежить у `dist/outlay-ui`. Тому перед ним запускай `npm run ios:sync`, інакше в застосунок потрапить веб-збірка.
- **Симулятори.** Для перевірок бери iPhone 17 з iOS 27.0: runtime 27.1 тут підтримує лише «iPhone Duo». Мінімальна підтримувана версія — iOS 17 (є runtime 17.5).

## Команда агентів (`.claude/agents/`)

| Агент | Роль |
|---|---|
| `project-manager` | розуміє продукт, придумує й пріоритизує задачі, веде `.claude/pm/backlog.md`, накопичує знання в `.claude/pm/knowledge.md` |
| `coder` | пише продакшн-код рівно під задачу, без тестів і без розширення скоупу |
| `tester` | перевіряє зміну, пише юніт-тести, видає `PASS` / `FAIL` і при `FAIL` повертає пункти для coder |

Картки UI-задач мають поле «Платформа» (`web`, `ios` або `web+ios`): від нього залежить, як coder і tester перевіряють зміну. Картки з «Виконавець: власник» агентам не віддаємо.

Субагенти не можуть викликати один одного, тому цикл веде головна сесія:

1. `project-manager` видає задачу (картка `OUT-NNN`).
2. `coder` отримує картку і повертає звіт зі списком змінених файлів.
3. `tester` отримує картку + звіт coder і повертає `PASS` / `FAIL` / `BLOCKED`.
4. При `FAIL` секцію «Для кодера» передати `coder` дослівно, потім знову `tester`. Максимум 3 кола, далі питати користувача.
5. При `PASS` передати звіти `project-manager`: він закриває задачу, оновлює беклог і записує уроки.

Комітимо лише тоді, коли просить користувач.
