import { KRAKOW_PLACES, KrakowPlace } from './krakowData';

export interface AddressSearchResult {
  id: string;
  title: string;
  subtitle: string;
  coordinates: {
    latitude: number;
    longitude: number;
  };
  isPoi: boolean;
  placeData?: KrakowPlace;
}

export async function searchKrakowAddresses(query: string): Promise<AddressSearchResult[]> {
  const trimmed = query.trim();
  if (!trimmed) return [];

  const results: AddressSearchResult[] = [];

  // 1. Search local curated Kraków POIs
  const localMatches = KRAKOW_PLACES.filter(
    (p) =>
      p.name.toLowerCase().includes(trimmed.toLowerCase()) ||
      p.address.toLowerCase().includes(trimmed.toLowerCase())
  );

  for (const match of localMatches) {
    results.push({
      id: 'poi-' + match.id,
      title: match.name,
      subtitle: match.address,
      coordinates: match.coordinates,
      isPoi: true,
      placeData: match,
    });
  }

  // 2. Query OpenStreetMap Photon Geocoder (CORS enabled, fast, no forbidden headers)
  if (trimmed.length >= 2) {
    try {
      const url = `https://photon.komoot.io/api/?q=${encodeURIComponent(
        trimmed + ' Kraków'
      )}&lat=50.0617&lon=19.9373&limit=5`;

      const response = await fetch(url);
      if (response.ok) {
        const data = await response.json();
        const features = data.features || [];

        for (const f of features) {
          const coords = f.geometry?.coordinates;
          if (!coords || coords.length < 2) continue;

          const lon = coords[0];
          const lat = coords[1];
          const props = f.properties || {};

          // Focus on Kraków and immediate vicinity
          const city = props.city || props.county || 'Kraków';
          const placeName = props.name;
          const street = props.street || '';
          const house = props.housenumber ? ` ${props.housenumber}` : '';
          
          let title = '';
          let subtitle = '';
          
          if (placeName && street) {
            title = placeName;
            subtitle = `${street}${house}, ${props.district || props.suburb || 'Kraków'}`;
          } else {
            title = placeName || `${street}${house}` || trimmed;
            subtitle = `${props.district || props.suburb || 'Kraków'}, ${city}`;
          }

          // Avoid duplicates
          const isDuplicate = results.some(
            (r) =>
              Math.abs(r.coordinates.latitude - lat) < 0.0003 &&
              Math.abs(r.coordinates.longitude - lon) < 0.0003
          );

          if (!isDuplicate) {
            results.push({
              id: 'osm-' + (props.osm_id || Math.random().toString(36).substring(7)),
              title,
              subtitle,
              coordinates: {
                latitude: lat,
                longitude: lon,
              },
              isPoi: false,
            });
          }
        }
      }
    } catch (err) {
      console.warn('OSM Geocoding warning:', err);
    }
  }

  return results.slice(0, 6);
}
