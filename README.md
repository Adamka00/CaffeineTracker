# ☕ Caffeine Tracker

🌍 **Live App:** [koffein.adamka00.hu](https://koffein.adamka00.hu)

*(Scroll down for the Hungarian version / Magyar verzió lejjebb)*

A modern, mobile-first ASP.NET Core MVC Progressive Web App (PWA) that helps you track your daily caffeine intake. It uses a biological mathematical model to calculate your active caffeine level, predicts the peak time, and tells you exactly when you are ready to sleep.

## ✨ Features
* **User Accounts & Seamless Guest Mode:** Start tracking instantly without an account using a cookie-based guest mode. If you decide to register later, all your previous guest data is automatically migrated to your secure, password-hashed profile.
* **Progressive Web App (PWA):** Fully installable on your mobile home screen for a native app experience.
* **Active Level Calculation:** Calculates the exact amount of caffeine currently active in your bloodstream based on the superposition of multiple drinks.
* **Biological Math Model:** Uses a 45-minute linear absorption peak and an exponential decay model with a 5-hour half-life.
* **Sleep Readiness Indicator:** Calculates the exact time your active caffeine level will drop below the safe sleep threshold (25 mg). The prediction is locked from the absorption peak for maximum stability.
* **Sleep Quality Forecast:** Set your planned bedtime, and the app predicts your active caffeine level at that exact time, providing a personalized sleep quality forecast.
* **Custom Drink Creator:** Not on the list? Add your own custom beverages by easily entering their mg/100ml ratio. Custom drinks are permanently saved to your database for future use.
* **24-Hour Interactive Chart:** Visualizes your caffeine levels throughout the day using Chart.js.
* **Privacy & GDPR Compliant:** Built-in cookie consent banner and a one-click permanent account & data deletion feature.
* **Modern UI:** Dark mode, glassmorphism design built with Tailwind CSS.
* **Localization:** Full English and Hungarian (HU/EN) support with a built-in language switcher.

## 🛠️ Tech Stack
* **Backend:** C#, ASP.NET Core MVC (.NET), Cookie Authentication, Password Hashing
* **Database:** SQLite & Entity Framework Core (Code-First)
* **Frontend:** HTML5, Tailwind CSS (via CDN), Chart.js, PWA Manifest & Service Worker
* **Architecture:** Repository Pattern, Strategy Pattern, Dependency Injection

---
---

# ☕ Caffeine Tracker (Magyar verzió)

🌍 **Éles alkalmazás:** [koffein.adamka00.hu](https://koffein.adamka00.hu)

Egy modern, mobilra optimalizált ASP.NET Core MVC Progressive Web App (PWA), amely segít nyomon követni a napi koffeinbeviteledet. Biológiai és matematikai modellek alapján kiszámolja az aktív koffeinszintedet, a felszívódási csúcsot, és megmondja, mikor tudsz nyugodtan aludni.

## ✨ Funkciók
* **Profilok és Okos Vendégmód:** Kezdd el a használatot azonnal, regisztráció nélkül! Ha később a regisztráció mellett döntesz, a vendégként rögzített adataid automatikusan átkerülnek a biztonságos, titkosított fiókodba.
* **Progressive Web App (PWA):** Telepíthető a telefon főképernyőjére, így egy teljes értékű, natív mobilapp élményét nyújtja.
* **Aktív Szint Számítás:** Pontosan kiszámolja, mennyi koffein pörög épp a véredben, figyelembe véve a többszöri fogyasztások összeadódását.
* **Biológiai Modell:** 45 perces lineáris felszívódási csúccsal és 5 órás exponenciális felezési idővel számol.
* **Alvásbarométer (Sleep Readiness):** Kiszámolja azt a pontos időpontot, amikor a koffeinszinted a biztonságos alvásküszöb (25 mg) alá esik. A számítás a felszívódási csúcsból indul, így az előrejelzés teljesen stabil.
* **Alvásminőség Előrejelzés:** Állítsd be, mikor tervezel elaludni, az app pedig kiszámolja a várható koffeinszintedet, és előrejelzést ad az alvásod várható minőségéről.
* **Saját Ital Létrehozása:** Nem találod az italodat a listában? Add hozzá a sajátodat a mg/100ml érték megadásával. A rendszer elmenti az adatbázisba, így bármikor újra választhatod.
* **24 Órás Interaktív Grafikon:** Vonaldiagramon ábrázolja a napi koffeinszinted alakulását a Chart.js segítségével.
* **Adatvédelem és GDPR:** Beépített sütitájékoztató és egykattintásos, végleges fiók- és adattörlési lehetőség.
* **Modern UI:** Sötét témájú, üveghatású (glassmorphism) dizájn Tailwind CSS-el.
* **Többnyelvűség:** Teljes Magyar és Angol (HU/EN) támogatás, beépített nyelvváltó gombbal.

## 🛠️ Használt Technológiák
* **Backend:** C#, ASP.NET Core MVC (.NET), Sütialapú Autentikáció, Jelszó Hashelés
* **Adatbázis:** SQLite & Entity Framework Core (Code-First)
* **Frontend:** HTML5, Tailwind CSS (CDN), Chart.js, PWA Manifest és Service Worker
* **Architektúra:** Repository Minta, Strategy Minta, Dependency Injection
