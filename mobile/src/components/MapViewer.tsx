import React, { useEffect, useRef, useState } from 'react';
import { View, Text, StyleSheet, Pressable, ActivityIndicator } from 'react-native';
import { WebView, WebViewMessageEvent } from 'react-native-webview';
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
  const webViewRef = useRef<WebView>(null);
  const [userLocation, setUserLocation] = useState<{ latitude: number; longitude: number } | null>(null);
  const [isLocating, setIsLocating] = useState(false);

  // Generate initial Leaflet HTML
  const htmlContent = generateLeafletHtml(
    places,
    selectedPlace?.id,
    userLocation,
    isHighContrast
  );

  const handleMessage = (event: WebViewMessageEvent) => {
    try {
      const data = JSON.parse(event.nativeEvent.data);
      if (data && data.type === 'SELECT_PLACE') {
        const found = places.find((p) => p.id === data.placeId);
        if (found) {
          onSelectPlace(found);
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
      // Kraków Old Town fallback
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
        source={{ html: htmlContent }}
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
