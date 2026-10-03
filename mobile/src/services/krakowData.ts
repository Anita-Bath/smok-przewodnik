export interface AccessibilityFactItem {
  id: string;
  name: string;
  status: 'verified' | 'unverified' | 'to_check' | 'missing';
  label: string;
  description?: string;
}

export interface KrakowPlace {
  id: string;
  name: string;
  address: string;
  distanceFromUserMeters: number;
  category: 'monument' | 'transit' | 'restroom' | 'park' | 'culture' | 'cafe';
  coordinates: {
    latitude: number;
    longitude: number;
  };
  confidenceState: 'unverified' | 'supported' | 'disputed' | 'verified_official';
  confidenceLabel: string;
  facts: AccessibilityFactItem[];
  generalNote?: string;
  hasStepFreeAccess: boolean;
  hasElevator: boolean;
  hasAccessibleToilet: boolean;
  hasInductionLoop: boolean;
  hasAudioGuidance: boolean;
  hasRoughSurfaceNotice: boolean;
}

export const KRAKOW_PLACES: KrakowPlace[] = [
  {
    id: 'wawel',
    name: 'Wawel',
    address: 'Wawel 5, Kraków',
    distanceFromUserMeters: 400,
    category: 'monument',
    coordinates: {
      latitude: 50.0540,
      longitude: 19.9354,
    },
    confidenceState: 'unverified',
    confidenceLabel: 'Dane demonstracyjne · niepotwierdzone',
    facts: [
      { id: 'f1', name: 'Wejście', status: 'to_check', label: 'Wejście: do sprawdzenia', description: 'Główne podejście z podjazdem od ul. Bernardyńskiej.' },
      { id: 'f2', name: 'Winda', status: 'to_check', label: 'Winda: do sprawdzenia', description: 'Winda w skrzydle zachodnim Zamku Królewskiego.' },
      { id: 'f3', name: 'Pętla', status: 'to_check', label: 'Pętla: do sprawdzenia', description: 'Pętla indukcyjna w kasie biletowej Centrum Informacji.' },
      { id: 'f4', name: 'Toaleta', status: 'to_check', label: 'Toaleta: do sprawdzenia', description: 'Dostosowana toaleta na Dziedzińcu Zewnętrznym.' },
    ],
    generalNote: 'Dostępność może się zmieniać. Przed wizytą sprawdź informacje u miejsca.',
    hasStepFreeAccess: true,
    hasElevator: true,
    hasAccessibleToilet: true,
    hasInductionLoop: true,
    hasAudioGuidance: true,
    hasRoughSurfaceNotice: true,
  },
  {
    id: 'rynek-glowny',
    name: 'Rynek Główny',
    address: 'Rynek Główny, Kraków',
    distanceFromUserMeters: 250,
    category: 'culture',
    coordinates: {
      latitude: 50.0619,
      longitude: 19.9368,
    },
    confidenceState: 'unverified',
    confidenceLabel: 'Brak potwierdzenia',
    facts: [
      { id: 'f1', name: 'Wejście', status: 'to_check', label: 'Wejście: bez schodów', description: 'Płyta rynku dostępna z każdego kierunku.' },
      { id: 'f2', name: 'Nawierzchnia', status: 'to_check', label: 'Nawierzchnia: kostka brukowa', description: 'Odcinki z nierówną zabytkową kostką, sugerowane gładkie płyty granitowe wzdłuż linii A-B.' },
      { id: 'f3', name: 'Miejsca odpoczynku', status: 'verified', label: 'Ławki: dostępne', description: 'Liczne ławki miejskie wokół Sukiennic.' },
    ],
    generalNote: 'Duże zagęszczenie pieszych w godzinach popołudniowych.',
    hasStepFreeAccess: true,
    hasElevator: false,
    hasAccessibleToilet: false,
    hasInductionLoop: false,
    hasAudioGuidance: false,
    hasRoughSurfaceNotice: true,
  },
  {
    id: 'planty',
    name: 'Planty',
    address: 'Planty Krakowskie, Kraków',
    distanceFromUserMeters: 180,
    category: 'park',
    coordinates: {
      latitude: 50.0592,
      longitude: 19.9405,
    },
    confidenceState: 'unverified',
    confidenceLabel: 'Brak potwierdzenia',
    facts: [
      { id: 'f1', name: 'Trasa', status: 'verified', label: 'Alejki: płaskie asfaltowe', description: 'Szerokie, bezpieczne alejki bez progów architektonicznych.' },
      { id: 'f2', name: 'Oświetlenie', status: 'verified', label: 'Oświetlenie: dobre', description: 'Oświetlenie parkowe LED na całym obwodzie.' },
      { id: 'f3', name: 'Ławki', status: 'verified', label: 'Miejsca spoczynku: co 100m', description: 'Regularnie rozmieszczone ławki z oparciami.' },
    ],
    generalNote: 'Spokojna, cicha trasa omijająca hałaśliwy ruch kołowy.',
    hasStepFreeAccess: true,
    hasElevator: false,
    hasAccessibleToilet: false,
    hasInductionLoop: false,
    hasAudioGuidance: false,
    hasRoughSurfaceNotice: false,
  },
  {
    id: 'dworzec-glowny',
    name: 'Dworzec Główny',
    address: 'Pawia 5a, Kraków',
    distanceFromUserMeters: 850,
    category: 'transit',
    coordinates: {
      latitude: 50.0681,
      longitude: 19.9482,
    },
    confidenceState: 'verified_official',
    confidenceLabel: 'Oficjalnie potwierdzone (MPK / PKP)',
    facts: [
      { id: 'f1', name: 'Windy', status: 'verified', label: 'Windy: działające na wszystkie perony', description: 'Bezpośredni zjazd z tunelu Magda na perony 1-5.' },
      { id: 'f2', name: 'Ścieżki dotykowe', status: 'verified', label: 'Pasy prowadzące: na całej długości', description: 'Faktury ostrzegawcze i linie naprowadzające dla osób niewidomych.' },
      { id: 'f3', name: 'Obsługa asystenta', status: 'verified', label: 'Punkt asysty PKP: czynny 24/7', description: 'Bezpłatna asysta dla podróżnych z niepełnosprawnościami.' },
      { id: 'f4', name: 'Toalety TSR', status: 'verified', label: 'Toalety bez barier: dostępne', description: 'System Euro-Key oraz obsługa bezdotykowa.' },
    ],
    generalNote: 'W pełni zintegrowany węzeł kolejowo-tramwajowo-autobusowy.',
    hasStepFreeAccess: true,
    hasElevator: true,
    hasAccessibleToilet: true,
    hasInductionLoop: true,
    hasAudioGuidance: true,
    hasRoughSurfaceNotice: false,
  },
  {
    id: 'toalety-publiczne',
    name: 'Toalety publiczne',
    address: 'Plac Wszystkich Świętych, Kraków',
    distanceFromUserMeters: 310,
    category: 'restroom',
    coordinates: {
      latitude: 50.0588,
      longitude: 19.9380,
    },
    confidenceState: 'unverified',
    confidenceLabel: 'Brak potwierdzenia',
    facts: [
      { id: 'f1', name: 'Dostępność', status: 'verified', label: 'Wejście z poziomu chodnika', description: 'Automatycznie rozsuwane drzwi o szerokości 100 cm.' },
      { id: 'f2', name: 'Uchwyty', status: 'verified', label: 'Pochwyty uchylne po obu stronach', description: 'Przystosowane dla wózków inwalidzkich z systemem alarmowym SOS.' },
      { id: 'f3', name: 'Przewijak', status: 'verified', label: 'Przewijak dziecięcy', description: 'Dostępny zintegrowany stół do przewijania niemowląt.' },
    ],
    generalNote: 'Czynne codziennie w godzinach 7:00 - 22:00.',
    hasStepFreeAccess: true,
    hasElevator: false,
    hasAccessibleToilet: true,
    hasInductionLoop: false,
    hasAudioGuidance: false,
    hasRoughSurfaceNotice: false,
  },
  {
    id: 'sukiennice',
    name: 'Sukiennice (Galeria Sztuki)',
    address: 'Rynek Główny 1/3, Kraków',
    distanceFromUserMeters: 260,
    category: 'culture',
    coordinates: {
      latitude: 50.0617,
      longitude: 19.9373,
    },
    confidenceState: 'supported',
    confidenceLabel: 'Potwierdzone społecznościowo (4 głosy)',
    facts: [
      { id: 'f1', name: 'Winda', status: 'verified', label: 'Winda: dostęp do galerii na piętrze', description: 'Winda dostępna od strony podcieni od ul. Szewskiej.' },
      { id: 'f2', name: 'Audioprzewodnik', status: 'verified', label: 'Audiodeskrypcja: dostępna', description: 'Dedykowane ścieżki zwiedzania z audiodeskrypcją dla niewidomych.' },
      { id: 'f3', name: 'Pętla indukcyjna', status: 'verified', label: 'Kasa z pętlą indukcyjną', description: 'Oznakowane stanowisko kasowe z pętlą słuchową.' },
    ],
    generalNote: 'Oddział Muzeum Narodowego w Krakowie.',
    hasStepFreeAccess: true,
    hasElevator: true,
    hasAccessibleToilet: true,
    hasInductionLoop: true,
    hasAudioGuidance: true,
    hasRoughSurfaceNotice: false,
  },
];
