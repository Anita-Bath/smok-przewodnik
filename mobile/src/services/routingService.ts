import { ConstraintLevel, TravelMode } from '@/context/AccessibilityContext';
import { KrakowPlace } from './krakowData';

export interface RouteManeuver {
  instruction: string;
  distanceMeters: number;
  durationSeconds: number;
  type: 'depart' | 'turn_left' | 'turn_right' | 'continue' | 'ramp' | 'elevator' | 'transit_board' | 'arrive';
  accessibilityNote?: string;
  landmark?: string;
  warning?: string;
}

export interface RouteAlternative {
  id: string;
  profileType: 'fastest' | 'easiest' | 'quietest' | 'best_supported';
  title: string;
  subtitle: string;
  durationMinutes: number;
  distanceMeters: number;
  stairsCount: number;
  curbCount: number;
  hasRoughSurface: boolean;
  transitChanges: number;
  confidenceScore: number; // 0 - 100
  dominantAdvantage: string;
  accessibilitySummary: {
    satisfiedHardConstraints: string[];
    warnings: string[];
    highlights: string[];
  };
  maneuvers: RouteManeuver[];
}

export interface RoutePlanResult {
  originName: string;
  destination: KrakowPlace;
  alternatives: RouteAlternative[];
  blockedByConstraints?: string[];
}

export function calculateKrakowRoutes(
  destination: KrakowPlace,
  constraints: Record<string, ConstraintLevel>,
  travelModes: TravelMode[]
): RoutePlanResult {
  const mustAvoidStairs = constraints['stairs'] === 'must_avoid';
  const preferAvoidRough = constraints['rough_surface'] === 'prefer_avoid';
  const mustAvoidSteep = constraints['steep_slope'] === 'must_avoid';
  const isMobilityAid = travelModes.includes('mobility_aid');

  // Alternative 1: Najłatwiejsza (Easiest - 100% płaska, przez Planty, podjazdy, zero schodów)
  const easiestRoute: RouteAlternative = {
    id: 'route-easiest',
    profileType: 'easiest',
    title: 'Najłatwiejsza',
    subtitle: 'Zero schodów, łagodne nachylenie przez Planty',
    durationMinutes: 12,
    distanceMeters: 620,
    stairsCount: 0,
    curbCount: 0,
    hasRoughSurface: false,
    transitChanges: 0,
    confidenceScore: 98,
    dominantAdvantage: 'Brak barier architektonicznych i gładka nawierzchnia',
    accessibilitySummary: {
      satisfiedHardConstraints: ['Zero stopni i krawężników', 'Nachylenie poniżej 4%', 'Szerokie chodniki (>1.8m)'],
      warnings: [],
      highlights: ['Alejki parkowe Plant z oświetleniem', 'Ławki co 80 metrów', 'Podjazd z poręczą przy wejściu'],
    },
    maneuvers: [
      {
        instruction: 'Wyrusz na zachód alejką Plant Krakowskich',
        distanceMeters: 180,
        durationSeconds: 190,
        type: 'depart',
        landmark: 'Wyspa zieleni Plant',
        accessibilityNote: 'Równa nawierzchnia asfaltowa, brak krawężników.',
      },
      {
        instruction: 'Skręć w prawo w ul. Grodzką przez obniżony krawężnik',
        distanceMeters: 120,
        durationSeconds: 140,
        type: 'turn_right',
        landmark: 'Kościół św. Piotra i Pawła',
        accessibilityNote: 'Pasy z gładkich płyt granitowych wyznaczone dla wózków.',
      },
      {
        instruction: 'Wjedź łagodnym podjazdem z poręczą na Dziedziniec',
        distanceMeters: 140,
        durationSeconds: 160,
        type: 'ramp',
        landmark: 'Brama Wazów',
        accessibilityNote: 'Pochylnia 5%, podwójne poręcze na wys. 75 cm i 90 cm.',
      },
      {
        instruction: 'Dotarłeś do celu: ' + destination.name,
        distanceMeters: 0,
        durationSeconds: 0,
        type: 'arrive',
        landmark: destination.name,
      },
    ],
  };

  // Alternative 2: Najszybsza (Fastest - w linii prostej, może mieć zabytkowy bruk)
  const fastestRoute: RouteAlternative = {
    id: 'route-fastest',
    profileType: 'fastest',
    title: 'Najszybsza',
    subtitle: 'Krótki dystans ulicą Kanoniczą',
    durationMinutes: 7,
    distanceMeters: 410,
    stairsCount: 0,
    curbCount: 2,
    hasRoughSurface: true,
    transitChanges: 0,
    confidenceScore: 84,
    dominantAdvantage: 'Najkrótszy czas marszu (410 m)',
    accessibilitySummary: {
      satisfiedHardConstraints: ['Brak schodów'],
      warnings: preferAvoidRough
        ? ['Zabytkowy nierówny bruk (ul. Kanonicza) — zalecany wybór trasy Najłatwiejszej']
        : ['Odcinek z zabytkową kostką brukową'],
      highlights: ['Krótki dystans', 'Dobre doświetlenie uliczne'],
    },
    maneuvers: [
      {
        instruction: 'Skieruj się prosto ul. Kanoniczą',
        distanceMeters: 250,
        durationSeconds: 210,
        type: 'depart',
        warning: 'Nawierzchnia z kamienia polnego, zalecane ostrożne tempo.',
      },
      {
        instruction: 'Skręć łagodnie w lewo pod wzgórze',
        distanceMeters: 160,
        durationSeconds: 150,
        type: 'turn_left',
        landmark: 'Wzgórze',
      },
      {
        instruction: 'Dotarłeś do celu: ' + destination.name,
        distanceMeters: 0,
        durationSeconds: 0,
        type: 'arrive',
        landmark: destination.name,
      },
    ],
  };

  // Alternative 3: Najcichsza (Quietest - omijająca tłum i hałas uliczny)
  const quietestRoute: RouteAlternative = {
    id: 'route-quietest',
    profileType: 'quietest',
    title: 'Najcichsza',
    subtitle: 'Zaciszna trasa parkowa z dala od zgiełku',
    durationMinutes: 14,
    distanceMeters: 690,
    stairsCount: 0,
    curbCount: 0,
    hasRoughSurface: false,
    transitChanges: 0,
    confidenceScore: 92,
    dominantAdvantage: 'Niski poziom hałasu i brak tłumów pieszych',
    accessibilitySummary: {
      satisfiedHardConstraints: ['Zero barier architektonicznych', 'Niski poziom bodźców sensorycznych'],
      warnings: [],
      highlights: ['Trasa przez strefę ciszy Plant', 'Brak ruchu kołowego'],
    },
    maneuvers: [
      {
        instruction: 'Idź cichą aleją w cieniu drzew',
        distanceMeters: 400,
        durationSeconds: 380,
        type: 'continue',
        landmark: 'Ogród Muzeum Archeologicznego',
        accessibilityNote: 'Pojedynczy piesi, brak głośnych lokali.',
      },
      {
        instruction: 'Skręć w stronę wejścia rekreacyjnego',
        distanceMeters: 290,
        durationSeconds: 270,
        type: 'turn_left',
      },
      {
        instruction: 'Cel osiągnięty: ' + destination.name,
        distanceMeters: 0,
        durationSeconds: 0,
        type: 'arrive',
      },
    ],
  };

  // Alternative 4: Najlepiej wspierana (Best Supported - transport niskoemisyjny/niski tabor MPK)
  const bestSupportedRoute: RouteAlternative = {
    id: 'route-supported',
    profileType: 'best_supported',
    title: 'Najlepiej wspierana',
    subtitle: 'Niskopodłogowy tramwaj MPK Kraków + rampa',
    durationMinutes: 9,
    distanceMeters: 950,
    stairsCount: 0,
    curbCount: 0,
    hasRoughSurface: false,
    transitChanges: 1,
    confidenceScore: 99,
    dominantAdvantage: 'Pojazd 100% niskopodłogowy z asystą motorniczego',
    accessibilitySummary: {
      satisfiedHardConstraints: ['Tabor 100% niskopodłogowy (Lajkonik PESA)', 'Winda i pochylnia peronowa'],
      warnings: [],
      highlights: ['Zapowiedzi głosowe w pojeździe i na przystanku', 'Wydzielone miejsce dla 2 wózków'],
    },
    maneuvers: [
      {
        instruction: 'Przejdź na przystanek Wawel (peron A, krawędź 22 cm)',
        distanceMeters: 90,
        durationSeconds: 80,
        type: 'depart',
        landmark: 'Przystanek MPK Wawel',
        accessibilityNote: 'Krawężnik kasetowy ułatwiający wsiadanie.',
      },
      {
        instruction: 'Wsiądź do tramwaju linii 18 (kierunek Czerwone Maki, tabor niskopodłogowy)',
        distanceMeters: 750,
        durationSeconds: 320,
        type: 'transit_board',
        landmark: 'Tramwaj MPK Lajkonik',
        accessibilityNote: 'Przycisk żądania wysunięcia rampy oznaczony braillem.',
      },
      {
        instruction: 'Wysiądź na przystanku docelowym i przejdź rampą do celu',
        distanceMeters: 110,
        durationSeconds: 100,
        type: 'arrive',
        landmark: destination.name,
      },
    ],
  };

  return {
    originName: 'Twoja lokalizacja (Kraków Stare Miasto)',
    destination,
    alternatives: [easiestRoute, fastestRoute, quietestRoute, bestSupportedRoute],
  };
}
