# ☕ Koffi

**More than caffeine tracking. / Több mint koffeinkövetés.**

[![Version](https://img.shields.io/badge/version-5.0-0891b2)](https://koffi.hu) [![.NET](https://img.shields.io/badge/.NET-10-512bd4)](https://dotnet.microsoft.com/) [![Languages](https://img.shields.io/badge/languages-HU%20%7C%20EN%20%7C%20DE-0e7490)](https://koffi.hu)

🌍 **Live app / Éles alkalmazás:** [koffi.hu](https://koffi.hu)  
💙 **Support / Támogatás:** [Buy Me a Coffee](https://buymeacoffee.com/adamka00)  
🚀 **Current release / Aktuális verzió:** **5.0**

**English** · [Magyar változat](#magyar)

---

## 🇬🇧 English

Koffi is a modern, mobile-first **ASP.NET Core MVC Progressive Web App (PWA)** for recording caffeine intake, estimating how caffeine levels change over time, planning consumption around sleep, and exploring individual patterns between caffeine habits and sleep.

It brings together a biological caffeine model, daily and weekly history, favorites, personal drinks, caffeine-free days, streaks, an advanced sleep diary, lifetime statistics, barcode-assisted drink discovery, guest access, secure accounts, and a multilingual email system.

**Koffi 5.0** introduces a redesigned sleep diary with analysis and statistics, editable and categorized personal drinks, a smarter guest-to-account experience, and secure email confirmation with optional weekly reports and product announcements.

### ✨ Features

#### ☕ Caffeine tracking and planning

- **Active caffeine calculation:** Estimate caffeine still active in your body, combining multiple drinks consumed at different times.
- **Biological model:** Approximate a 45-minute absorption phase followed by exponential elimination with a 5-hour half-life.
- **Interactive caffeine curve:** Visualize estimated caffeine levels throughout a selected day using Chart.js.
- **Sleep readiness:** Estimate when caffeine may fall below your configured personal target.
- **Planned bedtime forecast:** See estimated active caffeine at a chosen bedtime.
- **Cutoff planner:** Estimate the latest suitable time for a selected drink relative to your bedtime target.
- **What-if simulator:** Explore hypothetical caffeine intake without modifying your diary unless you choose to save it.
- **Daily diary and history:** Review drink names, quantities, caffeine amounts, consumption times, and previous dates.
- **Weekly overview:** Browse calendar weeks, including month and year boundaries.
- **Lifetime statistics:** Review trends, popular drinks, caffeine-free days, streaks, usual consumption times, and available yearly views.
- **Mobile-friendly activity heatmap:** Explore recorded activity in a compact, GitHub-inspired layout.

#### 🥤 Drinks and quick actions

- **Built-in drink catalog:** Choose from common caffeinated beverages, with localized names.
- **Custom drinks:** Define your own drinks using caffeine content in mg/100 ml.
- **Editable personal drinks (5.0):** Update names, caffeine values, serving sizes, and categories.
- **Custom categories (5.0):** Organize drinks as energy drinks, coffee, tea, soda, or other beverages.
- **Safe archiving (5.0):** Remove a personal drink from active selection while preserving previously recorded caffeine amounts and history.
- **Favorites and Quick Add:** Save a drink with its usual serving size, then log it immediately.
- **Global Quick Add:** Use the safe-area-aware `+` button from across the application.
- **Undo Quick Add:** Reverse an accidental recent Quick Add action.
- **Drink search:** Filter the catalog with case- and accent-insensitive search.
- **Barcode scanning (beta):** Scan supported product barcodes, review suggestions, and adjust data before logging; manual entry remains available.
- **Open Food Facts:** Use external product information as a reviewable suggestion, not as an unquestioned source of caffeine data. Missing or unreliable values are not invented.

#### 🌙 Sleep diary and analysis

- **Sleep Diary 2.0 (5.0):** Dedicated **Diary**, **Statistics**, and **Analysis** sections.
- **Detailed sleep logging (5.0):** Record actual sleep onset, final wake-up time, subjective sleep quality, optional planned bedtime, difficulty falling asleep, nighttime awakenings, and notes.
- **Weekly sleep statistics (5.0):** Explore Monday-to-Sunday averages, shortest and longest duration, sleep rating, awakenings, difficulty falling asleep, and planned-versus-actual timing.
- **Week comparison (5.0):** Navigate between weeks and compare available information with a previous week.
- **Readable durations (5.0):** Show hours and minutes instead of decimal-hour values.
- **Daylight-saving awareness (5.0):** Calculate elapsed time correctly across time changes for new entries, while retaining the meaning of legacy records.
- **Automatic caffeine context:** Link sleep records with relevant caffeine consumption from the preceding day.
- **Personal sleep insights:** Explore associations between your consumption habits, sleep timing, duration, and recorded quality.
- **Plain-language insights (5.0):** Show understandable explanations with technical details available separately.
- **Morning sleep reminder:** Display an optional in-app reminder when the previous night's entry is missing; dismissing it suppresses the reminder for that day.

Insights describe *associations*, not proven causes. Koffi does not provide medical diagnoses or advice.

#### 🔥 Habits, accounts, and experience

- **Logging streaks:** Track your current and longest streak. A day counts when you log a drink **or explicitly mark the day caffeine-free**.
- **Caffeine-free days:** Maintain a tracking habit without requiring caffeine consumption.
- **Guest mode:** Start immediately without registration using a cookie-based guest identity.
- **Guest-to-account migration:** Transfer supported guest data when creating an account.
- **Smart guest suggestion (5.0):** Suggest registration after 10 logged drinks or 5 active tracking days; dismiss for 14 days.
- **Account security:** ASP.NET Core cookie authentication, password hashing, security stamps, and user-specific data separation.
- **Password recovery:** Reset a password through expiring, single-use email tokens.
- **Account and data deletion:** Delete your account and associated tracking data within the application.
- **Guided onboarding:** A non-blocking walkthrough that can be skipped or reopened.
- **Help and What's New:** Built-in help, contextual `?` explanations, release-aware highlights, and a support page.
- **PWA installation:** Install on supported devices, with contextual prompts and iOS Home Screen guidance.
- **Cookie notice:** Explain the application cookies used by Koffi.
- **Three interface languages:** Hungarian, English, and German (HU/EN/DE).
- **Mobile-first UI:** Responsive dark, glassmorphism-inspired design refined for phones while preserving the Koffi 4.0 visual identity.

#### 📧 Email communication — live in Koffi 5.0

- **Email verification:** Confirm a registered address with a 24-hour, single-use link. Email verification is not mandatory for normal diary use.
- **Independent opt-in subscriptions:** Separately enable release announcements and personalized weekly summaries, **off by default** and available only to verified accounts.
- **Independent email language:** Select **HU, EN, or DE** for emails, regardless of the interface language.
- **Category-specific unsubscribe links:** Leave either mailing category without affecting the other.
- **HTML and plain-text email:** Responsive, localized messages designed for light and dark email-client appearances.
- **Personal weekly reports:** Summarize available caffeine and sleep data for a completed calendar week.
- **Release announcements:** Prepare, preview, test, and explicitly launch localized product updates from the operator CLI.
- **Reliable processing:** SQLite-backed mail queue, a background worker, controlled retries, delivery-state tracking, and per-recipient deduplication.

**Production status:** The mailing worker is **enabled** in production. Email-confirmation delivery and the first Koffi 5.0 announcement were successfully tested in live operation. Weekly summaries are configured to become due on **Mondays at 08:00, Europe/Budapest**, for eligible subscribers; the first scheduled production run is still to be observed. Announcement campaigns are **not sent automatically** and require a separate administrative launch.

### 🛠️ Technology stack

| Layer | Technology |
|---|---|
| Language and runtime | C# · .NET 10 |
| Backend | ASP.NET Core MVC · Razor Views |
| Database and ORM | SQLite · Entity Framework Core · Code-First migrations |
| Authentication | ASP.NET Core cookie authentication · `PasswordHasher` · security stamps |
| Email transport | MailKit · SMTP/STARTTLS · Postfix · OpenDKIM |
| Email safety | SHA-256-hashed tokens · ASP.NET Core Data Protection · CSRF protection · rate limiting |
| Email processing | SQLite-backed outbox · hosted background worker · operator CLI |
| Frontend | HTML5 · JavaScript · Tailwind CSS |
| Charts | Chart.js |
| Barcode and product data | ZXing JavaScript · Open Food Facts |
| Localization | ASP.NET Core Localization · HU/EN/DE resource files |
| PWA | Web App Manifest · Service Worker |
| Architecture | Dependency injection · Repository Pattern · Strategy Pattern |
| Hosting | Docker · Linux VPS · Caddy reverse proxy · HTTPS |
| Testing | ASP.NET Core integration tests · Node.js frontend tests |

### 🧪 Tests

The latest completed local verification covered **167 .NET tests and 7 frontend JavaScript tests**. Coverage includes:

- Guest tracking, account creation, login, guest migration, and account deletion.
- Caffeine calculations, daily/weekly history, streaks, caffeine-free days, favorites, and Quick Add.
- Personal drinks, categories, editing, archiving, and preservation of historic caffeine values.
- Drink search, barcode suggestions, Open Food Facts fallback behavior, and input validation.
- Sleep entry creation and editing, user isolation, DST-related calculations, weekly comparisons, and personal insights.
- Planning, cutoff calculations, what-if simulations, and long-term statistics.
- Password-reset and email-confirmation token security, expiry, and single-use behavior.
- Consent, independent unsubscribe, mail-language preferences, rendering, HTML escaping, preview restrictions, queueing, and deduplication.
- Onboarding, localized resources, and legacy database migrations.

The 5.0 upgrade was also validated against a copy of a 4.0 production database, preserving existing values across **13 legacy data tables**.

Run the tests from the repository root:

```bash
dotnet build
dotnet test Caffeine.Tests/Caffeine.Tests.csproj
node --test Caffeine.Tests/Frontend/*.test.cjs
```

*Test counts describe the last confirmed local run and may increase with future changes.*

### 🧠 How the caffeine model works

Each logged drink contributes separately to the estimated active caffeine amount. Koffi approximates a **45-minute absorption period**, followed by exponential elimination using a **5-hour half-life**. It adds the estimated contribution from all recorded drinks to create the combined caffeine curve.

The same model powers the bedtime estimate, cutoff calculation, and what-if planning tools. Actual caffeine metabolism varies between individuals; results are approximations, **not medical advice**.

### 🌙 How sleep insights work

Koffi analyzes available personal sleep entries rather than claiming that caffeine *caused* a particular night of sleep. It can compare estimated caffeine at bedtime, previous-day intake, timing of the last drink, sleep ratings, difficulty falling asleep, and sleep duration.

| Nights recorded | Insight detail |
|---|---|
| Fewer than 10 | Insufficient data for a personal relationship conclusion |
| 10–19 | Early estimate with limited data |
| 20–29 | Emerging pattern |
| 30 or more | More detailed personal observations |

The application communicates observed direction and strength where data supports it, and makes technical details accessible separately.

### 📧 Email, subscriptions, and delivery

**Verification.** The app uses securely generated, one-time confirmation links with a **24-hour validity period**. The database stores SHA-256 hashes of verification tokens rather than plaintext tokens. A token is consumed on successful confirmation. Existing accounts remain usable even when unverified.

**Consent.** Product announcements and weekly statistics are independent, **opt-in** categories requiring a confirmed email address. Users choose their email language independently of the site's interface language. Consent history is stored, and unsubscribe links work per category.

**Weekly reports.** The default schedule is **Monday, 08:00 (Europe/Budapest)**. Reports cover the last eligible completed Monday-to-Sunday week. Depending on available data, they include total logged caffeine, a seven-day average, drink counts, prior-week comparison, and sleep statistics. Missing entries are not treated as proven zero consumption or zero sleep.

**Campaigns.** Product-update campaigns require operator-managed localized content, a preview and successful test delivery, followed by an explicit start command. The queue checks the user's verified status and subscription at send time. A completed campaign does not automatically go out to people who subscribe afterward.

**Queue and safety.** Outgoing jobs are persisted in SQLite with states including pending, sent, cancelled, failed, and uncertain. Retriable failures are handled conservatively; ambiguous SMTP outcomes are not blindly resent. The system deduplicates messages per user and campaign.

**Current production configuration:**

```env
Mailing__Enabled=true
```

The operational value is provided by the server's private environment configuration, not committed to the repository. Password-recovery mail uses its own existing delivery flow.

### 🚀 Deployment notes

Koffi is hosted on a Linux VPS using Docker and stable .NET 10 runtime images. **Caddy** provides HTTPS reverse proxying. The production **SQLite data directory** and **ASP.NET Core Data Protection keys** are persisted outside the application container so data and authentication-related keys survive upgrades.

Production updates should:

1. Back up the live SQLite database consistently, including any active WAL state, and preserve the Data Protection keys.
2. Build and test the updated application, then apply EF Core migrations using the project's established database upgrade path.
3. Deploy using the existing persistent database directory and private environment settings.
4. Verify HTTP availability, database integrity, and background-service health before treating the release as complete.
5. Preserve a rollback path. Enabling a mailing worker in a different environment requires a separate deliberate decision.

Koffi 5.0 uses the `Koffi50SleepMailing` migration for additional sleep tracking, personal-drink archiving, verification, subscriptions, and persistent mail jobs.

**Never commit** production databases, secrets, SMTP credentials, Data Protection keys, or private environment files.

---

<a id="magyar"></a>

## 🇭🇺 Koffi – Magyar változat

A Koffi egy modern, mobilra optimalizált **ASP.NET Core MVC Progressive Web App (PWA)**, amellyel követheted a koffeinbeviteledet, megbecsülheted a szervezetedben még aktív koffein mennyiségét, megtervezheted a fogyasztásodat az alvásodhoz igazítva, és megfigyelheted a saját koffein- és alvási szokásaid közötti összefüggéseket.

Egy alkalmazásban ötvözi a koffeinmodellt, a napi és heti előzményeket, a kedvenceket, a saját italokat, a koffeinmentes napokat, a streak rendszert, a részletes alvásnaplót, az élettartam-statisztikákat, a vonalkódos italkeresést, a vendégmódot, a felhasználói fiókokat és a háromnyelvű levelezést.

A **Koffi 5.0** újratervezett alvásnaplót és alváselemzést, szerkeszthető és kategorizálható saját italokat, intelligensebb vendégregisztrációt, valamint biztonságos e-mail-cím-megerősítést és önkéntesen kérhető heti összefoglalókat és újdonságértesítőket vezet be.

### ✨ Funkciók

#### ☕ Koffeinkövetés és tervezés

- **Aktív koffeinszint becslése:** Több, eltérő időpontban elfogyasztott ital együttes hatásának becslése.
- **Biológiai modell:** Körülbelül 45 perces felszívódási szakasszal és 5 órás felezési idejű, exponenciális kiürüléssel számol.
- **Interaktív koffeingörbe:** Chart.js alapú grafikon a kiválasztott nap becsült koffeinszintjéről.
- **Alvási küszöb:** Megbecsüli, mikor csökkenhet az aktív koffein a beállított célérték alá.
- **Tervezett lefekvés:** Megmutatja, mennyi koffein lehet még aktív a kiválasztott időpontban.
- **Koffein-cutoff:** Becsült utolsó alkalmas fogyasztási időpont a kiválasztott ital és a lefekvési cél alapján.
- **What-if kalkulátor:** Különböző fogyasztási forgatókönyvek kipróbálása automatikus naplómentés nélkül.
- **Napi napló és előzmények:** Italok, mennyiségek, koffeintartalom, fogyasztási időpontok és korábbi napok áttekintése.
- **Heti nézet:** Naptári hetek böngészése hónap- és évhatárokon át is.
- **Élettartam-statisztikák:** Trendek, gyakori italok, koffeinmentes napok, streakek, fogyasztási időpontok és elérhető éves kimutatások.
- **Mobilbarát aktivitási heatmap:** Kompakt, GitHub által ihletett aktivitási áttekintés.

#### 🥤 Italok és gyorsműveletek

- **Beépített itallista:** Gyakori koffeines italok lokalizált nevekkel.
- **Saját italok:** Egyéni italok megadása mg/100 ml koffeintartalommal.
- **Saját italok szerkesztése (5.0):** Név, koffeintartalom, alapértelmezett adag és kategória módosítása.
- **Italkategóriák (5.0):** Energiaital, kávé, tea, üdítő és egyéb kategóriák.
- **Biztonságos archiválás (5.0):** A már nem használt saját ital eltüntethető az aktív listából, miközben a korábbi bejegyzések adatai megmaradnak.
- **Kedvencek és gyors hozzáadás:** Gyakran fogyasztott italok mentése megszokott mennyiséggel.
- **Globális `+` gomb:** Gyors italrögzítés az alkalmazás különböző részeiből.
- **Gyors hozzáadás visszavonása:** A legutóbbi véletlen gyors rögzítés visszavonható.
- **Italkereső:** Kis- és nagybetűktől, illetve ékezetektől független szűrés.
- **Vonalkódolvasó (béta):** Termékkódok kamerás beolvasása, a találatok ellenőrzése és szerkesztése; kézi bevitelre is van lehetőség.
- **Open Food Facts:** Külső termékadatok ellenőrizhető javaslatként. A Koffi nem talál ki hiányzó vagy megbízhatatlan koffeinértékeket.

#### 🌙 Alvásnapló és elemzés

- **Alvásnapló 2.0 (5.0):** Három külön fül: **Napló**, **Statisztika**, **Elemzés**.
- **Részletes alvásrögzítés (5.0):** Tényleges elalvás és ébredés, szubjektív alvásminőség, opcionális tervezett lefekvés, elalvási nehézség, éjszakai ébredések száma és megjegyzések.
- **Heti alvásstatisztikák (5.0):** Hétfőtől vasárnapig számított átlagok, legrövidebb és leghosszabb alvásidő, minőség, ébredések, elalvási nehézség és tervezett/tényleges időpontok különbsége.
- **Hetek összehasonlítása (5.0):** Naptári hetek közötti navigáció és összevetés az előző héttel.
- **Óra–perc megjelenítés (5.0):** Az alvás hossza tizedes órák helyett érthető formátumban látható.
- **Óraátállítás kezelése (5.0):** Az új bejegyzések az időszámítás-váltásnál is helyes eltelt idővel számolnak, a régi rekordok értelmét megőrizve.
- **Automatikus koffeinkontextus:** Az alvásbejegyzések kapcsolata az előző napi releváns koffeinfogyasztással.
- **Személyes megfigyelések:** Összefüggések keresése a koffeinfogyasztás, az elalvás, az alvásidő és a rögzített minőség között.
- **Közérthető elemzés (5.0):** Egyszerű magyarázatok, külön elérhető technikai részletekkel.
- **Reggeli alvásnapló-emlékeztető:** Hiányzó előző éjszakai bejegyzésnél alkalmazáson belüli emlékeztető, amely aznapra elnémítható.

Az elemzések *együttjárásokat*, nem bizonyított ok-okozati kapcsolatokat írnak le. A Koffi nem ad orvosi diagnózist vagy tanácsot.

#### 🔥 Szokások, fiókok és felhasználói élmény

- **Streak rendszer:** Az aktuális és a leghosszabb naplózási sorozat követése. Egy nap akkor is beleszámít, ha **kifejezetten koffeinmentesnek jelölöd**.
- **Koffeinmentes napok:** A naplózási szokás fenntartása koffeinfogyasztás nélkül is.
- **Vendégmód:** Azonnali használat regisztráció nélkül, sütialapú vendégazonosítóval.
- **Vendégadatok átvitele:** A támogatott adatok átvezetése a létrehozott fiókba.
- **Intelligens regisztrációs ajánló (5.0):** Javaslat 10 rögzített ital vagy 5 aktív nap után; 14 napra elrejthető.
- **Fiókbiztonság:** ASP.NET Core cookie authentication, jelszó-hashelés, security stamp és felhasználónként elkülönített adatok.
- **Jelszó-visszaállítás:** Lejáró, egyszer használható e-mailes tokenek.
- **Fiók- és adattörlés:** A fiók és a hozzá tartozó követési adatok törlése az alkalmazásból.
- **Interaktív bevezető:** Átugorható és később újra megnyitható onboarding.
- **Súgó és újdonságok:** Beépített segítség, kontextusos `?` magyarázatok, verzióhoz kötött újdonságok és támogatási oldal.
- **PWA telepítés:** Telepíthető webalkalmazás támogatott eszközökön, iOS-es főképernyőre helyezési útmutatóval.
- **Sütitájékoztató:** A Koffi működéséhez használt sütik ismertetése.
- **Háromnyelvű felület:** Magyar, angol és német (HU/EN/DE).
- **Mobilra optimalizált dizájn:** Reszponzív, sötét, glassmorphism ihletésű felület a 4.0 arculatának megőrzésével.

#### 📧 Levelezés – éles a Koffi 5.0-ban

- **E-mail-cím-megerősítés:** 24 órán át érvényes, egyszer használatos link. A napló továbbra is használható megerősítés nélkül.
- **Külön feliratkozások:** Újdonságértesítők és személyre szabott heti összefoglalók egymástól függetlenül kapcsolhatók be. **Alapértelmezetten ki vannak kapcsolva**, és megerősített cím szükséges hozzájuk.
- **Önálló e-mail-nyelvválasztó:** A levelek nyelve **HU, EN vagy DE** lehet, függetlenül az alkalmazás felületének nyelvétől.
- **Kategóriánkénti leiratkozás:** Az egyik levéltípusról a másik megtartásával is le lehet iratkozni.
- **HTML- és egyszerű szöveges levelek:** Reszponzív, lokalizált sablonok világos és sötét levelezőkörnyezettel.
- **Heti személyes összefoglalók:** A lezárt naptári hét rögzített koffein- és elérhető alvásadatai alapján.
- **Újdonságértesítők:** Lokalizált kampányok adminisztrátori előnézettel, tesztküldéssel és külön jóváhagyott indítással.
- **Megbízható levélsor:** SQLite-adatbázisban tárolt feladatok, háttérfolyamat, ellenőrzött újrapróbálkozások, állapotkövetés és ismétlődésvédelem.

**Éles állapot:** Az automatikus levélfeldolgozó **be van kapcsolva**. Az e-mail-cím-megerősítési folyamatot és az első Koffi 5.0 újdonságkampányt éles környezetben sikeresen teszteltük. A heti összefoglalók **hétfőn, magyar idő szerint 08:00-kor** válnak esedékessé a jogosult feliratkozóknál; az első ütemezett éles futás ellenőrzése még hátravan. Az újdonságkampányok **nem indulnak automatikusan**, külön adminisztrátori parancs szükséges hozzájuk.

### 🛠️ Használt technológiák

| Terület | Technológia |
|---|---|
| Nyelv és futtatókörnyezet | C# · .NET 10 |
| Backend | ASP.NET Core MVC · Razor Views |
| Adatbázis és ORM | SQLite · Entity Framework Core · Code-First migrációk |
| Autentikáció | ASP.NET Core Cookie Authentication · `PasswordHasher` · security stamp |
| E-mail-küldés | MailKit · SMTP/STARTTLS · Postfix · OpenDKIM |
| E-mail-biztonság | SHA-256 tokenhashelés · ASP.NET Core Data Protection · CSRF-védelem · rate limiting |
| Levélfeldolgozás | SQLite-alapú levélsor · háttér-worker · adminisztrátori CLI |
| Frontend | HTML5 · JavaScript · Tailwind CSS |
| Grafikonok | Chart.js |
| Vonalkód és külső termékadatok | ZXing JavaScript · Open Food Facts |
| Lokalizáció | ASP.NET Core Localization · HU/EN/DE erőforrásfájlok |
| PWA | Web App Manifest · Service Worker |
| Architektúra | Dependency Injection · Repository Pattern · Strategy Pattern |
| Üzemeltetés | Docker · Linux VPS · Caddy reverse proxy · HTTPS |
| Tesztelés | ASP.NET Core integrációs tesztek · Node.js frontendtesztek |

### 🧪 Tesztelés

A legutóbbi sikeres helyi ellenőrzés során **167 .NET-teszt és 7 JavaScript frontendteszt** futott le. A tesztek többek között a következőket fedik le:

- Vendégmód, regisztráció, bejelentkezés, vendégadatok átvitele és fióktörlés.
- Koffeinszámítások, napi/heti előzmények, streakek, koffeinmentes napok, kedvencek és gyors hozzáadás.
- Saját italok létrehozása, kategorizálása, szerkesztése, archiválása és a korábbi adatok megőrzése.
- Italkeresés, vonalkódos termékjavaslatok, Open Food Facts tartalék működése és bemenetellenőrzés.
- Alvásbejegyzések, felhasználói adatok elkülönítése, óraátállítás, heti összehasonlítások és személyes elemzések.
- Koffeintervezés, cutoff, what-if kalkulátor és élettartam-statisztikák.
- Jelszó-visszaállítási és e-mail-megerősítési tokenek biztonsága, lejárata és egyszer használhatósága.
- Feliratkozási hozzájárulások, külön leiratkozás, e-mail-nyelvválasztás, HTML-kódolás, előnézetek, levélsor és ismétlődésvédelem.
- Onboarding, lokalizáció és korábbi adatbázisverziók frissítése.

Az 5.0-s verziót egy korábbi 4.0-s éles adatbázis másolatával is ellenőriztük: a migráció sikerült, és **13 régi adattábla** meglévő értékei megmaradtak.

Tesztelés a repository gyökérkönyvtárából:

```bash
dotnet build
dotnet test Caffeine.Tests/Caffeine.Tests.csproj
node --test Caffeine.Tests/Frontend/*.test.cjs
```

*A tesztszám az utolsó igazolt helyi futást tükrözi, és a későbbi fejlesztésekkel változhat.*

### 🧠 A koffeinmodell működése

Minden rögzített ital külön-külön járul hozzá a becsült aktív koffeinmennyiséghez. A Koffi körülbelül **45 perces felszívódási szakasszal**, majd **5 órás felezési idejű exponenciális kiürüléssel** számol. Az egyes italok becsült hozzájárulásait összeadva állítja elő az összesített koffeingörbét.

Ugyanez a modell szolgál a lefekvéskor várható koffeinszint, a cutoff és a what-if kalkulátor becsléseinek alapjául. Az egyéni koffeinlebontás eltérhet a modelltől, ezért az eredmények **becslések, nem orvosi tanácsok**.

### 🌙 Hogyan működik az alváselemzés?

A Koffi a rendelkezésre álló személyes alvásadatokat elemzi, és nem állítja, hogy a koffein *okozta* valamelyik éjszaka alvásminőségét. Figyelembe veheti a lefekvéskor becsült koffeinszintet, az előző napi fogyasztást, az utolsó ital időpontját, az alvásértékelést, az elalvási nehézséget és az alvás hosszát.

| Rögzített éjszakák | Az elemzés részletessége |
|---|---|
| 10-nél kevesebb | Nincs elegendő adat személyes összefüggés kimondásához |
| 10–19 | Korai becslés, kevés adat |
| 20–29 | Kirajzolódó mintázat |
| 30 vagy több | Részletesebb személyes megfigyelések |

Az alkalmazás az adatok által alátámasztott irányt és erősséget mutatja be, a technikai részleteket külön is elérhetővé téve.

### 📧 E-mail-megerősítés, feliratkozások és kézbesítés

**E-mail-cím-megerősítés.** A rendszer biztonságos, egyszer használható, **24 óráig érvényes** megerősítő hivatkozást állít elő. A tokeneknek csak a SHA-256 hash-e kerül az adatbázisba. A token a sikeres megerősítéskor felhasználttá válik. A régi fiókok megerősítés nélkül is használhatók.

**Hozzájárulás.** Az újdonságértesítő és a heti statisztikai összefoglaló két külön **önkéntes** feliratkozás. Mindkettőhöz megerősített e-mail-cím szükséges. A levelek nyelve külön állítható, a leiratkozó linkek pedig kategóriánként működnek; a hozzájárulások változásait a rendszer naplózza.

**Heti összefoglaló.** Az alapértelmezett ütemezés **hétfő 08:00, Europe/Budapest időzóna**. A jelentés a legutóbbi jogosult, lezárt hétfő–vasárnap időszakot foglalja össze. A meglévő adatoktól függően összes koffeinmennyiséget, hétnapos napi átlagot, italszámot, előző heti összehasonlítást és alvásstatisztikákat tartalmazhat. A hiányzó bejegyzések nem jelentenek automatikusan nulla fogyasztást vagy nulla alvást.

**Újdonságkampányok.** A kiküldéshez adminisztrátori felületen kívül kezelt, lokalizált kampánytartalom, előnézet, sikeres tesztküldés és külön indítóparancs kell. A sorba állított levél küldésekor a rendszer újra ellenőrzi a felhasználó megerősítését és feliratkozását. A már lefutott kampány nem megy ki visszamenőleg a később feliratkozóknak.

**Levélküldési sor és biztonság.** Az e-mail-feladatok SQLite-adatbázisban tárolódnak. A rendszer többek között várakozó, elküldött, visszavont, sikertelen és bizonytalan állapotokat különböztet meg. Bizonytalan SMTP-eredménynél nem küld automatikusan újabb példányt; az ismétlődésvédelem felhasználónként és kampányonként is működik.

**Jelenlegi éles beállítás:**

```env
Mailing__Enabled=true
```

A valódi konfiguráció a szerver privát környezeti változóiban van, nem a GitHubon. A jelszó-visszaállítás a már meglévő, külön levélküldési folyamatot használja.

### 🚀 Üzemeltetés és telepítés

A Koffi Linux VPS-en fut Docker-konténerben, stabil .NET 10 futtatókörnyezettel. A **Caddy** HTTPS reverse proxyként működik. Az éles **SQLite-adatkönyvtár** és az **ASP.NET Core Data Protection kulcsai** a konténeren kívül tartósan vannak tárolva, így a frissítések során is megmaradnak.

Éles frissítésnél ajánlott:

1. Konzisztens SQLite-mentést készíteni, figyelembe véve az esetleges aktív WAL-állapotot, és megőrizni a Data Protection kulcsokat.
2. Lefordítani és letesztelni az új verziót, majd az EF Core migrációkat a projekt meglévő adatbázis-frissítési folyamatával alkalmazni.
3. A friss konténert az eredeti tartós adatkönyvtárral és privát konfigurációval elindítani.
4. Ellenőrizni a HTTP-elérhetőséget, az adatbázis integritását és a háttérszolgáltatásokat.
5. Visszaállítási lehetőséget fenntartani. Más környezetben a levelező worker bekapcsolása mindig külön megfontolt döntés legyen.

Az 5.0 a `Koffi50SleepMailing` migrációt használja a bővített alvásnaplóhoz, a saját italok archiválásához, az e-mail-megerősítéshez, a feliratkozásokhoz és a tartós levélsorhoz.

**Soha ne kerüljön verziókezelésbe** éles adatbázis, jelszó, SMTP-hozzáférés, Data Protection kulcs vagy privát környezeti fájl.

---

**Made with ☕ and C# · Koffi 5.0**
