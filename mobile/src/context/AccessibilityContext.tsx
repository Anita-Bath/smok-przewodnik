import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { Platform, useColorScheme } from 'react-native';
import * as Location from 'expo-location';
import { Colors, ThemeColors } from '@/constants/theme';

export type ConstraintLevel = 'allowed' | 'prefer_avoid' | 'must_avoid';

export type TravelMode =
  | 'walk'
  | 'mobility_aid'
  | 'bicycle'
  | 'micromobility'
  | 'public_transport'
  | 'car';

export type FeedbackChannel = 'visual' | 'audio' | 'haptic' | 'voice';

export type FontScale = 'default' | 'large' | 'extraLarge';

export interface UserProfile {
  id: string;
  email: string;
  name: string;
}

export interface AccessibilitySettings {
  preferredLocale: 'pl-PL' | 'en-US';
  constraints: Record<string, ConstraintLevel>;
  transportCapabilities: TravelMode[];
  feedbackChannels: FeedbackChannel[];
  highContrast: boolean;
  fontScale: FontScale;
}

export interface Preset {
  id: string;
  name: string;
  description: string;
  icon: string;
  constraints: Record<string, ConstraintLevel>;
  transportCapabilities: TravelMode[];
  feedbackChannels: FeedbackChannel[];
}

export const ACCESSIBILITY_PRESETS: Preset[] = [
  {
    id: 'wheelchair',
    name: 'Wózek / Ograniczona mobilność',
    description: 'Bezwzględne omijanie schodów, wysokich krawężników oraz preferowanie wind i podjazdów.',
    icon: 'wheelchair',
    constraints: {
      stairs: 'must_avoid',
      curb: 'must_avoid',
      rough_surface: 'prefer_avoid',
      steep_slope: 'must_avoid',
      elevator_unavailable: 'must_avoid',
      crowding: 'prefer_avoid',
    },
    transportCapabilities: ['mobility_aid', 'public_transport', 'walk'],
    feedbackChannels: ['visual', 'audio', 'haptic'],
  },
  {
    id: 'blind_low_vision',
    name: 'Niewidomy / Słabowidzący',
    description: 'Wsparcie komunikatów głosowych, wibracji, omijanie przeszkód bez sygnalizacji dźwiękowej.',
    icon: 'eye-off',
    constraints: {
      missing_audio_signal: 'must_avoid',
      stairs: 'allowed',
      rough_surface: 'prefer_avoid',
      crowding: 'prefer_avoid',
    },
    transportCapabilities: ['walk', 'public_transport'],
    feedbackChannels: ['audio', 'haptic', 'voice'],
  },
  {
    id: 'deaf_hard_of_hearing',
    name: 'Niesłyszący / Niedosłyszący',
    description: 'Wskazówki wizualne, pętle indukcyjne oraz czytelne oznaczenia optyczne.',
    icon: 'ear-hearing-off',
    constraints: {
      missing_visual_announcement: 'prefer_avoid',
      crowding: 'allowed',
    },
    transportCapabilities: ['walk', 'public_transport', 'bicycle', 'car'],
    feedbackChannels: ['visual', 'haptic'],
  },
  {
    id: 'stroller',
    name: 'Wózek dziecięcy / Opiekun',
    description: 'Unikanie schodów i ciasnych przejść, preferowanie szerokich chodników i wind.',
    icon: 'baby-carriage',
    constraints: {
      stairs: 'must_avoid',
      rough_surface: 'prefer_avoid',
      narrow_passage: 'must_avoid',
    },
    transportCapabilities: ['walk', 'public_transport'],
    feedbackChannels: ['visual', 'audio'],
  },
  {
    id: 'senior_calm',
    name: 'Senior / Spokojne tempo',
    description: 'Omijanie stromych podejść, tłumów i hałasu, miejsca odpoczynku po drodze.',
    icon: 'account-heart',
    constraints: {
      steep_slope: 'prefer_avoid',
      stairs: 'prefer_avoid',
      crowding: 'prefer_avoid',
      rough_surface: 'prefer_avoid',
    },
    transportCapabilities: ['walk', 'public_transport'],
    feedbackChannels: ['visual', 'audio'],
  },
];

interface AccessibilityContextType {
  isHighContrast: boolean;
  toggleHighContrast: () => void;
  isDark: boolean;
  toggleDarkTheme: () => void;
  setDarkTheme: (enabled: boolean) => void;
  theme: ThemeColors;
  fontScale: FontScale;
  setFontScale: (scale: FontScale) => void;
  isGuest: boolean;
  user: UserProfile | null;
  points: number;
  activePresetId: string | null;
  constraints: Record<string, ConstraintLevel>;
  transportCapabilities: TravelMode[];
  feedbackChannels: FeedbackChannel[];
  savedPlaceIds: string[];
  applyPreset: (presetId: string) => void;
  setConstraint: (code: string, level: ConstraintLevel) => void;
  toggleTravelMode: (mode: TravelMode) => void;
  userLocation: { latitude: number; longitude: number } | null;
  setUserLocation: (loc: { latitude: number; longitude: number } | null) => void;
  toggleFeedbackChannel: (channel: FeedbackChannel) => void;
  toggleSavePlace: (placeId: string) => void;
  loginAsGuest: () => void;
  login: (email: string, name?: string) => void;
  logout: () => void;
  addPoints: (amount: number) => void;
}

const defaultConstraints: Record<string, ConstraintLevel> = {
  stairs: 'must_avoid',
  curb: 'prefer_avoid',
  rough_surface: 'prefer_avoid',
  steep_slope: 'prefer_avoid',
  elevator_unavailable: 'must_avoid',
  crowding: 'allowed',
  missing_audio_signal: 'allowed',
};

const AccessibilityContext = createContext<AccessibilityContextType | undefined>(undefined);

export function AccessibilityProvider({ children }: { children: ReactNode }) {
  const systemColorScheme = useColorScheme();
  const [isHighContrast, setIsHighContrast] = useState(false);
  const [isDark, setIsDark] = useState<boolean>(() => systemColorScheme === 'dark');
  const [fontScale, setFontScale] = useState<FontScale>('default');

  const toggleHighContrast = () => setIsHighContrast((prev) => !prev);
  const toggleDarkTheme = () => setIsDark((prev) => !prev);
  const setDarkTheme = (enabled: boolean) => setIsDark(enabled);

  const theme = isHighContrast
    ? (isDark ? Colors.highContrastDark : Colors.highContrastLight)
    : (isDark ? Colors.dark : Colors.light);
  const [isGuest, setIsGuest] = useState(true);
  const [user, setUser] = useState<UserProfile | null>(null);
  const [points, setPoints] = useState(120); // initial demo points
  const [activePresetId, setActivePresetId] = useState<string | null>('wheelchair');
  const [constraints, setConstraints] = useState<Record<string, ConstraintLevel>>(defaultConstraints);
  const [transportCapabilities, setTransportCapabilities] = useState<TravelMode[]>([
    'mobility_aid',
    'public_transport',
    'walk',
  ]);
  const [feedbackChannels, setFeedbackChannels] = useState<FeedbackChannel[]>([
    'visual',
    'audio',
    'haptic',
  ]);
  const [savedPlaceIds, setSavedPlaceIds] = useState<string[]>(['wawel', 'rynek-glowny']);
  const [userLocation, setUserLocation] = useState<{ latitude: number; longitude: number } | null>({
    latitude: 50.0617,
    longitude: 19.9373,
  });

  // Automatically acquire real user GPS localization on app startup
  useEffect(() => {
    let isMounted = true;
    let locationSubscription: { remove: () => void } | null = null;

    async function autoAcquireLocation() {
      try {
        if (Platform.OS === 'web') {
          if (typeof navigator !== 'undefined' && navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
              (pos) => {
                if (isMounted) {
                  setUserLocation({
                    latitude: pos.coords.latitude,
                    longitude: pos.coords.longitude,
                  });
                }
              },
              () => {},
              { timeout: 8000, enableHighAccuracy: true }
            );
          }
        } else {
          const { status } = await Location.requestForegroundPermissionsAsync();
          if (status === 'granted') {
            const initialPos = await Location.getCurrentPositionAsync({
              accuracy: Location.Accuracy.Balanced,
            });
            if (isMounted) {
              setUserLocation({
                latitude: initialPos.coords.latitude,
                longitude: initialPos.coords.longitude,
              });
            }

            locationSubscription = await Location.watchPositionAsync(
              {
                accuracy: Location.Accuracy.Balanced,
                distanceInterval: 15,
                timeInterval: 8000,
              },
              (location) => {
                if (isMounted) {
                  setUserLocation({
                    latitude: location.coords.latitude,
                    longitude: location.coords.longitude,
                  });
                }
              }
            );
          }
        }
      } catch (err) {
        // Fallback silently kept at Kraków center
      }
    }

    autoAcquireLocation();

    return () => {
      isMounted = false;
      if (locationSubscription) {
        locationSubscription.remove();
      }
    };
  }, []);

  const applyPreset = (presetId: string) => {
    const found = ACCESSIBILITY_PRESETS.find((p) => p.id === presetId);
    if (found) {
      setActivePresetId(presetId);
      setConstraints({ ...defaultConstraints, ...found.constraints });
      setTransportCapabilities([...found.transportCapabilities]);
      setFeedbackChannels([...found.feedbackChannels]);
    }
  };

  const setConstraint = (code: string, level: ConstraintLevel) => {
    setConstraints((prev) => ({
      ...prev,
      [code]: level,
    }));
  };

  const toggleTravelMode = (mode: TravelMode) => {
    setTransportCapabilities((prev) =>
      prev.includes(mode) ? prev.filter((m) => m !== mode) : [...prev, mode]
    );
  };

  const toggleFeedbackChannel = (channel: FeedbackChannel) => {
    setFeedbackChannels((prev) =>
      prev.includes(channel) ? prev.filter((c) => c !== channel) : [...prev, channel]
    );
  };

  const toggleSavePlace = (placeId: string) => {
    setSavedPlaceIds((prev) =>
      prev.includes(placeId) ? prev.filter((id) => id !== placeId) : [...prev, placeId]
    );
  };

  const loginAsGuest = () => {
    setIsGuest(true);
    setUser(null);
  };

  const login = (email: string, name = 'Użytkownik') => {
    setIsGuest(false);
    setUser({
      id: 'usr-krakow-01',
      email,
      name,
    });
  };

  const logout = () => {
    setIsGuest(true);
    setUser(null);
  };

  const addPoints = (amount: number) => {
    setPoints((prev) => prev + amount);
  };

  return (
    <AccessibilityContext.Provider
      value={{
        isHighContrast,
        toggleHighContrast,
        isDark,
        toggleDarkTheme,
        setDarkTheme,
        theme,
        fontScale,
        setFontScale,
        isGuest,
        user,
        points,
        activePresetId,
        constraints,
        transportCapabilities,
        feedbackChannels,
        savedPlaceIds,
        applyPreset,
        setConstraint,
        toggleTravelMode,
        userLocation,
        setUserLocation,
        toggleFeedbackChannel,
        toggleSavePlace,
        loginAsGuest,
        login,
        logout,
        addPoints,
      }}>
      {children}
    </AccessibilityContext.Provider>
  );
}

export function useAccessibility() {
  const context = useContext(AccessibilityContext);
  if (!context) {
    throw new Error('useAccessibility must be used within an AccessibilityProvider');
  }
  return context;
}
