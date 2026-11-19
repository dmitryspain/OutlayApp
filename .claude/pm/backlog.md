# Беклог Outlay

> Веде агент `project-manager`. Формат картки описано в `.claude/agents/project-manager.md`, із полями «Виконавець», «Платформа» і «Залежить від». Нумерація наскрізна (OUT-001, OUT-002, …).
> Нативна iOS-оболонка лежить в OutlayUI-latest (`ios/`), тому її картки мають `Репо: ui`. У картці coder, яка в роботі, змінюю лише статус. У картках власника відмічаю виконані пункти.

## Готовність карток (2026-10-19)

- **Готово:** OUT-002, OUT-003, OUT-004, OUT-008.
- **В роботі:**
  - OUT-005 — coder, UI-репо;
  - OUT-001 — власник: лишились Apple ID і Developer Mode, обидва лише для встановлення на iPhone.
- **Власник може робити зараз:**
  - **OUT-012** — оновити .NET. Вона блокує OUT-011, а до кінця підтримки .NET 8 лишилось кілька тижнів;
  - **OUT-006** — Tailscale Serve.
- **Черга UI-репо** (по одній картці, правило 11): OUT-005 → **OUT-009** → OUT-007 (коли буде OUT-006) → **OUT-010**. Якщо OUT-006 ще не готова, OUT-010 іде раніше за OUT-007.
- **Бекенд-репо:**
  - **OUT-011** — щойно власник виконає OUT-012. Рішення 002 прийнято, варіант A;
  - OUT-011 і OUT-007 не паралелити: обидві змінюють бекенд-репо.
- **Термін:** підтримка .NET 8 закінчується **2026-11-10**.

## Епіки

### Е1 · iOS-застосунок
- **Статус:** у роботі. Рішення 001 прийнято 2026-10-18 (`.claude/pm/decisions/001-ios-stack.md`), відкритих питань немає.
- **Мета:** той самий Outlay на iPhone власника, що виглядає як веб і має ті самі 5 тем + «Авто». Одна UI-кодова база на веб, iOS і згодом Android.
- **Рішення власника:**
  - стек Capacitor + Angular;
  - застосунок лише для себе: встановлення з Xcode з безкоштовним Apple ID (Personal Team), Developer Program не купуємо, TestFlight і App Store поза планом;
  - бекенд на Маку, доступний iPhone через **Tailscale Serve**;
  - оболонка — `ios/` і `capacitor.config.ts` у корені OutlayUI-latest.
- **Рішення PM (2026-10-19, оборотне): лише iPhone.**
  - Шаблон Capacitor увімкнув iPad (`TARGETED_DEVICE_FAMILY = "1,2"`). Ставимо `1`: застосунок лише для iPhone власника, а кожна зайва форма — це ще один набір перевірок.
  - Альбомну орієнтацію лишаємо, її покриває OUT-005.
  - Застосуємо в картці «іконка, launch screen і чистка шаблону».
- **Середовище (2026-10-19):**
  - Xcode 27.1 (27A9275), iOS 27.1 SDK, симулятори iOS 27.0, 27.1 і 17.x. Застосунок симулятора тепер називається **DeviceHub** (`open -a DeviceHub`), Simulator.app немає;
  - Apple ID у Xcode ще не додано, Tailscale не встановлено.
- **Нотатки для tester:**
  - пристрій для перевірок — **iPhone 17 з iOS 27.0**, бо runtime iOS 27.1 тут підтримує лише «iPhone Duo». Мінімальна версія — iPhone 15 з iOS 17.5;
  - `xcrun simctl io booted screenshot` на iPhone 17 / iOS 27.0 працює без відкритого DeviceHub;
  - на runtime 17.5 `webinspectord` при першому підключенні піднімається близько 1,5 хв;
  - до виконання OUT-009 `npm run ios:run` (native-run) встановлює застосунок, але завершується з exit 1. Запускати через `xcrun simctl launch`, перед тим `npm run ios:sync`;
  - UI на симуляторі перевіряють клієнтом протоколу Web Inspector (`webinspectord_sim`), який поки лежить у scratchpad головної сесії (OUT-010 переносить його в репо);
  - API не стартує без Postgres: так задумано, бо `LegacyTokenMigrator` працює до Kestrel. Для runtime-перевірок — `docker compose up -d` в `OutlayApp.API`.
- **Для власника (не блокує, головна сесія вже сказала):**
  - `sudo chown -R 501:20 ~/.npm`, бо в кеші npm є файли root;
  - за бажанням `npx cap telemetry off`.
- **Personal Team (офіційно Apple):**
  - профіль живе 7 днів, далі поновлення через Product → Run;
  - до 10 App ID, до 3 пристроїв, до 3 застосунків на пристрій;
  - **доступно:** App Groups, Keychain sharing, Background modes;
  - **недоступно:** push (APNs), Time Sensitive, iCloud, Associated Domains, Siri, TestFlight, App Store.

**Дорожня карта**

- **Етап 0 · Рішення й передумови**
  - ✅ Рішення 001, питання 1–5.
  - ✅ Процес команди: CLAUDE.md і визначення агентів.
  - OUT-001 (власник): Xcode ✅, Bundle ID ✅ (типовий); Apple ID і Developer Mode — перед встановленням на iPhone.
- **Етап 1 · MVP «Outlay на моєму iPhone»**
  - Результат етапу: застосунок із тими самими темами працює на симуляторі й на iPhone власника з бекендом на Маку через Tailscale.
  - ✅ OUT-002 [backend] CORS і розділ README «Доступ з телефона (iOS)».
  - ✅ OUT-003 [ui · web+ios] збірка `mobile`, адреса API в `src/environments/*`.
  - ✅ OUT-004 [ui · ios] каркас Capacitor-оболонки. Теми збігаються з вебом, iOS 17.5 і 27.0.
  - OUT-005 [ui · web+ios] статус-бар і безпечні зони в тон теми. У роботі.
  - OUT-009 [ui · ios] окрема тека збірки `mobile` + `ios:run` з exit 0 на Xcode 27.1.
  - OUT-006 [власник] Tailscale Serve: стабільна адреса `*.ts.net`.
  - OUT-007 [both · ios] README про Tailscale (англійською) + адреса `*.ts.net` у `environment.mobile.ts` + виправлення README UI.
  - OUT-010 [ui · ios] dev-утиліта для tester: керування WebView на симуляторі через Web Inspector.
  - Далі (картки створю пізніше):
    - [ui · ios] **іконка, launch screen у кольорах «Авто» і чистка шаблону Capacitor**:
      - `TARGETED_DEVICE_FAMILY = 1` (лише iPhone);
      - `UIRequiredDeviceCapabilities`: `armv7` → `arm64`;
      - прибрати `-D COCOAPODS` з `OTHER_SWIFT_FLAGS` (Debug), бо проєкт на SPM;
      - `@UIApplicationMain` → `@main` (Swift 6);
      - splash ховати після першого рендеру;
    - [ui · ios] live reload: `ng serve -c mobile` + `npx cap run ios -l` (див. ідеї);
    - [ui] нижня панель на телефоні: 5 вкладок замість 7. Стосується й мобільного вебу;
    - [ui] сесійний токен у Keychain на iOS (плагін secure storage 8.x) з міграцією з localStorage;
    - [ui] життєвий цикл через `@capacitor/app`: у фоні закривати SSE. При поверненні перепідключити SSE, синхронізувати й оновити баланс;
    - [ui] зовнішні посилання (`api.monobank.ua`) відкривати через `@capacitor/browser`;
    - [власник] перше встановлення на iPhone і тиждень щоденного користування з нотатками. Кроки: Apple ID у Xcode → Personal Team, Developer Mode, Settings → General → VPN & Device Management → довірити сертифікат, Tailscale на iPhone увімкнено, далі щотижня Product → Run, не видаляючи застосунок.
- **Етап 2 · Нативне відчуття** (усе доступно на Personal Team)
  - Haptics, pull-to-refresh.
  - Face ID-замок і розмиття сум у перемикачі застосунків.
  - Клавіатура в тон теми.
  - Віртуалізація довгих списків.
  - Замір продуктивності Aurora на iPhone.
  - Жест «назад» на сторінці отримувача.
- **Етап 3 · Push через APNs — відкладено: потребує Apple Developer Program.** Обсяг: APNs-ключ, `POST/DELETE /api/devices` і відправник APNs, `@capacitor/push-notifications`.
- **Етап 3a · Сповіщення без APNs** (Personal Team)
  - Локальні нагадування про списання підписок (`@capacitor/local-notifications` 8.3.1).
  - Опційно: «best effort» фонове оновлення (`@capacitor/background-runner` 3.0.0).
- **Етап 3b · Вебхук Monobank** (опційно)
  - Tailscale Funnel лише на `/api/webhooks/monobank` (на macOS потрібен open-source `tailscaled`) або quick-тунель тільки для вебхука.
- **Етап 4 · Поза застосунком**
  - Віджет WidgetKit через App Group. Перша картка етапу — перевірка App Group на Personal Team.
  - Альтернативні іконки під теми.
- **Етап 5 · Розповсюдження — відкладено: потребує Apple Developer Program і поза планом власника.**
- **Етап 6 · Android**
  - `npx cap add android`, CORS для `https://localhost`, кнопка «назад», edge-to-edge. Tailscale є й на Android.

### Е2 · Платформа бекенду: .NET 10 LTS
- **Статус:** рішення 002 прийнято 2026-10-19, варіант A (`.claude/pm/decisions/002-dotnet-10.md`). Чекаємо, поки власник виконає OUT-012.
- **Чому зараз:**
  - підтримка .NET 8 закінчується 2026-11-10;
  - локальний рантайм 8.0.13 відстає від 8.0.32 на ~19 патчів;
  - net10 вмикає аудит транзитивних пакетів.
- **Картки:**
  - OUT-012 (власник, зараз) — оновлення SDK і рантаймів;
  - OUT-011 (coder) — міграція, лише за варіанта A;
  - після OUT-011 — смоук власника на реальних даних.

## В роботі

### OUT-001 · Власник: підготувати середовище для iOS
- **Статус:** in-progress. Xcode готовий, лишились Apple ID і Developer Mode, обидва лише для встановлення на iPhone.
- **Виконавець:** власник (не coder)
- **Пріоритет:** P1
- **Репо:** — (локальне середовище й акаунти)
- **Залежить від:** —
- **Навіщо:** без Xcode неможливо зібрати iOS-застосунок і перевіряти мобільні картки. Без Apple ID і Bundle ID збірку не поставити на iPhone.
- **Виконано (перевірено 2026-10-18):**
  - ✅ Xcode 27.1 (27A9275) встановлено й вибрано через `xcode-select` (`/Applications/Xcode.app/Contents/Developer`), перше налаштування зроблено;
  - ✅ iOS 27.1 SDK, симулятори iOS 27.0, 27.1 і старі 17.x (17.x знадобляться для перевірки мінімальної версії iOS 17);
  - ✅ Bundle ID: OUT-004 узяв типовий `io.github.dmitryspain.outlay`. Власник може змінити його до першого встановлення на iPhone. Після встановлення зміна означає новий застосунок без старих даних.
- **Лишилось:**
  1. **Apple ID у Xcode** → Settings → Accounts, з'явиться Personal Team. Потрібен **лише для встановлення на iPhone**, для симулятора не потрібен. Можна зробити безпосередньо перед першим встановленням.
  2. **Developer Mode на iPhone** (iOS 17+): Settings → Privacy & Security → Developer Mode, після першого підключення до Мака. Теж лише для встановлення на пристрій.
  3. ~~Apple Developer Program~~ — **не потрібно** (рішення власника).
- **Довідка про Personal Team:** профіль діє 7 днів, далі поновлення через Product → Run. До 10 App ID, до 3 пристроїв, до 3 застосунків на пристрій.
- **Bundle ID:** `io.github.dmitryspain.outlay` (типовий, узятий в OUT-004; власник не змінював)
- **Критерії приймання:**
  - [x] `xcodebuild -version` → `Xcode 27.1` (27A9275)
  - [x] `xcrun simctl list runtimes` містить iOS 27.0 і 27.1
  - [x] `xcrun simctl list devices available` містить iPhone-симулятори
  - [ ] у Xcode → Settings → Accounts є Personal Team (зараз акаунтів немає)
  - [x] Bundle ID вписано в цю картку
- **Для tester:** пункти 1–3 уже доведено командами. Решта — ручні дії власника.
- **Контракт API:** без змін
- **Поза скоупом:** CocoaPods, Flutter, `dotnet workload install maui/ios`, Apple Developer Program.

### OUT-005 · UI: статус-бар і безпечні зони iPhone в тон теми
- **Статус:** in-progress
- **Виконавець:** coder
- **Пріоритет:** P1
- **Репо:** ui
- **Платформа:** web+ios
- **Залежить від:** OUT-004
- **Навіщо:** у нативній оболонці WebView займає весь екран. Верхня панель заходить під годинник і Dynamic Island, а колір тексту статус-бару від теми не залежить, тож на світлих темах світлий годинник не видно. Це перше, що бачить користувач, і частина вимоги «ті самі теми на мобілці».
- **Що зробити:**
  1. **Дозволений npm-пакет:** `@capacitor/status-bar` ^8.0.4 у `OutlayUI-latest/package.json` (`@capacitor/core` уже доданий в OUT-004). Потім `npx cap sync ios`.
  2. Новий `src/app/core/platform.ts`:
     - DI-токен `IS_NATIVE_APP` (`InjectionToken<boolean>`, factory → `Capacitor.isNativePlatform()`);
     - DI-токен `STATUS_BAR` (factory → плагін `StatusBar`), щоб тести могли підставити фейк;
     - чиста функція `statusBarStyleFor(theme: ThemeId): 'light-text' | 'dark-text'` за `THEMES[…].scheme`: `dark` → `'light-text'`, `light` → `'dark-text'`.
  3. Новий `NativeShellService` (`src/app/core/native-shell.service.ts`) з методом `syncStatusBar(theme: ThemeId)`:
     - на вебі (`IS_NATIVE_APP` = false) нічого не робить;
     - у застосунку викликає `STATUS_BAR.setStyle({ style })`. Увага: у Capacitor `Style.Dark` означає «світлий текст для темного фону», а `Style.Light` — «темний текст для світлого»;
     - помилку плагіна ковтає, щоб не зламати застосування теми.
  4. `PrefsService.applyDom()`: після виставлення `data-theme` викликати `syncStatusBar(this.resolvedTheme())`. Так статус-бар іде за ручним вибором теми, за «Авто» і за холодним стартом. Ще виставляти `html[data-native="ios"]`, якщо `IS_NATIVE_APP`.
  5. Перевірити, що в `ios/App/App/Info.plist` `UIViewControllerBasedStatusBarAppearance` = `YES`: цього вимагає плагін.
  6. Безпечні зони в CSS (діють і в мобільному Safari, а на десктопі `env()` = 0):
     - `TopbarComponent` `.bar`: `padding-top: env(safe-area-inset-top)`, висота `calc(var(--bar-h) + env(safe-area-inset-top))`. Ліві й праві відступи `max(<поточне значення>, env(safe-area-inset-left / right))`;
     - `AppComponent` `.content` на ширині ≤ 719px: ліві й праві відступи з урахуванням `env(safe-area-inset-left / right)`;
     - нижню панель (`nav.component.ts`) і bottom-sheet (`components.css`) не чіпати: вони вже враховують `env(safe-area-inset-bottom)`.
- **Критерії приймання:**
  - [ ] юніт-тест: `statusBarStyleFor` повертає `'light-text'` для `aurora` і `terminal`, `'dark-text'` для `daylight`, `ledger`, `brutal`, і проходить по всіх `THEMES`
  - [ ] юніт-тест: з `IS_NATIVE_APP` = false виклик `applyDom()` не викликає фейковий `STATUS_BAR.setStyle`, а з `true` викликає його з правильним стилем
  - [ ] на симуляторі iPhone з Dynamic Island у портретній орієнтації верхня панель повністю нижче статус-бару в усіх 5 темах. Годинник і батарея читаються: світлі на Aurora й Terminal, темні на Daylight, Ledger і Brutal
  - [ ] будь-яка зміна теми (меню палітри в топбарі, Налаштування, ⌘K) одразу змінює стиль статус-бару. «Авто» разом зі статус-баром іде за системною темою симулятора (`xcrun simctl ui booted appearance dark|light`)
  - [ ] холодний старт зі збереженою світлою темою: статус-бар правильний одразу, без чекання на перемикання
  - [ ] в альбомній орієнтації вміст і панелі не заходять під виріз
  - [ ] веб (`npm start`, Chrome desktop і вікно шириною 390 px) виглядає як до змін
  - [ ] `npx ng build`, `npm run build:mobile`, `npx ng test --watch=false --browsers=ChromeHeadless` зелені, `npx cap sync ios` показує плагін StatusBar
- **Для tester:**
  - Пристрій для перевірок з Dynamic Island — **iPhone 17 з iOS 27.0**: runtime iOS 27.1 тут підтримує лише «iPhone Duo».
  - Перед `xcrun simctl io booted screenshot` відкрий Simulator.app (`open -a Simulator`), інакше кадр чорний.
  - Пройди всі 5 тем і «Авто» в демо-режимі.
  - Перевір, що `data-native` на вебі не виставляється.
  - Переконайся, що `ThemePreviewComponent` (вкладені `[data-theme]`) не впливає на статус-бар: стиль має залежати лише від теми документа.
  - Перевір і на iPhone без Dynamic Island (з вирізом або з кнопкою Home) на симуляторі iOS 17.x.
- **Контракт API:** без змін
- **Поза скоупом:** 5 вкладок замість 7, Keychain, клавіатура, haptics, launch screen та іконка.

## Наступні

### OUT-012 · Власник: оновити .NET — SDK 10.0.402 і ASP.NET Core Runtime 8.0.32
- **Статус:** todo · можна робити зараз (потрібна за будь-якого варіанта рішення 002)
- **Виконавець:** власник (не coder)
- **Пріоритет:** P1
- **Репо:** — (локальне середовище)
- **Залежить від:** —
- **Навіщо:** бекенд зараз працює на локальному рантаймі .NET 8 / ASP.NET Core 8 версії **8.0.13** (лютий 2025), тобто без ~19 патчів. Актуальна версія — 8.0.32 (2026-10-13, security). SDK 10 стоїть як 10.0.100 з рантаймом 10.0.0, а актуальний SDK — **10.0.402** з рантаймом 10.0.13. Він потрібен для міграції (OUT-011) і щоб net10-код не працював на рантаймі без 13 патчів. `dotnet list package --vulnerable` рантайм не перевіряє, тож аудит цього не показує.
- **Що зробити:**
  1. https://dotnet.microsoft.com/download/dotnet/10.0 → **SDK 10.0.402**, інсталятор macOS Arm64. До нього входять рантайми .NET і ASP.NET Core 10.0.13.
  2. https://dotnet.microsoft.com/download/dotnet/8.0 → **ASP.NET Core Runtime 8.0.32**, macOS Arm64. До нього входить і .NET Runtime 8.0.32. До завершення міграції net8-бекенд автоматично перейде на найновіший встановлений патч 8.0.x.
  3. За бажанням прибрати застарілі SDK і рантайми (6.0, 7.0, preview 8.0, 9.0.2). Це не обов'язково.
  4. Перезапустити бекенд.
- **Критерії приймання:**
  - [ ] `dotnet --list-sdks` містить `10.0.402`
  - [ ] `dotnet --list-runtimes` містить `Microsoft.AspNetCore.App 8.0.32`, `Microsoft.NETCore.App 8.0.32`, `Microsoft.AspNetCore.App 10.0.13` і `Microsoft.NETCore.App 10.0.13`
  - [ ] у корені OutlayApp `dotnet build` і `dotnet test` зелені (32/32)
  - [ ] бекенд стартує (Postgres + `dotnet run --project OutlayApp.API --launch-profile https`) і працює як раніше
- **Для tester:** після виконання повтори критерії 1–3 командами.
- **Контракт API:** без змін
- **Поза скоупом:** зміна TargetFramework (це OUT-011).

### OUT-006 · Власник: Tailscale Serve — стабільна HTTPS-адреса бекенду для iPhone
- **Статус:** todo · можна робити зараз
- **Виконавець:** власник (не coder)
- **Пріоритет:** P1
- **Репо:** — (локальне середовище й акаунти)
- **Залежить від:** —
- **Навіщо:** iPhone не бачить `localhost:7016`, а quick-тунель Cloudflare змінює адресу й не пропускає SSE. Tailscale Serve дає постійну адресу `https://<mac>.<tailnet>.ts.net` із сертифікатом Let's Encrypt, яку бачать лише ваші пристрої. Цю адресу coder впише в мобільну збірку (OUT-007).
- **Що зробити:**
  1. **Мак:** встановити Tailscale, варіант **Standalone** (tailscale.com/download/mac → Standalone; цей варіант рекомендує Tailscale). Увійти акаунтом Google, Apple, GitHub або Microsoft, план Personal безкоштовний. Варіант із Mac App Store для проксі теж годиться, але тоді CLI запускається як `/Applications/Tailscale.app/Contents/MacOS/Tailscale`.
  2. **CLI у Standalone:** Tailscale → Settings → CLI integration → Show me how → Install Now. З'явиться `/usr/local/bin/tailscale`.
  3. **Ім'я Мака** не має містити нічого чутливого: воно стане частиною адреси й потрапить у публічні CT-логи сертифікатів. Перейменувати можна в адмінці (Machines → Edit machine name).
  4. **Адмінка** https://login.tailscale.com/admin/dns: увімкнути **MagicDNS**, потім **HTTPS Certificates → Enable HTTPS**.
  5. **Бекенд:** запустити як завжди: Postgres + `dotnet run --project OutlayApp.API --launch-profile https`. Без Postgres API не стартує.
  6. **Serve:** `tailscale serve --bg https+insecure://127.0.0.1:7016`. Команда виведе «Available within your tailnet: https://<mac>.<tailnet>.ts.net/». Перший запуск отримує сертифікат, це може зайняти до хвилини.
     - Чому саме так: профіль `https` слухає лише `https://localhost:7016`, а `https+insecure` означає TLS до Kestrel без перевірки self-signed dev-сертифіката. Проксі на http-порт зламав би `UseHttpsRedirection()`.
     - З `--bg` налаштування переживає перезавантаження. Вимкнути: `tailscale serve reset`.
  7. **iPhone:** встановити Tailscale з App Store, увійти тим самим акаунтом, увімкнути VPN. Щоб тунель піднімався сам, у налаштуваннях застосунку Tailscale можна ввімкнути VPN On Demand.
  8. Пройти критерії нижче й вписати адресу в поле «Адреса».
- **Адреса:** _(вписати `https://<mac>.<tailnet>.ts.net`)_
- **Критерії приймання:**
  - [ ] `tailscale serve status` показує `https://<mac>.<tailnet>.ts.net` → проксі на `https+insecure://127.0.0.1:7016`
  - [ ] на Маку `curl -s -o /dev/null -w '%{http_code}\n' https://<mac>.<tailnet>.ts.net/api/clients/cards` → `401`. Тобто бекенд відповідає через Serve, а сертифікат валідний (curl без `-k`)
  - [ ] на iPhone з увімкненим Tailscale Safari відкриває `https://<mac>.<tailnet>.ts.net/swagger` без попередження про сертифікат. Swagger доступний у Development
  - [ ] SSE проходить через Serve: `curl -N "https://<mac>.<tailnet>.ts.net/api/events?cardId=<id картки>&access_token=<ваш ots_…>"` одразу показує `: connected`, приблизно за 25 с — `: ping`, і з'єднання не обривається. Id картки й `ots_…` візьміть у вебі: DevTools → Application → Local Storage → ключ `state`. Токен не вставляйте ні в картку, ні в чат
  - [ ] адресу вписано в поле «Адреса»
- **Для tester:** після виконання повтори з Мака перші два критерії. Решта — ручні перевірки власника.
- **Контракт API:** без змін
- **Поза скоупом:** Tailscale Funnel і публічний вебхук Monobank (етап 3b); деплой; зміни в коді, `launchSettings.json` і `appsettings*.json`.

### OUT-009 · iOS: окрема тека збірки `mobile` і `ios:run`, що збирає, встановлює й запускає з exit 0
- **Статус:** todo · після OUT-005 (той самий репо)
- **Виконавець:** coder
- **Пріоритет:** P1
- **Репо:** ui
- **Платформа:** ios
- **Залежить від:** OUT-005 (лише черга UI-репо; за змістом не залежить)
- **Навіщо:**
  - **Не та збірка в застосунку.** `cap run` і `cap sync` копіюють у застосунок вміст `dist/outlay-ui`. Після звичайного `npm run build` туди лягає веб-збірка з `localhost`, і tester OUT-004 це відтворив. Після OUT-007 адреса Tailscale буде лише в збірці `mobile`, тож iPhone отримав би неправильну адресу.
  - **Скрипт падає.** `native-run` 2.0.3 (найновіший на 2026-10-19), через який `cap run` запускає симулятор, шукає `Simulator.app`. У Xcode 27.1 його замінив DeviceHub, тож `npm run ios:run` встановлює застосунок, але завершується з exit 1.
- **Що зробити:**
  1. `angular.json`: у конфігурації `mobile` задати `"outputPath": "dist/outlay-ui-mobile"`. Веб лишається в `dist/outlay-ui`.
  2. `capacitor.config.ts`: `webDir: 'dist/outlay-ui-mobile'`. Оновити коментар у файлі.
  3. Новий `scripts/ios-run-sim.sh` (bash, `set -euo pipefail`, без нових npm-залежностей). Він робить таке:
     - визначає симулятор зі змінної `IOS_SIM` (назва або UDID). За замовчуванням — iPhone 17 з iOS 27.0. Для розбору `xcrun simctl list devices available -j` використати `node -e`;
     - збирає: `xcodebuild -project ios/App/App.xcodeproj -scheme App -configuration Debug -sdk iphonesimulator -destination "id=<UDID>" -derivedDataPath ios/DerivedData build`;
     - завантажує симулятор: `xcrun simctl boot <UDID>`, якщо він ще не запущений, потім `open -a DeviceHub`. Якщо такого застосунку немає (старіший Xcode), то `open -a Simulator`;
     - встановлює й запускає: `xcrun simctl install <UDID> ios/DerivedData/Build/Products/Debug-iphonesimulator/App.app`, далі `xcrun simctl launch <UDID> <Bundle ID>`. Bundle ID читати з `Info.plist` зібраного `App.app` через `/usr/libexec/PlistBuddy`, а не зашивати;
     - якщо симулятора не знайдено, друкує зрозуміле повідомлення й виходить із ненульовим кодом.
  4. `package.json`:
     - `"ios:run": "npm run ios:sync && bash scripts/ios-run-sim.sh"`;
     - `ios:sync` лишається `npm run build:mobile && npx cap sync ios`.
  5. `README.md` UI, розділ «iOS»:
     - `ios:run` = збірка `mobile` + запуск на симуляторі через скрипт;
     - змінна `IOS_SIM`;
     - симулятор тепер називається DeviceHub (`open -a DeviceHub`);
     - для фізичного iPhone: `npm run ios:sync`, потім Xcode → Product → Run.
- **Критерії приймання:**
  - [ ] `npm run build` складає веб у `dist/outlay-ui` (без рядка `"mobile"`), а `npm run build:mobile` — у `dist/outlay-ui-mobile` (з рядком `"mobile"`). Теки одна одну не перетирають
  - [ ] спершу `npm run build` (веб), потім `npm run ios:run`: на симуляторі Налаштування → «Про застосунок» показує `(mobile)`, а `ios/App/App/public/main.*.js` містить `"mobile"`
  - [ ] `npm run ios:run; echo $?` → застосунок встановлено й запущено на iPhone 17 / iOS 27.0, код виходу `0`
  - [ ] `IOS_SIM="iPhone 15" npm run ios:run; echo $?` (iOS 17.5) → `0`
  - [ ] `IOS_SIM="Nope" npm run ios:run; echo $?` → ненульовий код і повідомлення, що симулятор не знайдено
  - [ ] `npx cap doctor` без помилок; `npm run build` і `npx ng test --watch=false --browsers=ChromeHeadless` зелені
- **Для tester:** перевір, що README відповідає скриптам, а скрипт не потребує відкритого Xcode.
- **Контракт API:** без змін
- **Поза скоупом:** патч чи форк `native-run`; запуск на фізичному iPhone з CLI; live reload; іконка й чистка шаблону.

### OUT-007 · Docs + UI: Tailscale-адреса бекенду для iPhone
- **Статус:** todo · заблоковано (OUT-006, OUT-009)
- **Виконавець:** coder
- **Пріоритет:** P1
- **Репо:** both
- **Платформа:** ios (змінюється лише мобільна збірка, веб не зачеплено)
- **Залежить від:** OUT-006 (адреса й перевірені команди); OUT-009 (окрема тека `dist/outlay-ui-mobile` і `ios:run` зі збіркою mobile); ✅ OUT-002; ✅ OUT-003. Не запускати паралельно з OUT-011 (бекенд-репо) і з іншими UI-картками.
- **Навіщо:** застосунок на iPhone має ходити на бекенд Мака через постійну адресу Tailscale, а налаштування має відтворюватися з документації.
  - Документацію й адресу об'єднано в одну картку: обидві зміни запускає одна подія (власник повідомив адресу), а перевіряються вони одним прогоном tester.
  - Сюди ж зведено зауваження з перевірки OUT-002 і OUT-003, бо вони стосуються тих самих розділів README.
- **Що зробити:**
  1. **Бекенд `README.md`**, розділ «Доступ з телефона (iOS)» (з OUT-002, зараз рядки ~40–64):
     - перекласти англійською, бо решта README англійська. Заголовок — «Access from a phone (iOS)». Зміст пунктів (а)–(ґ) зберегти, включно з форматами ключів `Cors__Origins__N` (змінна середовища) і `Cors--Origins--N` (Key Vault);
     - додати примітку: після зміни `Cors:Origins` API треба перезапустити, бо політику CORS `Program.cs` будує один раз на старті;
     - новий підрозділ «Stable address via Tailscale Serve». У ньому:
       - встановлення: на Мак варіант Standalone + CLI integration, на iPhone з App Store, той самий акаунт;
       - адмінка: MagicDNS і HTTPS Certificates, з попередженням про публічні CT-логи для імен машин;
       - команда `tailscale serve --bg https+insecure://127.0.0.1:7016` з поясненням: профіль `https` слухає лише 7016; `https+insecure` — TLS без перевірки dev-сертифіката; http-порт зламав би `UseHttpsRedirection()`;
       - `tailscale serve status` і `tailscale serve reset`;
       - умови: Мак не спить, бекенд і Postgres запущені (без Postgres API не стартує), на iPhone увімкнено Tailscale;
       - перевірка SSE через `curl -N …/api/events?cardId=…&access_token=…` з власним токеном;
       - адреса лише як плейсхолдер `https://<mac>.<tailnet>.ts.net`.
     - У пункті про quick-тунель додати посилання на новий підрозділ.
  2. **UI `README.md`**, рядок 12: прибрати твердження, що адреса бекенду задана в `src/app/core/config.ts`. Написати українською, як решта цього README, що адреса задана в `src/environments/environment.ts` (веб, `https://localhost:7016/api`) і `src/environments/environment.mobile.ts` (збірка `npm run build:mobile` для iOS, адреса Tailscale), а `config.ts` лише читає `environment.apiBaseUrl`. Розділ «iOS» з OUT-004 і OUT-009 не змінювати.
  3. **UI `src/environments/environment.mobile.ts`:** `apiBaseUrl` = адреса з OUT-006 + `/api`, наприклад `https://<mac>.<tailnet>.ts.net/api`. Коментар англійською: адресу дає Tailscale Serve; вона не секрет, бо доступ дає лише членство в tailnet. `environment.ts` (веб) не чіпати.
- **Критерії приймання:**
  - [ ] у README бекенду розділ «Access from a phone (iOS)» англійською: пункти (а)–(ґ), примітка про перезапуск після зміни `Cors:Origins` і підрозділ «Stable address via Tailscale Serve» з усіма пунктами кроку 1. Реального імені tailnet і жодних токенів у README немає
  - [ ] README UI більше не називає `config.ts` місцем адреси, натомість згадує обидва environment-файли
  - [ ] `npm run build:mobile` → `dist/outlay-ui-mobile/main.*.js` містить `<адреса з OUT-006>/api`
  - [ ] `npm run build` (веб) → `dist/outlay-ui/main.*.js` містить `localhost:7016/api` і не містить `ts.net`
  - [ ] при запущених бекенді й Serve: `curl -s -D - -o /dev/null -X OPTIONS <адреса>/api/clients/cards -H 'Origin: capacitor://localhost' -H 'Access-Control-Request-Method: GET' -H 'Access-Control-Request-Headers: authorization'` → 204 і `Access-Control-Allow-Origin: capacitor://localhost` (без `-k`: сертифікат валідний)
  - [ ] `curl -s -D - -o /dev/null <адреса>/api/clients/cards -H 'Origin: capacitor://localhost'` → 401 і `Access-Control-Allow-Origin: capacitor://localhost`
  - [ ] `npm run ios:run` → на симуляторі (iPhone 17, iOS 27.0) Налаштування → «Про застосунок» показує `<адреса>/api (mobile)`
  - [ ] `npx ng test --watch=false --browsers=ChromeHeadless` і `dotnet test` зелені
- **Для tester:** критерії 5–7 потребують Мака в tailnet із запущеним `tailscale serve` (OUT-006) і Postgres. Якщо Serve не запущено, вердикт BLOCKED із поясненням, а не FAIL. Симулятор працює через мережу Мака, тож `*.ts.net` має резолвитися.
- **Контракт API:** без змін
- **Поза скоупом:** код бекенду, `launchSettings.json`, `appsettings*.json`; розділ README UI «Про бекенд» про DbContext (окрема ідея); Funnel і вебхук (етап 3b); поле адреси в інтерфейсі; деплой.

### OUT-010 · Dev-утиліта для tester: керування WebView застосунку на симуляторі через Web Inspector
- **Статус:** todo · у черзі UI-репо після OUT-009
- **Виконавець:** coder
- **Пріоритет:** P2
- **Репо:** ui
- **Платформа:** ios
- **Залежить від:** OUT-009 (команда `ios:run`, яка запускає застосунок)
- **Навіщо:** tester перевіряє iOS-картки без тапів. Протокол Web Inspector симулятора (`webinspectord_sim`) дозволяє натискати елементи DOM, читати обчислені стилі й консоль. Так було доведено OUT-004: 864 властивості на 6 тем. Але клієнт лежить лише в тимчасовому scratchpad головної сесії. Без інструмента в репо кожна наступна iOS-картка (5 вкладок, Keychain, життєвий цикл, іконка) залежатиме від тимчасового скрипта або ручних тапів власника.
- **Що зробити:**
  1. Перенести клієнт у `tools/ios-inspect/` як dev-інструмент: у збірку застосунку й у `dist` він не потрапляє. Основа — наявний скрипт зі scratchpad головної сесії (шлях передасть головна сесія). Якщо його вже немає, написати заново за цим описом.
  2. Команди:
     - `pages` — список інспектованих WebView на завантаженому симуляторі, із Bundle ID;
     - `eval <js>` — результат у JSON;
     - `click <css-selector>`;
     - `style <css-selector> <property…>` — обчислені стилі;
     - `console [--seconds N]` — повідомлення консолі.
  3. Без нових залежностей у `package.json`: Node або Python 3 лише зі стандартною бібліотекою. Для бінарного plist протоколу підходить `plistlib`. Мову обирає наявний скрипт.
  4. `package.json`: `"ios:inspect": "<запуск інструмента>"`. Аргументи передаються через `npm run ios:inspect -- …`.
  5. `tools/ios-inspect/README.md`: як підключитися, приклади команд. Окремо зазначити, що на runtime iOS 17.5 `webinspectord` при першому підключенні піднімається близько 1,5 хв.
- **Критерії приймання** (застосунок запущено через `npm run ios:run` на iPhone 17 / iOS 27.0):
  - [ ] `npm run ios:inspect -- pages` показує WebView застосунку `io.github.dmitryspain.outlay`
  - [ ] `npm run ios:inspect -- eval "document.documentElement.dataset.theme"` друкує поточну тему
  - [ ] клік по кнопці теми Ledger у Налаштуваннях (`click` із селектором або `eval`), а потім той самий `eval` → `"ledger"`
  - [ ] `npm run ios:inspect -- style body font-family` на темі Ledger повертає стек із `ui-serif`
  - [ ] `console` ловить `console.error('probe')`, викликаний через `eval` (позитивний контроль)
  - [ ] при вимкненому симуляторі — ненульовий код виходу й зрозуміле повідомлення
  - [ ] `git status` після роботи інструмента показує зміни лише в `tools/`, `package.json` і README
- **Для tester:** це твій робочий інструмент. Перевір його на справжньому сценарії: теми з OUT-004 або статус-бар з OUT-005.
- **Контракт API:** без змін
- **Поза скоупом:** XCUITest і Appium; фізичний iPhone; CI.

### OUT-011 · Backend: перехід на .NET 10 LTS
- **Статус:** todo · чекає лише OUT-012
- **Виконавець:** coder
- **Пріоритет:** P1 (термін — до 2026-11-10, коли закінчується підтримка .NET 8)
- **Репо:** backend
- **Залежить від:** ✅ рішення 002 (accepted 2026-10-19, варіант A); OUT-012 (SDK 10.0.402 встановлено; зараз стоять SDK 10.0.100 і рантайми 8.0.13 / 10.0.0). Не паралельно з OUT-007 (бекенд-репо).
- **Навіщо:**
  - .NET 8 підтримується до 2026-11-10, .NET 10 LTS — до 2028-11-14 (офіційні release-metadata).
  - На net10 `NuGetAuditMode` = `all` за замовчуванням, тож транзитивні вразливості, як Pkcs в OUT-008, видно в звичайній збірці.
- **Що зробити:**
  1. `TargetFramework`: `net8.0` → `net10.0` у всіх 5 csproj (Domain, Application, Infrastructure, API, Tests).
  2. `global.json`: `"version": "10.0.402"`, `"rollForward": "latestFeature"`, `"allowPrerelease": false`. Зараз там `8.0.0` + `latestMajor` + `allowPrerelease: true`, а це може підхопити preview SDK.
  3. Microsoft.* 8.0.x → останній 10.0.x:
     - `Microsoft.EntityFrameworkCore` (+ `.Design`, `.Relational`, `.Tools`);
     - `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore`;
     - `Microsoft.Extensions.Configuration`, `.Configuration.Json`, `.DependencyInjection.Abstractions`, `.Http`;
     - `Microsoft.Extensions.Logging.AzureAppServices`;
     - `Microsoft.Extensions.Http.Resilience` → 10.x;
     - `Npgsql.EntityFrameworkCore.PostgreSQL` → 10.x;
     - Azure-пакети конфігурації й Key Vault → актуальні мінорні.
  4. Прибрати `Microsoft.AspNetCore.OpenApi` з `OutlayApp.API.csproj`: у коді він не використовується (немає `AddOpenApi` / `WithOpenApi` / `MapOpenApi`).
  5. **Не оновлювати:**
    - `MediatR` лишається на 12.x: з 13.0 подвійна ліцензія (RPL-1.5 або комерційна з ключем);
    - `Quartz` лишається на 3.13.x: 4.x — мажорна з breaking changes;
    - `Autofac.*` — як є;
    - `Swashbuckle.AspNetCore` лишається на 6.9.0, якщо `/swagger` у Development працює на net10. Якщо ні — зупинитися й повернути питання: 10.x означає Microsoft.OpenApi 2 і зміни в `Program.cs`.
  6. Піни `System.Security.Cryptography.Xml` і `Pkcs` 10.0.13. Перевірити через `dotnet list package --include-transitive`. Якщо на net10 їх відсікає package pruning (постачає рантайм), пін прибрати. Тест версій у `TokenEncryptionTests` має й далі проходити на рантаймі 10.0.13.
  7. Помилки й попередження компіляції від breaking changes (EF Core 9/10, Npgsql 9/10, ASP.NET Core 9/10, System.Text.Json) виправляти мінімально, без зміни поведінки. Кожне виправлення описати у звіті.
- **Дозволені пакети:** лише перелічені вище, оновлення версій, і видалення `Microsoft.AspNetCore.OpenApi`.
- **Критерії приймання:**
  - [ ] усі 5 проєктів на `net10.0`; `dotnet --version` у корені OutlayApp → `10.0.402`
  - [ ] `dotnet build --no-incremental` без помилок і без жодного NU190x. Інших попереджень не більше, ніж було (7 × CS8618)
  - [ ] `dotnet list OutlayApp.sln package --vulnerable --include-transitive` → чисто для всіх проєктів
  - [ ] `dotnet test` зелений (32 і більше)
  - [ ] нові тести серіалізації DTO, від яких залежить UI: `ClientCardDto`, `ClientTransactionDto`, `BackfillStatus`, `WebhookStatus`, `ConnectResult` серіалізуються з web-налаштуваннями ASP.NET у ті самі імена полів camelCase і з датами в UTC (`…Z`), які очікують `toCard`, `toTxns`, `toBackfill` в `../OutlayUI-latest/src/app/core/api.service.ts`
  - [ ] з Postgres (`docker compose up -d` в `OutlayApp.API`):
    - `dotnet ef migrations has-pending-model-changes --context OutlayContext --project OutlayApp.Infrastructure --startup-project OutlayApp.API` → змін моделі немає;
    - API стартує;
    - `/swagger` відкривається;
    - preflight з `Origin: capacitor://localhost` → 204 з `Access-Control-Allow-Origin`.
- **Для tester:**
  - Тести серіалізації пиши за полями, які реально читає `api.service.ts`, а не за поточним виводом. Так тест ловить і перейменування, і зміну формату дат.
  - Ризики:
    - EF Core 9+ робить `PendingModelChangesWarning` помилкою;
    - Npgsql 9/10 змінив деякі мапінги;
    - тести `TimeAndMoney` (UTC, київський час) мають лишатися зеленими.
  - Без Postgres останній критерій — BLOCKED.
- **Контракт API:** без змін. Це доводиться тестами серіалізації DTO.
- **Поза скоупом:** Swashbuckle 10 і Microsoft.OpenApi 2; мажорні оновлення MediatR і Quartz; `Dockerfile`; нові можливості C#; CS8618.

## Ідеї (ще не картки)

- [ui · ios] **Live reload для iOS** (P2, перед наступними UI-задачами для iOS): `serve.configurations.mobile` в `angular.json` + `npx cap run ios -l`. Або власний варіант, якщо `native-run` усе ще ламатиметься на Xcode 27.1. У симуляторі WebView бере `http://localhost:4200`: ATS не діє для `localhost`, а origin уже є в `Cors:Origins`.
- [ui] **Оновлення Angular 17 → 22** (P2, окремий епік). Версія 17 без підтримки з травня 2025, а Capacitor 8 вимагає Node ≥ 22, який Angular CLI 17.0.8 позначає як Unsupported.
- [backend] **`UserSecretsId` = `{GUID}`** (P2): шаблонний плейсхолдер, через який user-secrets спільні з будь-яким проєктом із таким самим. Coder ставить справжній GUID, а власник синхронно переносить `secrets.json`. Не під час MVP iOS.
- [backend] **`NuGetAuditMode=all`** у `Directory.Build.props` — лише якщо в рішенні 002 оберуть B. На net10 це вмикається саме.
- [backend] **Спільна реєстрація CORS** (P3): `CorsTests` дублює налаштування з `Program.cs`. Винести `AddOutlayCors(configuration)`.
- [backend] **CS8618** (P3): 7 попереджень, усі були й раніше.
- [backend] **`LegacyTokenMigrator`** (P3): одноразова міграція біжить на кожному старті до Kestrel. Прибрати, коли в БД не лишилось `LegacyPlainToken`.
- [backend] **`Dockerfile`** (P3): образи .NET 7, порт 80. Якщо колись знадобиться контейнер — одразу на .NET 10.
- [backend] Бюджети й підписки на сервері, щоб на телефоні й у вебі були ті самі. Контракт розширюється.
- [backend] Ковзний строк сесії й назва пристрою в сесії. P3.
- [ui] Прибрати невикористані залежності з `package.json`: bootstrap, @angular/material, ng-bootstrap, jquery, chart.js, ng2-charts, fontawesome, tailwind.
- [ui] Тест синхронізації `THEMES`/`DEFAULT_HUES` ↔ `--accent-h-default` у `themes.css`.
- [docs] README UI, розділ «Про бекенд», застарів: `DbContext` уже scoped (`AddDbContext`).
- [qa] Screenshot-baseline у Playwright WebKit для 5 тем.

## Готово

### OUT-008 · Backend: System.Security.Cryptography.Xml → 10.0.13 — done (2026-10-19)
- **Результат:**
  - `dotnet list package --vulnerable --include-transitive` чисто в усіх 5 проєктах;
  - `dotnet build --no-incremental` дає 7 попереджень замість 13, лишились тільки CS8618.
- **Тести:** `dotnet test` 32/32. Новий `OutlayApp.Tests/TokenEncryptionTests.cs` (7 тестів):
  - round-trip `TokenProtector`;
  - шифротекст не розкриває токен;
  - сумісність із purpose v1;
  - інший purpose чи інші ключі не розшифровують;
  - постійна перевірка «завантажено Xml і Pkcs ≥ 10.0.13» через `AssemblyInformationalVersion`. Assembly version в обох версіях 10.0.0.0, тож через неї вразливу версію не відрізнити.

  Мутації M1–M5 тести вбивають.
- **Не перевірено:** старт API на наявному кільці ключів у БД, бо немає Postgres. Ризик низький: Data Protection без `ProtectKeysWith*`, тож EncryptedXml і Pkcs на цьому шляху не виконуються.
- **Пішло далі:**
  - аудит лише прямих залежностей → рішення 002 (на net10 `NuGetAuditMode=all`) або ідея для варіанта B;
  - кінець підтримки .NET 8 і застарілий локальний рантайм 8.0.13 → рішення 002, OUT-011, OUT-012.

### OUT-004 · UI/iOS: каркас iOS-оболонки на Capacitor — done (2026-10-19)
- **Результат:**
  - `cap doctor`, `ios:sync` і `xcodebuild` (чиста збірка теж) пройшли, `MinimumOSVersion` 17.0;
  - Bundle ID `io.github.dmitryspain.outlay`, `DEVELOPMENT_TEAM` у git немає;
  - згенероване ігнорується git, а pbxproj, `Package.swift`, `Package.resolved` і `Info.plist` відстежуються.
- **Перевірка на симуляторах** iPhone 17 / iOS 27.0 та iPhone 15 / iOS 17.5. UI проганяли через протокол Web Inspector, натискаючи справжні елементи DOM:
  - демо;
  - 6 карток тем;
  - «Авто» за системною темою;
  - превʼю;
  - повзунок акценту;
  - «(mobile)» у Налаштуваннях;
  - холодний перезапуск: тема, мова, акцент і демо зберігаються.
- **Консоль** чиста, позитивний контроль спрацьовує.
- **Шрифти** підтверджено вимірами: New York, SF Pro Rounded, SF Mono. Паритет із вебом — 864 обчислені властивості на 6 тем, різниця лише у відмінностях рушіїв Blink і WebKit. Головна сесія переглянула порівняльні скріншоти.
- **iOS 17.5:** без View Transitions спрацьовує запасний шлях.
- **Для власника (не блокує):** справжні тапи з анімацією кола при зміні теми і Safari на 390 px.
- **Пішло далі:**
  - `native-run` на Xcode 27.1, веб-збірка в застосунку, DeviceHub у README → OUT-009;
  - клієнт Web Inspector → OUT-010;
  - шаблон (iPad, `armv7`, `-D COCOAPODS`, `@UIApplicationMain`) → рішення «лише iPhone» і майбутня картка про іконку й чистку шаблону;
  - lock-файл (`semver` 7.8.5, `minipass` 7.1.3, застарілий `uuid@7.0.3` через `@capacitor/cli` → `xcode@3.0.1`) → knowledge, без дій;
  - `~/.npm` з файлами root і телеметрія Capacitor → нотатки для власника.

### OUT-002 · Backend: CORS для iOS-застосунку й доступ з телефона — done (2026-10-18)
- **Результат:** `capacitor://localhost` у `Cors:Origins` (Development), у README з'явився розділ «Доступ з телефона (iOS)». Пункт (б) перевірено на симуляторі.
- **Тести:** 25/25 (+7 у `CorsTests`), мутації ловляться.
- **Відхилення (схвалено):** для Key Vault правильний ключ `Cors--Origins--N`, а не `Cors__Origins__N`, як стояло в картці.
- **Пішло далі:** мова розділу й примітка про перезапуск → OUT-007; спільна реєстрація CORS, `UserSecretsId`, CS8618 → ідеї.

### OUT-003 · UI: адреса API з конфігурації збірки — done (2026-10-18)
- **Результат:** адреса API в `src/environments/*`, конфігурація `mobile`, скрипт `build:mobile`, назва збірки в Налаштуваннях.
- **Тести:** 66/66 (+4 spec-файли). Мутації M1–M5 ловляться, підміну адреси доведено в рантаймі.
- **Пішло далі:** рядок 12 README UI → OUT-007; live reload → ідеї; Node 22 / Angular 17 → ідея оновлення Angular.

### Рішення й процес
- **Рішення 001 · Стек iOS** — прийнято 2026-10-18, з питанням 5 (Tailscale Serve).
- **Рішення 002 · .NET 10** — запропоновано й прийнято 2026-10-19, варіант A.
- **Процес команди:** 2026-10-18 погоджені пропозиції внесено в `.claude/CLAUDE.md` і визначення агентів. Головна сесія далі оновлює CLAUDE.md: місце адреси API (після OUT-003) і iOS-примітки про `ios:run`, DeviceHub та вибір симулятора (після OUT-004).
