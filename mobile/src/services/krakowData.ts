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

import { supabase } from './supabase';

function parseEWKBPoint(hexStr: string) {
  if (!hexStr || hexStr.length < 50) return { latitude: 50.0619, longitude: 19.9368 };
  const bytes = new Uint8Array(hexStr.match(/.{1,2}/g)!.map(byte => parseInt(byte, 16)));
  const view = new DataView(bytes.buffer);
  const longitude = view.getFloat64(9, true); // little-endian
  const latitude = view.getFloat64(17, true);
  return { latitude, longitude };
}

export async function fetchKrakowPlacesFromDB(): Promise<KrakowPlace[]> {
  const { data, error } = await supabase
    .from('spatial_entities')
    .select(`
      id, kind, geometry, confidence_state,
      entity_translations(locale, name, description),
      spatial_entity_details(place_category_code),
      accessibility_facts(id, attribute_code, value_kind, boolean_value, evidence_kind)
    `);

  if (error || !data) {
    console.error('Error fetching places:', error);
    return KRAKOW_PLACES; // fallback
  }

  return data.map((row: any) => {
    const coords = parseEWKBPoint(row.geometry);
    const tr = row.entity_translations && row.entity_translations.length > 0 ? row.entity_translations[0] : { name: row.kind, description: '' };
    const details = row.spatial_entity_details && row.spatial_entity_details.length > 0 ? row.spatial_entity_details[0] : {};
    const facts = row.accessibility_facts || [];

    return {
      id: row.id,
      name: tr.name || row.kind,
      address: 'Kraków',
      distanceFromUserMeters: 500,
      category: (details.place_category_code as any) || 'monument',
      coordinates: coords,
      confidenceState: row.confidence_state as any,
      confidenceLabel: 'Pobrano z bazy',
      generalNote: tr.description,
      hasStepFreeAccess: facts.some((f: any) => f.attribute_code === 'step_free' && f.boolean_value),
      hasElevator: facts.some((f: any) => f.attribute_code === 'elevator' && f.boolean_value),
      hasAccessibleToilet: facts.some((f: any) => f.attribute_code === 'accessible_toilet' && f.boolean_value),
      hasInductionLoop: facts.some((f: any) => f.attribute_code === 'induction_loop' && f.boolean_value),
      hasAudioGuidance: facts.some((f: any) => f.attribute_code === 'audio_guidance' && f.boolean_value),
      hasRoughSurfaceNotice: false,
      facts: facts.map((f: any) => ({
        id: f.id,
        name: f.attribute_code,
        status: f.evidence_kind === 'verified' ? 'verified' : 'unverified',
        label: f.attribute_code,
        description: ''
      }))
    };
  });
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
    facts: [],
    generalNote: 'Dostępność może się zmieniać. Przed wizytą sprawdź informacje u miejsca.',
    hasStepFreeAccess: true,
    hasElevator: true,
    hasAccessibleToilet: true,
    hasInductionLoop: true,
    hasAudioGuidance: true,
    hasRoughSurfaceNotice: true,
  }
];
