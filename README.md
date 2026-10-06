# ☕ Koffi

💙 **Support:** [buymeacoffe.com](https://buymeacoffee.com/adamka00)

🌍 **Live App:** [koffi.hu](https://koffi.hu)

*(Scroll down for the Hungarian version / Magyar verzió lejjebb)*

A modern, mobile-first ASP.NET Core MVC Progressive Web App (PWA) for tracking caffeine intake, estimating how caffeine levels change in your body over time, planning caffeine consumption around sleep, and discovering personal patterns between caffeine and sleep.

Koffi combines a biological caffeine model with daily and weekly history, favorites, custom drinks, streaks, caffeine-free days, sleep tracking, planning tools, long-term statistics, barcode-assisted drink lookup, guided onboarding, guest mode, and user accounts.

## ✨ Features

* **User Accounts & Seamless Guest Mode:** Start tracking immediately without registration using a cookie-based guest identity. When you create an account, supported guest data can be transferred to your new profile.
* **Secure Authentication:** Cookie-based authentication with ASP.NET Core password hashing, security stamps, and user-specific data separation.
* **Password Recovery by Email:** Registered users can securely request a password reset using expiring, single-use reset tokens delivered through the configured SMTP service.
* **Progressive Web App (PWA):** Installable on supported devices for an app-like mobile experience.
* **Smart PWA Install Guidance:** Koffi can suggest installation after repeated use and includes iOS-specific instructions for adding the app to the Home Screen.
* **Guided Onboarding:** Progressive onboarding introduces the main features without blocking normal use. It can be skipped and opened again later.
* **Help, Feature Explanations & What's New:** Built-in help pages, contextual `?` explanations, a version-aware What's New experience, and a dedicated support page make the app easier to discover and understand.
* **In-App Morning Sleep Reminder:** If the previous night's sleep entry is missing, Koffi can show a morning reminder inside the app. Dismissing it suppresses the reminder for the rest of the day.
* **Active Caffeine Calculation:** Estimates the amount of caffeine currently active in your body while accounting for multiple drinks consumed at different times.
* **Biological Caffeine Model:** Models caffeine using a 45-minute absorption phase followed by exponential elimination with a 5-hour half-life.
* **Sleep Readiness Indicator:** Estimates when your active caffeine level will fall below your configured personal target.
* **Planned Sleep Forecast:** Set your planned bedtime and see the estimated amount of caffeine that may still be active at that time.
* **Caffeine Planning:** Explore how a drink would affect your later caffeine level before deciding whether to log it.
* **Caffeine Cutoff Estimation:** Estimate how late you could consume a selected drink while staying near your configured caffeine target by bedtime.
* **What-If Calculator:** Simulate caffeine intake without modifying your diary unless you explicitly choose to save it.
* **Sleep Diary:** Record bedtime, wake time, sleep quality, difficulty falling asleep, awakenings, and optional notes.
* **Automatic Sleep Context:** Sleep entries are automatically connected with relevant caffeine data from the previous day.
* **Personal Sleep Insights:** Compare your own caffeine habits with recorded sleep quality and duration using confidence levels that become more informative as more nights are logged.
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
* **Custom Drink Creator:** Create your own beverages by entering their caffeine content in mg/100 ml. Custom drinks are stored per user and remain available for future logging.
* **Built-in Drink Database:** Quickly select common caffeinated drinks with localized names.
* **Barcode Scanner (Beta):** Scan product barcodes on supported secure browsers, review and edit the detected product before logging it, or fall back to manual entry when camera access is unavailable.
* **Open Food Facts Integration:** Barcode lookups can use Open Food Facts as an external suggestion source. External data is reviewable and Koffi remains usable if the service is unavailable.
* **Safe Product Lookup Fallbacks:** Missing or unreliable caffeine data is never guessed automatically; the user can review and complete the information manually.
* **Drink Search:** Quickly filter the drink list using case- and accent-insensitive search.
* **Interactive Caffeine Curve:** Chart.js visualizes how your estimated active caffeine level changes throughout the selected day.
* **Detailed Drink Diary:** See logged drinks, serving sizes, caffeine amounts, and consumption times for each day.
* **Safe Data Migration:** Entity Framework Core migrations and a dedicated database-upgrade path preserve existing data while upgrading older database schemas.
* **Account & Data Deletion:** Permanently delete your account and its associated tracking data directly from the application.
* **Cookie Consent:** Built-in cookie consent notice for application cookies.
* **Localization:** Full Hungarian, English, and German (HU/EN/DE) interface with an integrated language switcher.
* **Modern Mobile-First UI:** Refined responsive dark interface with glassmorphism styling, consistent components, and improved phone-first usability.

## 🛠️ Tech Stack

* **Language / Runtime:** C# / .NET 10
* **Backend:** ASP.NET Core MVC
* **Database:** SQLite
* **ORM:** Entity Framework Core with Code-First migrations
* **Authentication:** ASP.NET Core Cookie Authentication
* **Password Security:** ASP.NET Core `PasswordHasher`
* **Email:** MailKit / SMTP for password-reset delivery
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

## 🧪 Testing

The project currently includes **126 automated .NET tests** and **7 frontend JavaScript tests** covering important application workflows such as:

* guest tracking and guest identity handling
* registration and login
* guest-to-account data transfer
* account deletion and data cleanup
* caffeine logging and calculations
* streak calculation
* caffeine-free days
* daily and weekly history
* favorites and Quick Add
* custom drinks and drink search
* barcode lookup and product review flows
* Open Food Facts fallbacks
* sleep diary and user isolation
* sleep statistics and personal insights
* caffeine planning and cutoff calculations
* what-if calculations without unintended data persistence
* lifetime statistics
* password reset security and expiry
* onboarding and experience-state behavior
* localization, including German
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

The insight system adapts its presentation to the amount of personal sleep data available:

* **10–19 nights:** early estimate / limited data;
* **20–29 nights:** pattern emerging;
* **30+ nights:** fuller personal analysis.

Koffi intentionally describes these results as **associations rather than causes**, communicates the direction and strength of observed relationships, and avoids medical conclusions.

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

💙 **Támogatás:** [buymeacoffe.com](https://buymeacoffee.com/adamka00)

🌍 **Éles alkalmazás:** [koffi.hu](https://koffi.hu)

A Koffi egy modern, mobilra optimalizált ASP.NET Core MVC Progressive Web App (PWA) a koffeinbevitel követésére, a szervezetben lévő aktív koffeinszint időbeli becslésére, a koffeinfogyasztás alváshoz történő tervezésére és a saját koffein- és alvási szokások közötti mintázatok megfigyelésére.

A Koffi a biológiai koffeinmodellt napi és heti előzményekkel, kedvencekkel, saját italokkal, streak rendszerrel, koffeinmentes napokkal, alvásnaplóval, tervezési eszközökkel, hosszú távú statisztikákkal, vonalkódos italkereséssel, onboardinggal, vendégmóddal és felhasználói fiókokkal egészíti ki.

## ✨ Funkciók

* **Felhasználói Fiókok és Vendégmód:** Regisztráció nélkül azonnal elkezdheted a koffein követését egy sütialapú vendégazonosítóval. Későbbi regisztrációnál a támogatott vendégadatok átvihetők az új profilba.
* **Biztonságos Bejelentkezés:** Sütialapú autentikáció, ASP.NET Core jelszó-hashelés, biztonsági stamp és felhasználónként elkülönített adatok.
* **Jelszó-visszaállítás E-mailben:** A regisztrált felhasználók biztonságos, lejáró és egyszer használható tokennel, SMTP-n kézbesített levél segítségével állíthatják vissza jelszavukat.
* **Progressive Web App (PWA):** Támogatott eszközökön telepíthető, így mobilon alkalmazásszerű élményt nyújt.
* **Intelligens PWA Telepítési Javaslat:** A Koffi több használat után felajánlhatja a telepítést, iOS-en pedig külön útmutatót ad a Megosztás → Hozzáadás a Főképernyőhöz lépésekhez.
* **Interaktív Bevezető:** A fő funkciókat fokozatos onboarding mutatja be. Átugorható, és később újra megnyitható.
* **Súgó, Funkciómagyarázatok és Újdonságok:** Beépített súgóoldalak, kontextusos `?` magyarázatok, verzióhoz kötött újdonságok és külön támogatási oldal segíti az eligazodást.
* **Reggeli Alvásnapló Emlékeztető:** Ha az előző éjszakához nincs alvásbejegyzés, a Koffi reggel az alkalmazáson belül emlékeztethet rá. A „Most nem” választás aznapra elnémítja.
* **Aktív Koffeinszint Számítás:** Megbecsüli az aktuálisan aktív koffein mennyiségét, egyszerre több, különböző időpontban elfogyasztott italt is figyelembe véve.
* **Biológiai Koffeinmodell:** 45 perces felszívódási szakasszal, majd 5 órás felezési idejű exponenciális kiürüléssel számol.
* **Alvásküszöb Előrejelzés:** Megbecsüli, mikor csökken az aktív koffeinszint a beállított személyes célérték alá.
* **Tervezett Alvásidő:** Beállíthatod, mikor tervezel lefeküdni, az alkalmazás pedig megmutatja az arra az időpontra becsült aktív koffeinszintedet.
* **Koffeintervező:** Rögzítés előtt megnézheted, hogyan befolyásolna egy ital egy későbbi koffeinszintet.
* **Koffein Cutoff:** A Koffi megbecsülheti, hogy egy adott italt körülbelül meddig fogyaszthatsz el úgy, hogy lefekvésre a beállított célérték közelébe kerülj.
* **What-If Kalkulátor:** Kipróbálhatsz különböző fogyasztási forgatókönyveket anélkül, hogy azok automatikusan bekerülnének a naplóba.
* **Alvásnapló:** Rögzíthető a lefekvés, ébredés, alvásminőség, elalvási nehézség, éjszakai ébredések és opcionális megjegyzés.
* **Automatikus Alvási Kontextus:** A Koffi automatikusan összekapcsolja az alvásbejegyzést az előző napi releváns koffeinadatokkal.
* **Személyes Alvási Trendek:** A saját koffein- és alvásadataid alapján 10–19 éjszakánál korai becslést, 20–29 éjszakánál kirajzolódó mintázatot, 30+ éjszakánál pedig teljesebb személyes elemzést mutat.
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
* **Saját Italok:** Saját italokat hozhatsz létre a koffeintartalom mg/100 ml értékének megadásával. Ezek felhasználóhoz kötve kerülnek mentésre, és később újra felhasználhatók.
* **Beépített Itallista:** Gyakori koffeintartalmú italok gyors kiválasztása lokalizált elnevezésekkel.
* **Vonalkódolvasó (Béta):** Támogatott, biztonságos böngészőkben a kamera segítségével beolvasható a termék vonalkódja. A találat mentés előtt ellenőrizhető és szerkeszthető, kamera nélkül pedig kézi bevitelre lehet váltani.
* **Open Food Facts Integráció:** A vonalkódos keresés külső javaslatként használhatja az Open Food Facts adatbázisát. A Koffi nem tekinti automatikusan hitelesnek a külső adatot, és akkor is használható marad, ha a szolgáltatás nem érhető el.
* **Biztonságos Termékkeresési Fallback:** Ha a koffeintartalom nem megbízható vagy hiányzik, a Koffi nem talál ki helyette értéket; a felhasználó ellenőrizheti és kiegészítheti az adatokat.
* **Italkeresés:** Az itallista gyorsan szűrhető kis- és nagybetűktől, illetve magyar ékezetektől független kereséssel.
* **Interaktív Koffeingörbe:** A Chart.js grafikonon jeleníti meg a kiválasztott nap becsült aktív koffeinszintjének alakulását.
* **Részletes Italnapló:** Naponként megtekintheted a rögzített italokat, mennyiségeket, koffeintartalmat és fogyasztási időpontokat.
* **Biztonságos Adatbázis-frissítés:** Az Entity Framework Core migrációi és a külön adatbázis-upgrade folyamat lehetővé teszik a régebbi adatbázisok frissítését a meglévő adatok megőrzésével.
* **Fiók- és Adattörlés:** Az alkalmazásból véglegesen törölhető a felhasználói fiók és a hozzá tartozó követési adat.
* **Sütikezelés:** Beépített tájékoztató az alkalmazás által használt sütikről.
* **Többnyelvűség:** Teljes magyar, angol és német (HU/EN/DE) felület beépített nyelvváltóval.
* **Modern, Mobile-First UI:** Finomhangolt reszponzív, sötét, glassmorphism stílusú felület, egységesebb komponensekkel és javított mobilos használhatósággal.

## 🛠️ Használt Technológiák

* **Nyelv / Runtime:** C# / .NET 10
* **Backend:** ASP.NET Core MVC
* **Adatbázis:** SQLite
* **ORM:** Entity Framework Core, Code-First migrációkkal
* **Autentikáció:** ASP.NET Core Cookie Authentication
* **Jelszóbiztonság:** ASP.NET Core `PasswordHasher`
* **E-mail:** MailKit / SMTP a jelszó-visszaállító levelekhez
* **Frontend:** Razor Views, HTML5, JavaScript, Tailwind CSS
* **Grafikonok:** Chart.js
* **Vonalkódolvasás:** ZXing JavaScript
* **Külső Termékadatok:** Open Food Facts
* **Lokalizáció:** ASP.NET Core Localization és lokalizált Razor erőforrások (HU/EN/DE)
* **PWA:** Web App Manifest és Service Worker
* **Architektúra:** Repository Pattern, Strategy Pattern, Dependency Injection
* **Adatbázis-frissítés:** EF Core migrációk egyedi legacy adatbázis-kompatibilitási logikával
* **Tesztelés:** Automatizált ASP.NET Core integrációs és frontend JavaScript tesztek
* **Production:** Docker konténer Linux VPS-en, Caddy reverse proxy és HTTPS mögött
* **Éles Adatbázis:** A konténertől függetlenül, tartósan tárolt SQLite adatbázis

## 🧪 Tesztelés

A projekt jelenleg **126 automatizált .NET tesztet** és **7 frontend JavaScript tesztet** tartalmaz, amelyek többek között ellenőrzik:

* a vendégmód és vendégazonosító működését
* a regisztrációt és bejelentkezést
* a vendégadatok felhasználói fiókba történő átvitelét
* a fióktörlést és kapcsolódó adatok törlését
* az italrögzítést és koffeinszámításokat
* a streak rendszert
* a koffeinmentes napokat
* a napi és heti előzményeket
* a kedvenceket és gyors hozzáadást
* a saját italokat és az italkeresést
* a vonalkódos termékkeresést és review folyamatot
* az Open Food Facts fallbackeket
* az alvásnapló működését és felhasználói adatok elkülönítését
* az alvási statisztikákat és személyes trendeket
* a koffeintervezőt és cutoff számítást
* a rögzítés nélküli what-if számításokat
* a lifetime statisztikákat
* a jelszó-visszaállítás biztonsági működését és lejáratát
* az onboarding és experience-state működését
* a lokalizációt, beleértve a német felületet
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

Az insight felület a rendelkezésre álló saját adatok mennyiségéhez igazítja, mennyire erősen fogalmaz:

* **10–19 éjszaka:** korai becslés / kevés adat;
* **20–29 éjszaka:** kirajzolódó mintázat;
* **30+ éjszaka:** teljesebb személyes elemzés.

A Koffi **összefüggéseket, nem okokat** mutat, jelzi az irányt és az erősséget, és nem tesz orvosi következtetéseket.

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
