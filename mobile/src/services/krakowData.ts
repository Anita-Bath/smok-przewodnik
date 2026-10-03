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

import { Platform } from 'react-native';

let API_URL = process.env.EXPO_PUBLIC_API_URL || 'http://localhost:5123/api/v1';

if (Platform.OS === 'android' && API_URL.includes('localhost')) {
  // Android emulator needs 10.0.2.2 to access the host machine's localhost
  API_URL = API_URL.replace('localhost', '10.0.2.2');
} else if (Platform.OS === 'ios' && API_URL.includes('localhost')) {
  // Fallback to the local network IP if .env failed to load
  API_URL = API_URL.replace('localhost', '172.20.10.2');
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

    // Filter out only places (Kind enum may be serialized as string 'Place' or int 1)
    const placeEntities = entities.filter((e: any) => e.kind === 'Place' || e.kind === 1);

    const places: (KrakowPlace | null)[] = await Promise.all(
      placeEntities.map(async (entity: any) => {
        try {
          const detailResp = await fetch(`${API_URL}/spatials/places/${entity.id}`);
          if (!detailResp.ok) {
            return null;
          }
          const placeDetail = await detailResp.json();
          
          const coords = entity.geometry?.coordinates?.[0] || { latitude: 50.0619, longitude: 19.9368 };
          
          let name = 'Nieznane miejsce';
          let description = '';
          if (placeDetail.translations && placeDetail.translations.length > 0) {
            const pl = placeDetail.translations.find((t: any) => t.locale === 'pl') || placeDetail.translations[0];
            name = pl.name;
            description = pl.description;
          }

          return {
            id: entity.id,
            name: name,
            address: placeDetail.contact?.email || 'Kraków',
            distanceFromUserMeters: 500,
            category: (placeDetail.categoryCode as any) || 'monument',
            coordinates: coords,
            confidenceState: 'unverified',
            confidenceLabel: 'Pobrano z bazy (.NET API)',
            generalNote: description,
            hasStepFreeAccess: false,
            hasElevator: false,
            hasAccessibleToilet: false,
            hasInductionLoop: false,
            hasAudioGuidance: false,
            hasRoughSurfaceNotice: false,
            facts: []
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
  }
];
