import React from 'react';
import { Platform } from 'react-native';
import { Tabs } from 'expo-router';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { BrandColors } from '@/constants/theme';
import { useAccessibility } from '@/context/AccessibilityContext';

export default function TabsLayout() {
  const { isHighContrast, isDark } = useAccessibility();
  const insets = useSafeAreaInsets();

  const activeColor = isHighContrast
    ? (isDark ? '#FFFFFF' : '#000000')
    : (isDark ? '#38BDF8' : BrandColors.primary);
  const inactiveColor = isHighContrast
    ? (isDark ? '#A1A1AA' : '#555555')
    : (isDark ? '#64748B' : '#94A3B8');

  // Safe bottom padding:
  // On Android with gesture navigation (or edge-to-edge), insets.bottom is typically 24-48dp.
  // We add insets.bottom so the labels sit safely above the Android horizontal gesture line.
  // Even if insets.bottom is 0, provide at least 20dp on Android and 10dp on web.
  const bottomPadding = Platform.select({
    android: Math.max(insets.bottom + 6, 20),
    ios: Math.max(insets.bottom, 12),
    default: 10,
  });
  const tabHeight = 58 + bottomPadding;

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: activeColor,
        tabBarInactiveTintColor: inactiveColor,
        tabBarStyle: {
          height: tabHeight,
          paddingBottom: bottomPadding,
          paddingTop: 8,
          backgroundColor: isHighContrast
            ? (isDark ? '#000000' : '#FFFFFF')
            : (isDark ? '#0F172A' : '#FFFFFF'),
          borderTopWidth: isHighContrast ? 3 : 1,
          borderTopColor: isHighContrast
            ? (isDark ? '#FFFFFF' : '#000000')
            : (isDark ? '#1E293B' : '#E2E8F0'),
          elevation: 8,
          shadowColor: '#000',
          shadowOffset: { width: 0, height: -2 },
          shadowOpacity: isDark ? 0.3 : 0.06,
          shadowRadius: 4,
        },
        tabBarItemStyle: {
          paddingTop: 2,
        },
        tabBarLabelStyle: {
          fontSize: isHighContrast ? 13 : 12,
          fontWeight: isHighContrast ? '800' : '600',
          marginBottom: 2,
        },
      }}>
      <Tabs.Screen
        name="index"
        options={{
          title: 'Eksploruj',
          tabBarAccessibilityLabel: 'Zakładka: Eksploruj mapę',
          tabBarIcon: ({ color, size, focused }) => (
            <MaterialCommunityIcons
              name={focused ? 'map' : 'map-outline'}
              size={size + 2}
              color={color}
            />
          ),
        }}
      />

      <Tabs.Screen
        name="saved"
        options={{
          title: 'Zapisane',
          tabBarAccessibilityLabel: 'Zakładka: Zapisane miejsca i trasy',
          tabBarIcon: ({ color, size, focused }) => (
            <MaterialCommunityIcons
              name={focused ? 'bookmark' : 'bookmark-outline'}
              size={size + 2}
              color={color}
            />
          ),
        }}
      />

      <Tabs.Screen
        name="profile"
        options={{
          title: 'Ty',
          tabBarAccessibilityLabel: 'Zakładka: Profil i ustawienia dostępności',
          tabBarIcon: ({ color, size, focused }) => (
            <MaterialCommunityIcons
              name={focused ? 'account' : 'account-outline'}
              size={size + 2}
              color={color}
            />
          ),
        }}
      />
    </Tabs>
  );
}
