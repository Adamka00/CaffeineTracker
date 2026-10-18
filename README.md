# ☕ Koffi

💙 **Support:** [buymeacoffee.com](https://buymeacoffee.com/adamka00)

🌍 **Live App:** [koffi.hu](https://koffi.hu)

**Current Version: 5.0**

*(Scroll down for the Hungarian version / Magyar verzió lejjebb)*

A modern, mobile-first ASP.NET Core MVC Progressive Web App (PWA) for tracking caffeine intake, estimating how caffeine levels change in your body over time, planning caffeine consumption around sleep, and discovering personal patterns between caffeine and sleep.

Koffi combines a biological caffeine model with daily and weekly history, favorites, editable custom drinks, streaks, caffeine-free days, detailed sleep tracking, weekly sleep statistics, planning tools, long-term statistics, barcode-assisted drink lookup, guided onboarding, guest mode, and user accounts.

**Koffi 5.0** expands the sleep diary, introduces categorized and editable personal drinks, improves the guest-to-account experience, and adds an email verification and opt-in mailing system.

The new email infrastructure is implemented, but **automated mailing is currently disabled in production** pending final configuration and verification.

## ✨ Features

* **User Accounts & Seamless Guest Mode:** Start tracking immediately without registration using a cookie-based guest identity. When you create an account, supported guest data can be transferred to your new profile.
* **Smart Guest Registration Suggestion (5.0):** After 10 logged drinks or 5 active tracking days, guest users may see a subtle, non-blocking suggestion to create an account. The suggestion can be dismissed for 14 days.
* **Secure Authentication:** Cookie-based authentication with ASP.NET Core password hashing, security stamps, and user-specific data separation.
* **Password Recovery by Email:** Registered users can securely request a password reset using expiring, single-use reset tokens delivered through the configured SMTP service.
* **Email Address Verification (5.0):** New accounts can verify their email addresses through secure, single-use links. Verification is not required to continue using the caffeine tracker.
* **Optional Email Subscriptions (5.0):** Verified users can independently subscribe to product updates and personalized weekly summaries. Both subscriptions are disabled by default and require explicit consent.
* **Email Preferences & Unsubscription (5.0):** Manage subscriptions separately, select the preferred email language, and unsubscribe from an individual email category without affecting the other.
* **Progressive Web App (PWA):** Installable on supported devices for an app-like mobile experience.
* **Smart PWA Install Guidance:** Koffi can suggest installation after repeated use and includes iOS-specific instructions for adding the app to the Home Screen.
* **Guided Onboarding:** Progressive onboarding introduces the main features without blocking normal use. It can be skipped and opened again later.
* **Help, Feature Explanations & What's New:** Built-in help pages, contextual `?` explanations, a version-aware What's New experience, and a dedicated support page make the app easier to discover and understand. Version 5.0 refines the placement and appearance of contextual help.
* **In-App Morning Sleep Reminder:** If the previous night's sleep entry is missing, Koffi can show a morning reminder inside the app. Dismissing it suppresses the reminder for the rest of the day.
* **Active Caffeine Calculation:** Estimates the amount of caffeine currently active in your body while accounting for multiple drinks consumed at different times.
* **Biological Caffeine Model:** Models caffeine using a 45-minute absorption phase followed by exponential elimination with a 5-hour half-life.
* **Sleep Readiness Indicator:** Estimates when your active caffeine level will fall below your configured personal target.
* **Planned Sleep Forecast:** Set your planned bedtime and see the estimated amount of caffeine that may still be active at that time.
* **Caffeine Planning:** Explore how a drink would affect your later caffeine level before deciding whether to log it.
* **Caffeine Cutoff Estimation:** Estimate how late you could consume a selected drink while staying near your configured caffeine target by bedtime.
* **What-If Calculator:** Simulate caffeine intake without modifying your diary unless you explicitly choose to save it.
* **Sleep Diary 2.0 (5.0):** A redesigned sleep section with three dedicated tabs: **Diary, Statistics, and Analysis**.
* **Detailed Sleep Logging (5.0):** Record actual sleep onset, final wake time, sleep quality, optional planned bedtime, difficulty falling asleep, number of nighttime awakenings, and personal notes.
* **Weekly Sleep Statistics (5.0):** Explore Monday-to-Sunday sleep statistics, including average, shortest and longest sleep duration, sleep quality, awakenings, difficulty falling asleep, and differences between planned and actual sleep onset.
* **Sleep Week Comparison (5.0):** Navigate between calendar weeks and compare available sleep data with the previous week.
* **Readable Sleep Durations (5.0):** Sleep duration is displayed in hours and minutes instead of decimal-hour values.
* **Time Zone-Aware Sleep Duration (5.0):** New sleep entries support accurate elapsed-time calculations across daylight-saving time changes, while preserving the meaning of previously stored records.
* **Automatic Sleep Context:** Sleep entries are automatically connected with relevant caffeine data from the previous day.
* **Personal Sleep Insights:** Compare your own caffeine habits with recorded sleep quality and duration using observation levels that become more informative as more nights are logged.
* **Plain-Language Sleep Analysis (5.0):** Personal observations are presented in understandable language, with technical calculation details available separately.
* **Careful Insight Wording:** Sleep insights describe associations, direction, and strength in your own data instead of making causal or medical claims.
* **Streak Tracking:** Track your current and longest logging streak.
* **Caffeine-Free Days:** Explicitly mark a day as caffeine-free without breaking your streak.
* **Daily History & Date Navigation:** Browse previous days using previous/next controls or a built-in date picker.
* **Weekly Overview:** View caffeine consumption across calendar weeks, including weeks that cross month or year boundaries.
* **Redesigned Lifetime Statistics:** Explore long-term caffeine statistics using clearer cards, trends, charts, frequent drinks, caffeine-free days, streaks, average caffeine timing, and year-based views where available.
* **Mobile-Friendly Activity Heatmap:** A redesigned activity overview keeps the GitHub-style heatmap concept while remaining practical on smaller screens.
* **Favorites & Quick Add:** Save frequently consumed drinks with their usual serving size and add them instantly from the daily dashboard.
* **Global Quick Add Button:** A fixed, safe-area-aware `+` action keeps drink logging within easy reach throughout the app.
* **Undo Quick Add:** Accidentally added a favorite? Quickly undo the latest Quick Add action.
* **Custom Drink Creator:** Create your own beverages by entering their caffeine content in mg/100 ml.
* **Personal Drink Management (5.0):** Edit the name, caffeine content, default serving size, and category of previously created personal drinks.
* **Custom Drink Categories (5.0):** Organize personal drinks as energy drinks, coffee, tea, soda, or other beverages. Category selection is also supported during manual and barcode-assisted drink creation.
* **Safe Drink Archiving (5.0):** Archive personal drinks instead of permanently deleting them. Archived drinks are removed from active selection while historical log entries and their recorded caffeine amounts remain intact.
* **Built-in Drink Database:** Quickly select common caffeinated drinks with localized names.
* **Barcode Scanner (Beta):** Scan product barcodes on supported secure browsers, review and edit the detected product before logging it, or fall back to manual entry when camera access is unavailable.
* **Open Food Facts Integration:** Barcode lookups can use Open Food Facts as an external suggestion source. External data is reviewable and Koffi remains usable if the service is unavailable.
* **Safe Product Lookup Fallbacks:** Missing or unreliable caffeine data is never guessed automatically; the user can review and complete the information manually.
* **Drink Search:** Quickly filter the drink list using case- and accent-insensitive search.
* **Interactive Caffeine Curve:** Chart.js visualizes how your estimated active caffeine level changes throughout the selected day.
* **Detailed Drink Diary:** See logged drinks, serving sizes, caffeine amounts, and consumption times for each day.
* **Personalized Weekly Email Summaries (5.0):** An opt-in system can generate weekly summaries of caffeine intake and available sleep data for completed calendar weeks.
* **Product Update Emails (5.0):** An opt-in announcement system supports localized product news and release notifications, with administrative preview and test-delivery workflows.
* **Persistent Email Queue (5.0):** Email jobs are stored in SQLite and processed by a background worker, with retry handling, deduplication, and delivery-state tracking.
* **Safe Data Migration:** Entity Framework Core migrations and a dedicated database-upgrade path preserve existing data while upgrading older database schemas.
* **Account & Data Deletion:** Permanently delete your account and its associated tracking data directly from the application.
* **Cookie Consent:** Built-in cookie consent notice for application cookies.
* **Localization:** Full Hungarian, English, and German (HU/EN/DE) interface with an integrated language switcher. The 5.0 sleep, drink, and email experiences are localized as well.
* **Modern Mobile-First UI:** Refined responsive dark interface with glassmorphism styling, consistent components, and improved phone-first usability. Version 5.0 preserves the established visual identity rather than replacing the 4.0 design.

## 🛠️ Tech Stack

* **Language / Runtime:** C# / .NET 10
* **Backend:** ASP.NET Core MVC
* **Database:** SQLite
* **ORM:** Entity Framework Core with Code-First migrations
* **Authentication:** ASP.NET Core Cookie Authentication
* **Password Security:** ASP.NET Core `PasswordHasher`
* **Email Transport:** MailKit / SMTP with STARTTLS
* **Production Mail Infrastructure:** Postfix with OpenDKIM
* **Email Security:** SHA-256-hashed verification tokens, single-use links, ASP.NET Core Data Protection, CSRF protection, and rate limiting
* **Email Processing:** SQLite-backed persistent mail queue, hosted background worker, and administrative CLI
* **Frontend:** Razor Views, HTML5, JavaScript, Tailwind CSS
* **Charts:** Chart.js
* **Barcode Scanning:** ZXing JavaScript
* **External Product Data:** Open Food Facts
* **Localization:** ASP.NET Core Localization & localized Razor resources (HU/EN/DE)
* **PWA:** Web App Manifest & Service Worker
* **Architecture:** Repository Pattern, Strategy Pattern, Dependency Injection
* **Data Upgrade:** EF Core migrations with custom legacy database reconciliation
* **Testing:** Automated ASP.NET Core integration tests and frontend JavaScript tests
* **Production:** Docker container deployment behind a Caddy reverse proxy with HTTPS
* **Production Database:** Persistent SQLite database mounted outside the application container
* **Container Runtime:** Stable .NET 10 SDK and ASP.NET Core runtime images

## 🧪 Testing

The project currently includes **166 automated .NET tests** and **7 frontend JavaScript tests**, covering important application workflows such as:

* guest tracking and guest identity handling
* registration and login
* guest-to-account data transfer
* guest registration suggestions and dismissal periods
* account deletion and data cleanup
* caffeine logging and calculations
* streak calculation
* caffeine-free days
* daily and weekly history
* favorites and Quick Add
* personal drink creation, editing, categorization, and archiving
* archived-drink restrictions and preservation of historical caffeine values
* drink search and filtering
* barcode lookup and product review flows
* Open Food Facts fallbacks
* sleep diary creation, editing, and user isolation
* sleep duration calculations, including daylight-saving time cases
* weekly sleep statistics and week comparisons
* optional sleep metrics and missing-data handling
* plain-language personal sleep observations
* caffeine planning and cutoff calculations
* what-if calculations without unintended data persistence
* lifetime statistics
* password reset security and expiry
* email verification token security, expiry, and single-use behavior
* email subscription consent and independent unsubscription
* weekly email summary generation
* email campaign previews and test-send restrictions
* email rendering and HTML escaping
* persistent email queue behavior, retries, and deduplication
* onboarding and experience-state behavior
* localization, including German
* invalid or tampered input handling
* database upgrades and preservation of existing data

The 5.0 release was also tested against a copy of an existing 4.0 production database. The migration completed successfully, and all existing values across **13 legacy data tables** were preserved.

### Run Tests

```bash
dotnet build
dotnet test Caffeine.Tests/Caffeine.Tests.csproj
node --test Caffeine.Tests/Frontend/*.test.cjs
```

## 🧠 Caffeine Model

Each consumed drink contributes independently to the estimated active caffeine level.

The model uses:

1. a **45-minute absorption phase**, during which the caffeine contribution rises toward its peak;
2. an **exponential elimination phase** after the peak;
3. a **5-hour caffeine half-life**;
4. superposition of all logged drinks to calculate the combined active level.

The same model is also used by Koffi's planning features to estimate caffeine remaining at a future time or at planned bedtime.

The model is intended as an estimate and should not be treated as medical advice.

## 🌙 Sleep Insights

Koffi 5.0 organizes sleep tracking into three dedicated sections:

* **Diary:** Record and review individual nights, including actual sleep onset, final wake time, subjective sleep quality, nighttime awakenings, difficulty falling asleep, and optional notes.
* **Statistics:** Review sleep duration in hours and minutes, average sleep quality, minimum and maximum duration, optional averages, daily sleep duration, and comparisons between completed or selected calendar weeks.
* **Analysis:** Explore personal relationships between caffeine consumption and sleep with easy-to-understand explanations.

Koffi can examine relationships between:

* estimated caffeine at bedtime and sleep rating;
* previous-day caffeine intake and sleep rating;
* last caffeine timing and difficulty falling asleep;
* caffeine consumption and sleep duration.

The insight system adapts its presentation to the amount of personal sleep data available:

* **Fewer than 10 nights:** insufficient data for personal relationship conclusions;
* **10–19 nights:** early estimate / limited data;
* **20–29 nights:** pattern emerging;
* **30+ nights:** more detailed personal observations.

Technical calculation details are available separately for users who want to understand the methodology.

Koffi intentionally describes these results as **associations rather than causes**, communicates the direction and strength of observed relationships, and avoids medical conclusions.

## 📧 Email & Notifications

Koffi 5.0 introduces an optional email communication system built on the existing MailKit and Postfix infrastructure.

### Email Verification

New accounts can request and confirm ownership of their email address using secure, expiring, single-use links.

Verification tokens are stored as SHA-256 hashes, have a 24-hour validity period, and are consumed only after explicit confirmation.

Email verification does not block ordinary caffeine tracking. Existing accounts are not automatically marked as verified and can continue using their accounts normally.

### Optional Subscriptions

Two independent subscriptions are available:

* **Product Updates:** News, important changes, and release announcements.
* **Weekly Personal Statistics:** A summary of the user's caffeine intake and available sleep statistics for the preceding completed calendar week.

Both subscriptions are **off by default**, and both require a verified email address.

Users can manage their preferences independently and unsubscribe from one category without disabling the other.

Emails support Hungarian, English, and German, with both HTML and plain-text versions.

### Weekly Summaries

Weekly summaries use Monday-to-Sunday periods and are designed to run on a configurable schedule, defaulting to Monday at 08:00 in the `Europe/Budapest` time zone.

The summaries can include total logged caffeine intake, a seven-day average based on recorded intake, logged drink counts, comparisons with the previous week where enough data exists, and available sleep statistics.

Missing sleep entries and unrecorded days are handled carefully rather than being presented as confirmed zero values.

### Delivery & Administration

Emails are processed through a persistent SQLite-backed queue with delivery-state tracking, deduplication, and controlled retries.

Release campaigns are managed through local administrative commands, not a public campaign-sending endpoint. The workflow supports preparing localized content, previewing emails, sending a test message, and explicitly confirming a campaign before scheduling it for subscribed recipients.

### Current Production Status

**The email features are implemented in Koffi 5.0, but automated mailing is currently disabled in production.**

```env
Mailing__Enabled=false
```

The mailing worker must be enabled explicitly after production verification of SMTP delivery, email confirmation, subscription preferences, and test messages.

The existing password-recovery email mechanism is maintained separately.

No product update or weekly-summary campaign is sent automatically while the mailing worker is disabled.

## 🔥 Streak Philosophy

Koffi's streak system rewards consistent tracking rather than caffeine consumption.

A day counts toward the streak when the user either:

* logs at least one caffeinated drink; or
* explicitly marks the day as caffeine-free.

This allows users to take caffeine-free days without losing their tracking streak.

## 🚀 Deployment

The production application runs in a Docker container on a Linux VPS using stable .NET 10 images.

Caddy acts as the reverse proxy and provides HTTPS, while the SQLite database and ASP.NET Core Data Protection keys are stored persistently outside the application container.

The existing SMTP infrastructure uses Postfix, OpenDKIM, and MailKit, with the email transport configuration supplied through private environment variables.

Database schema changes are handled through Entity Framework Core migrations and the application's dedicated upgrade path, allowing existing production data to be preserved during version upgrades.

Version 5.0 introduces the `Koffi50SleepMailing` migration for additional sleep timestamps, personal drink archiving, email verification, subscription preferences, and persistent email jobs.

A production upgrade should preserve the existing database and Data Protection keys, create a consistent database backup, apply migrations, and verify the application before enabling new mailing functionality.

Automated mailing remains disabled unless explicitly configured:

```env
Mailing__Enabled=false
```

Sensitive settings, SMTP credentials, encryption keys, and production databases must remain outside version control.

---

---

# ☕ Koffi – Magyar verzió

💙 **Támogatás:** [buymeacoffee.com](https://buymeacoffee.com/adamka00)

🌍 **Éles alkalmazás:** [koffi.hu](https://koffi.hu)

**Aktuális verzió: 5.0**

A Koffi egy modern, mobilra optimalizált ASP.NET Core MVC Progressive Web App (PWA) a koffeinbevitel követésére, a szervezetben lévő aktív koffeinszint időbeli becslésére, a koffeinfogyasztás alváshoz történő tervezésére és a saját koffein- és alvási szokások közötti mintázatok megfigyelésére.

A Koffi a biológiai koffeinmodellt napi és heti előzményekkel, kedvencekkel, szerkeszthető saját italokkal, streak rendszerrel, koffeinmentes napokkal, részletes alvásnaplóval, heti alvásstatisztikákkal, tervezési eszközökkel, hosszú távú statisztikákkal, vonalkódos italkereséssel, onboardinggal, vendégmóddal és felhasználói fiókokkal egészíti ki.

A **Koffi 5.0** kibővített alvásnaplót, kategorizálható és szerkeszthető saját italokat, intelligensebb vendégregisztrációs ajánlót, valamint e-mail-cím-megerősítést és önkéntes feliratkozásokon alapuló levelezési rendszert vezet be.

Az új e-mailes infrastruktúra elkészült, de az **automatikus levélküldés jelenleg még ki van kapcsolva az éles környezetben**, amíg a végső beállítás és ellenőrzés meg nem történik.

## ✨ Funkciók

* **Felhasználói Fiókok és Vendégmód:** Regisztráció nélkül azonnal elkezdheted a koffein követését egy sütialapú vendégazonosítóval. Későbbi regisztrációnál a támogatott vendégadatok átvihetők az új profilba.
* **Intelligens Regisztrációs Ajánló (5.0):** Vendégként 10 rögzített ital vagy 5 aktív nap után diszkrét, a használatot nem akadályozó regisztrációs javaslat jelenhet meg. Az ajánló 14 napra elrejthető.
* **Biztonságos Bejelentkezés:** Sütialapú autentikáció, ASP.NET Core jelszó-hashelés, biztonsági stamp és felhasználónként elkülönített adatok.
* **Jelszó-visszaállítás E-mailben:** A regisztrált felhasználók biztonságos, lejáró és egyszer használható tokennel, SMTP-n kézbesített levél segítségével állíthatják vissza jelszavukat.
* **E-mail-cím Megerősítése (5.0):** Az új felhasználók biztonságos, egyszer használható hivatkozással erősíthetik meg e-mail-címüket. A naplózás és a fiók használata megerősítés nélkül is lehetséges.
* **Önkéntes E-mail-feliratkozások (5.0):** A megerősített e-mail-címmel rendelkező felhasználók egymástól függetlenül kérhetnek újdonságértesítéseket és személyes heti összefoglalókat. Mindkét lehetőség alapértelmezetten ki van kapcsolva.
* **E-mail-beállítások és Leiratkozás (5.0):** A két feliratkozás külön kezelhető, megadható a levelek nyelve, és az egyik értesítési csatornáról a másik megtartásával is le lehet iratkozni.
* **Progressive Web App (PWA):** Támogatott eszközökön telepíthető, így mobilon alkalmazásszerű élményt nyújt.
* **Intelligens PWA Telepítési Javaslat:** A Koffi több használat után felajánlhatja a telepítést, iOS-en pedig külön útmutatót ad a Megosztás → Hozzáadás a Főképernyőhöz lépésekhez.
* **Interaktív Bevezető:** A fő funkciókat fokozatos onboarding mutatja be. Átugorható, és később újra megnyitható.
* **Súgó, Funkciómagyarázatok és Újdonságok:** Beépített súgóoldalak, kontextusos `?` magyarázatok, verzióhoz kötött újdonságok és külön támogatási oldal segíti az eligazodást. Az 5.0 finomítja a súgóikonok elhelyezését és megjelenését.
* **Reggeli Alvásnapló Emlékeztető:** Ha az előző éjszakához nincs alvásbejegyzés, a Koffi reggel az alkalmazáson belül emlékeztethet rá. A „Most nem” választás aznapra elnémítja.
* **Aktív Koffeinszint Számítás:** Megbecsüli az aktuálisan aktív koffein mennyiségét, egyszerre több, különböző időpontban elfogyasztott italt is figyelembe véve.
* **Biológiai Koffeinmodell:** 45 perces felszívódási szakasszal, majd 5 órás felezési idejű exponenciális kiürüléssel számol.
* **Alvásküszöb Előrejelzés:** Megbecsüli, mikor csökken az aktív koffeinszint a beállított személyes célérték alá.
* **Tervezett Alvásidő:** Beállíthatod, mikor tervezel lefeküdni, az alkalmazás pedig megmutatja az arra az időpontra becsült aktív koffeinszintedet.
* **Koffeintervező:** Rögzítés előtt megnézheted, hogyan befolyásolna egy ital egy későbbi koffeinszintet.
* **Koffein Cutoff:** A Koffi megbecsülheti, hogy egy adott italt körülbelül meddig fogyaszthatsz el úgy, hogy lefekvésre a beállított célérték közelébe kerülj.
* **What-If Kalkulátor:** Kipróbálhatsz különböző fogyasztási forgatókönyveket anélkül, hogy azok automatikusan bekerülnének a naplóba.
* **Alvásnapló 2.0 (5.0):** Az alváskövetés három különálló fülre tagolódik: **Napló, Statisztika és Elemzés**.
* **Részletes Alvásrögzítés (5.0):** Rögzíthető a tényleges elalvás és végső ébredés időpontja, az alvásminőség, az opcionális tervezett lefekvés, az elalvási nehézség, az éjszakai felébredések száma és a megjegyzés.
* **Heti Alvásstatisztikák (5.0):** Hétfőtől vasárnapig áttekinthető az átlagos, legrövidebb és leghosszabb alvásidő, az alvásminőség, a felébredések, az elalvási nehézség és a tervezett időponttól való eltérés.
* **Hetek Összehasonlítása (5.0):** A korábbi naptári hetek között is lehet navigálni, és az elérhető adatok az előző héttel összevethetők.
* **Óra–Perc Alapú Alvásidő (5.0):** Az alvás hossza mindenhol érthető, például „7 óra 30 perc” formában jelenik meg.
* **Időzóna- és Óraátállítás-kezelés (5.0):** Az új alvásbejegyzések a nyári és téli időszámítás közötti váltásnál is pontos eltelt időt tudnak számolni, miközben a korábbi bejegyzések jelentése változatlan marad.
* **Automatikus Alvási Kontextus:** A Koffi automatikusan összekapcsolja az alvásbejegyzést az előző napi releváns koffeinadatokkal.
* **Személyes Alvási Trendek:** A saját koffein- és alvásadatok alapján fokozatosan részletesebbé váló megfigyelések jelenhetnek meg.
* **Közérthető Alváselemzés (5.0):** A személyes összefüggéseket egyszerűbb, magyarázó szövegek mutatják be, a technikai számítási részletek külön érhetők el.
* **Óvatos Következtetések:** A Koffi összefüggéseket, irányt és erősséget ír le, nem állít ok-okozati vagy orvosi következtetést.
* **Streak Rendszer:** Követhető az aktuális és a valaha elért leghosszabb naplózási sorozat.
* **Koffeinmentes Nap:** Egy nap explicit koffeinmentesként jelölhető meg úgy, hogy közben a streak folytatódik.
* **Napi Előzmények és Dátumválasztás:** A nyilakkal vagy a beépített dátumválasztóval korábbi napokra is visszanézhetsz.
* **Heti Áttekintés:** Naptári hetek szerint is áttekintheted a koffeinbeviteledet, beleértve a hónap- és évhatáron átnyúló heteket.
* **Újratervezett Lifetime Statisztikák:** Átláthatóbb kártyákon, trendeken és grafikonokon jelennek meg a hosszú távú adatok, gyakori italok, koffeinmentes napok, streakek és fogyasztási időzítések.
* **Mobilbarát Aktivitási Heatmap:** Megmaradt a GitHub-szerű aktivitási nézet, de mobilon nem kényszerít hatalmas vízszintes görgetésre.
* **Kedvencek és Gyors Hozzáadás:** A gyakran fogyasztott italokat a megszokott mennyiséggel együtt kedvencként mentheted, majd közvetlenül a napi nézetből egy mozdulattal rögzítheted őket.
* **Globális Gyors Hozzáadás:** A fix, safe-area-aware `+` gombbal az italrögzítés az alkalmazás több pontjáról is gyorsan elérhető.
* **Gyors Hozzáadás Visszavonása:** Ha véletlenül rögzítettél egy kedvenc italt, a legutóbbi gyors hozzáadás visszavonható.
* **Saját Italok Létrehozása:** Saját italokat hozhatsz létre a koffeintartalom mg/100 ml értékének megadásával.
* **Saját Italok Kezelése (5.0):** A már létrehozott italok neve, koffeintartalma, alapértelmezett kiszerelése és kategóriája utólag szerkeszthető.
* **Saját Italkategóriák (5.0):** Az italok energiaital, kávé, tea, üdítő és egyéb kategóriába sorolhatók. Kategória választható kézi vagy vonalkódos rögzítésnél is.
* **Biztonságos Italarchiválás (5.0):** A saját italok végleges törlés helyett archiválhatók. Eltűnnek az aktív listákból, de a korábbi naplóbejegyzések és azok rögzített koffeinmennyiségei megmaradnak.
* **Beépített Itallista:** Gyakori koffeintartalmú italok gyors kiválasztása lokalizált elnevezésekkel.
* **Vonalkódolvasó (Béta):** Támogatott, biztonságos böngészőkben a kamera segítségével beolvasható a termék vonalkódja. A találat mentés előtt ellenőrizhető és szerkeszthető, kamera nélkül pedig kézi bevitelre lehet váltani.
* **Open Food Facts Integráció:** A vonalkódos keresés külső javaslatként használhatja az Open Food Facts adatbázisát. A Koffi nem tekinti automatikusan hitelesnek a külső adatot, és akkor is használható marad, ha a szolgáltatás nem érhető el.
* **Biztonságos Termékkeresési Fallback:** Ha a koffeintartalom nem megbízható vagy hiányzik, a Koffi nem talál ki helyette értéket; a felhasználó ellenőrizheti és kiegészítheti az adatokat.
* **Italkeresés:** Az itallista gyorsan szűrhető kis- és nagybetűktől, illetve magyar ékezetektől független kereséssel.
* **Interaktív Koffeingörbe:** A Chart.js grafikonon jeleníti meg a kiválasztott nap becsült aktív koffeinszintjének alakulását.
* **Részletes Italnapló:** Naponként megtekintheted a rögzített italokat, mennyiségeket, koffeintartalmat és fogyasztási időpontokat.
* **Személyes Heti E-mail-összefoglalók (5.0):** Az önkéntesen bekapcsolható rendszer a lezárt naptári hetek koffein- és elérhető alvásadataiból állít össze személyre szabott összefoglalókat.
* **Újdonságértesítő E-mailek (5.0):** Önálló feliratkozáson alapuló kiadási és újdonságértesítések, adminisztrátori előnézeti és tesztküldési folyamattal.
* **Tartós Levélküldési Sor (5.0):** Az e-mail-feladatok SQLite-adatbázisban tárolódnak, háttérfolyamat dolgozza fel őket, állapotkövetéssel, ismétlődésvédelemmel és ellenőrzött újrapróbálkozással.
* **Biztonságos Adatbázis-frissítés:** Az Entity Framework Core migrációi és a külön adatbázis-upgrade folyamat lehetővé teszik a régebbi adatbázisok frissítését a meglévő adatok megőrzésével.
* **Fiók- és Adattörlés:** Az alkalmazásból véglegesen törölhető a felhasználói fiók és a hozzá tartozó követési adat.
* **Sütikezelés:** Beépített tájékoztató az alkalmazás által használt sütikről.
* **Többnyelvűség:** Teljes magyar, angol és német (HU/EN/DE) felület beépített nyelvváltóval. Az 5.0 új alvás-, ital- és e-mailes felületei is lokalizáltak.
* **Modern, Mobile-First UI:** Finomhangolt reszponzív, sötét, glassmorphism stílusú felület, egységesebb komponensekkel és javított mobilos használhatósággal. Az 5.0 a megszokott 4.0-s arculatot megőrizte.

## 🛠️ Használt Technológiák

* **Nyelv / Runtime:** C# / .NET 10
* **Backend:** ASP.NET Core MVC
* **Adatbázis:** SQLite
* **ORM:** Entity Framework Core, Code-First migrációkkal
* **Autentikáció:** ASP.NET Core Cookie Authentication
* **Jelszóbiztonság:** ASP.NET Core `PasswordHasher`
* **E-mail-küldés:** MailKit / SMTP, STARTTLS kapcsolattal
* **Éles Levelezési Infrastruktúra:** Postfix és OpenDKIM
* **E-mail-biztonság:** SHA-256-tal hashelt megerősítő tokenek, egyszer használható linkek, ASP.NET Core Data Protection, CSRF-védelem és rate limiting
* **Levélfeldolgozás:** Tartós SQLite-alapú levélsor, háttérben futó worker és adminisztrátori CLI
* **Frontend:** Razor Views, HTML5, JavaScript, Tailwind CSS
* **Grafikonok:** Chart.js
* **Vonalkódolvasás:** ZXing JavaScript
* **Külső Termékadatok:** Open Food Facts
* **Lokalizáció:** ASP.NET Core Localization és lokalizált Razor erőforrások (HU/EN/DE)
* **PWA:** Web App Manifest és Service Worker
* **Architektúra:** Repository Pattern, Strategy Pattern, Dependency Injection
* **Adatbázis-frissítés:** EF Core migrációk egyedi legacy adatbázis-kompatibilitási logikával
* **Tesztelés:** Automatizált
