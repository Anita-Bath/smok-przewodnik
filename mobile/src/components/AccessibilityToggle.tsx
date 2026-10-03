import React from 'react';
import { View, Text, StyleSheet, Switch, Pressable } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { BrandColors, Spacing } from '@/constants/theme';
import { useAccessibility } from '@/context/AccessibilityContext';

export function AccessibilityToggle() {
  const { isHighContrast, toggleHighContrast, isDark, toggleDarkTheme } = useAccessibility();

  if (isHighContrast) {
    return (
      <View
        style={[
          styles.highContrastContainer,
          {
            backgroundColor: isDark ? '#000000' : '#003366',
            borderColor: isDark ? '#FFFFFF' : '#001A33',
            borderWidth: 2.5,
          },
        ]}
        accessible
        accessibilityRole="summary"
        accessibilityLabel="Pasek ułatwień dostępu">
        <View style={styles.togglesRow}>
          {/* High Contrast Switch */}
          <View style={styles.toggleItem}>
            <View style={styles.toggleLabelRow}>
              <MaterialCommunityIcons name="eye-outline" size={22} color="#FFFFFF" />
              <Text style={styles.highContrastTitle}>Kontrast</Text>
            </View>
            <Switch
              value={isHighContrast}
              onValueChange={toggleHighContrast}
              trackColor={{ false: '#4B5563', true: '#2563EB' }}
              thumbColor="#FFFFFF"
              accessibilityLabel="Wysoki kontrast"
            />
          </View>

          <View style={[styles.separator, { backgroundColor: isDark ? '#333333' : '#1E40AF' }]} />

          {/* Dark Mode Switch */}
          <View style={styles.toggleItem}>
            <View style={styles.toggleLabelRow}>
              <MaterialCommunityIcons
                name={isDark ? 'weather-night' : 'white-balance-sunny'}
                size={22}
                color="#FFFFFF"
              />
              <Text style={styles.highContrastTitle}>Ciemny</Text>
            </View>
            <Switch
              value={isDark}
              onValueChange={toggleDarkTheme}
              trackColor={{ false: '#4B5563', true: '#60A5FA' }}
              thumbColor="#FFFFFF"
              accessibilityLabel="Ciemny motyw"
            />
          </View>
        </View>

        <Pressable
          onPress={toggleHighContrast}
          style={[styles.resetLink, { borderTopColor: isDark ? '#333333' : '#1E40AF' }]}
          accessible
          accessibilityRole="button"
          accessibilityLabel="Wróć do standardowego trybu">
          <Text style={styles.resetLinkText}>Wróć do standardowego kontrastu</Text>
        </Pressable>
      </View>
    );
  }

  return (
    <View
      style={[
        styles.normalContainer,
        {
          backgroundColor: isDark ? '#1E293B' : '#FFFFFF',
          borderColor: isDark ? '#334155' : '#008779',
        },
      ]}
      accessible
      accessibilityRole="none">
      <View style={styles.togglesRow}>
        {/* High Contrast Switch */}
        <View style={styles.toggleItem}>
          <View style={styles.toggleLabelRow}>
            <View
              style={[
                styles.iconCircle,
                { backgroundColor: isDark ? '#334155' : '#E6F5F3' },
              ]}>
              <MaterialCommunityIcons
                name="eye-outline"
                size={18}
                color={isDark ? '#2DD4BF' : '#008779'}
              />
            </View>
            <Text
              style={[
                styles.normalTitle,
                { color: isDark ? '#F8FAFC' : '#008779' },
              ]}>
              Kontrast
            </Text>
          </View>
          <Switch
            value={isHighContrast}
            onValueChange={toggleHighContrast}
            trackColor={{ false: isDark ? '#475569' : '#D1D5DB', true: '#008779' }}
            thumbColor="#FFFFFF"
            accessibilityLabel="Włącz wysoki kontrast i duży tekst"
          />
        </View>

        <View style={[styles.separator, { backgroundColor: isDark ? '#334155' : '#E2E8F0' }]} />

        {/* Dark Mode Switch */}
        <View style={styles.toggleItem}>
          <View style={styles.toggleLabelRow}>
            <View
              style={[
                styles.iconCircle,
                { backgroundColor: isDark ? '#334155' : '#FEF3C7' },
              ]}>
              <MaterialCommunityIcons
                name={isDark ? 'weather-night' : 'white-balance-sunny'}
                size={18}
                color={isDark ? '#38BDF8' : '#D97706'}
              />
            </View>
            <Text
              style={[
                styles.normalTitle,
                { color: isDark ? '#F8FAFC' : '#1E293B' },
              ]}>
              Ciemny
            </Text>
          </View>
          <Switch
            value={isDark}
            onValueChange={toggleDarkTheme}
            trackColor={{ false: isDark ? '#475569' : '#D1D5DB', true: '#38BDF8' }}
            thumbColor="#FFFFFF"
            accessibilityLabel="Włącz ciemny motyw"
          />
        </View>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  normalContainer: {
    borderWidth: 1.5,
    borderRadius: 24,
    paddingVertical: 8,
    paddingHorizontal: 14,
    marginVertical: Spacing.two,
  },
  highContrastContainer: {
    borderRadius: 20,
    paddingVertical: 12,
    paddingHorizontal: 16,
    marginVertical: Spacing.two,
  },
  togglesRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  toggleItem: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    flex: 1,
    paddingHorizontal: 4,
  },
  toggleLabelRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  separator: {
    width: 1,
    height: 28,
    marginHorizontal: 8,
  },
  iconCircle: {
    width: 32,
    height: 32,
    borderRadius: 16,
    justifyContent: 'center',
    alignItems: 'center',
  },
  normalTitle: {
    fontSize: 14,
    fontWeight: '700',
  },
  highContrastTitle: {
    fontSize: 15,
    fontWeight: '800',
    color: '#FFFFFF',
  },
  resetLink: {
    marginTop: 8,
    paddingTop: 8,
    borderTopWidth: 1,
  },
  resetLinkText: {
    color: '#93C5FD',
    fontSize: 13,
    fontWeight: '600',
    textDecorationLine: 'underline',
  },
});
