import React, { useEffect, useRef, useState, useMemo } from 'react';
import { View, Text, StyleSheet, Pressable, ActivityIndicator } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { BrandColors } from '@/constants/theme';
import { KrakowPlace, KRAKOW_PLACES } from '@/services/krakowData';
import { useAccessibility } from '@/context/AccessibilityContext';
import { generateLeafletHtml } from './leafletMapHtml';
import * as Location from 'expo-location';

interface MapViewerProps {
  selectedPlace?: KrakowPlace | null;
  onSelectPlace?: (place: KrakowPlace) => void;
  onLocateMe?: (coords: { latitude: number; longitude: number }) => void;
  onMapClick?: () => void;
  places?: KrakowPlace[];
  searchPin?: { coords: { latitude: number; longitude: number }; label: string } | null;
  activeRoute?: { coordinates: [number, number][]; profileType?: string } | null;
  focusedManeuver?: [number, number] | null;
}

export function MapViewer({
  selectedPlace = null,
  onSelectPlace,
  onLocateMe,
  onMapClick,
  places = KRAKOW_PLACES,
  searchPin,
  activeRoute,
  focusedManeuver,
}: MapViewerProps) {
  const { isHighContrast, isDark, userLocation, setUserLocation } = useAccessibility();
  const iframeRef = useRef<HTMLIFrameElement>(null);
  const [isLocating, setIsLocating] = useState(false);
  const isMapReadyRef = useRef(false);

  // Generate initial HTML ONCE so iframe is not destroyed & recreated on re-renders!
  const initialHtml = useMemo(
    () => generateLeafletHtml(places, selectedPlace?.id, userLocation, isHighContrast, isDark),
    []
  );

  const sendMessageToIframe = (data: any) => {
    if (iframeRef.current && iframeRef.current.contentWindow) {
      iframeRef.current.contentWindow.postMessage(JSON.stringify(data), '*');
    }
  };

  // Listen to messages from the Leaflet iframe
  useEffect(() => {
    const handleMessage = (event: MessageEvent) => {
      try {
        const data = typeof event.data === 'string' ? JSON.parse(event.data) : event.data;
        if (!data) return;

        if (data.type === 'SELECT_PLACE') {
          const found = places.find((p) => p.id === data.placeId);
          if (found && onSelectPlace) {
            onSelectPlace(found);
          }
        } else if (data.type === 'MAP_CLICKED') {
          if (onMapClick) {
            onMapClick();
          }
        } else if (data.type === 'MAP_READY') {
          isMapReadyRef.current = true;
          // Sync current states
          sendMessageToIframe({ type: 'SET_DARK_MODE', enabled: isDark });
          sendMessageToIframe({ type: 'SET_HIGH_CONTRAST', enabled: isHighContrast });
          if (userLocation) {
            sendMessageToIframe({
              type: 'SET_USER_LOCATION',
              coords: userLocation,
              panToUser: !selectedPlace && !searchPin && !activeRoute,
            });
          }
          if (selectedPlace) {
            sendMessageToIframe({ type: 'SET_SELECTED_PLACE', placeId: selectedPlace.id });
          }
          if (searchPin) {
            sendMessageToIframe({ type: 'SET_SEARCH_PIN', coords: searchPin.coords, label: searchPin.label });
          }
          if (activeRoute && activeRoute.coordinates && activeRoute.coordinates.length > 0) {
            sendMessageToIframe({
              type: 'DRAW_ROUTE',
              coordinates: activeRoute.coordinates,
              profileType: activeRoute.profileType,
            });
          }
        }
      } catch (err) {}
    };

    window.addEventListener('message', handleMessage);
    return () => window.removeEventListener('message', handleMessage);
  }, [places, onSelectPlace, onMapClick, selectedPlace, searchPin, userLocation, activeRoute, isDark, isHighContrast]);

  // Sync dark mode updates to iframe
  useEffect(() => {
    sendMessageToIframe({
      type: 'SET_DARK_MODE',
      enabled: isDark,
    });
  }, [isDark]);

  // Sync highContrast updates to iframe
  useEffect(() => {
    sendMessageToIframe({
      type: 'SET_HIGH_CONTRAST',
      enabled: isHighContrast,
    });
  }, [isHighContrast]);

  // Sync userLocation updates to iframe
  useEffect(() => {
    if (userLocation) {
      sendMessageToIframe({
        type: 'SET_USER_LOCATION',
        coords: userLocation,
        panToUser: !selectedPlace && !searchPin && !activeRoute,
      });
    }
  }, [userLocation?.latitude, userLocation?.longitude]);

  // Sync selectedPlace updates to iframe
  useEffect(() => {
    if (selectedPlace) {
      sendMessageToIframe({
        type: 'SET_SELECTED_PLACE',
        placeId: selectedPlace.id,
      });
    }
  }, [selectedPlace?.id]);

  // Sync searchPin updates
  useEffect(() => {
    if (searchPin) {
      sendMessageToIframe({
        type: 'SET_SEARCH_PIN',
        coords: searchPin.coords,
        label: searchPin.label,
      });
    }
  }, [searchPin]);

  // Sync highContrast updates
  useEffect(() => {
    sendMessageToIframe({
      type: 'SET_HIGH_CONTRAST',
      enabled: isHighContrast,
    });
  }, [isHighContrast]);

  // Sync places updates (e.g., when filtered)
  useEffect(() => {
    sendMessageToIframe({
      type: 'UPDATE_PLACES',
      places,
    });
  }, [places]);

  // Sync activeRoute updates
  useEffect(() => {
    if (activeRoute && activeRoute.coordinates && activeRoute.coordinates.length > 0) {
      sendMessageToIframe({
        type: 'DRAW_ROUTE',
        coordinates: activeRoute.coordinates,
        profileType: activeRoute.profileType,
      });
    } else {
      sendMessageToIframe({
        type: 'CLEAR_ROUTE',
      });
    }
  }, [activeRoute]);

  // Sync focusedManeuver updates
  useEffect(() => {
    if (focusedManeuver) {
      sendMessageToIframe({
        type: 'FOCUS_MANEUVER',
        location: focusedManeuver,
      });
    }
  }, [focusedManeuver]);

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
      sendMessageToIframe({
        type: 'SET_USER_LOCATION',
        coords,
        panToUser: true,
      });
    } catch (err) {
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
        srcDoc={initialHtml}
        style={{
          width: '100%',
          height: '100%',
          border: isHighContrast
            ? (isDark ? '3px solid #FFFFFF' : '3px solid #000000')
            : (isDark ? '1px solid #334155' : '1px solid #CBD5E1'),
          borderRadius: 16,
          backgroundColor: isDark ? '#0F172A' : '#F1F5F9',
        }}
        title="OpenStreetMap Kraków"
      />

      {/* Floating "Lokalizuj mnie" button */}
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
            backgroundColor: isHighContrast
              ? (isDark ? '#000000' : '#FFFFFF')
              : (isDark ? '#1E293B' : '#FFFFFF'),
            borderColor: isHighContrast
              ? (isDark ? '#FFFFFF' : '#000000')
              : (isDark ? '#38BDF8' : BrandColors.accentTeal),
            borderWidth: isHighContrast ? 3 : 1.5,
            opacity: pressed || isLocating ? 0.8 : 1,
          },
        ]}>
        {isLocating ? (
          <ActivityIndicator
            size="small"
            color={isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#38BDF8' : BrandColors.accentTeal)}
          />
        ) : (
          <MaterialCommunityIcons
            name="crosshairs-gps"
            size={20}
            color={isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#38BDF8' : BrandColors.accentTeal)}
          />
        )}
        <Text
          style={[
            styles.locateText,
            {
              color: isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#38BDF8' : BrandColors.accentTeal),
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
