# 001 · Стек iOS-застосунку Outlay

- **Статус:** accepted. Власник підтвердив 2026-10-18.
- **Історія:** запропоновано 2026-10-18 → прийнято 2026-10-18 з відповідями на питання 1–4 → того ж дня відповідь на питання 5: Tailscale Serve (§0). Пропозиції для процесу команди (§9) внесено. Версії фреймворків, можливості акаунтів Apple і поведінку Tailscale Serve перевірено 2026-10-17/18.
- **Автор:** project-manager
- **Епік:** Е1 «iOS-застосунок» у `.claude/pm/backlog.md`
- **Відкриті питання:** немає. Підтвердження Bundle ID — це крок власника в OUT-001, а не питання до рішення.

## 0. Рішення власника (2026-10-18)

1. **Стек:** Capacitor + Angular. Рекомендацію підтверджено.
2. **Аудиторія:** лише власник. Застосунок ставиться на свій iPhone з Xcode з безкоштовним Apple ID (Personal Team). Apple Developer Program не купуємо, TestFlight і App Store поза планом.
3. **Бекенд на час MVP:** тунель до Mac.
4. **Оболонка:** варіант B. Папка `ios/` і `capacitor.config.ts` лежать у корені `OutlayUI-latest`, проєкт `outlay-mobile` не створюємо.
5. **Стабільна адреса бекенду для iPhone:** Tailscale Serve, приватна мережа (§6).

**Стан середовища (2026-10-18).**
- **Готово:** Xcode 27.1 (27A9275) встановлено й вибрано через xcode-select, перше налаштування виконано. Є iOS 27.1 SDK і симулятори iOS 27.0, 27.1 та 17.x.
- **Ще ні:** Apple ID у Xcode (потрібен лише для встановлення на iPhone), Tailscale, підтвердження Bundle ID.

**Процес команди.** Пропозиції з §9 внесено в `.claude/CLAUDE.md` і визначення агентів: поля «Виконавець», «Платформа», «Залежить від» та iOS-правила для coder і tester.

**Наслідки для решти документа:**
- розташування й підпис — §3;
- віджети на безкоштовному акаунті — §5.3;
- тунель і push — §6;
- можливості й ліміти Personal Team — §7;
- ризики — §8;
- у дорожній карті етапи «APNs» і «Розповсюдження» позначено «відкладено: потребує Apple Developer Program».

## 1. Контекст

**Запит власника (2026-10-17).** Адаптувати Outlay під iPhone. Стек бажано ближчий до .NET, але це не обов'язково, якщо інший дає вагомі переваги. Застосунок має виглядати дуже схоже на веб-фронт OutlayUI-latest і мати ті самі теми.

**Веб (перевірено в коді).**
- Angular 17, ~9 200 рядків без тестів: стилі 797, чиста логіка 717, сервіси й API 1 591, демо 218, i18n 1 276, `shared` 1 787, сторінки 1 800, лейаут 722. Ще 543 рядки тестів.
- Дизайн-система на CSS-змінних (`tokens.css` → `themes.css` → `base.css` → `components.css`). Компоненти читають тільки змінні.
- 5 тем + «Авто» (Aurora для темної системи, Daylight для світлої). Теми різняться не лише кольорами:
  - Aurora: скло з `backdrop-filter`, анімоване «сяйво», градієнтна кнопка;
  - Ledger: New York, шум паперу через SVG `feTurbulence`, подвійні лінії, small-caps;
  - Brutal: рамки 2.5px, жорсткі тіні, SF Pro Rounded;
  - Terminal: SF Mono, сканлайни, мигаючий курсор.
- Акцент в OKLCH: L і C задає тема, H обирає користувач. Похідні токени будуються через `color-mix(in oklab)`. Палітра графіків `--viz-1…8` окрема для темних і світлих тем. Превʼю тем малюються справжніми токенами.
- Графіки вручну на SVG, i18n uk/en, демо-режим.
- Веб частково готовий до телефона: `viewport-fit=cover`, safe-area знизу, нижня панель. Але на телефоні там **7 вкладок** (HIG радить до 5), а верхній відступ під статус-бар не враховано.
- Бюджети, підписки й сесійний токен `ots_…` зберігаються в localStorage.

**Бекенд.**
- .NET 8, сесії `ots_…` (Bearer) зі строком 180 днів, CORS з `Cors:Origins`, SSE `GET /api/events?access_token=`, вебхук Monobank `api/webhooks/monobank/{secret}`.
- Спільного з будь-яким клієнтом лише ~5 DTO. Уся аналітика рахується на клієнті в TS.

**Середовище (2026-10-17).** Mac arm64, macOS 27.0.1, ~62 ГБ вільно. Xcode **немає**, лише CLT. Є Rider 2026.2.3, Android Studio 2026.1 з Android SDK, .NET SDK 6–10 без workloads, Node 22.13.1.

**Платформа Apple (жовтень 2026).**
- Xcode 27 вийшов 14.09.2026. Потребує macOS 26.6+, налагоджує iOS 17+.
- iOS 27 SDK вимагає UIScene.
- З ~квітня 2027 App Store прийматиме лише збірки з iOS 27 SDK. Нас це не стосується, бо App Store поза планом.
- За даними Ionic, CocoaPods trunk стає read-only 02.12.2026.

## 2. Варіанти й оцінки

Шкала 1–5, де 5 найкраще для Outlay. Критерії:
- **а** — візуальна схожість і перенесення тем;
- **б** — перевикористання коду з Angular і/або .NET;
- **в** — близькість до навичок власника;
- **г** — нативність і доступ до iOS;
- **ґ** — вартість підтримки;
- **д** — зрілість і ризики 2026;
- **е** — Rider на Mac;
- **є** — легкість додати Android.

| Варіант | а | б | в | г | ґ | д | е | є | Σ /40 |
|---|---|---|---|---|---|---|---|---|---|
| **Capacitor поверх нашого Angular** (обрано) | 5 | 5 | 5 | 3 | 5 | 4 | 4 | 5 | **36** |
| React Native / Expo | 3 | 3 | 3 | 5 | 3 | 4 | 3 | 5 | 29 |
| .NET MAUI Blazor Hybrid | 4 | 2 | 4 | 3 | 2 | 2 | 4 | 4 | 25 |
| Flutter | 4 | 1 | 1 | 4 | 2 | 5 | 1 | 5 | 23 |
| .NET MAUI (нативний XAML) | 2 | 1 | 3 | 4 | 2 | 2 | 4 | 4 | 22 |
| Avalonia / Uno Platform (Skia) | 3 | 1 | 3 | 2 | 2 | 2 | 4 | 4 | 21 |
| Нативний SwiftUI | 4 | 1 | 1 | 5 | 1 | 5 | 1 | 1 | 19 |

**Нотатки до оцінок:**
- **Capacitor 8.5.3.** WKWebView з нашим зібраним Angular, нативні можливості через плагіни.
  - Теми ті самі за побудовою. Без змін переходить ~90–95% коду.
  - Плагіни 8.x для Keychain, Face ID, локальних сповіщень, haptics і статус-бару актуальні на жовтень 2026. Віджети пишуться лише на Swift.
  - Версія 8.5 має UIScene для Xcode 27. Capacitor 9 очікується наприкінці листопада 2026.
- **React Native 0.88 / Expo SDK 58.** Нативні UIKit-компоненти. Чисту TS-логіку (~2 200 рядків) можна винести в спільний пакет, але UI і теми переписуємо: CSS-змінних, `oklch()` і `color-mix()` там немає. Власник не пише на React.
- **MAUI Blazor Hybrid.** CSS тем переходить напряму, але шаблони (~4 300 рядків) і логіку (~2 300) переписуємо на C# і ведемо дві UI-бази.
- **.NET MAUI (XAML).** Повне переписування з ручним відтворенням усіх ефектів тем.
  - **Ризики MAUI (стосуються обох MAUI-варіантів):**
    - кожна версія SDK прив'язана до однієї версії Xcode. Підтримка Xcode 27 у .NET 10 вийшла через 15 днів після Xcode 27;
    - MAUI 10 підтримується до 11.05.2027, тож оновлюватись треба щороку;
    - у .NET 11 рантайм змінюється на CoreCLR.
- **Avalonia 12.1.4 / Uno 6.7.30.** Свій рендер, але все пишемо заново, контроли ненативні, а мобільних плагінів мало.
- **Flutter 3.47.** Зрілий, але мова нова (Dart), коду не переходить нічого. Офіційної підтримки в Rider немає.
- **SwiftUI.** Максимальна нативність, але нова мова, працювати лише в Xcode, і Android буде окремим застосунком.
- **PWA**, для порівняння: той самий код, але без Keychain, Face ID і віджетів. Капсула Capacitor — це PWA плюс нативні можливості.

## 3. Рішення: Capacitor поверх наявного Angular + точкові нативні модулі на Swift

**Чому:**
1. «Ті самі теми» гарантовано конструкцією: на телефоні працює той самий `themes.css`.
2. Переходить ~90–95% із ~9 200 рядків.
3. Одна UI-кодова база на веб, iOS і згодом Android.
4. Власник уже пише цей фронт на Angular, у Rider.
5. .NET на мобілці дав би лише ~5 спільних DTO.
6. Перший результат (усі теми на симуляторі) досяжний за 3–4 невеликі картки.

**Як влаштовано (варіант B):**
```
OutlayUI-latest — один репо: Angular + iOS-оболонка
  ├─ src/                   один код UI для вебу й iOS
  ├─ npm run build          → веб, як зараз
  ├─ npm run build:mobile   → dist/outlay-ui → npx cap sync ios
  ├─ capacitor.config.ts    appId = Bundle ID, webDir = dist/outlay-ui, iosScheme = capacitor
  └─ ios/App                Xcode-проєкт (SPM), WKWebView з origin capacitor://localhost, нативні плагіни
OutlayApp (бекенд на Маку): https://<mac>.<tailnet>.ts.net через Tailscale Serve → https+insecure://127.0.0.1:7016
                            + CORS capacitor://localhost; контракт без змін
```
- **Платформні відмінності** йдуть лише через `src/app/core/platform.ts` і DI-токени плагінів, а в CSS через `html[data-native]`.
- **iOS 17+, лише SPM, Capacitor ≥ 8.5.**
- **Підпис:** безкоштовна Personal Team власника. Встановлення з Xcode на свій iPhone, поновлення раз на 7 днів (§7).

## 4. Чому відкинуто інші варіанти

- **MAUI Blazor Hybrid.** Найсильніший .NET-варіант, але це переписування шаблонів і логіки, дві UI-бази й ризики MAUI.
- **MAUI XAML.** Ручне відтворення кожної теми, тобто протилежність головній вимозі.
- **Avalonia / Uno.** Переписування, ненативні контроли, бідна мобільна екосистема.
- **Flutter.** Нова мова, нуль перевикористання, без Rider.
- **React Native.** Другий за сумою, але весь UI доведеться переписати на React.
- **SwiftUI.** Нова мова, лише Xcode, Android окремо.

## 5. Стратегія перенесення тем

**5.1. Принцип.** Джерело правди лишається одне: `src/styles/tokens.css` + `themes.css` + `components.css` і `src/app/core/themes.ts`. WebView рендерить ті самі змінні. Генерація токенів потрібна лише для нативних поверхонь (5.3).

**5.2. Сумісність WebKit на iOS 17+:**

| Можливість | Де в нас | Safari / iOS |
|---|---|---|
| `oklch()`, `color-mix(in oklab)` | акцент, `*-soft`, `--viz-accent` | 16.2+ |
| `@container` | `.bento` | 16+ |
| `:has()` | `.seg` | 15.4+ |
| `<dialog>`, `::backdrop`, `dvh` | палітра, перемикач карток, оболонка | 15.4+ |
| `-webkit-backdrop-filter` | скло Aurora, топбар | так (CSS уже з префіксом) |
| `ui-serif` / `ui-rounded` / `ui-monospace` | Ledger / Brutal / Terminal | New York / SF Pro Rounded / SF Mono |
| View Transitions | «коло» при зміні теми | 18+. На 17 спрацьовує запасний шлях у `prefs.service.ts` |
| `text-wrap: balance` | заголовки | 17.5+, прогресивне покращення |

**5.3. Нативні поверхні в тон теми:**
- **Статус-бар** (OUT-005): стиль тексту за `THEMES[i].scheme`, перемикається разом із темою, у тому числі для «Авто».
- **Launch screen** (етап 1): колір-ассет Any = `#f3f4fa` (Daylight `--bg`), Dark = `#060913` (Aurora `--bg`). Splash ховаємо після першого рендеру.
- **Клавіатура** (етап 2): світла чи темна за `scheme`.
- **Віджет WidgetKit** (етап 4): `OutlayTheme.swift` генерується з `themes.css`, OKLCH → sRGB за формулою браузера. Тему й акцент застосунок передає через App Group.
  - **На безкоштовному акаунті це можливо.** За офіційною таблицею Apple, App Groups і Keychain sharing доступні Personal Team (§7). Окремі сторонні гайди стверджують протилежне, тому перша картка віджета починається з практичної перевірки. Якщо Xcode не дасть App Group, віджет переходить у «відкладено».
- **Іконки під теми** (етап 4): альтернативні іконки окремої capability не потребують.

**5.4. Що адаптуємо на телефоні, не змінюючи тем.** Лише лейаут і жести: 5 вкладок, safe areas, pull-to-refresh, haptics, за потреби полегшений фон Aurora. Кольори, шрифти, рамки й тіні ідентичні вебу.

**5.5. Як гарантуємо паритет:**
1. За побудовою: це та сама збірка.
2. Тест синхронізації `THEMES`/`DEFAULT_HUES` ↔ `themes.css`.
3. Чекліст tester: 5 тем × 4 екрани, веб у WebKit на ширині 390 px проти симулятора (`xcrun simctl io booted screenshot`).
4. Згодом screenshot-baseline у Playwright WebKit.

**5.6. В інших стеках** довелося б завести JSON-токени, генерацію під платформу й ручне відтворення скла, шуму, сканлайнів, жорстких тіней, подвійних ліній, курсора й системних шрифтів.

## 6. Зміни на бекенді

**Для MVP контракт API не змінюється.**

1. **CORS** (OUT-002): `capacitor://localhost` у `Cors:Origins`. Origin не є автентифікацією: доступ і далі лише за Bearer, cookies немає.
2. **Досяжність і стабільна адреса.** На MVP бекенд крутиться на Маку, а телефон ходить через тунель (рішення власника).
   - iOS ATS пропускає лише HTTPS із довіреним сертифікатом. Симулятор бачить `https://localhost:7016`, якщо довіряє dev-сертифікату.
   - **Quick-тунель Cloudflare (`*.trycloudflare.com`) для телефона не годиться.** За документацією Cloudflare: адреса змінюється при кожному запуску, SSE не підтримується, доступність не гарантується, обмеження 200 одночасних запитів. Тим часом OUT-003 вшиває адресу в збірку.
   - **Рішення (питання 5): Tailscale Serve.** Перевірено 2026-10-18:
     - **план:** Personal безкоштовний, до 6 користувачів, необмежено пристроїв. Адреса постійна: `https://<mac>.<tailnet>.ts.net`, сертифікат Let's Encrypt, тож ATS його приймає. Доступ лише з пристроїв tailnet, тому iPhone має бути підключений до Tailscale;
     - **команда:** `tailscale serve --bg https+insecure://127.0.0.1:7016`.
       - Профіль `https` у `launchSettings.json` слухає лише `https://localhost:7016`. Http-порт там тільки в IIS Express, на Маку він не діє.
       - Проксі на http-порт вимагав би змінювати профіль, а `UseHttpsRedirection()` перенаправляв би запити на `:7016` з хостом `*.ts.net`, що зламалося б.
       - `https+insecure` — це TLS до Kestrel без перевірки self-signed dev-сертифіката. Код бекенду при цьому не змінюється.
       - З `--bg` налаштування переживає перезапуск і `tailscale down/up`. Перевірити — `tailscale serve status`, вимкнути — `tailscale serve reset`;
     - **SSE проходить** (перевірено за кодом). Serve проксіює через Go `httputil.ReverseProxy` (`ipn/ipnlocal/serve.go`), а той для `text/event-stream` скидає буфер одразу (`net/http/httputil/reverseproxy.go`, `flushInterval`). У `http.Server` Serve немає Read/Write/Idle-таймаутів, а бекенд шле `: ping` кожні 25 с. Практичну перевірку через `curl -N` робить власник в OUT-006;
     - **варіант для macOS:** Standalone, його рекомендує Tailscale. CLI є в усіх трьох варіантах. Через пісочницю macOS Serve у GUI-варіантах не роздає файли, але для проксі це не перешкода. Funnel працює лише у варіанті open-source `tailscaled`;
     - **налаштування:** в адмінці ввімкнути MagicDNS і HTTPS Certificates. Імена машин потрапляють у публічні CT-логи, тож ім'я Мака не має бути чутливим. Через це ж адреса не секрет, і її можна тримати в `environment.mobile.ts`: доступ дає лише членство в tailnet.
   - **Відкинуті альтернативи:**
     - іменований Cloudflare-тунель потребує домену;
     - поле адреси + quick-тунель не дає SSE, а адресу треба щоразу вводити заново;
     - ngrok free показує сторінку-попередження для браузерного трафіку.
   - **Вебхук Monobank** (live-оновлення) потребує публічної адреси. З приватним Tailscale Serve варіанти такі:
     - Tailscale Funnel лише на шлях `/api/webhooks/monobank`. Funnel доступний на всіх планах, слухає порти 443, 8443 або 10000, Serve і Funnel не можуть ділити один порт, а на macOS потрібен open-source варіант Tailscale;
     - або, як зараз, quick-тунель тільки для вебхука.
   - Без вебхука застосунок працює через синхронізацію за запитом.
   - Телефон працює, лише поки Мак увімкнений, а бекенд і Postgres запущені. Production-деплой зараз не плануємо. Застарілий `Dockerfile` на .NET 7 лишається ідеєю на потім.
3. **Сесія і Keychain.** Сервер без змін (`ots_…`, 180 днів). На клієнті токен переїжджає в Keychain (етап 1), бо WKWebView може очистити localStorage. Пізніше (P3): ковзний строк сесії й назва пристрою.
4. **SSE на iOS** працює, лише поки застосунок на екрані. При поверненні клієнт перепідключається й перезавантажує дані (етап 1).
   - **Push через APNs відкладено: потребує Apple Developer Program.** Push notifications недоступні Personal Team (§7). Заплановані для цього `POST/DELETE /api/devices` і відправник APNs чекають на платний акаунт.
   - **Без APNs на безкоштовному акаунті** (етап 3a):
     - локальні сповіщення (`@capacitor/local-notifications` 8.3.1) окремої capability не потребують. Наприклад, нагадування про завтрашнє списання підписки за датами з `recurring.ts`;
     - фонове оновлення (`@capacitor/background-runner` 3.0.0; Background modes доступні) з локальним сповіщенням про нові транзакції. Коли запускати, вирішує iOS, тож це лише «best effort»;
     - Time Sensitive notifications недоступні.
5. **Бюджети на сервері** (ідея). Поки бюджети локальні, телефон і веб мають різні.

## 7. Передумови й можливості безкоштовного акаунта

| Що | Хто | Коли |
|---|---|---|
| Xcode 27.x + iOS 27 Simulator, `xcode-select`, ліцензія, `xcodebuild -runFirstLaunch` | власник (OUT-001) | ✅ 2026-10-18: Xcode 27.1, симулятори iOS 27.0, 27.1 і 17.x |
| Bundle ID, наприклад `io.github.dmitryspain.outlay` | власник (OUT-001) | до першого встановлення на iPhone. OUT-004 бере типовий, якщо власник не назве інший |
| Apple ID у Xcode → Personal Team | власник (OUT-001) | лише перед встановленням на iPhone; для симулятора не потрібен |
| iPhone з iOS 17+ і Developer Mode. Після першого встановлення: Settings → General → VPN & Device Management → довірити сертифікат розробника | власник | MVP на пристрої |
| Стабільна HTTPS-адреса бекенду: Tailscale Serve | власник (OUT-006) | MVP на пристрої |
| Apple Developer Program | **не потрібно** (рішення власника). Знадобився б лише для APNs, TestFlight і App Store | — |
| Node ≥ 22 (є 22.13.1). CocoaPods, Flutter і dotnet workloads не потрібні | — | є |

**Personal Team: ліміти (офіційно, Apple «Choosing a Membership», перевірено 2026-10-18).**
- До 10 App ID, кожен живе 7 днів. Застосунок і майбутнє розширення віджета — це 2 ID.
- До 3 пристроїв, реєстрація теж на 7 днів.
- До 3 застосунків на пристрій.
- Профіль діє 7 днів, далі «rebuild and reinstall». Поновлення робиться через Product → Run у Xcode з підключеним або спареним по мережі iPhone.
- Перевстановлення поверх наявного застосунку з тим самим Bundle ID зазвичай зберігає дані. Перевіримо на першому поновленні, а застосунок перед поновленням не видаляємо.

**Personal Team: можливості (офіційна таблиця Apple «Supported capabilities (iOS)», колонка «Apple Developer»; розібрано з HTML 2026-10-18).**
- **Доступно:** App Groups, Keychain sharing, Background modes, Data protection (а також HealthKit, HomeKit, Maps, Inter-App Audio, Wireless Accessory Configuration).
- **Не потребують окремої capability:** Face ID (лише `NSFaceIDUsageDescription`), haptics, локальні сповіщення, альтернативні іконки, власна URL-схема.
- **Недоступно:** Push notifications, Time Sensitive і Communication Notifications, iCloud (усі види), Associated Domains (universal links), Sign in with Apple, Siri, Wallet, Apple Pay, In-App Purchase, App Attest, Network extensions і Personal VPN, NFC, WeatherKit. Розповсюдження (TestFlight, App Store) теж недоступне.

## 8. Ризики

| Ризик | Вплив | Пом'якшення |
|---|---|---|
| WebView відчувається «не нативно» | користувач помітить | HIG-адаптації етапів 1–2, нативні плагіни |
| Продуктивність blur-карток Aurora і довгих списків | ривки й батарея | замір на iPhone, полегшений режим, віртуалізація |
| Зміни платформи: UIScene, Capacitor 9, кінець CocoaPods | збірка ламається після оновлення Xcode | Capacitor ≥ 8.5, лише SPM, оновлення окремою карткою |
| WKWebView очищає localStorage | сесія зникає | токен у Keychain (етап 1) |
| Веб і iOS — один код | зміна UI ламає обидва | tester перевіряє обидві збірки; відмінності лише через `platform.ts` і `data-native` |
| Профіль Personal Team живе 7 днів | щотижня застосунок перестає запускатися | прийнято власником: щотижневе Product → Run, нагадування в календарі; дані зберігаються при встановленні поверх |
| Телефон залежить від увімкненого Мака й тунелю | застосунок без даних, коли Мак вимкнено | прийнятно для MVP; деплой — окреме рішення пізніше |
| Quick-тунель Cloudflare: змінна адреса, без SSE | збірка з неактуальною адресою, немає live-оновлень | вирішено: Tailscale Serve (OUT-006, OUT-007) |
| iPhone без активного Tailscale або Мак у сні | застосунок не бачить бекенд | на iPhone увімкнути VPN On Demand у Tailscale; Мак не присипляти, поки потрібен телефон; демо-режим працює й без бекенду |
| App Groups на Personal Team: офіційно доступні, але сторонні гайди кажуть «ні» | віджет може не отримати даних | практична перевірка першою карткою етапу 4; якщо ні — віджет відкладено |
| Бюджети й підписки локальні | різні цифри на телефоні й у вебі | ідея «бюджети на сервері» |
| Angular 17 без підтримки з травня 2025 | немає security-патчів | окремий епік оновлення |
| Публічний реліз | — | поза планом (рішення власника). Якщо колись знадобиться: Apple 4.2 і 5.1.1(ix), «API для провайдерів послуг» Monobank |

## 9. Наслідки для процесу команди (пропозиції під варіант B; власник погодив, головна сесія внесла 2026-10-18)

- **`.claude/CLAUDE.md`, розділ «iOS-застосунок (Capacitor)» у блоці про OutlayUI-latest:**
  - нативна оболонка лежить в `../OutlayUI-latest/ios`, конфіг у `capacitor.config.ts`, підпис через Personal Team власника;
  - команди: `npm run build:mobile`, `npm run ios:sync` (= build:mobile + `npx cap sync ios`), `npm run ios:run`, `xcodebuild -project ios/App/App.xcodeproj -scheme App -sdk iphonesimulator -destination 'generic/platform=iOS Simulator' CODE_SIGNING_ALLOWED=NO build`;
  - правила: зміна Angular-UI — це зміна і вебу, і iOS; платформні відмінності лише через `core/platform.ts` і `html[data-native]`; `capacitor://localhost` — origin, від якого залежить CORS;
  - згенерованого (`ios/App/App/public`, `capacitor.config.json` в `ios/App/App`) не редагувати й не комітити.
- **`project-manager.md`:** у формат картки додати поля «Залежить від», «Виконавець: coder | власник» і «Платформа: web | ios | web+ios» для `Репо: ui`. Окреме значення `mobile` не потрібне.
- **`coder.md`:** у межах картки дозволено змінювати `../OutlayUI-latest/ios/**` і `capacitor.config.ts`. Перевірка перед здачею для карток з iOS: `npm run ios:sync` і збірка під симулятор.
- **`tester.md`:**
  - для карток з «Платформа: ios / web+ios» перевіряти через `xcodebuild` під симулятор, `xcrun simctl` (встановлення, скріншоти, `ui appearance dark|light`) і Web Inspector;
  - юніт-тести лишаються Jasmine;
  - без Xcode — вердикт BLOCKED;
  - нативний код не редагувати.
- **`.claude/settings.json`:** змін не треба, `../OutlayUI-latest/` уже в `additionalDirectories`.

## 10. Питання до власника

**Відповіді від 2026-10-18 (рішення прийнято):**
1. **Стек** → Capacitor + Angular.
2. **Аудиторія** → лише власник, Xcode + безкоштовний Apple ID. Developer Program не купуємо, TestFlight і App Store поза планом.
3. **Бекенд на MVP** → тунель до Mac.
4. **Оболонка** → `ios/` і `capacitor.config.ts` в OutlayUI-latest, без `outlay-mobile`.

5. **Стабільна адреса бекенду** → Tailscale Serve, приватна мережа (відповідь 2026-10-18). Деталі в §6, кроки в OUT-006 (власник) і OUT-007 (README + адреса в `environment.mobile.ts`).

Відкритих питань немає.

## Джерела (перевірено 2026-10-17/18)

- Capacitor: https://ionic.io/blog/announcing-capacitor-8 · https://ionic.io/blog/capacitor-8-5-released · https://capacitorjs.com/docs/updating/8-0 · https://capacitorjs.com/docs/config · https://capacitorjs.com/docs/apis/status-bar
- Версії пакетів (`npm view`): @capacitor/core та ios 8.5.3, status-bar 8.0.4, local-notifications 8.3.1, background-runner 3.0.0, preferences 8.0.1; react-native 0.88.0; expo 58.0.14; @angular/core 22.2.3. NuGet: Avalonia.iOS 12.1.4, Uno.Sdk 6.7.30, Microsoft.Maui.Controls 10.0.110
- .NET for iOS і MAUI: https://github.com/dotnet/macios/releases · https://dotnet.microsoft.com/en-us/platform/support/policy/maui · https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-11 · https://www.jetbrains.com/help/rider/2026.2/MAUI.html
- Flutter 3.47: https://flutter.dev/blog/whats-new-in-flutter-3-47 · Flutter у Rider: https://github.com/flutter/flutter-intellij/issues/7205
- Xcode 27 / iOS 27 SDK: https://blakecrosley.com/blog/xcode-27-release · https://iphonesoft.fr/2026/09/11/ios-27-developpeurs-soumettre-applications-xcode-27-obligatoire-avril
- Apple, можливості безкоштовного акаунта (таблиця, розібрана з HTML): https://developer.apple.com/help/account/reference/supported-capabilities-ios
- Apple, ліміти Personal Team: https://developer.apple.com/support/compare-memberships/ · довіра сертифікату й поновлення: https://flaviocopes.com/install-iphone-apps/ · https://developer.apple.com/forums/thread/712540
- Apple Review Guidelines (для довідки, якщо колись буде публічний реліз): https://developer.apple.com/app-store/review/guidelines/
- Cloudflare quick-тунелі (немає SSE, адреса змінюється): https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/do-more-with-tunnels/trycloudflare/ · іменований тунель потребує домену: https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/get-started/create-remote-tunnel/
- Tailscale: https://tailscale.com/kb/1312/serve · https://tailscale.com/kb/1242/tailscale-serve · https://tailscale.com/kb/1153/enabling-https · https://tailscale.com/kb/1223/funnel · https://tailscale.com/kb/1065/macos-variants · https://tailscale.com/pricing
- Tailscale Serve, проксі й SSE (код): https://github.com/tailscale/tailscale/blob/main/ipn/ipnlocal/serve.go (`httputil.ReverseProxy`, `https+insecure`, `http.Server` без таймаутів) · https://github.com/golang/go/blob/master/src/net/http/httputil/reverseproxy.go (`flushInterval`: `text/event-stream` скидається одразу)
- ngrok free: https://ngrok.com/docs/pricing-limits/free-plan-limits
- Monobank Open API: https://api.monobank.ua/docs/
- Підтримка CSS у Safari: https://web.dev/web-platform-03-2023/ · https://webkit.org/blog/15443/news-from-wwdc24-webkit-in-safari-18-beta/
