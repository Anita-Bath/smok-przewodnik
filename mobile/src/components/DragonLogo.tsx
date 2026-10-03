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

  const badgeSize = size;
  const iconSize = Math.round(size * 0.6);
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
            width: badgeSize,
            height: badgeSize,
            borderRadius: radius,
            backgroundColor: isHighContrast ? '#162C41' : BrandColors.primary,
            borderWidth: isHighContrast ? 2 : 0,
            borderColor: '#FFFFFF',
          },
        ]}>
        <MaterialCommunityIcons
          name="dragon"
          size={iconSize}
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

