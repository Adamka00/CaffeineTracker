# ☕ Caffeine Tracker

*(Scroll down for the Hungarian version / Magyar verzió lejjebb)*

A modern, mobile-first ASP.NET Core MVC web application that helps you track your daily caffeine intake. It uses a biological mathematical model to calculate your active caffeine level, predicts the peak time, and tells you exactly when you are ready to sleep.

## ✨ Features
* **Active Level Calculation:** Calculates the exact amount of caffeine currently active in your bloodstream based on the superposition of multiple drinks.
* **Biological Math Model:** Uses a 45-minute linear absorption peak and an exponential decay model with a 5-hour half-life.
* **Sleep Readiness Indicator:** Calculates the exact time your active caffeine level will drop below the safe sleep threshold (25 mg). The prediction is locked from the absorption peak for maximum stability.
* **Sleep Quality Forecast:** Set your planned bedtime, and the app predicts your active caffeine level at that exact time, providing a personalized sleep quality forecast.
* **Custom Drink Creator:** Not on the list? Add your own custom beverages by easily entering their mg/100ml ratio. Custom drinks are permanently saved to your database for future use.
* **24-Hour Interactive Chart:** Visualizes your caffeine levels throughout the day using Chart.js.
* **Rich Database:** Pre-seeded with 38 popular drinks (Energy drinks, Nespresso/Dolce Gusto capsules, Coffees, Teas, Sodas) with accurate mg/100ml data.
* **Modern UI:** Dark mode, glassmorphism design built with Tailwind CSS.
* **Localization:** Full English and Hungarian (HU/EN) support with a built-in language switcher.

## 🛠️ Tech Stack
* **Backend:** C#, ASP.NET Core MVC (.NET)
* **Database:** SQLite & Entity Framework Core (Code-First)
* **Frontend:** HTML5, Tailwind CSS (via CDN), Chart.js
* **Architecture:** Repository Pattern, Strategy Pattern, Dependency Injection

---
---

# ☕ Caffeine Tracker (Magyar verzió)

Egy modern, mobilra optimalizált ASP.NET Core MVC webalkalmazás, amely segít nyomon követni a napi koffeinbeviteledet. Biológiai és matematikai modellek alapján kiszámolja az aktív koffeinszintedet, a felszívódási csúcsot, és megmondja, mikor tudsz nyugodtan aludni.

## ✨ Funkciók
* **Aktív Szint Számítás:** Pontosan kiszámolja, mennyi koffein pörög épp a véredben, figyelembe véve a többszöri fogyasztások összeadódását.
* **Biológiai Modell:** 45 perces lineáris felszívódási csúccsal és 5 órás exponenciális felezési idővel számol.
* **Alvásbarométer (Sleep Readiness):** Kiszámolja azt a pontos időpontot, amikor a koffeinszinted a biztonságos alvásküszöb (25 mg) alá esik. A számítás a felszívódási csúcsból indul, így az előrejelzés teljesen stabil.
* **Alvásminőség Előrejelzés:** Állítsd be, mikor tervezel elaludni, az app pedig kiszámolja a várható koffeinszintedet, és előrejelzést ad az alvásod várható minőségéről.
* **Saját Ital Létrehozása:** Nem találod az italodat a listában? Add hozzá a sajátodat a mg/100ml érték megadásával. A rendszer elmenti az adatbázisba, így bármikor újra választhatod.
* **24 Órás Interaktív Grafikon:** Vonaldiagramon ábrázolja a napi koffeinszinted alakulását a Chart.js segítségével.
* **Gazdag Adatbázis:** 38 népszerű itallal (Energiaitalok, Nespresso/Dolce Gusto kapszulák, Kávék, Kólák) előre feltöltve, valós mg/100ml adatokkal.
* **Modern UI:** Sötét témájú, üveghatású (glassmorphism) dizájn Tailwind CSS-el.
* **Többnyelvűség:** Teljes Magyar és Angol (HU/EN) támogatás, beépített nyelvváltó gombbal.

## 🛠️ Használt Technológiák
* **Backend:** C#, ASP.NET Core MVC (.NET)
* **Adatbázis:** SQLite & Entity Framework Core (Code-First)
* **Frontend:** HTML5, Tailwind CSS (CDN), Chart.js
* **Architektúra:** Repository Minta, Strategy Minta, Dependency Injection
