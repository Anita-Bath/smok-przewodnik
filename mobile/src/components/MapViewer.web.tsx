import React, { useEffect, useRef, useState } from 'react';
import { View, Text, StyleSheet, Pressable, ActivityIndicator } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { BrandColors } from '@/constants/theme';
import { KrakowPlace, KRAKOW_PLACES } from '@/services/krakowData';
import { useAccessibility } from '@/context/AccessibilityContext';
import { generateLeafletHtml } from './leafletMapHtml';
import * as Location from 'expo-location';

interface MapViewerProps {
  selectedPlace: KrakowPlace | null;
  onSelectPlace: (place: KrakowPlace) => void;
  onLocateMe?: (coords: { latitude: number; longitude: number }) => void;
  places?: KrakowPlace[];
}

export function MapViewer({
  selectedPlace,
  onSelectPlace,
  onLocateMe,
  places = KRAKOW_PLACES,
}: MapViewerProps) {
  const { isHighContrast } = useAccessibility();
  const iframeRef = useRef<HTMLIFrameElement>(null);
  const [userLocation, setUserLocation] = useState<{ latitude: number; longitude: number } | null>(null);
  const [isLocating, setIsLocating] = useState(false);

  // Generate initial HTML
  const htmlContent = generateLeafletHtml(
    places,
    selectedPlace?.id,
    userLocation,
    isHighContrast
  );

  // Listen to messages from the Leaflet iframe
  useEffect(() => {
    const handleMessage = (event: MessageEvent) => {
      try {
        const data = typeof event.data === 'string' ? JSON.parse(event.data) : event.data;
        if (data && data.type === 'SELECT_PLACE') {
          const found = places.find((p) => p.id === data.placeId);
          if (found) {
            onSelectPlace(found);
          }
        }
      } catch (err) {}
    };

    window.addEventListener('message', handleMessage);
    return () => window.removeEventListener('message', handleMessage);
  }, [places, onSelectPlace]);

  // Sync selectedPlace updates to iframe
  useEffect(() => {
    if (iframeRef.current && iframeRef.current.contentWindow && selectedPlace) {
      iframeRef.current.contentWindow.postMessage(
        JSON.stringify({
          type: 'SET_SELECTED_PLACE',
          placeId: selectedPlace.id,
        }),
        '*'
      );
    }
  }, [selectedPlace?.id]);

  // Sync highContrast updates
  useEffect(() => {
    if (iframeRef.current && iframeRef.current.contentWindow) {
      iframeRef.current.contentWindow.postMessage(
        JSON.stringify({
          type: 'SET_HIGH_CONTRAST',
          enabled: isHighContrast,
        }),
        '*'
      );
    }
  }, [isHighContrast]);

  // Sync places updates (e.g., when filtered)
  useEffect(() => {
    if (iframeRef.current && iframeRef.current.contentWindow) {
      iframeRef.current.contentWindow.postMessage(
        JSON.stringify({
          type: 'UPDATE_PLACES',
          places,
        }),
        '*'
      );
    }
  }, [places]);

  // Handle "Lokalizuj mnie" with actual geolocation
  const handleLocateMe = async () => {
    setIsLocating(true);
    try {
      let coords = { latitude: 50.0617, longitude: 19.9373 }; // Kraków Rynek default

      if (navigator.geolocation) {
        coords = await new Promise<{ latitude: number; longitude: number }>((resolve) => {
          navigator.geolocation.getCurrentPosition(
            (pos) => resolve({ latitude: pos.coords.latitude, longitude: pos.coords.longitude }),
            () => resolve({ latitude: 50.0617, longitude: 19.9373 }),
            { timeout: 8000, enableHighAccuracy: true }
          );
        });
      } else {
        const { status } = await Location.requestForegroundPermissionsAsync();
        if (status === 'granted') {
          const pos = await Location.getCurrentPositionAsync({ accuracy: Location.Accuracy.Balanced });
          coords = { latitude: pos.coords.latitude, longitude: pos.coords.longitude };
        }
      }

      setUserLocation(coords);
      if (onLocateMe) {
        onLocateMe(coords);
      }

      // Notify iframe to move to user location
      if (iframeRef.current && iframeRef.current.contentWindow) {
        iframeRef.current.contentWindow.postMessage(
          JSON.stringify({
            type: 'SET_USER_LOCATION',
            coords,
            panToUser: true,
          }),
          '*'
        );
      }
    } catch (err) {
      // Default to Kraków Rynek
      const coords = { latitude: 50.0617, longitude: 19.9373 };
      setUserLocation(coords);
      if (onLocateMe) onLocateMe(coords);
    } finally {
      setIsLocating(false);
    }
  };

  return (
    <View style={styles.container}>
      {/* Real OpenStreetMap + Leaflet Map */}
      <iframe
        ref={iframeRef}
        srcDoc={htmlContent}
        style={{
          width: '100%',
          height: '100%',
          border: isHighContrast ? '3px solid #000000' : '1px solid #CBD5E1',
          borderRadius: 16,
          backgroundColor: '#F1F5F9',
        }}
        title="OpenStreetMap Kraków"
      />

      {/* Floating "Lokalizuj mnie" button with real location execution */}
      <Pressable
        onPress={handleLocateMe}
        disabled={isLocating}
        accessible
        accessibilityRole="button"
        accessibilityLabel="Lokalizuj mnie na mapie"
        accessibilityHint="Pobiera Twoją aktualną pozycję GPS i centruje mapę"
        style={({ pressed }) => [
          styles.locateButton,
          {
            backgroundColor: '#FFFFFF',
            borderColor: isHighContrast ? '#000000' : BrandColors.accentTeal,
            borderWidth: isHighContrast ? 3 : 1.5,
            opacity: pressed || isLocating ? 0.8 : 1,
          },
        ]}>
        {isLocating ? (
          <ActivityIndicator
            size="small"
            color={isHighContrast ? '#000000' : BrandColors.accentTeal}
          />
        ) : (
          <MaterialCommunityIcons
            name="crosshairs-gps"
            size={20}
            color={isHighContrast ? '#000000' : BrandColors.accentTeal}
          />
        )}
        <Text
          style={[
            styles.locateText,
            {
              color: isHighContrast ? '#000000' : BrandColors.accentTeal,
              fontWeight: isHighContrast ? '900' : '700',
            },
          ]}>
          {isLocating ? 'Lokalizuję...' : 'Lokalizuj mnie'}
        </Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    position: 'relative',
    marginHorizontal: 12,
    marginVertical: 4,
    borderRadius: 16,
    overflow: 'hidden',
  },
  locateButton: {
    position: 'absolute',
    right: 14,
    top: 14,
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: 24,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.2,
    shadowRadius: 4,
    elevation: 4,
    zIndex: 1000,
  },
  locateText: {
    fontSize: 14,
  },
});
