# ☕ Koffi

🌍 **Live App:** [koffi.hu](https://koffi.hu)

*(Scroll down for the Hungarian version / Magyar verzió lejjebb)*

A modern, mobile-first ASP.NET Core MVC Progressive Web App (PWA) for tracking caffeine intake, estimating how caffeine levels change in your body over time, planning caffeine consumption around sleep, and discovering personal patterns between caffeine and sleep.

Koffi combines a biological caffeine model with daily and weekly history, favorites, custom drinks, streaks, caffeine-free days, sleep tracking, planning tools, lifetime statistics, notifications, guest mode, and user accounts.

## ✨ Features

* **User Accounts & Seamless Guest Mode:** Start tracking immediately without registration using a cookie-based guest identity. When you create an account, supported guest data can be transferred to your new profile.
* **Secure Authentication:** Cookie-based authentication with ASP.NET Core password hashing, security stamps, and user-specific data separation.
* **Password Recovery:** Registered users can securely request a password reset using expiring, single-use reset tokens.
* **Progressive Web App (PWA):** Installable on supported devices for an app-like mobile experience.
* **Web Push Notifications:** Optional notifications for streak reminders, morning sleep check-ins, caffeine cutoff reminders, and reaching a configured caffeine target.
* **Active Caffeine Calculation:** Estimates the amount of caffeine currently active in your body while accounting for multiple drinks consumed at different times.
* **Biological Caffeine Model:** Models caffeine using a 45-minute absorption phase followed by exponential elimination with a 5-hour half-life.
* **Sleep Readiness Indicator:** Estimates when your active caffeine level will fall below your configured personal target.
* **Planned Sleep Forecast:** Set your planned bedtime and see the estimated amount of caffeine that may still be active at that time.
* **Caffeine Planning:** Explore how a drink would affect your later caffeine level before deciding whether to log it.
* **Caffeine Cutoff Estimation:** Estimate how late you could consume a selected drink while staying near your configured caffeine target by bedtime.
* **What-If Calculator:** Simulate caffeine intake without modifying your diary unless you explicitly choose to save it.
* **Sleep Diary:** Record bedtime, wake time, sleep quality, difficulty falling asleep, awakenings, and optional notes.
* **Automatic Sleep Context:** Sleep entries are automatically connected with relevant caffeine data from the previous day.
* **Personal Sleep Insights:** Compare your own caffeine habits with recorded sleep quality and duration. Insights describe associations in your personal data and do not claim causation.
* **Streak Tracking:** Track your current and longest logging streak.
* **Caffeine-Free Days:** Explicitly mark a day as caffeine-free without breaking your streak.
* **Daily History & Date Navigation:** Browse previous days using previous/next controls or a built-in date picker.
* **Weekly Overview:** View caffeine consumption across calendar weeks, including weeks that cross month or year boundaries.
* **Lifetime Statistics:** Explore long-term caffeine statistics, frequent drinks, caffeine-free days, streaks, average caffeine timing, and other historical trends.
* **Activity Heatmap:** Visual overview of recent logging activity inspired by GitHub contribution graphs.
* **Favorites & Quick Add:** Save frequently consumed drinks with their usual serving size and add them instantly from the daily dashboard.
* **Undo Quick Add:** Accidentally added a favorite? Quickly undo the latest Quick Add action.
* **Custom Drink Creator:** Create your own beverages by entering their caffeine content in mg/100 ml. Custom drinks are stored per user and remain available for future logging.
* **Built-in Drink Database:** Quickly select common caffeinated drinks with localized names.
* **Drink Search:** Quickly filter the drink list using case- and accent-insensitive search.
* **Interactive Caffeine Curve:** Chart.js visualizes how your estimated active caffeine level changes throughout the selected day.
* **Detailed Drink Diary:** See logged drinks, serving sizes, caffeine amounts, and consumption times for each day.
* **Safe Data Migration:** Entity Framework Core migrations and a dedicated database-upgrade path preserve existing data while upgrading older database schemas.
* **Account & Data Deletion:** Permanently delete your account and its associated tracking data directly from the application.
* **Cookie Consent:** Built-in cookie consent notice for application cookies.
* **Localization:** Full Hungarian and English (HU/EN) interface with an integrated language switcher.
* **Modern Mobile-First UI:** Responsive dark interface with glassmorphism styling, designed primarily for phones while remaining usable on desktop.

## 🛠️ Tech Stack

* **Language / Runtime:** C# / .NET 10
* **Backend:** ASP.NET Core MVC
* **Database:** SQLite
* **ORM:** Entity Framework Core with Code-First migrations
* **Authentication:** ASP.NET Core Cookie Authentication
* **Password Security:** ASP.NET Core `PasswordHasher`
* **Frontend:** Razor Views, HTML5, JavaScript, Tailwind CSS
* **Charts:** Chart.js
* **Localization:** ASP.NET Core Localization & localized Razor resources
* **PWA:** Web App Manifest & Service Worker
* **Push Notifications:** Web Push with VAPID
* **Architecture:** Repository Pattern, Strategy Pattern, Dependency Injection
* **Data Upgrade:** EF Core migrations with custom legacy database reconciliation
* **Testing:** Automated ASP.NET Core integration tests and frontend JavaScript tests
* **Production:** Docker container deployment behind a Caddy reverse proxy with HTTPS
* **Production Database:** Persistent SQLite database mounted outside the application container

## 🧪 Testing

The project includes automated tests covering important application workflows such as:

* guest tracking and guest identity handling
* registration and login
* guest-to-account data transfer
* account deletion and data cleanup
* caffeine logging and calculations
* streak calculation
* caffeine-free days
* daily and weekly history
* favorites and Quick Add
* custom drinks
* drink search
* sleep diary and user isolation
* sleep statistics and personal insights
* caffeine planning and cutoff calculations
* what-if calculations without unintended data persistence
* lifetime statistics
* password reset security and expiry
* push subscription isolation
* localization
* invalid or tampered input handling
* database upgrades and preservation of existing data

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

Koffi can compare recorded sleep data with previous caffeine consumption.

For example, it can examine relationships between:

* estimated caffeine at bedtime and sleep rating;
* previous-day caffeine intake and sleep rating;
* last caffeine timing and difficulty falling asleep;
* caffeine consumption and sleep duration.

These insights are based on the user's own logged data.

Koffi intentionally describes these results as **associations rather than causes**, and avoids presenting conclusions when there is not enough data.

## 🔥 Streak Philosophy

Koffi's streak system rewards consistent tracking rather than caffeine consumption.

A day counts toward the streak when the user either:

* logs at least one caffeinated drink; or
* explicitly marks the day as caffeine-free.

This allows users to take caffeine-free days without losing their tracking streak.

## 🚀 Deployment

The production application runs in a Docker container on a Linux VPS.

Caddy acts as the reverse proxy and provides HTTPS, while the SQLite database is stored persistently outside the application container.

Database schema changes are handled through Entity Framework Core migrations and the application's dedicated upgrade path, allowing existing production data to be preserved during version upgrades.

---

---

# ☕ Koffi – Magyar verzió

🌍 **Éles alkalmazás:** [koffi.hu](https://koffi.hu)

A Koffi egy modern, mobilra optimalizált ASP.NET Core MVC Progressive Web App (PWA) a koffeinbevitel követésére, a szervezetben lévő aktív koffeinszint időbeli becslésére, a koffeinfogyasztás alváshoz történő tervezésére és a saját koffein- és alvási szokások közötti mintázatok megfigyelésére.

A Koffi a biológiai koffeinmodellt napi és heti előzményekkel, kedvencekkel, saját italokkal, streak rendszerrel, koffeinmentes napokkal, alvásnaplóval, tervezési eszközökkel, hosszú távú statisztikákkal, értesítésekkel, vendégmóddal és felhasználói fiókokkal egészíti ki.

## ✨ Funkciók

* **Felhasználói Fiókok és Vendégmód:** Regisztráció nélkül azonnal elkezdheted a koffein követését egy sütialapú vendégazonosítóval. Későbbi regisztrációnál a támogatott vendégadatok átvihetők az új profilba.
* **Biztonságos Bejelentkezés:** Sütialapú autentikáció, ASP.NET Core jelszó-hashelés, biztonsági stamp és felhasználónként elkülönített adatok.
* **Jelszó-visszaállítás:** A regisztrált felhasználók biztonságos, lejáró és egyszer használható tokennel állíthatják vissza jelszavukat.
* **Progressive Web App (PWA):** Támogatott eszközökön telepíthető, így mobilon alkalmazásszerű élményt nyújt.
* **Web Push Értesítések:** Opcionális értesítések streak emlékeztetőhöz, reggeli alvásnaplóhoz, koffein cutoffhoz és a beállított koffeincél eléréséhez.
* **Aktív Koffeinszint Számítás:** Megbecsüli az aktuálisan aktív koffein mennyiségét, egyszerre több, különböző időpontban elfogyasztott italt is figyelembe véve.
* **Biológiai Koffeinmodell:** 45 perces felszívódási szakasszal, majd 5 órás felezési idejű exponenciális kiürüléssel számol.
* **Alvásküszöb Előrejelzés:** Megbecsüli, mikor csökken az aktív koffeinszint a beállított személyes célérték alá.
* **Tervezett Alvásidő:** Beállíthatod, mikor tervezel lefeküdni, az alkalmazás pedig megmutatja az arra az időpontra becsült aktív koffeinszintedet.
* **Koffeintervező:** Rögzítés előtt megnézheted, hogyan befolyásolna egy ital egy későbbi koffeinszintet.
* **Koffein Cutoff:** A Koffi megbecsülheti, hogy egy adott italt körülbelül meddig fogyaszthatsz el úgy, hogy lefekvésre a beállított célérték közelébe kerülj.
* **What-If Kalkulátor:** Kipróbálhatsz különböző fogyasztási forgatókönyveket anélkül, hogy azok automatikusan bekerülnének a naplóba.
* **Alvásnapló:** Rögzíthető a lefekvés, ébredés, alvásminőség, elalvási nehézség, éjszakai ébredések és opcionális megjegyzés.
* **Automatikus Alvási Kontextus:** A Koffi automatikusan összekapcsolja az alvásbejegyzést az előző napi releváns koffeinadatokkal.
* **Személyes Alvási Trendek:** A saját koffeinfogyasztási és alvási adataid közötti összefüggéseket vizsgálja anélkül, hogy ok-okozati kapcsolatot állítana.
* **Streak Rendszer:** Követhető az aktuális és a valaha elért leghosszabb naplózási sorozat.
* **Koffeinmentes Nap:** Egy nap explicit koffeinmentesként jelölhető meg úgy, hogy közben a streak folytatódik.
* **Napi Előzmények és Dátumválasztás:** A nyilakkal vagy a beépített dátumválasztóval korábbi napokra is visszanézhetsz.
* **Heti Áttekintés:** Naptári hetek szerint is áttekintheted a koffeinbeviteledet, beleértve a hónap- és évhatáron átnyúló heteket.
* **Lifetime Statisztikák:** Hosszabb távon is áttekinthető a koffeinfogyasztás, a leggyakoribb italok, a koffeinmentes napok, streakek és a fogyasztás időzítése.
* **Aktivitási Heatmap:** GitHub-stílusú vizuális áttekintés a közelmúlt naplózási aktivitásáról.
* **Kedvencek és Gyors Hozzáadás:** A gyakran fogyasztott italokat a megszokott mennyiséggel együtt kedvencként mentheted, majd közvetlenül a napi nézetből egy mozdulattal rögzítheted őket.
* **Gyors Hozzáadás Visszavonása:** Ha véletlenül rögzítettél egy kedvenc italt, a legutóbbi gyors hozzáadás visszavonható.
* **Saját Italok:** Saját italokat hozhatsz létre a koffeintartalom mg/100 ml értékének megadásával. Ezek felhasználóhoz kötve kerülnek mentésre, és később újra felhasználhatók.
* **Beépített Itallista:** Gyakori koffeintartalmú italok gyors kiválasztása lokalizált elnevezésekkel.
* **Italkeresés:** Az itallista gyorsan szűrhető kis- és nagybetűktől, illetve magyar ékezetektől független kereséssel.
* **Interaktív Koffeingörbe:** A Chart.js grafikonon jeleníti meg a kiválasztott nap becsült aktív koffeinszintjének alakulását.
* **Részletes Italnapló:** Naponként megtekintheted a rögzített italokat, mennyiségeket, koffeintartalmat és fogyasztási időpontokat.
* **Biztonságos Adatbázis-frissítés:** Az Entity Framework Core migrációi és a külön adatbázis-upgrade folyamat lehetővé teszik a régebbi adatbázisok frissítését a meglévő adatok megőrzésével.
* **Fiók- és Adattörlés:** Az alkalmazásból véglegesen törölhető a felhasználói fiók és a hozzá tartozó követési adat.
* **Sütikezelés:** Beépített tájékoztató az alkalmazás által használt sütikről.
* **Többnyelvűség:** Teljes magyar és angol (HU/EN) felület beépített nyelvváltóval.
* **Modern, Mobile-First UI:** Reszponzív, sötét, glassmorphism stílusú felület, elsősorban mobilos használatra optimalizálva.

## 🛠️ Használt Technológiák

* **Nyelv / Runtime:** C# / .NET 10
* **Backend:** ASP.NET Core MVC
* **Adatbázis:** SQLite
* **ORM:** Entity Framework Core, Code-First migrációkkal
* **Autentikáció:** ASP.NET Core Cookie Authentication
* **Jelszóbiztonság:** ASP.NET Core `PasswordHasher`
* **Frontend:** Razor Views, HTML5, JavaScript, Tailwind CSS
* **Grafikonok:** Chart.js
* **Lokalizáció:** ASP.NET Core Localization és lokalizált Razor erőforrások
* **PWA:** Web App Manifest és Service Worker
* **Push Értesítések:** Web Push és VAPID
* **Architektúra:** Repository Pattern, Strategy Pattern, Dependency Injection
* **Adatbázis-frissítés:** EF Core migrációk egyedi legacy adatbázis-kompatibilitási logikával
* **Tesztelés:** Automatizált ASP.NET Core integrációs és frontend JavaScript tesztek
* **Production:** Docker konténer Linux VPS-en, Caddy reverse proxy és HTTPS mögött
* **Éles Adatbázis:** A konténertől függetlenül, tartósan tárolt SQLite adatbázis

## 🧪 Tesztelés

A projekt automatizált tesztekkel ellenőrzi többek között:

* a vendégmód és vendégazonosító működését
* a regisztrációt és bejelentkezést
* a vendégadatok felhasználói fiókba történő átvitelét
* a fióktörlést és kapcsolódó adatok törlését
* az italrögzítést és koffeinszámításokat
* a streak rendszert
* a koffeinmentes napokat
* a napi és heti előzményeket
* a kedvenceket és gyors hozzáadást
* a saját italokat
* az italkeresést
* az alvásnapló működését és felhasználói adatok elkülönítését
* az alvási statisztikákat és trendeket
* a koffeintervezőt és cutoff számítást
* a rögzítés nélküli what-if számításokat
* a lifetime statisztikákat
* a jelszó-visszaállítás biztonsági működését
* a push subscriptionök felhasználói elkülönítését
* a lokalizációt
* a hibás vagy manipulált bemenetek kezelését
* az adatbázis-frissítést és a meglévő adatok megőrzését

## 🧠 Koffeinmodell

Minden elfogyasztott ital külön járul hozzá a becsült aktív koffeinszinthez.

A modell:

1. **45 perces felszívódási szakasszal** számol, amely alatt az adott ital koffeinhozzájárulása a csúcsérték felé emelkedik;
2. a csúcs után **exponenciális kiürülést** alkalmaz;
3. **5 órás koffein-felezési időt** használ;
4. az összes rögzített ital hatását összeadja az aktuális aktív koffeinszint kiszámításához.

Ugyanezt a modellt használják a Koffi tervezési funkciói is a jövőbeli vagy lefekvéskori koffeinszint becslésére.

A számítás becslés, nem tekintendő orvosi tanácsnak.

## 🌙 Alvási Trendek

A Koffi össze tudja hasonlítani a rögzített alvási adatokat a korábbi koffeinfogyasztással.

Vizsgálható például:

* a lefekvéskor becsült koffeinszint és az alvásértékelés;
* az előző napi koffeinbevitel és az alvásértékelés;
* az utolsó koffein időpontja és az elalvási nehézség;
* a koffeinfogyasztás és az alvás hossza.

A megjelenített eredmények a felhasználó saját naplóadataiban található **összefüggéseket** mutatják, és nem jelentenek bizonyított ok-okozati kapcsolatot.

Kevés adat esetén a Koffi nem próbál komoly következtetést levonni.

## 🔥 Streak

A Koffi streak rendszere a következetes naplózást jutalmazza, nem a koffeinfogyasztást.

Egy nap akkor számít teljesítettnek, ha a felhasználó:

* legalább egy koffeines italt rögzített; vagy
* explicit megjelölte, hogy aznap nem ivott koffeint.

Így egy koffeinmentes nap ugyanúgy továbbviheti a streaket.

## 🚀 Üzemeltetés

Az éles alkalmazás Docker konténerben fut egy Linux VPS-en.

A Caddy reverse proxy biztosítja a külső elérést és a HTTPS-t, az SQLite adatbázis pedig a konténeren kívül, tartós tárhelyen található.

Az adatbázisséma változásait EF Core migrációk és az alkalmazás külön upgrade folyamata kezeli, így a verziófrissítések során a meglévő éles adatok megőrizhetők.
