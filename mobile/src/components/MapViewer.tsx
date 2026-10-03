import React, { useEffect, useRef, useState, useMemo } from 'react';
import { View, Text, StyleSheet, Pressable, ActivityIndicator } from 'react-native';
import { WebView, WebViewMessageEvent } from 'react-native-webview';
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
  const { isHighContrast, userLocation, setUserLocation } = useAccessibility();
  const webViewRef = useRef<WebView>(null);
  const [isLocating, setIsLocating] = useState(false);

  // Generate initial HTML ONCE with useMemo
  const initialHtml = useMemo(
    () => generateLeafletHtml(places, selectedPlace?.id, userLocation, isHighContrast),
    []
  );

  const handleMessage = (event: WebViewMessageEvent) => {
    try {
      const data = JSON.parse(event.nativeEvent.data);
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
      }
    } catch (err) {}
  };

  // Sync selectedPlace changes to WebView
  useEffect(() => {
    if (webViewRef.current && selectedPlace) {
      const js = `window.postMessage(JSON.stringify({ type: 'SET_SELECTED_PLACE', placeId: '${selectedPlace.id}' }), '*'); true;`;
      webViewRef.current.injectJavaScript(js);
    }
  }, [selectedPlace?.id]);

  // Sync searchPin changes to WebView
  useEffect(() => {
    if (webViewRef.current && searchPin) {
      const js = `window.postMessage(JSON.stringify({ type: 'SET_SEARCH_PIN', coords: ${JSON.stringify(searchPin.coords)}, label: '${searchPin.label}' }), '*'); true;`;
      webViewRef.current.injectJavaScript(js);
    }
  }, [searchPin]);

  // Sync highContrast changes to WebView
  useEffect(() => {
    if (webViewRef.current) {
      const js = `window.postMessage(JSON.stringify({ type: 'SET_HIGH_CONTRAST', enabled: ${isHighContrast} }), '*'); true;`;
      webViewRef.current.injectJavaScript(js);
    }
  }, [isHighContrast]);

  // Sync places updates
  useEffect(() => {
    if (webViewRef.current) {
      const js = `window.postMessage(JSON.stringify({ type: 'UPDATE_PLACES', places: ${JSON.stringify(places)} }), '*'); true;`;
      webViewRef.current.injectJavaScript(js);
    }
  }, [places]);

  // Sync activeRoute updates
  useEffect(() => {
    if (webViewRef.current) {
      if (activeRoute && activeRoute.coordinates && activeRoute.coordinates.length > 0) {
        const js = `window.postMessage(JSON.stringify({ type: 'DRAW_ROUTE', coordinates: ${JSON.stringify(activeRoute.coordinates)}, profileType: '${activeRoute.profileType || 'easiest'}' }), '*'); true;`;
        webViewRef.current.injectJavaScript(js);
      } else {
        const js = `window.postMessage(JSON.stringify({ type: 'CLEAR_ROUTE' }), '*'); true;`;
        webViewRef.current.injectJavaScript(js);
      }
    }
  }, [activeRoute]);

  // Sync focusedManeuver updates
  useEffect(() => {
    if (webViewRef.current && focusedManeuver) {
      const js = `window.postMessage(JSON.stringify({ type: 'FOCUS_MANEUVER', location: ${JSON.stringify(focusedManeuver)} }), '*'); true;`;
      webViewRef.current.injectJavaScript(js);
    }
  }, [focusedManeuver]);

  // Handle "Lokalizuj mnie" with actual GPS geolocation
  const handleLocateMe = async () => {
    setIsLocating(true);
    try {
      const { status } = await Location.requestForegroundPermissionsAsync();
      let coords = { latitude: 50.0617, longitude: 19.9373 };

      if (status === 'granted') {
        const pos = await Location.getCurrentPositionAsync({
          accuracy: Location.Accuracy.Balanced,
        });
        coords = {
          latitude: pos.coords.latitude,
          longitude: pos.coords.longitude,
        };
      }

      setUserLocation(coords);
      if (onLocateMe) {
        onLocateMe(coords);
      }

      if (webViewRef.current) {
        const js = `window.postMessage(JSON.stringify({ type: 'SET_USER_LOCATION', coords: ${JSON.stringify(coords)}, panToUser: true }), '*'); true;`;
        webViewRef.current.injectJavaScript(js);
      }
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
      <WebView
        ref={webViewRef}
        originWhitelist={['*']}
        source={{ html: initialHtml }}
        onMessage={handleMessage}
        javaScriptEnabled={true}
        domStorageEnabled={true}
        style={[
          styles.webview,
          {
            borderColor: isHighContrast ? '#000000' : '#CBD5E1',
            borderWidth: isHighContrast ? 3 : 1,
          },
        ]}
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
  webview: {
    flex: 1,
    borderRadius: 16,
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
