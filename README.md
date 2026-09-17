# ☕ Caffeine Tracker

🌍 **Live App:** [koffein.adamka00.hu](https://koffein.adamka00.hu)

*(Scroll down for the Hungarian version / Magyar verzió lejjebb)*

A modern, mobile-first ASP.NET Core MVC Progressive Web App (PWA) for tracking caffeine intake and estimating how caffeine levels change in your body over time. It combines a biological caffeine model with daily and weekly history, favorites, custom drinks, sleep-time predictions, guest tracking, and user accounts.

## ✨ Features

* **User Accounts & Seamless Guest Mode:** Start tracking immediately without registration using a cookie-based guest identity. When you create an account, your existing guest logs, custom drinks, and favorites can be transferred to your new profile.
* **Secure Authentication:** Cookie-based authentication with securely hashed passwords and user-specific data separation.
* **Progressive Web App (PWA):** Installable on supported devices for a more app-like mobile experience.
* **Active Caffeine Calculation:** Estimates the amount of caffeine currently active in your body while accounting for multiple drinks consumed at different times.
* **Biological Caffeine Model:** Models caffeine using a 45-minute absorption phase followed by exponential elimination with a 5-hour half-life.
* **Sleep Readiness Indicator:** Estimates when your active caffeine level will fall below the configured 25 mg sleep threshold.
* **Planned Sleep Forecast:** Set your planned bedtime and see the estimated amount of caffeine that will still be active at that time.
* **Daily History & Date Navigation:** Browse previous days using previous/next controls or a built-in date picker without losing your historical caffeine data.
* **Weekly Overview:** View caffeine consumption across calendar weeks, including weeks that cross month or year boundaries.
* **Favorites & Quick Add:** Save frequently consumed drinks with their usual serving size and add them instantly from the daily dashboard.
* **Undo Quick Add:** Accidentally added a favorite? Quickly undo the latest Quick Add action.
* **Custom Drink Creator:** Create your own beverages by entering their caffeine content in mg/100 ml. Custom drinks are stored per user and remain available for future logging.
* **Built-in Drink Database:** Quickly select common caffeinated drinks with localized names.
* **Interactive Caffeine Curve:** Chart.js visualizes how your estimated active caffeine level changes throughout the selected day.
* **Detailed Drink Diary:** See logged drinks, serving sizes, caffeine amounts, and consumption times for each day.
* **Safe Data Migration:** Entity Framework Core migrations and a dedicated database-upgrade path preserve existing data while upgrading older database schemas.
* **Account & Data Deletion:** Permanently delete your account and its associated tracking data directly from the application.
* **Cookie Consent:** Built-in cookie consent notice for application cookies.
* **Localization:** Full Hungarian and English (HU/EN) interface with an integrated language switcher.
* **Modern Mobile-First UI:** Responsive dark interface with glassmorphism styling, designed primarily for phones while remaining usable on desktop.

## 🛠️ Tech Stack

* **Language / Runtime:** C# / .NET
* **Backend:** ASP.NET Core MVC
* **Database:** SQLite
* **ORM:** Entity Framework Core with Code-First migrations
* **Authentication:** ASP.NET Core Cookie Authentication
* **Password Security:** BCrypt password hashing
* **Frontend:** Razor Views, HTML5, Tailwind CSS (CDN)
* **Charts:** Chart.js
* **Localization:** ASP.NET Core Localization & localized Razor resources
* **PWA:** Web App Manifest & Service Worker
* **Architecture:** Repository Pattern, Strategy Pattern, Dependency Injection
* **Data Upgrade:** EF Core migrations with custom legacy database reconciliation
* **Testing:** Automated ASP.NET Core integration tests
* **Production:** Docker container deployment behind a Caddy reverse proxy with HTTPS
* **Production Database:** Persistent SQLite database mounted outside the application container

## 🧪 Testing

The project includes automated integration tests covering important application workflows such as:

* guest tracking and guest identity handling
* registration and login
* guest-to-account data transfer
* caffeine logging and calculations
* daily and weekly history
* favorites and Quick Add
* custom drinks
* localization
* invalid/tampered input handling
* database upgrades and preservation of existing data

## 🧠 Caffeine Model

Each consumed drink contributes independently to the estimated active caffeine level.

The model uses:

1. a **45-minute absorption phase**, during which the caffeine contribution rises toward its peak;
2. an **exponential elimination phase** after the peak;
3. a **5-hour caffeine half-life**;
4. superposition of all logged drinks to calculate the combined active level.

The model is intended as an estimate and should not be treated as medical advice.

## 🚀 Deployment

The production application runs in a Docker container on a Linux VPS. Caddy acts as the reverse proxy and provides HTTPS, while the SQLite database is stored persistently outside the container.

Database schema changes are handled through EF Core migrations and the application's dedicated migration mode, allowing existing production data to be preserved during upgrades.

---

---

# ☕ Caffeine Tracker (Magyar verzió)

🌍 **Éles alkalmazás:** [koffein.adamka00.hu](https://koffein.adamka00.hu)

Egy modern, mobilra optimalizált ASP.NET Core MVC Progressive Web App (PWA) a koffeinbevitel követésére és a szervezetben lévő aktív koffeinszint időbeli becslésére. A biológiai koffeinmodellt napi és heti előzményekkel, kedvencekkel, saját italokkal, alvásidő-előrejelzéssel, vendégmóddal és felhasználói fiókokkal egészíti ki.

## ✨ Funkciók

* **Felhasználói Fiókok és Vendégmód:** Regisztráció nélkül azonnal elkezdheted a koffein követését egy sütialapú vendégazonosítóval. Későbbi regisztrációnál a vendégként rögzített naplóbejegyzések, saját italok és kedvencek átvihetők az új profilba.
* **Biztonságos Bejelentkezés:** Sütialapú autentikáció, biztonságosan hash-elt jelszavak és felhasználónként elkülönített adatok.
* **Progressive Web App (PWA):** Támogatott eszközökön telepíthető, így mobilon alkalmazásszerű élményt nyújt.
* **Aktív Koffeinszint Számítás:** Megbecsüli az aktuálisan aktív koffein mennyiségét, egyszerre több, különböző időpontban elfogyasztott italt is figyelembe véve.
* **Biológiai Koffeinmodell:** 45 perces felszívódási szakasszal, majd 5 órás felezési idejű exponenciális kiürüléssel számol.
* **Alvásküszöb Előrejelzés:** Megbecsüli, mikor csökken az aktív koffeinszint a beállított 25 mg-os alvásküszöb alá.
* **Tervezett Alvásidő:** Beállíthatod, mikor tervezel lefeküdni, az alkalmazás pedig megmutatja az arra az időpontra becsült aktív koffeinszintedet.
* **Napi Előzmények és Dátumválasztás:** A nyilakkal vagy a beépített dátumválasztóval korábbi napokra is visszanézhetsz.
* **Heti Áttekintés:** Naptári hetek szerint is áttekintheted a koffeinbeviteledet, beleértve a hónap- és évhatáron átnyúló heteket.
* **Kedvencek és Gyors Hozzáadás:** A gyakran fogyasztott italokat a megszokott mennyiséggel együtt kedvencként mentheted, majd közvetlenül a napi nézetből egy mozdulattal rögzítheted őket.
* **Gyors Hozzáadás Visszavonása:** Ha véletlenül rögzítettél egy kedvenc italt, a legutóbbi gyors hozzáadás visszavonható.
* **Saját Italok:** Saját italokat hozhatsz létre a koffeintartalom mg/100 ml értékének megadásával. Ezek felhasználóhoz kötve kerülnek mentésre, és később újra felhasználhatók.
* **Beépített Itallista:** Gyakori koffeintartalmú italok gyors kiválasztása lokalizált elnevezésekkel.
* **Interaktív Koffeingörbe:** A Chart.js grafikonon jeleníti meg a kiválasztott nap becsült aktív koffeinszintjének alakulását.
* **Részletes Italnapló:** Naponként megtekintheted a rögzített italokat, mennyiségeket, koffeintartalmat és fogyasztási időpontokat.
* **Biztonságos Adatbázis-frissítés:** Az Entity Framework Core migrációi és a külön adatbázis-upgrade folyamat lehetővé teszik a régebbi adatbázisok frissítését a meglévő adatok megőrzésével.
* **Fiók- és Adattörlés:** Az alkalmazásból véglegesen törölhető a felhasználói fiók és a hozzá tartozó követési adat.
* **Sütikezelés:** Beépített tájékoztató az alkalmazás által használt sütikről.
* **Többnyelvűség:** Teljes magyar és angol (HU/EN) felület beépített nyelvváltóval.
* **Modern, Mobile-First UI:** Reszponzív, sötét, glassmorphism stílusú felület, elsősorban mobilos használatra optimalizálva.

## 🛠️ Használt Technológiák

* **Nyelv / Runtime:** C# / .NET
* **Backend:** ASP.NET Core MVC
* **Adatbázis:** SQLite
* **ORM:** Entity Framework Core, Code-First migrációkkal
* **Autentikáció:** ASP.NET Core Cookie Authentication
* **Jelszóbiztonság:** BCrypt jelszó-hashelés
* **Frontend:** Razor Views, HTML5, Tailwind CSS (CDN)
* **Grafikonok:** Chart.js
* **Lokalizáció:** ASP.NET Core Localization és lokalizált Razor erőforrások
* **PWA:** Web App Manifest és Service Worker
* **Architektúra:** Repository Pattern, Strategy Pattern, Dependency Injection
* **Adatbázis-frissítés:** EF Core migrációk egyedi legacy adatbázis-kompatibilitási logikával
* **Tesztelés:** Automatizált ASP.NET Core integrációs tesztek
* **Production:** Docker konténer Linux VPS-en, Caddy reverse proxy és HTTPS mögött
* **Éles Adatbázis:** A konténertől függetlenül, tartósan tárolt SQLite adatbázis

## 🧪 Tesztelés

A projekt automatizált integrációs tesztekkel ellenőrzi többek között:

* a vendégmód és vendégazonosító működését
* a regisztrációt és bejelentkezést
* a vendégadatok felhasználói fiókba történő átvitelét
* az italrögzítést és koffeinszámításokat
* a napi és heti előzményeket
* a kedvenceket és gyors hozzáadást
* a saját italokat
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

A számítás becslés, nem tekintendő orvosi tanácsnak.

## 🚀 Üzemeltetés

Az éles alkalmazás Docker konténerben fut egy Linux VPS-en. A Caddy reverse proxy biztosítja a külső elérést és a HTTPS-t, az SQLite adatbázis pedig a konténeren kívül, tartós tárhelyen található.

Az adatbázisséma változásait EF Core migrációk és az alkalmazás külön migrációs módja kezeli, így a verziófrissítések során a meglévő éles adatok megőrizhetők.
