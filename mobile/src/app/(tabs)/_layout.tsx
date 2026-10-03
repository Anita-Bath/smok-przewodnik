import React from 'react';
import { Tabs } from 'expo-router';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { BrandColors } from '@/constants/theme';
import { useAccessibility } from '@/context/AccessibilityContext';

export default function TabsLayout() {
  const { isHighContrast } = useAccessibility();

  const activeColor = isHighContrast ? '#000000' : BrandColors.primary;
  const inactiveColor = isHighContrast ? '#555555' : '#64748B';

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: activeColor,
        tabBarInactiveTintColor: inactiveColor,
        tabBarStyle: {
          height: 64,
          paddingBottom: 10,
          paddingTop: 8,
          backgroundColor: '#FFFFFF',
          borderTopWidth: isHighContrast ? 3 : 1,
          borderTopColor: isHighContrast ? '#000000' : '#E2E8F0',
        },
        tabBarLabelStyle: {
          fontSize: isHighContrast ? 13 : 12,
          fontWeight: isHighContrast ? '800' : '600',
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
