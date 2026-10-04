# Smok Przewodnik

**Smok Przewodnik** to innowacyjna nawigacja i mapa miejska, stworzona z myślą o osobach ze szczególnymi potrzebami. Projekt został przygotowany na hackathon w ramach zadania **„Kraków bez barier”**.

---

## Problem i kontekst

W Krakowie w 2023 r. ok. **51 tys.** osób posiadało orzeczenie o niepełnosprawności, co stanowi ponad 630 osób na każde 10 tysięcy mieszkańców. Co ważne, **35%** tych orzeczeń dotyczy dysfunkcji narządu ruchu. Szersza definicja (niepełnosprawność prawna lub biologiczna) obejmuje według NSP 2021 około **13,9% mieszkańców** Krakowa (ok. 1 na 7 osób).

Kraków posiada liczne rozwiązania dostępnościowe (np. w 100% niskopodłogowe autobusy, systemy informacji głosowej), jednak głównym problemem pozostaje **integracja rozproszonych informacji**. Standardowe nawigacje podają czas i dystans, podczas gdy osoby ze szczególnymi potrzebami potrzebują innych danych:
- *Czy na trasie znajdują się krawężniki i schody?*
- *Czy w danym przejściu podziemnym działa winda?*
- *Czy wejście do budynku głównego ma próg?*

## Nasze rozwiązanie

**Smok Przewodnik** to nawigacja, która dostosowuje sugerowaną trasę do konkretnego profilu użytkownika. Aplikacja przeznaczona jest dla:
- osób na wózkach inwalidzkich i z ograniczoną mobilnością,
- osób niewidomych i słabowidzących,
- osób niesłyszących i niedosłyszących,
- seniorów oraz rodziców z wózkami dziecięcymi.

### Główne funkcjonalności

**1. Tryb Gościa (bez logowania):**
- Wybór jednorazowego zestawu udogodnień i profilu (np. wózek, brak wzroku).
- Wyznaczanie spersonalizowanej trasy bez barier architektonicznych.

**2. Tryb Użytkownika (zalogowany):**
- Zapis preferencji i stałego zestawu potrzeb użytkownika.
- **Raportowanie na żywo (Crowdsourcing):** Dodawanie alertów o niedziałających windach, remontach czy zamkniętych przejściach.
- Aktualizowanie informacji o punktach docelowych (np. weryfikacja czy miejsce posiada rampę wjazdową).

---

## Architektura i technologie

Aplikacja wykorzystuje nowoczesną, w pełni rozdzieloną architekturę mikroserwisową. Składa się z mobilnego interfejsu (Frontend) oraz silnika backendowego wspartego specjalistycznym silnikiem tras.

### Aplikacja Mobilna (Frontend)
- **Technologia:** React Native / Expo (SDK 57)
- **Nawigacja:** Expo Router
- **Style i Komponenty:** React Native Paper, React Native Reanimated
- **Mapa:** Leaflet
- **Język:** TypeScript

### Backend (API)
- **Technologia:** .NET 10 (ASP.NET Core) zorganizowany we wzorcu **Clean Architecture** (Api, Application, Domain, Infrastructure).
- **Baza Danych:** PostgreSQL + **PostGIS** (obsługa danych przestrzennych).
- **ORM:** Entity Framework Core 10.
- **Autoryzacja:** Supabase Auth (weryfikacja tokenów JWT).

### Usługi Nawigacyjne (Routing)
- Zewnętrzny silnik **[Valhalla](https://github.com/valhalla/valhalla)** oparty na danych OpenStreetMap. Valhalla przelicza kafelki routingu dla Krakowa, pozwalając unikać przeszkód zgłoszonych przez użytkowników lub wynikających z mapy.

---

## Struktura Repozytorium

```text
smok-przewodnik/
├── api/                   # Backend API (.NET 10)
│   └── src/
│       ├── AB.SmokPrzewodnik.Api/            # REST API (Prezentacja)
│       ├── AB.SmokPrzewodnik.Application/    # Use Cases (CQRS)
│       ├── AB.SmokPrzewodnik.Domain/         # Encje biznesowe
│       └── AB.SmokPrzewodnik.Infrastructure/ # Integracja z bazą danych i zew. usługami
├── mobile/                # Aplikacja Expo (React Native)
│   ├── app/               # Ekrany (Expo Router)
│   ├── components/        # Komponenty współdzielone UI
│   └── services/          # Połączenia HTTP do API oraz Supabase
├── sb/                    # Konfiguracja środowiska lokalnego Supabase (Docker)
└── docs/                  # Dokumentacja szczegółowa (DEVELOPMENT.md)
```

---

## Uruchomienie projektu (Szybki Start)

Poniżej znajduje się skrócona instrukcja uruchomienia środowiska deweloperskiego. Kompletny przewodnik krok po kroku znajdziesz w pliku **[docs/DEVELOPMENT.md](docs/DEVELOPMENT.md)**.

### Wymagania
- .NET SDK 10.x, EF Core CLI (`dotnet-ef`)
- Node.js (LTS), npm
- Docker (wymagany do uruchomienia Valhalli oraz usług bazy danych)
- Supabase CLI

### 1. Zależności i infrastruktura
Zalecamy uruchomienie bazy danych przez lokalny stos Supabase oraz silnika nawigacyjnego w izolowanym kontenerze:

```bash
# Uruchomienie usług bazodanowych (Supabase)
supabase start --workdir sb

# Uruchomienie lokalnego silnika tras (Valhalla - upewnij się, że pobrano paczki kafelków)
docker start smok-valhalla
```

### 2. Backend (API)
Wyeksportuj zmienne środowiskowe, zastosuj migracje bazy i wystartuj profil HTTP.
```bash
# Aktualizacja bazy
dotnet ef database update --project api/src/AB.SmokPrzewodnik.Infrastructure/AB.SmokPrzewodnik.Infrastructure.csproj --startup-project api/src/AB.SmokPrzewodnik.Api/AB.SmokPrzewodnik.Api.csproj

# Uruchomienie
dotnet run --project api/src/AB.SmokPrzewodnik.Api/AB.SmokPrzewodnik.Api.csproj --launch-profile http
```
*(API domyślnie nasłuchuje na porcie 5123)*

### 3. Aplikacja Mobilna (Mobile)
Skonfiguruj plik `mobile/.env.local` wskazując własny wewnętrzny adres IP komputera (dla urządzeń fizycznych) m.in.: `EXPO_PUBLIC_API_URL=http://<TWOJE_IP>:5123/v1`
```bash
cd mobile
npm ci
npm start
```
Wciśnij `a` aby uruchomić emulator Androida, `i` aby uruchomić symulator iOS lub zeskanuj kod QR aplikacją **Expo Go** na swoim telefonie.

---

## Zespół (Drużyna: Anita Bath)
- Michał Dudnik
- Jakub Wójtowicz
- Bartosz Lwowski
- Marcin Mikuła
