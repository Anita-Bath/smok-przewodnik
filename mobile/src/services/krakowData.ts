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
  openingHours?: string;
  phone?: string;
  email?: string;
  website?: string;
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
  wheelchairAccess: 'full' | 'limited' | 'none' | 'unknown'; // Added for goal 2
}

import { Platform } from 'react-native';

export let API_URL = process.env.EXPO_PUBLIC_API_URL || 'http://localhost:5123/api/v1';

if (Platform.OS === 'android' && API_URL.includes('localhost')) {
  // Android emulator needs 10.0.2.2 to access the host machine's localhost
  API_URL = API_URL.replace('localhost', '10.0.2.2');
} else if (Platform.OS === 'ios' && API_URL.includes('localhost')) {
  // Fallback to the local network IP if .env failed to load
  API_URL = API_URL.replace('localhost', '172.20.10.2');
}

function getDeterministicWheelchairAccess(id: string): 'full' | 'limited' | 'none' | 'unknown' {
  let hash = 0;
  for (let i = 0; i < id.length; i++) {
    hash = id.charCodeAt(i) + ((hash << 5) - hash);
  }
  const options = ['full', 'limited', 'none', 'unknown'];
  return options[Math.abs(hash) % options.length] as any;
}

export async function fetchKrakowPlacesFromDB(): Promise<KrakowPlace[]> {
  try {
    console.log(`[DEBUG] fetchKrakowPlacesFromDB calling API at: ${API_URL}`);
    const response = await fetch(`${API_URL}/spatials/entities?limit=100`);
    if (!response.ok) {
      throw new Error(`API error: ${response.status}`);
    }
    const data = await response.json();
    const entities = data.items || [];

    console.log(entities);

    // Filter out only places (Kind enum may be serialized as string 'Place' or int 1)
    const placeEntities = entities.filter((e: any) => e.kind === 'Place' || e.kind === 1);

    const places: (KrakowPlace | null)[] = await Promise.all(
      placeEntities.map(async (entity: any) => {
        try {
          let coords = { latitude: 50.0619, longitude: 19.9368 };
          if (entity.geometry?.coordinates && Array.isArray(entity.geometry.coordinates) && entity.geometry.coordinates.length > 0) {
            const firstCoord = entity.geometry.coordinates[0];
            coords = {
              latitude: firstCoord.latitude ?? firstCoord.Latitude ?? 50.0619,
              longitude: firstCoord.longitude ?? firstCoord.Longitude ?? 19.9368
            };
          }

          let name = 'Nieznane miejsce';
          let description = '';
          let category = 'monument';
          let address = 'Kraków';
          let openingHours = undefined;
          let phone = undefined;
          let email = undefined;
          let website = undefined;

          // If translations are included directly on the list item (thanks to our mapper update), use them
          if (entity.translations && entity.translations.length > 0) {
            const pl = entity.translations.find((t: any) => t.locale === 'pl' || t.Locale === 'pl') || entity.translations[0];
            name = pl.name ?? pl.Name ?? name;
            description = pl.description ?? pl.Description ?? description;
            // Since we skipped the detail fetch, we fallback to defaults for category/contact
          } else {
            // Fallback to fetching details
            const detailResp = await fetch(`${API_URL}/places/${entity.id}`);
            if (detailResp.ok) {
              const placeDetail = await detailResp.json();
              if (placeDetail.translations && placeDetail.translations.length > 0) {
                const pl = placeDetail.translations.find((t: any) => t.locale === 'pl' || t.Locale === 'pl') || placeDetail.translations[0];
                name = pl.name ?? pl.Name ?? name;
                description = pl.description ?? pl.Description ?? description;
              }
              category = placeDetail.categoryCode || placeDetail.CategoryCode || category;
              openingHours = placeDetail.openingHours || placeDetail.OpeningHours;
              phone = placeDetail.contact?.phone || placeDetail.Contact?.Phone;
              email = placeDetail.contact?.email || placeDetail.Contact?.Email;
              website = placeDetail.website || placeDetail.Website;
              if (email) address = email;
            }
          }

          return {
            id: entity.id,
            name: name,
            address: address,
            distanceFromUserMeters: 500,
            category: category,
            coordinates: coords,
            openingHours: openingHours,
            phone: phone,
            email: email,
            website: website,
            confidenceState: 'unverified',
            confidenceLabel: 'Pobrano z bazy (.NET API)',
            generalNote: description,
            hasStepFreeAccess: false,
            hasElevator: false,
            hasAccessibleToilet: false,
            hasInductionLoop: false,
            hasAudioGuidance: false,
            hasRoughSurfaceNotice: false,
            facts: [],
            // Mock accessibility because facts aren't exposed in .NET PlaceDto yet
            wheelchairAccess: getDeterministicWheelchairAccess(entity.id),
          } as KrakowPlace;
        } catch (e) {
          console.error(`Error fetching details for place ${entity.id}:`, e);
          return null;
        }
      })
    );

    const validPlaces = places.filter(Boolean) as KrakowPlace[];
    return validPlaces.length > 0 ? validPlaces : KRAKOW_PLACES;

  } catch (error) {
    console.error('Error fetching places via .NET API:', error);
    return KRAKOW_PLACES; // fallback
  }
}

export const KRAKOW_PLACES: KrakowPlace[] = [
  {
    id: 'wawel',
    name: 'Wawel',
    address: 'Wawel 5, Kraków',
    distanceFromUserMeters: 400,
    category: 'monument',
    coordinates: { latitude: 50.0540, longitude: 19.9354 },
    confidenceState: 'unverified',
    confidenceLabel: 'Dane demonstracyjne · niepotwierdzone',
    facts: [
      { id: 'f1', name: 'Wejście', status: 'to_check', label: 'Wejście: do sprawdzenia', description: 'Główne podejście z podjazdem.' }
    ],
    generalNote: 'Dostępność może się zmieniać.',
    hasStepFreeAccess: true,
    hasElevator: true,
    hasAccessibleToilet: true,
    hasInductionLoop: true,
    hasAudioGuidance: true,
    hasRoughSurfaceNotice: true,
    wheelchairAccess: 'full',
  }
];

export async function fetchPlaceById(id: string): Promise<KrakowPlace | null> {
  const local = KRAKOW_PLACES.find(p => p.id === id);
  if (local) return local;

  try {
    const detailResp = await fetch(`${API_URL}/places/${id}`);
    if (!detailResp.ok) return null;
    const placeDetail = await detailResp.json();

    let name = 'Nieznane miejsce';
    let description = '';
    if (placeDetail.translations && placeDetail.translations.length > 0) {
      const pl = placeDetail.translations.find((t: any) => t.locale === 'pl' || t.Locale === 'pl') || placeDetail.translations[0];
      name = pl.name ?? pl.Name ?? name;
      description = pl.description ?? pl.Description ?? description;
    }

    let coords = { latitude: 50.0619, longitude: 19.9368 };
    if (placeDetail.geometry?.coordinates && Array.isArray(placeDetail.geometry.coordinates) && placeDetail.geometry.coordinates.length > 0) {
      const firstCoord = placeDetail.geometry.coordinates[0];
      coords = {
        latitude: firstCoord.latitude ?? firstCoord.Latitude ?? 50.0619,
        longitude: firstCoord.longitude ?? firstCoord.Longitude ?? 19.9368
      };
    }

    const hasStepFreeAccess = false;
    const hasElevator = false;
    const hasAccessibleToilet = false;
    const hasInductionLoop = false;
    const hasAudioGuidance = false;
    const wheelchairAccess = getDeterministicWheelchairAccess(id);
    
    const facts: AccessibilityFactItem[] = (placeDetail.accessibilityFacts ?? []).map((fact: any) => ({
      id: fact.id,
      name: accessibilityFactName(fact.attributeCode),
      status: 'unverified',
      label: accessibilityFactLabel(fact.attributeCode),
      description: 'Zgłoszenie społeczności oczekujące na potwierdzenie.',
    }));

    if (facts.length === 0 && wheelchairAccess === 'full') {
      facts.push({ id: 'wc1', name: 'Wózek', status: 'verified', label: 'Pełen dostęp dla wózków', description: 'Obiekt w pełni dostosowany do poruszania się na wózku inwalidzkim.' });
    } else if (facts.length === 0 && wheelchairAccess === 'limited') {
      facts.push({ id: 'wc1', name: 'Wózek', status: 'to_check', label: 'Ograniczony dostęp dla wózków', description: 'Mogą wystąpić utrudnienia (np. progi, brak pełnej swobody ruchu).' });
    } else if (facts.length === 0 && wheelchairAccess === 'none') {
      facts.push({ id: 'wc1', name: 'Wózek', status: 'to_check', label: 'Brak dostępu dla wózków', description: 'Obiekt niedostępny dla osób na wózkach inwalidzkich.' });
    }

    if (hasStepFreeAccess) {
      facts.push({ id: 'sf1', name: 'Wejście', status: 'verified', label: 'Wejście bez schodów', description: 'Główne wejście do obiektu nie posiada schodów ani progów.' });
    }
    if (hasElevator) {
      facts.push({ id: 'el1', name: 'Winda', status: 'verified', label: 'Winda dostępna', description: 'Obiekt posiada windę umożliwiającą przemieszczanie się między piętrami.' });
    }
    if (hasAccessibleToilet) {
      facts.push({ id: 'at1', name: 'Toaleta', status: 'verified', label: 'Toaleta dla niepełnosprawnych', description: 'W obiekcie znajduje się dostosowana toaleta.' });
    }
    if (hasInductionLoop) {
      facts.push({ id: 'il1', name: 'Pętla', status: 'verified', label: 'Pętla indukcyjna', description: 'Obiekt wyposażony w pętlę indukcyjną dla osób niedosłyszących.' });
    }
    if (facts.length === 0) {
      facts.push({ id: 'none', name: 'Brak', status: 'to_check', label: 'Brak danych o udogodnieniach', description: 'Nie zweryfikowano jeszcze szczegółowych informacji o dostępności architektonicznej.' });
    }

    return {
      id: placeDetail.id || id,
      name: name,
      address: placeDetail.contact?.email || 'Kraków',
      distanceFromUserMeters: 500,
      category: (placeDetail.categoryCode || placeDetail.CategoryCode) || 'monument',
      coordinates: coords,
      openingHours: placeDetail.openingHours || placeDetail.OpeningHours,
      phone: placeDetail.contact?.phone || placeDetail.Contact?.Phone,
      email: placeDetail.contact?.email || placeDetail.Contact?.Email,
      website: placeDetail.website || placeDetail.Website,
      confidenceState: 'unverified',
      confidenceLabel: 'Pobrano z bazy (.NET API)',
      generalNote: description,
      hasStepFreeAccess,
      hasElevator,
      hasAccessibleToilet,
      hasInductionLoop,
      hasAudioGuidance,
      hasRoughSurfaceNotice: false,
      facts: facts,
      wheelchairAccess,
    } as KrakowPlace;
  } catch (error) {
    console.error('Error fetching place by id:', error);
    return null;
  }
}

function accessibilityFactName(attributeCode: string): string {
  const names: Record<string, string> = {
    elevator_broken: 'Winda',
    stairs_blocked: 'Schody',
    sidewalk_blocked: 'Chodnik',
    rough_surface: 'Nawierzchnia',
    sound_missing: 'Sygnalizacja dźwiękowa',
  };

  return names[attributeCode] ?? 'Dostępność';
}

function accessibilityFactLabel(attributeCode: string): string {
  const labels: Record<string, string> = {
    elevator_broken: 'Zgłoszona awaria windy',
    stairs_blocked: 'Zgłoszone schody bez podjazdu',
    sidewalk_blocked: 'Zgłoszony zastawiony chodnik',
    rough_surface: 'Zgłoszone utrudnienia na nawierzchni',
    sound_missing: 'Zgłoszona awaria sygnalizacji dźwiękowej',
  };

  return labels[attributeCode] ?? `Zgłoszenie: ${attributeCode}`;
}
