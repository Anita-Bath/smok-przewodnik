import React from 'react';
import { View, Text, StyleSheet, ScrollView, Pressable } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { AccessibilityToggle } from '@/components/AccessibilityToggle';
import { AccessibleButton } from '@/components/AccessibleButton';
import { BrandColors, Spacing, MaxContentWidth } from '@/constants/theme';
import { KRAKOW_PLACES } from '@/services/krakowData';
import { useAccessibility } from '@/context/AccessibilityContext';

export default function PlaceDetailScreen() {
  const router = useRouter();
  const { id } = useLocalSearchParams<{ id: string }>();
  const { isHighContrast, savedPlaceIds, toggleSavePlace, isGuest } = useAccessibility();

  const place = KRAKOW_PLACES.find((p) => p.id === id) || KRAKOW_PLACES[0];
  const isSaved = savedPlaceIds.includes(place.id);

  return (
    <SafeAreaView
      style={[
        styles.safeArea,
        { backgroundColor: isHighContrast ? '#FFFFFF' : '#F8FAFC' },
      ]}>
      {/* Header */}
      <View
        style={[
          styles.header,
          {
            borderBottomColor: isHighContrast ? '#000000' : '#E2E8F0',
            borderBottomWidth: isHighContrast ? 2.5 : 1,
            backgroundColor: '#FFFFFF',
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
            color={isHighContrast ? '#000000' : '#0F172A'}
          />
        </Pressable>

        <Text
          style={[
            styles.headerTitle,
            {
              color: isHighContrast ? '#000000' : '#0F172A',
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
            color={isHighContrast ? '#000000' : BrandColors.primary}
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
                backgroundColor: '#FFFFFF',
                borderColor: isHighContrast ? '#000000' : '#E2E8F0',
                borderWidth: isHighContrast ? 2.5 : 1,
              },
            ]}>
            <Text
              style={[
                styles.placeName,
                {
                  color: isHighContrast ? '#000000' : '#0F172A',
                  fontWeight: isHighContrast ? '900' : '800',
                },
              ]}>
              {place.name}
            </Text>
            <Text
              style={[
                styles.placeAddress,
                { color: isHighContrast ? '#1E293B' : '#64748B' },
              ]}>
              {place.distanceFromUserMeters} m stąd · {place.address}
            </Text>

            <View
              style={[
                styles.statusBanner,
                {
                  backgroundColor: isHighContrast ? '#E8F5E9' : '#E6F5F3',
                  borderColor: isHighContrast ? '#000000' : '#B2DFDB',
                  borderWidth: isHighContrast ? 2 : 1,
                },
              ]}>
              <MaterialCommunityIcons
                name="shield-check-outline"
                size={20}
                color={isHighContrast ? '#000000' : BrandColors.accentTeal}
              />
              <Text
                style={[
                  styles.statusText,
                  {
                    color: isHighContrast ? '#000000' : '#00796B',
                    fontWeight: isHighContrast ? '800' : '700',
                  },
                ]}>
                Status wiarygodności: {place.confidenceLabel}
              </Text>
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
                color: isHighContrast ? '#000000' : '#0F172A',
                fontWeight: isHighContrast ? '900' : '800',
              },
            ]}>
            Karta Dostępności Architektonicznej
          </Text>

          <View
            style={[
              styles.card,
              {
                backgroundColor: '#FFFFFF',
                borderColor: isHighContrast ? '#000000' : '#E2E8F0',
                borderWidth: isHighContrast ? 2.5 : 1,
              },
            ]}>
            {place.facts.map((fact, index) => (
              <View
                key={fact.id}
                style={[
                  styles.factItem,
                  index < place.facts.length - 1 && styles.borderBottom,
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
                        ? '#000000'
                        : fact.status === 'verified'
                        ? BrandColors.success
                        : BrandColors.accentTeal
                    }
                  />
                </View>
                <View style={styles.factTextWrap}>
                  <Text
                    style={[
                      styles.factTitle,
                      {
                        color: isHighContrast ? '#000000' : '#0F172A',
                        fontWeight: isHighContrast ? '800' : '700',
                      },
                    ]}>
                    {fact.label}
                  </Text>
                  {fact.description && (
                    <Text
                      style={[
                        styles.factDesc,
                        { color: isHighContrast ? '#1E293B' : '#64748B' },
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
                color: isHighContrast ? '#000000' : '#0F172A',
                fontWeight: isHighContrast ? '800' : '700',
              },
            ]}>
            Wskazówki dla odwiedzających
          </Text>

          <View
            style={[
              styles.card,
              {
                backgroundColor: '#FFFFFF',
                borderColor: isHighContrast ? '#000000' : '#E2E8F0',
                borderWidth: isHighContrast ? 2.5 : 1,
                marginBottom: 24,
              },
            ]}>
            <Text
              style={[
                styles.noteText,
                { color: isHighContrast ? '#000000' : '#334155' },
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
