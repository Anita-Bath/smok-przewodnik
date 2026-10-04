# Uruchamianie i weryfikacja projektu lokalnie

Ten dokument opisuje uruchomienie backendu ASP.NET Core oraz aplikacji mobilnej
Expo. Polecenia należy wykonywać z katalogu głównego repozytorium, chyba że
zaznaczono inaczej.

## 1. Wymagane zależności

| Zależność | Wymagana wersja lub zastosowanie |
| --- | --- |
| .NET SDK | 10.x; wszystkie projekty API używają `net10.0` |
| Entity Framework CLI | `dotnet-ef` 10.x, najlepiej w wersji zgodnej z pakietami EF Core 10.0.12 |
| Node.js | aktualna wersja LTS; projekt mobilny używa Expo SDK 57 |
| npm | instalowany razem z Node.js |
| PostgreSQL | wersja obsługująca rozszerzenie PostGIS; Supabase zapewnia oba elementy |
| Supabase | istniejący projekt hosted albo Supabase CLI z lokalnym stosem |
| Valhalla | działająca usługa nawigacyjna; wymagana przez `/v1/routes/plan` i zmianę trasy |
| Docker | potrzebny dla lokalnego Supabase, Valhalli i testów integracyjnych Testcontainers |
| Expo Go lub emulator | Expo Go na telefonie, Android Studio albo Xcode na macOS |

Do pobrania repozytorium potrzebny jest także Git. Telefon z Expo Go i komputer
muszą znajdować się w tej samej sieci, chyba że Metro jest uruchamiane przez
tunel.

### Opcja A: środowisko Nix

Repozytorium zawiera `shell.nix`, który udostępnia .NET SDK, `dotnet-ef`, Node.js,
Supabase CLI oraz ngrok:

```bash
nix-shell
```

Docker Engine lub Docker Desktop nadal musi być zainstalowany i uruchomiony na
hoście.

### Opcja B: instalacja natywna

Po zainstalowaniu zależności warto sprawdzić środowisko:

```bash
dotnet --version
dotnet ef --version
node --version
npm --version
docker --version
```

Jeżeli brakuje narzędzia EF Core:

```bash
dotnet tool install --global dotnet-ef --version 10.0.12
```

Aktualizacja istniejącej instalacji:

```bash
dotnet tool update --global dotnet-ef --version 10.0.12
```

## 2. Supabase i baza danych

API używa bazy PostgreSQL/PostGIS, a aplikacja mobilna używa Supabase Auth.
Backend i aplikacja mobilna muszą wskazywać ten sam projekt Supabase.

### Wariant A: istniejący projekt Supabase

Z panelu Supabase potrzebne są:

- URL projektu, np. `https://<project-ref>.supabase.co`;
- klucz `anon`/publishable dla aplikacji mobilnej;
- connection string PostgreSQL dla API.

Nie używaj klucza `service_role` w aplikacji mobilnej.

### Wariant B: lokalny Supabase

Konfiguracja lokalnego stosu znajduje się w katalogu `sb`. Uruchom Docker, a
następnie:

```bash
supabase start --workdir sb
supabase status --workdir sb
```

Pierwsze uruchomienie pobiera obrazy kontenerów i może potrwać kilka minut.
Domyślne usługi projektu są dostępne pod następującymi adresami:

- Supabase API: `http://127.0.0.1:54321`;
- PostgreSQL: `127.0.0.1:54322`;
- Supabase Studio: `http://127.0.0.1:54323`.

`supabase status --workdir sb` wyświetla lokalny klucz anon potrzebny aplikacji
mobilnej. Lokalnego stosu Supabase nie należy wystawiać do Internetu.

Aby zatrzymać usługi bez usuwania danych:

```bash
supabase stop --workdir sb
```

## 3. Konfiguracja API

Najbezpieczniej nadpisać ustawienia przez zmienne środowiskowe. Poniższy przykład
pokazuje konfigurację lokalnego Supabase; dla wersji hosted należy podstawić URL
i connection string otrzymane z panelu projektu:

```bash
export ConnectionStrings__Default='Host=<db-host>;Port=<db-port>;Database=<db-name>;Username=<db-user>;Password=<db-password>'
export Supabase__Url='http://127.0.0.1:54321'
export Supabase__Audience='authenticated'
export Supabase__ClaimRole='authenticated'
export Pagination__SigningKey='replace-with-a-long-random-development-secret'
export Valhalla__BaseUrl='http://127.0.0.1:8002'
export Valhalla__TimeoutSeconds='10'
export Valhalla__Alternates='2'
```

Zmienne z podwójnym podkreśleniem odpowiadają zagnieżdżonym sekcjom konfiguracji
ASP.NET Core, np. `Supabase__Url` nadpisuje `Supabase:Url`.

`Pagination__SigningKey` podpisuje nieprzezroczyste kursory paginacji. Powinien
być losowy, tajny oraz stabilny pomiędzy restartami danej instancji API.

### Migracje bazy

Przy pierwszym uruchomieniu oraz po dodaniu migracji zastosuj je do wskazanej
bazy:

```bash
dotnet restore api/AB.SmokPrzewodnik.sln
dotnet ef database update \
  --project api/src/AB.SmokPrzewodnik.Infrastructure/AB.SmokPrzewodnik.Infrastructure.csproj \
  --startup-project api/src/AB.SmokPrzewodnik.Api/AB.SmokPrzewodnik.Api.csproj
```

Migracja tworzy rozszerzenie PostGIS. Użytkownik bazy musi mieć prawo do
wykonania `CREATE EXTENSION postgis`.

### Valhalla

API uruchomi się bez aktywnej Valhalli, ale planowanie i przeliczanie tras zwróci
błąd `503 Service Unavailable`. Usługa musi posiadać zbudowane kafelki routingu
dla Krakowa i nasłuchiwać pod adresem ustawionym w `Valhalla__BaseUrl`.

Przykładowe uruchomienie obrazu budującego dane dla Polski:

```bash
mkdir -p .local/valhalla
docker run -d \
  --name smok-valhalla \
  -p 8002:8002 \
  -v "$PWD/.local/valhalla:/custom_files" \
  -e tile_urls=https://download.geofabrik.de/europe/poland-latest.osm.pbf \
  ghcr.io/gis-ops/docker-valhalla/valhalla:latest
```

Pobranie danych OSM i pierwszy build grafu mogą być długie oraz wymagać kilku GB
miejsca. Gotowość usługi można sprawdzić przez:

```bash
curl http://127.0.0.1:8002/status
```

Jeżeli zespół korzysta ze wspólnej instancji Valhalli, pomiń kontener i ustaw jej
URL w `Valhalla__BaseUrl`.

## 4. Uruchomienie API

W terminalu, w którym ustawiono zmienne środowiskowe:

```bash
dotnet run \
  --project api/src/AB.SmokPrzewodnik.Api/AB.SmokPrzewodnik.Api.csproj \
  --launch-profile http
```

Profil HTTP nasłuchuje na wszystkich interfejsach na porcie `5123`, dzięki czemu
API jest dostępne również dla telefonu w sieci lokalnej.

Sprawdzenie działania:

```bash
curl http://localhost:5123/v1/health
```

Swagger w środowisku `Development` jest dostępny pod adresem:

```text
http://localhost:5123/v1/swagger
```

Publiczny prefiks endpointów to `/v1`, np.
`http://localhost:5123/v1/spatials/entities`.

## 5. Konfiguracja aplikacji mobilnej

Zainstaluj zależności dokładnie według `package-lock.json`:

```bash
cd mobile
npm ci
```

Utwórz plik `mobile/.env.local`:

```dotenv
EXPO_PUBLIC_API_URL=http://<API_HOST>:5123/v1
EXPO_PUBLIC_SUPABASE_URL=https://<project-ref>.supabase.co
EXPO_PUBLIC_SUPABASE_ANON_KEY=<supabase-anon-or-publishable-key>
```

Dla lokalnego Supabase drugi adres powinien wskazywać port `54321` hosta, np.
`http://192.168.1.20:54321`. Użyj klucza anon wyświetlonego przez `supabase
status --workdir sb`.

### Jaki adres wpisać jako `API_HOST`

| Miejsce uruchomienia aplikacji | Adres API |
| --- | --- |
| Fizyczny Android lub iPhone | adres LAN komputera, np. `192.168.1.20` |
| Emulator Android | `10.0.2.2` |
| Symulator iOS na macOS | `127.0.0.1` |

Na fizycznym telefonie `localhost` oznacza telefon, a nie komputer. Porty `5123`
i, przy lokalnym Supabase, `54321` muszą być dostępne w firewallu. API i telefon
powinny być w tej samej sieci Wi-Fi.

Adres LAN można znaleźć np. poleceniem:

```bash
hostname -I
```

Na macOS można użyć `ipconfig getifaddr en0`.

Po zmianie `.env.local` należy zrestartować Expo. Zmienne z prefiksem
`EXPO_PUBLIC_` trafiają do bundle aplikacji, dlatego nie mogą zawierać sekretów.

## 6. Uruchomienie aplikacji mobilnej

Z katalogu `mobile`:

```bash
npm start
```

Następnie:

- zeskanuj kod QR w Expo Go na urządzeniu fizycznym;
- naciśnij `a`, aby otworzyć emulator Android;
- na macOS naciśnij `i`, aby otworzyć symulator iOS.

Jeżeli telefon nie widzi serwera Metro pomimo wspólnej sieci, uruchom tunel:

```bash
npx expo start --tunnel
```

Tunel Metro nie tuneluje API ani lokalnego Supabase. W takim przypadku API nadal
musi mieć adres osiągalny z telefonu, np. przez LAN albo osobny tunel.

Projekt jest obecnie przeznaczony przede wszystkim dla Androida i iOS. Wersja
webowa wymaga dodatkowego dostosowania integracji SecureStore oraz polityki CORS
API, więc `npm run web` nie jest pełnym zamiennikiem testu mobilnego.

## 7. Kontrola poprawności

API:

```bash
dotnet build api/AB.SmokPrzewodnik.sln
dotnet test api/AB.SmokPrzewodnik.sln
```

Testy integracyjne używają Testcontainers, dlatego wymagają uruchomionego
Dockera.

Aplikacja mobilna:

```bash
cd mobile
npx tsc --noEmit
npm run lint
npx expo-doctor
```

## 8. Najczęstsze problemy

### Telefon nie może połączyć się z API

- sprawdź, czy `EXPO_PUBLIC_API_URL` kończy się na `/v1`;
- nie używaj `localhost` na fizycznym urządzeniu;
- sprawdź `curl http://<LAN-IP>:5123/v1/health` z innego urządzenia w sieci;
- upewnij się, że API uruchomiono z profilem `http`, który wiąże adres
  `0.0.0.0:5123`;
- sprawdź firewall i izolację klientów w sieci Wi-Fi.

### Logowanie działa, ale chroniony endpoint zwraca `401` lub `403`

- API i aplikacja mobilna muszą wskazywać ten sam projekt Supabase;
- `Supabase__Audience` powinno odpowiadać claimowi `aud` tokena;
- `Supabase__ClaimRole` powinno odpowiadać claimowi `role`, domyślnie
  `authenticated`;
- do żądania musi zostać dołączony access token użytkownika, nie klucz anon.

### `dotnet ef` zgłasza inną wersję niż runtime

Zaktualizuj narzędzie do wersji 10.0.12 poleceniem z sekcji zależności. Wersja
major narzędzia musi odpowiadać używanemu EF Core.

Jeżeli narzędzie nie może utworzyć `DbContext`, sprawdź, czy w bieżącym
terminalu ustawiono `ConnectionStrings__Default`.

### Planowanie trasy zwraca `503`

Sprawdź `curl http://127.0.0.1:8002/status`, logi kontenera Valhalli i wartość
`Valhalla__BaseUrl`. Sam działający kontener bez grafu dla Polski nie wystarczy.

## 9. Codzienny skrót

Po jednorazowej konfiguracji typowa sesja pracy wymaga trzech terminali:

```bash
# Terminal 1: usługi lokalne (pomiń przy hosted Supabase/Valhalla)
supabase start --workdir sb
docker start smok-valhalla

# Terminal 2: API; wcześniej wyeksportuj jego zmienne środowiskowe
dotnet run --project api/src/AB.SmokPrzewodnik.Api/AB.SmokPrzewodnik.Api.csproj --launch-profile http

# Terminal 3: aplikacja
cd mobile
npm start
```

Po zmianie modelu danych ponownie uruchom `dotnet ef database update` przed
startem API.

## 10. Dokumentacja narzędzi

- [Expo: uruchamianie projektu](https://docs.expo.dev/get-started/start-developing/)
- [Supabase CLI i lokalny stos](https://supabase.com/docs/guides/local-development/cli/getting-started)
- [Entity Framework Core CLI](https://learn.microsoft.com/ef/core/cli/dotnet)
- [Valhalla Docker](https://github.com/valhalla/valhalla/tree/master/docker)
