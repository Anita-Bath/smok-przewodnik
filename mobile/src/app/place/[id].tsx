import React, { useCallback, useState } from 'react';
import { View, Text, StyleSheet, ScrollView, Pressable } from 'react-native';
import { useFocusEffect, useLocalSearchParams, useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { AccessibilityToggle } from '@/components/AccessibilityToggle';
import { AccessibleButton } from '@/components/AccessibleButton';
import { BrandColors, Spacing, MaxContentWidth } from '@/constants/theme';
import { KRAKOW_PLACES, KrakowPlace, fetchPlaceById } from '@/services/krakowData';
import { useAccessibility } from '@/context/AccessibilityContext';
import { calculateDistanceMeters, formatDistance } from '@/services/routingService';

export default function PlaceDetailScreen() {
  const router = useRouter();
  const { id } = useLocalSearchParams<{ id: string }>();
  const { isDark, isHighContrast, savedPlaceIds, toggleSavePlace, isGuest, userLocation } = useAccessibility();

  const [place, setPlace] = useState<KrakowPlace | null>(
    KRAKOW_PLACES.find((p) => p.id === id) || null
  );

  useFocusEffect(
    useCallback(() => {
      let active = true;

      if (id) {
        fetchPlaceById(id).then((data) => {
          if (!active) return;
          setPlace((current) => data ?? current ?? KRAKOW_PLACES[0]);
        });
      }

      return () => {
        active = false;
      };
    }, [id]),
  );

  if (!place) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <Text>Ładowanie...</Text>
      </View>
    );
  }

  const isSaved = savedPlaceIds.includes(place.id);
  const distanceMeters = userLocation
    ? calculateDistanceMeters(userLocation, place.coordinates)
    : place.distanceFromUserMeters;

  const cardBg = isHighContrast
    ? (isDark ? '#000000' : '#FFFFFF')
    : (isDark ? '#1E293B' : '#FFFFFF');
  const cardBorder = isHighContrast
    ? (isDark ? '#FFFFFF' : '#000000')
    : (isDark ? '#334155' : '#E2E8F0');
  const dividerColor = isDark ? '#334155' : '#F1F5F9';

  return (
    <SafeAreaView
      style={[
        styles.safeArea,
        {
          backgroundColor: isHighContrast
            ? (isDark ? '#000000' : '#FFFFFF')
            : (isDark ? '#0F172A' : '#F8FAFC'),
        },
      ]}>
      {/* Header */}
      <View
        style={[
          styles.header,
          {
            borderBottomColor: cardBorder,
            borderBottomWidth: isHighContrast ? 2.5 : 1,
            backgroundColor: isHighContrast
              ? (isDark ? '#000000' : '#FFFFFF')
              : (isDark ? '#1E293B' : '#FFFFFF'),
          },
        ]}>
        <Pressable
          onPress={() => router.back()}
          accessible
          accessibilityRole="button"
          accessibilityLabel="Wróć"
          style={styles.backBtn}>
          <MaterialCommunityIcons
            name="arrow-left"
            size={24}
            color={
              isHighContrast
                ? (isDark ? '#FFFFFF' : '#000000')
                : (isDark ? '#F8FAFC' : '#0F172A')
            }
          />
        </Pressable>

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
          Szczegóły obiektu
        </Text>

        <Pressable
          onPress={() => toggleSavePlace(place.id)}
          accessible
          accessibilityRole="button"
          accessibilityLabel={isSaved ? 'Usuń z ulubionych' : 'Zapisz'}>
          <MaterialCommunityIcons
            name={isSaved ? 'bookmark' : 'bookmark-outline'}
            size={26}
            color={
              isHighContrast
                ? (isDark ? '#FFFFFF' : '#000000')
                : (isDark ? '#38BDF8' : BrandColors.primary)
            }
          />
        </Pressable>
      </View>

      <ScrollView contentContainerStyle={styles.scrollContainer}>
        <View style={styles.content}>
          <AccessibilityToggle />

          {/* Place Title Card */}
          <View
            style={[
              styles.card,
              {
                backgroundColor: cardBg,
                borderColor: cardBorder,
                borderWidth: isHighContrast ? 2.5 : 1,
              },
            ]}>
            <Text
              style={[
                styles.placeName,
                {
                  color: isHighContrast
                    ? (isDark ? '#FFFFFF' : '#000000')
                    : (isDark ? '#F8FAFC' : '#0F172A'),
                  fontWeight: isHighContrast ? '900' : '800',
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
              {formatDistance(distanceMeters)} stąd · {place.address}
            </Text>

            <View
              style={[
                styles.statusBanner,
                {
                  backgroundColor: isHighContrast
                    ? (isDark ? '#064E3B' : '#E8F5E9')
                    : (isDark ? '#0F2E2B' : '#E6F5F3'),
                  borderColor: isHighContrast
                    ? (isDark ? '#34D399' : '#000000')
                    : (isDark ? '#115E59' : '#B2DFDB'),
                  borderWidth: isHighContrast ? 2 : 1,
                },
              ]}>
              <MaterialCommunityIcons
                name="shield-check-outline"
                size={20}
                color={
                  isHighContrast
                    ? (isDark ? '#34D399' : '#000000')
                    : (isDark ? '#2DD4BF' : BrandColors.accentTeal)
                }
              />
              <Text
                style={[
                  styles.statusText,
                  {
                    color: isHighContrast
                      ? (isDark ? '#FFFFFF' : '#000000')
                      : (isDark ? '#2DD4BF' : '#00796B'),
                    fontWeight: isHighContrast ? '800' : '700',
                  },
                ]}>
                Status wiarygodności: {place.confidenceLabel}
              </Text>
            </View>

            {/* Additional Contact / Info */}
            <View style={{ marginVertical: 12, gap: 8 }}>
              {place.openingHours && (
                <View style={{ flexDirection: 'row', alignItems: 'center', gap: 8 }}>
                  <MaterialCommunityIcons name="clock-outline" size={18} color={isDark ? '#94A3B8' : '#64748B'} />
                  <Text style={{ color: isDark ? '#CBD5E1' : '#334155', fontSize: 14 }}>{place.openingHours}</Text>
                </View>
              )}
              {place.phone && (
                <View style={{ flexDirection: 'row', alignItems: 'center', gap: 8 }}>
                  <MaterialCommunityIcons name="phone-outline" size={18} color={isDark ? '#94A3B8' : '#64748B'} />
                  <Text style={{ color: isDark ? '#CBD5E1' : '#334155', fontSize: 14 }}>{place.phone}</Text>
                </View>
              )}
              {place.website && (
                <View style={{ flexDirection: 'row', alignItems: 'center', gap: 8 }}>
                  <MaterialCommunityIcons name="web" size={18} color={isDark ? '#94A3B8' : '#64748B'} />
                  <Text style={{ color: isDark ? '#38BDF8' : BrandColors.primary, fontSize: 14 }}>{place.website}</Text>
                </View>
              )}
            </View>

            {/* Quick action buttons */}
            <View style={styles.actionButtonsRow}>
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
                label="Zgłoś uwagę"
                icon="pencil-outline"
                variant="guest"
                onPress={() => {
                  if (isGuest) router.push('/auth/login');
                  else router.push({ pathname: '/report/new', params: { placeId: place.id } });
                }}
                style={styles.flexBtn}
              />
            </View>
          </View>

          {/* Accessibility Audit Section */}
          <Text
            style={[
              styles.sectionTitle,
              {
                color: isHighContrast
                  ? (isDark ? '#FFFFFF' : '#000000')
                  : (isDark ? '#F8FAFC' : '#0F172A'),
                fontWeight: isHighContrast ? '900' : '800',
              },
            ]}>
            Karta Dostępności Architektonicznej
          </Text>

          <View
            style={[
              styles.card,
              {
                backgroundColor: cardBg,
                borderColor: cardBorder,
                borderWidth: isHighContrast ? 2.5 : 1,
              },
            ]}>
            {place.facts.map((fact, index) => (
              <View
                key={fact.id}
                style={[
                  styles.factItem,
                  index < place.facts.length - 1 && {
                    borderBottomWidth: 1,
                    borderBottomColor: dividerColor,
                  },
                ]}>
                <View style={styles.factIconWrap}>
                  <MaterialCommunityIcons
                    name={
                      fact.status === 'verified'
                        ? 'check-circle'
                        : 'information-outline'
                    }
                    size={22}
                    color={
                      isHighContrast
                        ? (isDark ? '#FFFFFF' : '#000000')
                        : fact.status === 'verified'
                        ? (isDark ? '#4ADE80' : BrandColors.success)
                        : (isDark ? '#2DD4BF' : BrandColors.accentTeal)
                    }
                  />
                </View>
                <View style={styles.factTextWrap}>
                  <Text
                    style={[
                      styles.factTitle,
                      {
                        color: isHighContrast
                          ? (isDark ? '#FFFFFF' : '#000000')
                          : (isDark ? '#F8FAFC' : '#0F172A'),
                        fontWeight: isHighContrast ? '800' : '700',
                      },
                    ]}>
                    {fact.label}
                  </Text>
                  {fact.description && (
                    <Text
                      style={[
                        styles.factDesc,
                        {
                          color: isHighContrast
                            ? (isDark ? '#CBD5E1' : '#1E293B')
                            : (isDark ? '#94A3B8' : '#64748B'),
                        },
                      ]}>
                      {fact.description}
                    </Text>
                  )}
                </View>
              </View>
            ))}
          </View>

          {/* Practical Notes */}
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
            Wskazówki dla odwiedzających
          </Text>

          <View
            style={[
              styles.card,
              {
                backgroundColor: cardBg,
                borderColor: cardBorder,
                borderWidth: isHighContrast ? 2.5 : 1,
                marginBottom: 24,
              },
            ]}>
            <Text
              style={[
                styles.noteText,
                {
                  color: isHighContrast
                    ? (isDark ? '#FFFFFF' : '#000000')
                    : (isDark ? '#CBD5E1' : '#334155'),
                },
              ]}>
              {place.generalNote ||
                'Wszystkie dane pochodzą z miejskich rejestrów dostępności oraz weryfikacji społeczności Smok Przewodnik Kraków.'}
            </Text>
          </View>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingVertical: 14,
  },
  backBtn: {
    padding: 6,
  },
  headerTitle: {
    fontSize: 18,
  },
  scrollContainer: {
    padding: Spacing.three,
    alignItems: 'center',
  },
  content: {
    width: '100%',
    maxWidth: MaxContentWidth,
  },
  card: {
    borderRadius: 20,
    padding: 18,
    marginVertical: 8,
  },
  placeName: {
    fontSize: 26,
    letterSpacing: -0.3,
  },
  placeAddress: {
    fontSize: 14,
    marginTop: 4,
  },
  statusBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 12,
    borderRadius: 14,
    gap: 8,
    marginVertical: 12,
  },
  statusText: {
    fontSize: 13,
  },
  actionButtonsRow: {
    flexDirection: 'row',
    gap: 12,
    marginTop: 6,
  },
  flexBtn: {
    flex: 1,
  },
  sectionTitle: {
    fontSize: 18,
    marginTop: 14,
    marginBottom: 6,
  },
  factItem: {
    flexDirection: 'row',
    paddingVertical: 12,
    gap: 12,
  },
  factIconWrap: {
    marginTop: 2,
  },
  factTextWrap: {
    flex: 1,
  },
  factTitle: {
    fontSize: 15,
  },
  factDesc: {
    fontSize: 13,
    marginTop: 2,
    lineHeight: 18,
  },
  borderBottom: {
    borderBottomWidth: 1,
    borderBottomColor: '#F1F5F9',
  },
  noteText: {
    fontSize: 14,
    lineHeight: 20,
  },
});
