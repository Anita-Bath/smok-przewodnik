import React from 'react';
import { View, Text, StyleSheet, ScrollView, Pressable } from 'react-native';
import { useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { AccessibilityToggle } from '@/components/AccessibilityToggle';
import { AccessibleButton } from '@/components/AccessibleButton';
import { BrandColors, Spacing, MaxContentWidth } from '@/constants/theme';
import { KRAKOW_PLACES } from '@/services/krakowData';
import { useAccessibility } from '@/context/AccessibilityContext';
import { calculateDistanceMeters, formatDistance } from '@/services/routingService';

export default function SavedScreen() {
  const router = useRouter();
  const { isDark, isHighContrast, savedPlaceIds, toggleSavePlace, userLocation } = useAccessibility();

  const savedPlaces = KRAKOW_PLACES.filter((p) => savedPlaceIds.includes(p.id));

  return (
    <SafeAreaView
      edges={['top']}
      style={[
        styles.safeArea,
        {
          backgroundColor: isHighContrast
            ? (isDark ? '#000000' : '#FFFFFF')
            : (isDark ? '#0F172A' : '#F8FAFC'),
        },
      ]}>
      <ScrollView contentContainerStyle={styles.container}>
        <View style={styles.contentWrapper}>
          <Text
            style={[
              styles.headerTitle,
              {
                color: isHighContrast
                  ? (isDark ? '#FFFFFF' : '#000000')
                  : (isDark ? '#F8FAFC' : '#0F172A'),
                fontWeight: isHighContrast ? '900' : '800',
              },
            ]}>
            Zapisane
          </Text>

          <AccessibilityToggle />

          {/* Offline Area Pack Banner (Milestone 5 / Section 2.8) */}
          <View
            style={[
              styles.offlineCard,
              {
                backgroundColor: isHighContrast
                  ? (isDark ? '#082F49' : '#F0F9FF')
                  : (isDark ? '#1E293B' : '#E0F2FE'),
                borderColor: isHighContrast
                  ? (isDark ? '#38BDF8' : '#000000')
                  : (isDark ? '#334155' : '#BAE6FD'),
                borderWidth: isHighContrast ? 2.5 : 1,
              },
            ]}>
            <View style={styles.offlineHeader}>
              <MaterialCommunityIcons
                name="cloud-check-outline"
                size={24}
                color={isDark ? '#38BDF8' : BrandColors.primary}
              />
              <Text
                style={[
                  styles.offlineTitle,
                  {
                    color: isHighContrast
                      ? (isDark ? '#FFFFFF' : '#000000')
                      : (isDark ? '#38BDF8' : BrandColors.primaryDark),
                    fontWeight: isHighContrast ? '800' : '700',
                  },
                ]}>
                Paczka offline: Kraków Centrum
              </Text>
            </View>
            <Text
              style={[
                styles.offlineDesc,
                {
                  color: isHighContrast
                    ? (isDark ? '#E2E8F0' : '#1E293B')
                    : (isDark ? '#94A3B8' : '#334155'),
                },
              ]}>
              Pobrano dane dostępności, rozkłady MPK i geometrię 12 tras (24 MB).
              Nawigacja po zapisanych trasach działa bez połączenia z siecią.
            </Text>
            <View style={styles.offlineBadgeRow}>
              <Text
                style={[
                  styles.offlineStatus,
                  { color: isDark ? '#38BDF8' : '#0369A1' },
                ]}>
                ✓ Zaktualizowano dzisiaj
              </Text>
            </View>
          </View>

          {/* Section: Saved Places */}
          <Text
            style={[
              styles.sectionTitle,
              {
                color: isHighContrast
                  ? (isDark ? '#FFFFFF' : '#000000')
                  : (isDark ? '#F8FAFC' : '#0F172A'),
                fontWeight: isHighContrast ? '800' : '700',
              },
            ]}>
            Ulubione miejsca ({savedPlaces.length})
          </Text>

          {savedPlaces.length === 0 ? (
            <View style={styles.emptyState}>
              <Text
                style={[
                  styles.emptyText,
                  { color: isDark ? '#94A3B8' : '#64748B' },
                ]}>
                Nie masz jeszcze zapisanych miejsc.
              </Text>
            </View>
          ) : (
            savedPlaces.map((place) => (
              <View
                key={place.id}
                style={[
                  styles.placeCard,
                  {
                    backgroundColor: isHighContrast
                      ? (isDark ? '#000000' : '#FFFFFF')
                      : (isDark ? '#1E293B' : '#FFFFFF'),
                    borderColor: isHighContrast
                      ? (isDark ? '#FFFFFF' : '#000000')
                      : (isDark ? '#334155' : '#E2E8F0'),
                    borderWidth: isHighContrast ? 2.5 : 1,
                  },
                ]}>
                <View style={styles.placeCardHeader}>
                  <View style={styles.placeInfo}>
                    <Text
                      style={[
                        styles.placeName,
                        {
                          color: isHighContrast
                            ? (isDark ? '#FFFFFF' : '#000000')
                            : (isDark ? '#F8FAFC' : '#0F172A'),
                          fontWeight: isHighContrast ? '800' : '700',
                        },
                      ]}>
                      {place.name}
                    </Text>
                    <Text
                      style={[
                        styles.placeAddress,
                        {
                          color: isHighContrast
                            ? (isDark ? '#CBD5E1' : '#1E293B')
                            : (isDark ? '#94A3B8' : '#64748B'),
                        },
                      ]}>
                      {userLocation ? `${formatDistance(calculateDistanceMeters(userLocation, place.coordinates))} · ` : ''}
                      {place.address}
                    </Text>
                  </View>
                  <Pressable
                    onPress={() => toggleSavePlace(place.id)}
                    accessible
                    accessibilityRole="button"
                    accessibilityLabel={`Usuń ${place.name} z zapisanych`}>
                    <MaterialCommunityIcons
                      name="bookmark"
                      size={26}
                      color={
                        isHighContrast
                          ? (isDark ? '#FFFFFF' : '#000000')
                          : (isDark ? '#38BDF8' : BrandColors.primary)
                      }
                    />
                  </Pressable>
                </View>

                <View style={styles.actionsRow}>
                  <AccessibleButton
                    label="Wyznacz trasę"
                    icon="navigation-variant"
                    variant="primary"
                    onPress={() =>
                      router.push({
                        pathname: '/route-planner',
                        params: { placeId: place.id },
                      })
                    }
                    style={styles.flexBtn}
                  />
                  <AccessibleButton
                    label="Szczegóły"
                    variant="secondary"
                    onPress={() =>
                      router.push({
                        pathname: '/place/[id]',
                        params: { id: place.id },
                      })
                    }
                    style={styles.flexBtn}
                  />
                </View>
              </View>
            ))
          )}
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
  },
  container: {
    padding: Spacing.three,
    alignItems: 'center',
  },
  contentWrapper: {
    width: '100%',
    maxWidth: MaxContentWidth,
  },
  headerTitle: {
    fontSize: 28,
    marginBottom: 4,
  },
  offlineCard: {
    borderRadius: 18,
    padding: 16,
    marginVertical: 12,
  },
  offlineHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    marginBottom: 6,
  },
  offlineTitle: {
    fontSize: 16,
  },
  offlineDesc: {
    fontSize: 13,
    lineHeight: 18,
  },
  offlineBadgeRow: {
    marginTop: 10,
  },
  offlineStatus: {
    fontSize: 12,
    fontWeight: '700',
    color: '#0369A1',
  },
  sectionTitle: {
    fontSize: 18,
    marginTop: 14,
    marginBottom: 10,
  },
  emptyState: {
    paddingVertical: 32,
    alignItems: 'center',
  },
  emptyText: {
    fontSize: 14,
    color: '#64748B',
  },
  placeCard: {
    borderRadius: 18,
    padding: 16,
    marginBottom: 12,
  },
  placeCardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
  },
  placeInfo: {
    flex: 1,
    marginRight: 12,
  },
  placeName: {
    fontSize: 18,
  },
  placeAddress: {
    fontSize: 13,
    marginTop: 2,
  },
  actionsRow: {
    flexDirection: 'row',
    gap: 10,
    marginTop: 14,
  },
  flexBtn: {
    flex: 1,
  },
});
