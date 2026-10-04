import React, { useState, useEffect, useRef } from 'react';
import { View, Text, StyleSheet, ScrollView, Pressable } from 'react-native';
import * as Speech from 'expo-speech';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { AccessibilityToggle } from '@/components/AccessibilityToggle';
import { AccessibleButton } from '@/components/AccessibleButton';
import { MapViewer } from '@/components/MapViewer';
import { BrandColors, Spacing, MaxContentWidth } from '@/constants/theme';
import { KRAKOW_PLACES } from '@/services/krakowData';
import {
  calculateKrakowRoutes,
  fetchLiveKrakowRoutes,
  formatDistance,
  calculateDistanceMeters,
  RoutePlanResult,
} from '@/services/routingService';
import { useAccessibility } from '@/context/AccessibilityContext';

export default function RoutePlannerScreen() {
  const router = useRouter();
  const { placeId, name, address, lat, lon } = useLocalSearchParams<{
    placeId?: string;
    name?: string;
    address?: string;
    lat?: string;
    lon?: string;
  }>();
  const {
    isDark,
    isHighContrast,
    constraints,
    transportCapabilities,
    feedbackChannels,
    userLocation,
  } = useAccessibility();

  const [destination, setDestination] = useState<KrakowPlace>(KRAKOW_PLACES.find((p) => p.id === placeId) || KRAKOW_PLACES[0]);

  useEffect(() => {
    if (placeId) {
      const found = KRAKOW_PLACES.find((p) => p.id === placeId);
      if (found) {
        setDestination(found);
      } else {
        import('@/services/krakowData').then(({ fetchPlaceById }) => {
          fetchPlaceById(placeId).then((data) => {
            if (data) {
              setDestination(data);
            }
          });
        });
      }
    } else if (lat && lon) {
      setDestination({
        id: 'custom-destination',
        name: name || 'Wybrany punkt w Krakowie',
        address: address || 'Kraków',
        distanceFromUserMeters: 500,
        category: 'cafe',
        coordinates: { latitude: parseFloat(lat), longitude: parseFloat(lon) },
        confidenceState: 'unverified',
        confidenceLabel: 'Punkt z mapy',
        facts: [],
        generalNote: 'Nawigacja do wybranego punktu.',
        hasStepFreeAccess: true,
        hasElevator: false,
        hasAccessibleToilet: false,
        hasInductionLoop: false,
        hasAudioGuidance: false,
        hasRoughSurfaceNotice: false,
      } as any);
    }
  }, [placeId, lat, lon, name, address]);

  const [planResult, setPlanResult] = useState<RoutePlanResult>(() =>
    calculateKrakowRoutes(destination, constraints, transportCapabilities, userLocation)
  );
  const [selectedRouteId, setSelectedRouteId] = useState<string>(
    planResult.alternatives[0]?.id || 'route-easiest'
  );
  const [isNavigating, setIsNavigating] = useState(false);
  const [activeManeuverIndex, setActiveManeuverIndex] = useState(0);

  const lastCalculatedDestId = React.useRef<string | null>(null);

  // Re-fetch / calculate live OSRM routes only when destination changes (and we have userLocation)
  useEffect(() => {
    if (!userLocation || lastCalculatedDestId.current === destination.id) return;

    lastCalculatedDestId.current = destination.id;

    const fallback = calculateKrakowRoutes(
      destination,
      constraints,
      transportCapabilities,
      userLocation
    );
    setPlanResult(fallback);

    fetchLiveKrakowRoutes(
      destination,
      constraints,
      transportCapabilities,
      userLocation
    ).then((live) => {
      setPlanResult(live);
    });
  }, [destination.id, userLocation]);

  const selectedRoute =
    planResult.alternatives.find((r) => r.id === selectedRouteId) ||
    planResult.alternatives[0];

  const activeManeuver = selectedRoute.maneuvers[activeManeuverIndex] || selectedRoute.maneuvers[0];

  useEffect(() => {
    if (isNavigating && feedbackChannels.includes('audio') && activeManeuver) {
      Speech.stop();
      const textToSpeak = `${activeManeuver.instruction}. Następnie kontynuuj przez ${activeManeuver.distanceMeters} metrów.`;
      Speech.speak(textToSpeak, { language: 'pl-PL' });
    }
  }, [activeManeuverIndex, isNavigating, feedbackChannels]);

  // Auto-follow: Automatically advance to the next maneuver when the user approaches its location
  useEffect(() => {
    if (!isNavigating || !userLocation || !selectedRoute?.maneuvers) return;
    
    const nextIndex = activeManeuverIndex + 1;
    if (nextIndex >= selectedRoute.maneuvers.length) return;

    const nextManeuver = selectedRoute.maneuvers[nextIndex];
    if (nextManeuver && nextManeuver.location) {
      const maneuverCoords = {
        latitude: nextManeuver.location[0],
        longitude: nextManeuver.location[1]
      };
      const distanceToNext = calculateDistanceMeters(userLocation, maneuverCoords);
      
      // If user is within 15 meters of the next maneuver, advance automatically
      if (distanceToNext <= 15) {
        setActiveManeuverIndex(nextIndex);
      }
    }
  }, [userLocation, isNavigating, activeManeuverIndex, selectedRoute]);

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
      {/* Top Header */}
      <View
        style={[
          styles.header,
          {
            borderBottomColor: isHighContrast
              ? (isDark ? '#FFFFFF' : '#000000')
              : (isDark ? '#334155' : '#E2E8F0'),
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
          accessibilityLabel="Wróć do mapy"
          style={({ pressed }) => [
            styles.backBtn,
            { opacity: pressed ? 0.7 : 1, cursor: 'pointer' as any },
          ]}>
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
        <View style={styles.headerTitleBlock}>
          <Text
            numberOfLines={1}
            style={[
              styles.headerTitle,
              {
                color: isHighContrast
                  ? (isDark ? '#FFFFFF' : '#000000')
                  : (isDark ? '#F8FAFC' : '#0F172A'),
                fontWeight: isHighContrast ? '900' : '800',
              },
            ]}>
            Nawigacja: {destination.name}
          </Text>
          <Text
            numberOfLines={1}
            style={[
              styles.headerSubtitle,
              {
                color: isHighContrast
                  ? (isDark ? '#CBD5E1' : '#1E293B')
                  : (isDark ? '#94A3B8' : '#64748B'),
              },
            ]}>
            {planResult.originName} ({formatDistance(selectedRoute.distanceMeters)}) → {destination.address}
          </Text>
        </View>
      </View>

      {/* Embedded Live Leaflet Map Preview */}
      <View style={isNavigating ? { flex: 1, width: '100%' } : styles.mapContainer}>
        <MapViewer
          places={[destination]}
          selectedPlace={destination}
          activeRoute={{
            coordinates: selectedRoute.coordinates,
            profileType: selectedRoute.profileType,
          }}
          focusedManeuver={activeManeuver?.location}
          isAutoFollowing={isNavigating}
        />
      </View>

      <ScrollView 
        contentContainerStyle={styles.scrollContainer}
        style={isNavigating ? styles.navOverlay : undefined}
        pointerEvents="box-none"
      >
        <View style={styles.content} pointerEvents="box-none">
          {!isNavigating && <AccessibilityToggle />}

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
                  <Text style={styles.navLiveText}>
                    ● KROK {activeManeuverIndex + 1} Z {selectedRoute.maneuvers.length}
                  </Text>
                </View>
                <Pressable
                  onPress={() => setIsNavigating(false)}
                  accessible
                  accessibilityRole="button"
                  accessibilityLabel="Zakończ nawigację"
                  style={({ pressed }) => [
                    styles.stopNavBtn,
                    { opacity: pressed ? 0.7 : 1, cursor: 'pointer' as any },
                  ]}>
                  <Text style={styles.navStopText}>Zakończ nawigację</Text>
                </Pressable>
              </View>

              {/* Maneuver Instruction */}
              <View style={styles.maneuverBlock}>
                <MaterialCommunityIcons
                  name={
                    activeManeuver.type === 'turn_left'
                      ? 'arrow-left-bold-circle-outline'
                      : activeManeuver.type === 'turn_right'
                      ? 'arrow-right-bold-circle-outline'
                      : activeManeuver.type === 'ramp'
                      ? 'slope-uphill'
                      : activeManeuver.type === 'arrive'
                      ? 'flag-checkered'
                      : 'arrow-up-bold-circle-outline'
                  }
                  size={46}
                  color="#FFFFFF"
                />
                <View style={styles.maneuverTextWrap}>
                  <Text style={styles.maneuverInstruction}>
                    {activeManeuver.instruction}
                  </Text>
                  <Text style={styles.maneuverMeta}>
                    Za {activeManeuver.distanceMeters} m ·{' '}
                    {activeManeuver.landmark || 'Trasa dostępna'}
                  </Text>
                  {activeManeuver.accessibilityNote && (
                    <Text style={styles.maneuverNote}>
                      ✓ {activeManeuver.accessibilityNote}
                    </Text>
                  )}
                  {activeManeuver.warning && (
                    <Text style={styles.maneuverWarning}>
                      ⚠ {activeManeuver.warning}
                    </Text>
                  )}
                </View>
              </View>

              {/* Maneuver Navigation Controls */}
              <View style={styles.navControlsRow}>
                {activeManeuverIndex > 0 ? (
                  <View style={styles.stepBtnWrap}>
                    <AccessibleButton
                      label="Poprzedni krok"
                      icon="arrow-left"
                      variant="secondary"
                      onPress={() => setActiveManeuverIndex((p) => p - 1)}
                    />
                  </View>
                ) : <View style={styles.stepBtnWrap} />}

                {activeManeuverIndex < selectedRoute.maneuvers.length - 1 ? (
                  <View style={styles.stepBtnWrap}>
                    <AccessibleButton
                      label="Następny krok"
                      icon="arrow-right"
                      variant="primary"
                      onPress={() => setActiveManeuverIndex((p) => p + 1)}
                    />
                  </View>
                ) : (
                  <View style={styles.stepBtnWrap}>
                    <AccessibleButton
                      label="Dotarłeś do celu ✓"
                      variant="guest"
                      onPress={() => setIsNavigating(false)}
                    />
                  </View>
                )}
              </View>

              {/* Active Channels Info */}
              <View style={styles.channelsInfoRow}>
                <MaterialCommunityIcons name="broadcast" size={16} color="#CBD5E1" />
                <Text style={styles.channelsInfoText}>
                  Aktywne kanały: {feedbackChannels.join(' · ')}
                </Text>
              </View>
            </View>
          ) : (
            <>
              {/* Route Alternatives Selection */}
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
                    accessibilityLabel={`${alt.title}: ${alt.durationMinutes} min, ${formatDistance(alt.distanceMeters)}, schody: ${alt.stairsCount}`}
                    style={({ pressed }) => [
                      styles.routeCard,
                      {
                        backgroundColor: isSelected
                          ? (isHighContrast ? (isDark ? '#0284C7' : '#E8F1FC') : (isDark ? '#0F2744' : '#F0F7FF'))
                          : (isHighContrast ? (isDark ? '#000000' : '#FFFFFF') : (isDark ? '#1E293B' : '#FFFFFF')),
                        borderColor: isSelected
                          ? (isHighContrast ? '#38BDF8' : (isDark ? '#38BDF8' : BrandColors.primary))
                          : (isHighContrast ? (isDark ? '#475569' : '#000000') : (isDark ? '#334155' : '#E2E8F0')),
                        borderWidth: isSelected ? 3 : (isHighContrast ? 2 : 1),
                        cursor: 'pointer' as any,
                        opacity: pressed ? 0.85 : 1,
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
                          color={
                            isHighContrast
                              ? (isDark ? '#FFFFFF' : '#000000')
                              : (isDark ? '#38BDF8' : BrandColors.primary)
                          }
                        />
                        <Text
                          style={[
                            styles.routeTitle,
                            {
                              color: isHighContrast
                                ? (isDark ? '#FFFFFF' : '#000000')
                                : (isDark ? '#F8FAFC' : '#0F172A'),
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
                              color: isHighContrast
                                ? (isDark ? '#FFFFFF' : '#000000')
                                : (isDark ? '#38BDF8' : BrandColors.primary),
                              fontWeight: isHighContrast ? '900' : '800',
                            },
                          ]}>
                          {alt.durationMinutes} min
                        </Text>
                        <Text
                          style={[
                            styles.routeDist,
                            {
                              color: isHighContrast
                                ? (isDark ? '#CBD5E1' : '#1E293B')
                                : (isDark ? '#94A3B8' : '#64748B'),
                            },
                          ]}>
                          {formatDistance(alt.distanceMeters)}
                        </Text>
                      </View>
                    </View>

                    <Text
                      style={[
                        styles.routeAdvantage,
                        {
                          color: isHighContrast
                            ? (isDark ? '#5EEAD4' : '#000000')
                            : (isDark ? '#2DD4BF' : BrandColors.accentTeal),
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
                              alt.stairsCount === 0
                                ? (isDark ? '#14532D' : '#DCFCE7')
                                : (isDark ? '#7F1D1D' : '#FEE2E2'),
                          },
                        ]}>
                        <Text
                          style={[
                            styles.badgeChipText,
                            {
                              color:
                                alt.stairsCount === 0
                                  ? (isDark ? '#86EFAC' : '#15803D')
                                  : (isDark ? '#FCA5A5' : '#B91C1C'),
                            },
                          ]}>
                          {alt.stairsCount === 0 ? '0 schodów' : `${alt.stairsCount} schodów`}
                        </Text>
                      </View>

                      {alt.hasRoughSurface && (
                        <View
                          style={[
                            styles.badgeChip,
                            { backgroundColor: isDark ? '#78350F' : '#FEF3C7' },
                          ]}>
                          <Text
                            style={[
                              styles.badgeChipText,
                              { color: isDark ? '#FCD34D' : '#B45309' },
                            ]}>
                            Nierówny bruk
                          </Text>
                        </View>
                      )}

                      <View
                        style={[
                          styles.badgeChip,
                          { backgroundColor: isDark ? '#082F49' : '#E0F2FE' },
                        ]}>
                        <Text
                          style={[
                            styles.badgeChipText,
                            { color: isDark ? '#38BDF8' : '#0369A1' },
                          ]}>
                          Pewność {alt.confidenceScore}%
                        </Text>
                      </View>
                    </View>

                    {alt.accessibilitySummary.warnings.length > 0 && (
                      <View
                        style={[
                          styles.warningBox,
                          {
                            backgroundColor: isDark ? '#450A0A' : '#FEF2F2',
                            borderColor: isDark ? '#991B1B' : '#FECACA',
                          },
                        ]}>
                        <Text
                          style={[
                            styles.warningText,
                            { color: isDark ? '#FCA5A5' : '#DC2626' },
                          ]}>
                          ⚠ {alt.accessibilitySummary.warnings[0]}
                        </Text>
                      </View>
                    )}
                  </Pressable>
                );
              })}

              {/* Start Navigation Action */}
              <AccessibleButton
                label="Rozpocznij nawigację na żywo"
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
                    color: isHighContrast
                      ? (isDark ? '#FFFFFF' : '#000000')
                      : (isDark ? '#F8FAFC' : '#0F172A'),
                    fontWeight: isHighContrast ? '800' : '700',
                  },
                ]}>
                Szczegółowy przebieg trasy
              </Text>

              <View
                style={[
                  styles.maneuversList,
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
                {selectedRoute.maneuvers.map((m, idx) => (
                  <Pressable
                    key={idx}
                    onPress={() => {
                      setActiveManeuverIndex(idx);
                    }}
                    style={({ pressed }) => [
                      styles.maneuverItem,
                      idx < selectedRoute.maneuvers.length - 1 && {
                        borderBottomWidth: 1,
                        borderBottomColor: isDark ? '#334155' : '#F1F5F9',
                      },
                      idx === activeManeuverIndex && {
                        backgroundColor: isDark ? '#064E3B' : '#F0FDF4',
                      },
                      { opacity: pressed ? 0.7 : 1, cursor: 'pointer' as any },
                    ]}>
                    <View
                      style={[
                        styles.stepDot,
                        {
                          backgroundColor:
                            idx === activeManeuverIndex
                              ? BrandColors.accentTeal
                              : isHighContrast
                              ? (isDark ? '#38BDF8' : '#000000')
                              : (isDark ? '#0284C7' : BrandColors.primary),
                        },
                      ]}>
                      <Text style={styles.stepDotText}>{idx + 1}</Text>
                    </View>
                    <View style={styles.stepContent}>
                      <Text
                        style={[
                          styles.stepInstruction,
                          {
                            color: isHighContrast
                              ? (isDark ? '#FFFFFF' : '#000000')
                              : (isDark ? '#F8FAFC' : '#0F172A'),
                            fontWeight: isHighContrast ? '800' : '600',
                          },
                        ]}>
                        {m.instruction}
                      </Text>
                      <Text
                        style={[
                          styles.stepDistanceMeta,
                          { color: isDark ? '#94A3B8' : '#64748B' },
                        ]}>
                        Dystans: {m.distanceMeters} m
                      </Text>
                      {m.accessibilityNote && (
                        <Text
                          style={[
                            styles.stepNote,
                            {
                              color: isHighContrast
                                ? (isDark ? '#5EEAD4' : '#005A4E')
                                : (isDark ? '#2DD4BF' : BrandColors.accentTeal),
                            },
                          ]}>
                          ✓ {m.accessibilityNote}
                        </Text>
                      )}
                      {m.warning && (
                        <Text
                          style={[
                            styles.stepWarning,
                            { color: isDark ? '#F87171' : '#DC2626' },
                          ]}>
                          ⚠ {m.warning}
                        </Text>
                      )}
                    </View>
                  </Pressable>
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
    fontSize: 13,
    marginTop: 2,
  },
  mapContainer: {
    height: 220,
    width: '100%',
    backgroundColor: '#F1F5F9',
  },
  scrollContainer: {
    paddingBottom: 40,
  },
  navOverlay: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    maxHeight: '50%',
  },
  content: {
    padding: Spacing.four,
    maxWidth: MaxContentWidth,
    alignSelf: 'center',
    width: '100%',
    gap: Spacing.four,
  },
  navActiveBox: {
    borderRadius: 20,
    padding: 18,
    gap: 16,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.25,
    shadowRadius: 8,
    elevation: 8,
  },
  navTopRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  navLiveBadge: {
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    paddingHorizontal: 12,
    paddingVertical: 5,
    borderRadius: 12,
  },
  navLiveText: {
    color: '#FFFFFF',
    fontWeight: '800',
    fontSize: 12,
    letterSpacing: 0.5,
  },
  stopNavBtn: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    backgroundColor: 'rgba(239, 68, 68, 0.85)',
    borderRadius: 12,
  },
  navStopText: {
    color: '#FFFFFF',
    fontWeight: '700',
    fontSize: 13,
  },
  maneuverBlock: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 16,
  },
  maneuverTextWrap: {
    flex: 1,
  },
  maneuverInstruction: {
    color: '#FFFFFF',
    fontSize: 18,
    fontWeight: '800',
    lineHeight: 24,
  },
  maneuverMeta: {
    color: '#E2E8F0',
    fontSize: 14,
    marginTop: 4,
    fontWeight: '600',
  },
  maneuverNote: {
    color: '#86EFAC',
    fontSize: 13,
    marginTop: 6,
    fontWeight: '600',
  },
  maneuverWarning: {
    color: '#FDE047',
    fontSize: 13,
    marginTop: 4,
    fontWeight: '700',
  },
  navControlsRow: {
    flexDirection: 'row',
    gap: 10,
    marginTop: 4,
  },
  stepBtnWrap: {
    flex: 1,
  },
  channelsInfoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingTop: 8,
    borderTopWidth: 1,
    borderTopColor: 'rgba(255, 255, 255, 0.15)',
  },
  channelsInfoText: {
    color: '#CBD5E1',
    fontSize: 12,
  },
  sectionTitle: {
    fontSize: 18,
    marginTop: 6,
  },
  routeCard: {
    borderRadius: 18,
    padding: 16,
    gap: 10,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.06,
    shadowRadius: 6,
    elevation: 3,
  },
  routeCardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  routeTitleRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
  },
  routeTitle: {
    fontSize: 18,
  },
  routeTiming: {
    alignItems: 'flex-end',
  },
  routeDuration: {
    fontSize: 20,
  },
  routeDist: {
    fontSize: 13,
    marginTop: 2,
  },
  routeAdvantage: {
    fontSize: 14,
  },
  badgeChipsRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  badgeChip: {
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 8,
  },
  badgeChipText: {
    fontSize: 12,
    fontWeight: '700',
  },
  warningBox: {
    backgroundColor: '#FEF2F2',
    borderWidth: 1,
    borderColor: '#FECACA',
    borderRadius: 8,
    padding: 8,
  },
  warningText: {
    color: '#DC2626',
    fontSize: 12,
    fontWeight: '600',
  },
  startNavBtn: {
    marginTop: 6,
  },
  maneuversList: {
    borderRadius: 18,
    overflow: 'hidden',
  },
  maneuverItem: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    padding: 16,
    gap: 14,
  },
  activeManeuverRow: {
    backgroundColor: '#F0FDF4',
  },
  borderBottom: {
    borderBottomWidth: 1,
    borderBottomColor: '#F1F5F9',
  },
  stepDot: {
    width: 28,
    height: 28,
    borderRadius: 14,
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: 2,
  },
  stepDotText: {
    color: '#FFFFFF',
    fontWeight: '800',
    fontSize: 13,
  },
  stepContent: {
    flex: 1,
    gap: 3,
  },
  stepInstruction: {
    fontSize: 15,
  },
  stepDistanceMeta: {
    fontSize: 12,
    color: '#64748B',
  },
  stepNote: {
    fontSize: 13,
    fontWeight: '600',
  },
  stepWarning: {
    fontSize: 13,
    color: '#DC2626',
    fontWeight: '700',
  },
});
