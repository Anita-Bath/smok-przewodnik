import React from 'react';
import { View, StyleSheet, Text } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { BrandColors } from '@/constants/theme';
import { useAccessibility } from '@/context/AccessibilityContext';

interface DragonLogoProps {
  size?: number;
  showText?: boolean;
}

export function DragonLogo({ size = 44, showText = true }: DragonLogoProps) {
  const { isHighContrast } = useAccessibility();

  return (
    <View style={styles.container} accessible accessibilityRole="image" accessibilityLabel="Smok Przewodnik Logo">
      <View
        style={[
          styles.badge,
          {
            width: size,
            height: size,
            borderRadius: Math.round(size * 0.28),
            backgroundColor: isHighContrast ? '#000000' : BrandColors.primary,
            borderWidth: isHighContrast ? 2 : 0,
            borderColor: '#FFFFFF',
          },
        ]}>
        <MaterialCommunityIcons
          name="chess-knight"
          size={Math.round(size * 0.65)}
          color="#FFFFFF"
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
    gap: 12,
  },
  badge: {
    justifyContent: 'center',
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.15,
    shadowRadius: 4,
    elevation: 3,
  },
  textContainer: {
    justifyContent: 'center',
  },
  brandTitle: {
    fontSize: 22,
    letterSpacing: -0.3,
  },
});
