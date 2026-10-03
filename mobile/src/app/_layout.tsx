import { DarkTheme, DefaultTheme, ThemeProvider, Stack } from 'expo-router';
import * as SplashScreen from 'expo-splash-screen';
import React, { useEffect } from 'react';
import { useColorScheme } from 'react-native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { AccessibilityProvider } from '@/context/AccessibilityContext';

SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  const colorScheme = useColorScheme();

  useEffect(() => {
    SplashScreen.hideAsync();
  }, []);

  return (
    <SafeAreaProvider>
      <AccessibilityProvider>
        <ThemeProvider value={colorScheme === 'dark' ? DarkTheme : DefaultTheme}>
        <Stack screenOptions={{ headerShown: false }}>
          <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
          <Stack.Screen name="auth/login" options={{ headerShown: false }} />
          <Stack.Screen name="auth/register" options={{ headerShown: false }} />
          <Stack.Screen
            name="route-planner"
            options={{ presentation: 'modal', headerShown: false }}
          />
          <Stack.Screen
            name="place/[id]"
            options={{ presentation: 'modal', headerShown: false }}
          />
          <Stack.Screen
            name="report/new"
            options={{ presentation: 'modal', headerShown: false }}
          />
        </Stack>
      </ThemeProvider>
    </AccessibilityProvider>
    </SafeAreaProvider>
  );
}
