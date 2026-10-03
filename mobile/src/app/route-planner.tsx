import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, Pressable } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { AccessibilityToggle } from '@/components/AccessibilityToggle';
import { AccessibleButton } from '@/components/AccessibleButton';
import { BrandColors, Spacing, MaxContentWidth } from '@/constants/theme';
import { KRAKOW_PLACES } from '@/services/krakowData';
import { calculateKrakowRoutes, RouteAlternative } from '@/services/routingService';
import { useAccessibility } from '@/context/AccessibilityContext';

export default function RoutePlannerScreen() {
  const router = useRouter();
  const { placeId } = useLocalSearchParams<{ placeId?: string }>();
  const { isHighContrast, constraints, transportCapabilities, feedbackChannels } =
    useAccessibility();

  const destination =
    KRAKOW_PLACES.find((p) => p.id === placeId) || KRAKOW_PLACES[0];

  const planResult = calculateKrakowRoutes(destination, constraints, transportCapabilities);
  const [selectedRouteId, setSelectedRouteId] = useState<string>(
    planResult.alternatives[0].id
  );
  const [isNavigating, setIsNavigating] = useState(false);
  const [activeManeuverIndex, setActiveManeuverIndex] = useState(0);

  const selectedRoute =
    planResult.alternatives.find((r) => r.id === selectedRouteId) ||
    planResult.alternatives[0];

  return (
    <SafeAreaView
      style={[
        styles.safeArea,
        { backgroundColor: isHighContrast ? '#FFFFFF' : '#F8FAFC' },
      ]}>
      {/* Top Header */}
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
          accessibilityLabel="Wróć do mapy"
          style={styles.backBtn}>
          <MaterialCommunityIcons
            name="arrow-left"
            size={24}
            color={isHighContrast ? '#000000' : '#0F172A'}
          />
        </Pressable>
        <View style={styles.headerTitleBlock}>
          <Text
            style={[
              styles.headerTitle,
              {
                color: isHighContrast ? '#000000' : '#0F172A',
                fontWeight: isHighContrast ? '900' : '800',
              },
            ]}>
            Nawigacja: {destination.name}
          </Text>
          <Text
            style={[
              styles.headerSubtitle,
              { color: isHighContrast ? '#1E293B' : '#64748B' },
            ]}>
            Kraków Stare Miasto → {destination.address}
          </Text>
        </View>
      </View>

      <ScrollView contentContainerStyle={styles.scrollContainer}>
        <View style={styles.content}>
          <AccessibilityToggle />

          {/* Active Navigation Mode View */}
          {isNavigating ? (
            <View
              style={[
                styles.navActiveBox,
                {
                  backgroundColor: isHighContrast ? '#000000' : BrandColors.primaryDark,
                  borderColor: isHighContrast ? '#FFFFFF' : '#000000',
                  borderWidth: isHighContrast ? 3 : 0,
                },
              ]}>
              <View style={styles.navTopRow}>
                <View style={styles.navLiveBadge}>
                  <Text style={styles.navLiveText}>● NAWIGACJA AKTYWNA</Text>
                </View>
                <Pressable
                  onPress={() => setIsNavigating(false)}
                  accessible
                  accessibilityRole="button"
                  accessibilityLabel="Zakończ nawigację">
                  <Text style={styles.navStopText}>Zakończ</Text>
                </Pressable>
              </View>

              {/* Maneuver Instruction */}
              <View style={styles.maneuverBlock}>
                <MaterialCommunityIcons
                  name="arrow-up-bold-circle-outline"
                  size={44}
                  color="#FFFFFF"
                />
                <View style={styles.maneuverTextWrap}>
                  <Text style={styles.maneuverInstruction}>
                    {selectedRoute.maneuvers[activeManeuverIndex]?.instruction}
                  </Text>
                  <Text style={styles.maneuverMeta}>
                    Za {selectedRoute.maneuvers[activeManeuverIndex]?.distanceMeters} m ·{' '}
                    {selectedRoute.maneuvers[activeManeuverIndex]?.landmark || 'Trasa dostępna'}
                  </Text>
                  {selectedRoute.maneuvers[activeManeuverIndex]?.accessibilityNote && (
                    <Text style={styles.maneuverNote}>
                      ✓ {selectedRoute.maneuvers[activeManeuverIndex].accessibilityNote}
                    </Text>
                  )}
                  {selectedRoute.maneuvers[activeManeuverIndex]?.warning && (
                    <Text style={styles.maneuverWarning}>
                      ⚠ {selectedRoute.maneuvers[activeManeuverIndex].warning}
                    </Text>
                  )}
                </View>
              </View>

              {/* Active Channels Info */}
              <View style={styles.channelsInfoRow}>
                <Text style={styles.channelsInfoText}>
                  Aktywne kanały: {feedbackChannels.join(' · ')}
                </Text>
                {activeManeuverIndex < selectedRoute.maneuvers.length - 1 && (
                  <AccessibleButton
                    label="Następny krok"
                    variant="guest"
                    onPress={() => setActiveManeuverIndex((p) => p + 1)}
                    style={styles.nextStepBtn}
                  />
                )}
              </View>
            </View>
          ) : (
            <>
              {/* Route Alternatives Selection */}
              <Text
                style={[
                  styles.sectionTitle,
                  {
                    color: isHighContrast ? '#000000' : '#0F172A',
                    fontWeight: isHighContrast ? '900' : '800',
                  },
                ]}>
                Wybierz wariant trasy (Pareto)
              </Text>

              {planResult.alternatives.map((alt) => {
                const isSelected = selectedRouteId === alt.id;
                return (
                  <Pressable
                    key={alt.id}
                    onPress={() => setSelectedRouteId(alt.id)}
                    accessible
                    accessibilityRole="radio"
                    accessibilityState={{ selected: isSelected }}
                    accessibilityLabel={`${alt.title}: ${alt.durationMinutes} min, ${alt.distanceMeters} metrów, schody: ${alt.stairsCount}`}
                    style={[
                      styles.routeCard,
                      {
                        backgroundColor: isSelected
                          ? (isHighContrast ? '#E8F1FC' : '#F0F7FF')
                          : '#FFFFFF',
                        borderColor: isSelected
                          ? (isHighContrast ? '#000000' : BrandColors.primary)
                          : (isHighContrast ? '#000000' : '#E2E8F0'),
                        borderWidth: isSelected ? 3 : (isHighContrast ? 2 : 1),
                      },
                    ]}>
                    <View style={styles.routeCardHeader}>
                      <View style={styles.routeTitleRow}>
                        <MaterialCommunityIcons
                          name={
                            alt.profileType === 'easiest'
                              ? 'shield-check'
                              : alt.profileType === 'fastest'
                              ? 'lightning-bolt'
                              : alt.profileType === 'quietest'
                              ? 'leaf'
                              : 'bus'
                          }
                          size={24}
                          color={isHighContrast ? '#000000' : BrandColors.primary}
                        />
                        <Text
                          style={[
                            styles.routeTitle,
                            {
                              color: isHighContrast ? '#000000' : '#0F172A',
                              fontWeight: isHighContrast ? '900' : '700',
                            },
                          ]}>
                          {alt.title}
                        </Text>
                      </View>

                      <View style={styles.routeTiming}>
                        <Text
                          style={[
                            styles.routeDuration,
                            {
                              color: isHighContrast ? '#000000' : BrandColors.primary,
                              fontWeight: isHighContrast ? '900' : '800',
                            },
                          ]}>
                          {alt.durationMinutes} min
                        </Text>
                        <Text
                          style={[
                            styles.routeDist,
                            { color: isHighContrast ? '#1E293B' : '#64748B' },
                          ]}>
                          {alt.distanceMeters} m
                        </Text>
                      </View>
                    </View>

                    <Text
                      style={[
                        styles.routeAdvantage,
                        {
                          color: isHighContrast ? '#000000' : BrandColors.accentTeal,
                          fontWeight: isHighContrast ? '800' : '600',
                        },
                      ]}>
                      ✓ {alt.dominantAdvantage}
                    </Text>

                    {/* Constraints badges */}
                    <View style={styles.badgeChipsRow}>
                      <View
                        style={[
                          styles.badgeChip,
                          {
                            backgroundColor:
                              alt.stairsCount === 0 ? '#DCFCE7' : '#FEE2E2',
                          },
                        ]}>
                        <Text
                          style={[
                            styles.badgeChipText,
                            {
                              color: alt.stairsCount === 0 ? '#15803D' : '#B91C1C',
                            },
                          ]}>
                          {alt.stairsCount === 0 ? '0 schodów' : `${alt.stairsCount} schodów`}
                        </Text>
                      </View>

                      {alt.hasRoughSurface && (
                        <View style={[styles.badgeChip, { backgroundColor: '#FEF3C7' }]}>
                          <Text style={[styles.badgeChipText, { color: '#B45309' }]}>
                            Nierówny bruk
                          </Text>
                        </View>
                      )}

                      <View style={[styles.badgeChip, { backgroundColor: '#E0F2FE' }]}>
                        <Text style={[styles.badgeChipText, { color: '#0369A1' }]}>
                          Pewność {alt.confidenceScore}%
                        </Text>
                      </View>
                    </View>

                    {alt.accessibilitySummary.warnings.length > 0 && (
                      <View style={styles.warningBox}>
                        <Text style={styles.warningText}>
                          ⚠ {alt.accessibilitySummary.warnings[0]}
                        </Text>
                      </View>
                    )}
                  </Pressable>
                );
              })}

              {/* Start Navigation Action */}
              <AccessibleButton
                label="Rozpocznij nawigację"
                icon="navigation"
                variant="primary"
                onPress={() => {
                  setIsNavigating(true);
                  setActiveManeuverIndex(0);
                }}
                style={styles.startNavBtn}
              />

              {/* Maneuvers Detailed List */}
              <Text
                style={[
                  styles.sectionTitle,
                  {
                    color: isHighContrast ? '#000000' : '#0F172A',
                    fontWeight: isHighContrast ? '800' : '700',
                  },
                ]}>
                Szczegółowy przebieg trasy
              </Text>

              <View
                style={[
                  styles.maneuversList,
                  {
                    backgroundColor: '#FFFFFF',
                    borderColor: isHighContrast ? '#000000' : '#E2E8F0',
                    borderWidth: isHighContrast ? 2.5 : 1,
                  },
                ]}>
                {selectedRoute.maneuvers.map((m, idx) => (
                  <View
                    key={idx}
                    style={[
                      styles.maneuverItem,
                      idx < selectedRoute.maneuvers.length - 1 && styles.borderBottom,
                    ]}>
                    <View
                      style={[
                        styles.stepDot,
                        {
                          backgroundColor: isHighContrast
                            ? '#000000'
                            : BrandColors.primary,
                        },
                      ]}>
                      <Text style={styles.stepDotText}>{idx + 1}</Text>
                    </View>
                    <View style={styles.stepContent}>
                      <Text
                        style={[
                          styles.stepInstruction,
                          {
                            color: isHighContrast ? '#000000' : '#0F172A',
                            fontWeight: isHighContrast ? '800' : '600',
                          },
                        ]}>
                        {m.instruction}
                      </Text>
                      {m.accessibilityNote && (
                        <Text
                          style={[
                            styles.stepNote,
                            { color: isHighContrast ? '#005A4E' : BrandColors.accentTeal },
                          ]}>
                          ✓ {m.accessibilityNote}
                        </Text>
                      )}
                      {m.warning && (
                        <Text style={styles.stepWarning}>⚠ {m.warning}</Text>
                      )}
                    </View>
                  </View>
                ))}
              </View>
            </>
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
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 12,
    gap: 12,
  },
  backBtn: {
    padding: 6,
  },
  headerTitleBlock: {
    flex: 1,
  },
  headerTitle: {
    fontSize: 18,
  },
  headerSubtitle: {
    fontSize: 12,
    marginTop: 2,
  },
  scrollContainer: {
    padding: Spacing.three,
    alignItems: 'center',
  },
  content: {
    width: '100%',
    maxWidth: MaxContentWidth,
  },
  sectionTitle: {
    fontSize: 18,
    marginTop: 16,
    marginBottom: 10,
  },
  routeCard: {
    borderRadius: 20,
    padding: 16,
    marginBottom: 12,
  },
  routeCardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  routeTitleRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  routeTitle: {
    fontSize: 18,
  },
  routeTiming: {
    alignItems: 'flex-end',
  },
  routeDuration: {
    fontSize: 18,
  },
  routeDist: {
    fontSize: 12,
  },
  routeAdvantage: {
    fontSize: 13,
    marginTop: 6,
  },
  badgeChipsRow: {
    flexDirection: 'row',
    gap: 8,
    marginTop: 10,
  },
  badgeChip: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 8,
  },
  badgeChipText: {
    fontSize: 11,
    fontWeight: '700',
  },
  warningBox: {
    marginTop: 8,
    padding: 8,
    borderRadius: 8,
    backgroundColor: '#FFFBEB',
  },
  warningText: {
    fontSize: 12,
    color: '#B45309',
    fontWeight: '600',
  },
  startNavBtn: {
    marginVertical: 12,
  },
  maneuversList: {
    borderRadius: 20,
    overflow: 'hidden',
    marginBottom: 20,
  },
  maneuverItem: {
    flexDirection: 'row',
    padding: 16,
    gap: 12,
  },
  stepDot: {
    width: 26,
    height: 26,
    borderRadius: 13,
    justifyContent: 'center',
    alignItems: 'center',
  },
  stepDotText: {
    color: '#FFFFFF',
    fontSize: 12,
    fontWeight: '700',
  },
  stepContent: {
    flex: 1,
  },
  stepInstruction: {
    fontSize: 15,
  },
  stepNote: {
    fontSize: 13,
    marginTop: 3,
    fontWeight: '600',
  },
  stepWarning: {
    fontSize: 13,
    marginTop: 3,
    color: BrandColors.danger,
    fontWeight: '600',
  },
  borderBottom: {
    borderBottomWidth: 1,
    borderBottomColor: '#F1F5F9',
  },
  navActiveBox: {
    borderRadius: 24,
    padding: 20,
    marginVertical: 12,
  },
  navTopRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 16,
  },
  navLiveBadge: {
    backgroundColor: '#DC2626',
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 12,
  },
  navLiveText: {
    color: '#FFFFFF',
    fontSize: 11,
    fontWeight: '800',
  },
  navStopText: {
    color: '#F87171',
    fontWeight: '700',
    fontSize: 14,
  },
  maneuverBlock: {
    flexDirection: 'row',
    gap: 16,
    alignItems: 'flex-start',
  },
  maneuverTextWrap: {
    flex: 1,
  },
  maneuverInstruction: {
    color: '#FFFFFF',
    fontSize: 20,
    fontWeight: '800',
    lineHeight: 26,
  },
  maneuverMeta: {
    color: '#CBD5E1',
    fontSize: 14,
    marginTop: 4,
  },
  maneuverNote: {
    color: '#34D399',
    fontSize: 13,
    fontWeight: '700',
    marginTop: 6,
  },
  maneuverWarning: {
    color: '#FBBF24',
    fontSize: 13,
    fontWeight: '700',
    marginTop: 6,
  },
  channelsInfoRow: {
    marginTop: 20,
    paddingTop: 14,
    borderTopWidth: 1,
    borderTopColor: '#334155',
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  channelsInfoText: {
    color: '#94A3B8',
    fontSize: 12,
  },
  nextStepBtn: {
    minHeight: 36,
    paddingVertical: 6,
    paddingHorizontal: 14,
  },
});
