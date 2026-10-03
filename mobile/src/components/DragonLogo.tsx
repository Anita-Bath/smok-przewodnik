import React from 'react';
import { View, StyleSheet, Text } from 'react-native';
import { Image } from 'expo-image';
import { BrandColors } from '@/constants/theme';
import { useAccessibility } from '@/context/AccessibilityContext';

const DRAGON_SVG_URI = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzYiIGhlaWdodD0iMzYiIHZpZXdCb3g9IjAgMCAzNiAzNiIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8cmVjdCB3aWR0aD0iMzYiIGhlaWdodD0iMzYiIHJ4PSIxMiIgZmlsbD0iIzAwNEM5NyIvPgogIDxwYXRoIGQ9Ik05LjI1IDI5LjM3NUM4LjM3NSAyNC4xMjUgOS4yNSAyMC42MjUgMTIuNzUgMThMMTAuMTI1IDE0LjVMMTQuNSAxNS4zNzVMMTIuNzUgOS4yNUwxOCAxMi43NUwyMC42MjUgNi42MjVMMjMuMjUgMTIuNzVMMjcuNjI1IDE0LjVDMjkuMzc1IDE1LjA4MzMgMjkuOTU4MyAxNi41NDE3IDI5LjM3NSAxOC44NzVMMjUuODc1IDIxLjVMMjIuMzc1IDE5Ljc1TDI1IDE4TDIxLjUgMTcuMTI1QzE5LjE2NjYgMTcuNzA4MyAxOC4yOTE2IDE5LjQ1ODMgMTguODc1IDIyLjM3NUMxOS43NSAyNSAyMy4yNSAyNSAyNSAyOS4zNzVIOS4yNVoiIGZpbGw9IndoaXRlIi8+CiAgPHBhdGggZD0iTTIyLjM3NSAxNC41QzIyLjM3NSAxNC44MDE3IDIyLjQ5NDggMTUuMDkxIDIyLjcwODIgMTUuMzA0M0MyMi45MjE1IDE1LjUxNzcgMjMuMjEwOCAxNS42Mzc1IDIzLjUxMjUgMTUuNjM3NUMyMy44MTQyIDE1LjYzNzUgMjQuMTAzNSAxNS41MTc3IDI0LjMxNjggMTUuMzA0M0MyNC41MzAyIDE1LjA5MSAyNC42NSAxNC44MDE3IDI0LjY1IDE0LjVDMjQuNjUgMTQuMTk4MyAyNC41MzAyIDEzLjkwOSAyNC4zMTY4IDEzLjY5NTdDMjQuMTAzNSAxMy40ODIzIDIzLjgxNDIgMTMuMzYyNSAyMy41MTI1IDEzLjM2MjVDMjMuMjEwOCAxMy4zNjI1IDIyLjkyMTUgMTMuNDgyMyAyMi43MDgyIDEzLjY5NTdDMjIuNDk0OCAxMy45MDkgMjIuMzc1IDE0LjE5ODMgMjIuMzc1IDE0LjVaIiBmaWxsPSIjMTYyQzQxIi8+CiAgPHBhdGggZD0iTTEzLjYyNSAyMy4yNUwxNi4yNSAyNU0xMi43NSAyNi43NUwxNi4yNSAyOC41IiBzdHJva2U9IiMxNjJDNDEiIHN0cm9rZS13aWR0aD0iMS4zMTI1IiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPC9zdmc+';

interface DragonLogoProps {
  size?: number;
  showText?: boolean;
}

export function DragonLogo({ size = 44, showText = true }: DragonLogoProps) {
  const { isHighContrast } = useAccessibility();

  const radius = Math.round(size * 0.28);

  return (
    <View
      style={styles.container}
      accessible
      accessibilityRole="image"
      accessibilityLabel="Smok Przewodnik Logo">
      <View
        style={[
          styles.badge,
          {
            width: size,
            height: size,
            borderRadius: radius,
            backgroundColor: isHighContrast ? '#162C41' : 'transparent',
            borderWidth: isHighContrast ? 2 : 0,
            borderColor: '#FFFFFF',
            overflow: 'hidden',
          },
        ]}>
        <Image
          source={DRAGON_SVG_URI}
          style={{ width: size, height: size }}
          contentFit="contain"
        />
      </View>
      {showText && (
        <View style={styles.textContainer}>
          <Text
            style={[
              styles.brandTitle,
              {
                color: isHighContrast ? '#000000' : '#0F172A',
                fontWeight: isHighContrast ? '900' : '700',
              },
            ]}>
            Smok Przewodnik
          </Text>
        </View>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
  },
  badge: {
    justifyContent: 'center',
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.18,
    shadowRadius: 4,
    elevation: 3,
  },
  textContainer: {
    justifyContent: 'center',
  },
  brandTitle: {
    fontSize: 20,
    letterSpacing: -0.3,
  },
});

