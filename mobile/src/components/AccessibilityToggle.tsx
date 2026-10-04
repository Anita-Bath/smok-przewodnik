import React from 'react';
import { View, Text, StyleSheet, Switch, Pressable } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { BrandColors, Spacing } from '@/constants/theme';
import { useAccessibility } from '@/context/AccessibilityContext';

/**
 * Single-row accessibility toggle: "Tryb dla niedowidzących"
 * Toggles high-contrast mode. No dark-mode control, no reset link.
 */
export function AccessibilityToggle() {
  const { isHighContrast, toggleHighContrast } = useAccessibility();

  return (
    <Pressable
      onPress={toggleHighContrast}
      accessible
      accessibilityRole="switch"
      accessibilityLabel="Tryb dla niedowidzących – wysoki kontrast"
      accessibilityState={{ checked: isHighContrast }}
      style={[
        styles.container,
        {
          backgroundColor: isHighContrast ? '#162C41' : '#FFFFFF',
          borderColor: isHighContrast ? BrandColors.primary : BrandColors.accentTeal,
          borderWidth: 1.5,
        },
      ]}>

      {/* Icon */}
      <View
        style={[
          styles.iconCircle,
          {
            backgroundColor: isHighContrast
              ? 'rgba(0,150,146,0.2)'
              : BrandColors.accentTealLight,
          },
        ]}>
        <MaterialCommunityIcons
          name="eye-outline"
          size={20}
          color={isHighContrast ? '#009692' : BrandColors.accentTeal}
        />
      </View>

      {/* Labels */}
      <View style={styles.labelGroup}>
        <Text
          style={[
            styles.label,
            { color: isHighContrast ? '#FFFFFF' : '#0F172A' },
          ]}>
          Tryb dla niedowidzących
        </Text>
        <Text
          style={[
            styles.sublabel,
            { color: isHighContrast ? '#A8C4E0' : '#64748B' },
          ]}>
          {isHighContrast ? 'Wysoki kontrast · Włączony' : 'Wysoki kontrast · Wyłączony'}
        </Text>
      </View>

      {/* Switch – pointerEvents none so the Pressable handles the tap */}
      <Switch
        value={isHighContrast}
        onValueChange={toggleHighContrast}
        trackColor={{ false: '#D1D5DB', true: BrandColors.accentTeal }}
        thumbColor="#FFFFFF"
        accessibilityLabel="Przełącznik trybu dla niedowidzących"
        pointerEvents="none"
      />
    </Pressable>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    borderRadius: 18,
    paddingVertical: 10,
    paddingHorizontal: 14,
    marginVertical: Spacing.two,
    gap: 10,
  },
  iconCircle: {
    width: 36,
    height: 36,
    borderRadius: 18,
    justifyContent: 'center',
    alignItems: 'center',
    flexShrink: 0,
  },
  labelGroup: {
    flex: 1,
  },
  label: {
    fontSize: 14,
    fontWeight: '700',
  },
  sublabel: {
    fontSize: 11,
    marginTop: 1,
  },
});
