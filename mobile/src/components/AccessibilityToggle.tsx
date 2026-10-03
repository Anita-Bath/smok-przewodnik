import React from 'react';
import { View, Text, StyleSheet, Switch, Pressable } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { BrandColors, Spacing } from '@/constants/theme';
import { useAccessibility } from '@/context/AccessibilityContext';

export function AccessibilityToggle() {
  const { isHighContrast, toggleHighContrast } = useAccessibility();

  if (isHighContrast) {
    return (
      <View
        style={styles.highContrastContainer}
        accessible
        accessibilityRole="summary"
        accessibilityLabel="Duży tekst i kontrast włączony">
        <View style={styles.row}>
          <View style={styles.leftGroup}>
            <View style={styles.highContrastIconCircle}>
              <MaterialCommunityIcons name="eye-outline" size={24} color="#FFFFFF" />
            </View>
            <View>
              <Text style={styles.highContrastTitle}>Duży tekst i kontrast</Text>
              <Text style={styles.highContrastStatus}>Włączony</Text>
            </View>
          </View>
          <Switch
            value={isHighContrast}
            onValueChange={toggleHighContrast}
            trackColor={{ false: '#4B5563', true: '#2563EB' }}
            thumbColor="#FFFFFF"
            accessibilityLabel="Przełącznik trybu wysokiego kontrastu"
          />
        </View>

        <Pressable
          onPress={toggleHighContrast}
          style={styles.resetLink}
          accessible
          accessibilityRole="button"
          accessibilityLabel="Wróć do standardowego trybu">
          <Text style={styles.resetLinkText}>Wróć do standardowego trybu</Text>
        </Pressable>
      </View>
    );
  }

  return (
    <View
      style={styles.normalContainer}
      accessible
      accessibilityRole="none">
      <View style={styles.row}>
        <View style={styles.leftGroup}>
          <View style={styles.normalIconCircle}>
            <MaterialCommunityIcons name="eye-outline" size={22} color="#008779" />
          </View>
          <Text style={styles.normalTitle}>Duży tekst i kontrast</Text>
        </View>
        <Switch
          value={isHighContrast}
          onValueChange={toggleHighContrast}
          trackColor={{ false: '#D1D5DB', true: '#008779' }}
          thumbColor="#FFFFFF"
          accessibilityLabel="Włącz wysoki kontrast i duży tekst"
        />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  normalContainer: {
    backgroundColor: '#FFFFFF',
    borderWidth: 1.5,
    borderColor: '#008779',
    borderRadius: 24,
    paddingVertical: 10,
    paddingHorizontal: 16,
    marginVertical: Spacing.two,
  },
  highContrastContainer: {
    backgroundColor: '#003366',
    borderWidth: 2,
    borderColor: '#001A33',
    borderRadius: 20,
    paddingVertical: 14,
    paddingHorizontal: 18,
    marginVertical: Spacing.two,
  },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  leftGroup: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    flex: 1,
  },
  normalIconCircle: {
    width: 38,
    height: 38,
    borderRadius: 19,
    backgroundColor: '#E6F5F3',
    justifyContent: 'center',
    alignItems: 'center',
  },
  highContrastIconCircle: {
    width: 40,
    height: 40,
    borderRadius: 20,
    borderWidth: 2,
    borderColor: '#FFFFFF',
    backgroundColor: 'transparent',
    justifyContent: 'center',
    alignItems: 'center',
  },
  normalTitle: {
    fontSize: 16,
    fontWeight: '700',
    color: '#008779',
  },
  highContrastTitle: {
    fontSize: 18,
    fontWeight: '800',
    color: '#FFFFFF',
  },
  highContrastStatus: {
    fontSize: 14,
    fontWeight: '600',
    color: '#E0E7FF',
    marginTop: 2,
  },
  resetLink: {
    marginTop: 8,
    paddingTop: 8,
    borderTopWidth: 1,
    borderTopColor: '#1E40AF',
  },
  resetLinkText: {
    color: '#93C5FD',
    fontSize: 14,
    fontWeight: '600',
    textDecorationLine: 'underline',
  },
});
