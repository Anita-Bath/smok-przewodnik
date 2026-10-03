import React, { useState, useEffect, useRef, useMemo } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TextInput,
  ScrollView,
  Pressable,
  ActivityIndicator,
  LayoutAnimation,
  Platform,
  Keyboard,
} from 'react-native';
import { useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { AccessibilityToggle } from '@/components/AccessibilityToggle';
import { FilterChip } from '@/components/FilterChip';
import { MapViewer } from '@/components/MapViewer';
import { PlaceBottomSheet } from '@/components/PlaceBottomSheet';
import { BrandColors } from '@/constants/theme';
import { KRAKOW_PLACES, KrakowPlace } from '@/services/krakowData';
import { searchKrakowAddresses, AddressSearchResult } from '@/services/geocodingService';
import { useAccessibility } from '@/context/AccessibilityContext';
import {
  calculateDistanceMeters,
  formatDistance,
  calculateKrakowRoutes,
  fetchLiveKrakowRoutes,
  RoutePlanResult,
} from '@/services/routingService';

export default function ExploreScreen() {
  const router = useRouter();
  const {
    isHighContrast,
    isDark,
    isGuest,
    user,
    userLocation,
    constraints,
    transportCapabilities,
  } = useAccessibility();

  const [searchQuery, setSearchQuery] = useState('');
  const [suggestions, setSuggestions] = useState<AddressSearchResult[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [activeFilter, setActiveFilter] = useState<string>('no_stairs');

  // Bottom Sheet state: starts hidden until user selects a place or searches!
  const [selectedPlace, setSelectedPlace] = useState<KrakowPlace | null>(null);
  const [isSheetVisible, setIsSheetVisible] = useState(false);
  const [isSheetExpanded, setIsSheetExpanded] = useState(false);

  // Route preview state directly on map
  const [routePlan, setRoutePlan] = useState<RoutePlanResult | null>(null);
  const [selectedRouteAltId, setSelectedRouteAltId] = useState<string>('route-easiest');

  const activeRouteAlternative = useMemo(() => {
    if (!routePlan) return null;
    return (
      routePlan.alternatives.find((a) => a.id === selectedRouteAltId) ||
      routePlan.alternatives[0]
    );
  }, [routePlan, selectedRouteAltId]);

  const activeRoute = useMemo(() => {
    if (!activeRouteAlternative) return null;
    return {
      coordinates: activeRouteAlternative.coordinates,
      profileType: activeRouteAlternative.profileType,
    };
  }, [activeRouteAlternative]);

  const [filteredPlaces, setFilteredPlaces] = useState<KrakowPlace[]>(KRAKOW_PLACES);
  const [locatedNotice, setLocatedNotice] = useState<string | null>(null);
  const [searchPin, setSearchPin] = useState<{ coords: { latitude: number; longitude: number }; label: string } | null>(null);

  const searchDebounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Address search with debounce
  useEffect(() => {
    if (!searchQuery.trim()) {
      setSuggestions([]);
      setIsSearching(false);
      return;
    }

    setIsSearching(true);
    if (searchDebounceRef.current) {
      clearTimeout(searchDebounceRef.current);
    }

    searchDebounceRef.current = setTimeout(async () => {
      try {
        const results = await searchKrakowAddresses(searchQuery);
        setSuggestions(results);
      } catch (err) {
        setSuggestions([]);
      } finally {
        setIsSearching(false);
      }
    }, 250);

    return () => {
      if (searchDebounceRef.current) {
        clearTimeout(searchDebounceRef.current);
      }
    };
  }, [searchQuery]);

  const handleSelectSuggestion = (item: AddressSearchResult) => {
    Keyboard.dismiss();
    setSearchQuery(item.title);
    setSuggestions([]);

    let targetPlace: KrakowPlace;

    if (item.placeData) {
      targetPlace = item.placeData;
    } else {
      // Create synthetic place for searched OpenStreetMap address
      targetPlace = {
        id: item.id,
        name: item.title,
        address: item.subtitle,
        distanceFromUserMeters: 450,
        category: 'cafe',
        coordinates: item.coordinates,
        confidenceState: 'unverified',
        confidenceLabel: 'Wyszukany adres · OpenStreetMap',
        facts: [
          {
            id: 'f1',
            name: 'Wejście',
            status: 'to_check',
            label: 'Wejście: do sprawdzenia w terenie',
            description: 'Brak zweryfikowanych danych o schodach.',
          },
          {
            id: 'f2',
            name: 'Nawierzchnia',
            status: 'to_check',
            label: 'Nawierzchnia: miejska',
            description: 'Ulica w centrum Krakowa.',
          },
        ],
        generalNote: 'Adres z bazy OpenStreetMap. Sprawdź dostępność na miejscu.',
        hasStepFreeAccess: true,
        hasElevator: false,
        hasAccessibleToilet: false,
        hasInductionLoop: false,
        hasAudioGuidance: false,
        hasRoughSurfaceNotice: false,
      };
    }

    setSelectedPlace(targetPlace);
    setSearchPin({ coords: item.coordinates, label: item.title });

    // Open sheet in compact mode
    if (Platform.OS !== 'web') {
      LayoutAnimation.configureNext(LayoutAnimation.Presets.easeInEaseOut);
    }
    setIsSheetVisible(true);
    setIsSheetExpanded(false);
  };

  const handleSearchSubmit = async () => {
    Keyboard.dismiss();
    if (suggestions.length > 0) {
      handleSelectSuggestion(suggestions[0]);
    } else if (searchQuery.trim().length >= 2) {
      setIsSearching(true);
      try {
        const results = await searchKrakowAddresses(searchQuery);
        if (results.length > 0) {
          handleSelectSuggestion(results[0]);
        }
      } catch (err) {
      } finally {
        setIsSearching(false);
      }
    }
  };

  const handleFilterToggle = (filterKey: string) => {
    const next = activeFilter === filterKey ? '' : filterKey;
    setActiveFilter(next);

    if (next === 'no_stairs') {
      setFilteredPlaces(KRAKOW_PLACES.filter((p) => p.hasStepFreeAccess));
    } else if (next === 'toilets') {
      setFilteredPlaces(KRAKOW_PLACES.filter((p) => p.hasAccessibleToilet));
    } else if (next === 'loop') {
      setFilteredPlaces(KRAKOW_PLACES.filter((p) => p.hasInductionLoop));
    } else if (next === 'elevators') {
      setFilteredPlaces(KRAKOW_PLACES.filter((p) => p.hasElevator));
    } else {
      setFilteredPlaces(KRAKOW_PLACES);
    }
  };

  const handleSelectPlaceFromMap = (place: KrakowPlace) => {
    setSelectedPlace(place);
    if (Platform.OS !== 'web') {
      LayoutAnimation.configureNext(LayoutAnimation.Presets.easeInEaseOut);
    }
    setIsSheetVisible(true);
    setIsSheetExpanded(true); // Open expanded on marker tap
  };

  const handleMapBackgroundClick = () => {
    // When clicking empty map space, dismiss/hide bottom sheet
    if (isSheetVisible) {
      if (Platform.OS !== 'web') {
        LayoutAnimation.configureNext(LayoutAnimation.Presets.easeInEaseOut);
      }
      if (isSheetExpanded) {
        setIsSheetExpanded(false); // First collapse to peek
      } else {
        setIsSheetVisible(false); // Then hide
      }
    }
    setSuggestions([]);
  };

  const handleNavigateToRoute = (place: KrakowPlace) => {
    // Calculate route immediately and show polyline on the map
    const plan = calculateKrakowRoutes(place, constraints, transportCapabilities, userLocation);
    setRoutePlan(plan);
    setSelectedRouteAltId(plan.alternatives[0].id);
    setIsSheetVisible(false);

    // Refine geometry asynchronously with OSRM in background
    fetchLiveKrakowRoutes(place, constraints, transportCapabilities, userLocation).then((livePlan) => {
      setRoutePlan(livePlan);
    });
  };

  const handleViewDetails = (place: KrakowPlace) => {
    router.push({
      pathname: '/place/[id]',
      params: { id: place.id },
    });
  };

  const handleAddReport = (place: KrakowPlace) => {
    if (isGuest) {
      router.push('/auth/login');
    } else {
      router.push({
        pathname: '/report/new',
        params: { placeId: place.id },
      });
    }
  };

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
      {/* Top Search & Filter Container */}
      <View style={styles.topContainer}>
        <View
          style={[
            styles.searchBar,
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
          <MaterialCommunityIcons
            name="magnify"
            size={22}
            color={isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#94A3B8' : '#64748B')}
          />
          <TextInput
            placeholder="Szukaj adresu w Krakowie (np. Floriańska, Grodzka)"
            placeholderTextColor={isDark ? '#64748B' : '#94A3B8'}
            value={searchQuery}
            onChangeText={setSearchQuery}
            onSubmitEditing={handleSearchSubmit}
            returnKeyType="search"
            accessible
            accessibilityLabel="Wyszukiwarka adresów i obiektów w Krakowie"
            style={[
              styles.searchInput,
              {
                color: isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#F8FAFC' : '#0F172A'),
                fontWeight: isHighContrast ? '700' : '500',
              },
            ]}
          />
          {isSearching && (
            <ActivityIndicator
              size="small"
              color={isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#38BDF8' : BrandColors.primary)}
              style={{ marginRight: 6 }}
            />
          )}
          {searchQuery.length > 0 && !isSearching && (
            <Pressable
              onPress={() => {
                setSearchQuery('');
                setSuggestions([]);
              }}
              accessible
              accessibilityRole="button"
              accessibilityLabel="Wyczyść wyszukiwanie">
              <MaterialCommunityIcons
                name="close-circle"
                size={18}
                color={isDark ? '#64748B' : '#94A3B8'}
              />
            </Pressable>
          )}
        </View>

        {/* Address Autocomplete Dropdown */}
        {suggestions.length > 0 && (
          <View
            style={[
              styles.suggestionsCard,
              {
                backgroundColor: isHighContrast
                  ? (isDark ? '#000000' : '#FFFFFF')
                  : (isDark ? '#1E293B' : '#FFFFFF'),
                borderColor: isHighContrast
                  ? (isDark ? '#FFFFFF' : '#000000')
                  : (isDark ? '#334155' : '#CBD5E1'),
                borderWidth: isHighContrast ? 2.5 : 1.5,
              },
            ]}>
            <ScrollView
              keyboardShouldPersistTaps="handled"
              style={{ maxHeight: 240 }}>
              {suggestions.map((item, idx) => (
                <Pressable
                  key={item.id}
                  onPress={() => handleSelectSuggestion(item)}
                  accessible
                  accessibilityRole="button"
                  accessibilityLabel={`Wynik wyszukiwania: ${item.title}, ${item.subtitle}`}
                  style={[
                    styles.suggestionRow,
                    idx < suggestions.length - 1 && styles.suggestionBorder,
                    {
                      borderBottomColor: isDark ? '#334155' : '#F1F5F9',
                    },
                  ]}>
                  <MaterialCommunityIcons
                    name={item.isPoi ? 'castle' : 'map-marker'}
                    size={20}
                    color={
                      isHighContrast
                        ? (isDark ? '#FFFFFF' : '#000000')
                        : item.isPoi
                        ? (isDark ? '#38BDF8' : BrandColors.primary)
                        : (isDark ? '#2DD4BF' : BrandColors.accentTeal)
                    }
                  />
                  <View style={styles.suggestionTextWrap}>
                    <Text
                      style={[
                        styles.suggestionTitle,
                        {
                          color: isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#F8FAFC' : '#0F172A'),
                          fontWeight: isHighContrast ? '800' : '600',
                        },
                      ]}>
                      {item.title}
                    </Text>
                    <Text
                      numberOfLines={1}
                      style={[
                        styles.suggestionSubtitle,
                        { color: isHighContrast ? (isDark ? '#CBD5E1' : '#1E293B') : (isDark ? '#94A3B8' : '#64748B') },
                      ]}>
                      {userLocation ? `${formatDistance(calculateDistanceMeters(userLocation, item.coordinates))} · ` : ''}
                      {item.subtitle}
                    </Text>
                  </View>
                </Pressable>
              ))}
            </ScrollView>
          </View>
        )}

        {/* Filter Pills */}
        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.filtersScroll}>
          <FilterChip
            label="Filtry"
            icon="tune-variant"
            selected={false}
            onPress={() => {}}
          />
          <FilterChip
            label="Bez schodów"
            selected={activeFilter === 'no_stairs'}
            onPress={() => handleFilterToggle('no_stairs')}
          />
          <FilterChip
            label="Toalety"
            selected={activeFilter === 'toilets'}
            onPress={() => handleFilterToggle('toilets')}
          />
          <FilterChip
            label="Pętla"
            selected={activeFilter === 'loop'}
            onPress={() => handleFilterToggle('loop')}
          />
          <FilterChip
            label="Windy"
            selected={activeFilter === 'elevators'}
            onPress={() => handleFilterToggle('elevators')}
          />
        </ScrollView>

        {/* User Mode Badge & High Contrast Switch */}
        <View style={styles.badgeRow}>
          <View
            style={[
              styles.userBadge,
              {
                backgroundColor: isDark
                  ? '#1E293B'
                  : (isHighContrast ? '#E2F1EE' : '#E6F5F3'),
                borderColor: isDark
                  ? '#334155'
                  : (isHighContrast ? '#005A4E' : '#B2DFDB'),
                borderWidth: isHighContrast ? 2 : 1,
              },
            ]}>
            <MaterialCommunityIcons
              name="account-outline"
              size={16}
              color={isDark ? '#2DD4BF' : BrandColors.accentTeal}
            />
            <Text
              style={[
                styles.userBadgeText,
                {
                  color: isDark
                    ? '#2DD4BF'
                    : (isHighContrast ? '#004D40' : '#00796B'),
                },
              ]}>
              {isGuest ? 'Tryb gościa' : (user?.name || 'Zalogowany')}
            </Text>
          </View>
        </View>

        {locatedNotice && (
          <View
            style={[
              styles.locatedBanner,
              {
                backgroundColor: isDark
                  ? '#064E3B'
                  : (isHighContrast ? '#E8F5E9' : '#DCFCE7'),
                borderColor: isDark
                  ? '#059669'
                  : (isHighContrast ? '#000000' : '#86EFAC'),
                borderWidth: isHighContrast ? 2 : 1,
              },
            ]}>
            <MaterialCommunityIcons
              name="crosshairs-gps"
              size={16}
              color={isDark ? '#4ADE80' : '#15803D'}
            />
            <Text
              style={[
                styles.locatedNoticeText,
                { color: isDark ? '#6EE7B7' : '#15803D' },
              ]}>
              {locatedNotice}
            </Text>
          </View>
        )}

        <AccessibilityToggle />
      </View>

      {/* Main Map Viewer with Leaflet & OpenStreetMap */}
      <View style={styles.mapFlex}>
        <MapViewer
          places={filteredPlaces}
          selectedPlace={selectedPlace}
          searchPin={searchPin}
          activeRoute={activeRoute}
          onSelectPlace={handleSelectPlaceFromMap}
          onMapClick={handleMapBackgroundClick}
          onLocateMe={() => {
            setLocatedNotice('Zlokalizowano pozycję GPS w Krakowie');
            setTimeout(() => setLocatedNotice(null), 3500);
          }}
        />

        {/* Floating Route Preview Card on Map */}
        {routePlan && activeRouteAlternative && (
          <View
            style={[
              styles.routePreviewCard,
              {
                backgroundColor: isHighContrast
                  ? (isDark ? '#000000' : '#FFFFFF')
                  : (isDark ? '#1E293B' : '#FFFFFF'),
                borderColor: isHighContrast
                  ? (isDark ? '#FFFFFF' : '#000000')
                  : (isDark ? '#38BDF8' : BrandColors.primary),
                borderWidth: isHighContrast ? 3 : 2,
              },
            ]}>
            {/* Header: Destination name, distance & estimated time */}
            <View style={styles.routePreviewHeader}>
              <View style={styles.routePreviewTitleCol}>
                <View style={styles.routeHeaderBadge}>
                  <MaterialCommunityIcons
                    name="navigation-variant"
                    size={18}
                    color={isDark ? '#38BDF8' : BrandColors.primary}
                  />
                  <Text
                    numberOfLines={1}
                    style={[
                      styles.routeHeaderBadgeText,
                      {
                        color: isHighContrast
                          ? (isDark ? '#FFFFFF' : '#000000')
                          : (isDark ? '#38BDF8' : BrandColors.primary),
                        fontWeight: '800',
                      },
                    ]}>
                    Trasa: {routePlan.destination.name}
                  </Text>
                </View>
                <Text
                  style={[
                    styles.routePreviewMetrics,
                    {
                      color: isHighContrast
                        ? (isDark ? '#FFFFFF' : '#000000')
                        : (isDark ? '#F8FAFC' : '#0F172A'),
                      fontWeight: isHighContrast ? '800' : '700',
                    },
                  ]}>
                  {formatDistance(activeRouteAlternative.distanceMeters)} · ok. {activeRouteAlternative.durationMinutes} min
                  {activeRouteAlternative.stairsCount === 0 ? ' · 0 schodów' : ` · ${activeRouteAlternative.stairsCount} sch.`}
                </Text>
              </View>

              <Pressable
                onPress={() => setRoutePlan(null)}
                accessible
                accessibilityRole="button"
                accessibilityLabel="Zamknij podgląd trasy"
                style={({ pressed }) => [
                  styles.closeRouteBtn,
                  { opacity: pressed ? 0.7 : 1, cursor: 'pointer' as any },
                ]}>
                <MaterialCommunityIcons
                  name="close"
                  size={20}
                  color={isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#94A3B8' : '#475569')}
                />
              </Pressable>
            </View>

            {/* Alternatives selector */}
            <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.routeChipsScroll}>
              {routePlan.alternatives.map((alt) => {
                const isSelected = selectedRouteAltId === alt.id;
                return (
                  <Pressable
                    key={alt.id}
                    onPress={() => setSelectedRouteAltId(alt.id)}
                    style={({ pressed }) => [
                      styles.routeAltChip,
                      {
                        backgroundColor: isSelected
                          ? (isHighContrast
                              ? (isDark ? '#FFFFFF' : '#000000')
                              : (isDark ? '#0284C7' : BrandColors.primary))
                          : (isDark ? '#334155' : '#F1F5F9'),
                        borderColor: isHighContrast
                          ? (isDark ? '#FFFFFF' : '#000000')
                          : isSelected
                          ? (isDark ? '#38BDF8' : BrandColors.primary)
                          : (isDark ? '#475569' : '#CBD5E1'),
                        borderWidth: isHighContrast ? 2 : 1,
                        cursor: 'pointer' as any,
                        opacity: pressed ? 0.7 : 1,
                      },
                    ]}>
                    <Text
                      style={[
                        styles.routeAltChipText,
                        {
                          color: isSelected
                            ? (isHighContrast && isDark ? '#000000' : '#FFFFFF')
                            : (isDark ? '#F8FAFC' : (isHighContrast ? '#000000' : '#1E293B')),
                          fontWeight: isSelected ? '800' : '600',
                        },
                      ]}>
                      {alt.title} ({formatDistance(alt.distanceMeters)})
                    </Text>
                  </Pressable>
                );
              })}
            </ScrollView>

            {/* Start Turn-by-Turn Navigation Button */}
            <Pressable
              onPress={() => {
                router.push({
                  pathname: '/route-planner',
                  params: {
                    placeId: routePlan.destination.id,
                    name: routePlan.destination.name,
                    address: routePlan.destination.address,
                    lat: String(routePlan.destination.coordinates.latitude),
                    lon: String(routePlan.destination.coordinates.longitude),
                  },
                });
              }}
              style={({ pressed }) => [
                styles.navStartBtn,
                {
                  backgroundColor: isHighContrast
                    ? (isDark ? '#FFFFFF' : '#000000')
                    : (isDark ? '#0284C7' : BrandColors.primary),
                  borderColor: isHighContrast ? (isDark ? '#000000' : '#FFFFFF') : 'transparent',
                  cursor: 'pointer' as any,
                  opacity: pressed ? 0.8 : 1,
                },
              ]}>
              <MaterialCommunityIcons
                name="navigation"
                size={18}
                color={isHighContrast && isDark ? '#000000' : '#FFFFFF'}
              />
              <Text
                style={[
                  styles.navStartBtnText,
                  { color: isHighContrast && isDark ? '#000000' : '#FFFFFF' },
                ]}>
                Nawiguj krok po kroku
              </Text>
            </Pressable>
          </View>
        )}

        {/* Floating pill to restore sheet if user dismissed it */}
        {selectedPlace && !isSheetVisible && (
          <Pressable
            onPress={() => {
              if (Platform.OS !== 'web') {
                LayoutAnimation.configureNext(LayoutAnimation.Presets.easeInEaseOut);
              }
              setIsSheetVisible(true);
              setIsSheetExpanded(false);
            }}
            accessible
            accessibilityRole="button"
            accessibilityLabel={`Pokaż szczegóły dla ${selectedPlace.name}`}
            style={[
              styles.restoreSheetPill,
              {
                backgroundColor: isHighContrast
                  ? (isDark ? '#000000' : '#FFFFFF')
                  : (isDark ? '#1E293B' : '#FFFFFF'),
                borderColor: isHighContrast
                  ? (isDark ? '#FFFFFF' : '#000000')
                  : (isDark ? '#38BDF8' : BrandColors.primary),
                borderWidth: isHighContrast ? 2.5 : 1.5,
              },
            ]}>
            <MaterialCommunityIcons
              name="map-marker"
              size={20}
              color={isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#38BDF8' : BrandColors.primary)}
            />
            <Text
              style={[
                styles.restoreSheetText,
                {
                  color: isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#F8FAFC' : BrandColors.primary),
                  fontWeight: isHighContrast ? '900' : '700',
                },
              ]}>
              Pokaż: {selectedPlace.name}
            </Text>
            <MaterialCommunityIcons
              name="chevron-up"
              size={20}
              color={isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#38BDF8' : BrandColors.primary)}
            />
          </Pressable>
        )}

        {/* Interactive Bottom Sheet Floating Overlay */}
        {selectedPlace && isSheetVisible && (
          <View style={styles.bottomSheetOverlay}>
            <PlaceBottomSheet
              place={selectedPlace}
              isExpanded={isSheetExpanded}
              onToggleExpand={() => {
                setIsSheetExpanded((prev) => !prev);
              }}
              onClose={() => {
                setIsSheetVisible(false);
              }}
              onNavigateToRoute={handleNavigateToRoute}
              onViewDetails={handleViewDetails}
              onAddReport={handleAddReport}
            />
          </View>
        )}
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
  },
  topContainer: {
    paddingHorizontal: 16,
    paddingTop: 8,
    paddingBottom: 4,
    position: 'relative',
    zIndex: 50,
  },
  searchBar: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: 24,
    gap: 10,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.08,
    shadowRadius: 3,
    elevation: 2,
  },
  searchInput: {
    flex: 1,
    fontSize: 16,
    padding: 0,
  },
  suggestionsCard: {
    position: 'absolute',
    top: 58,
    left: 16,
    right: 16,
    borderRadius: 16,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 6 },
    shadowOpacity: 0.2,
    shadowRadius: 10,
    elevation: 20,
    zIndex: 9999,
    overflow: 'hidden',
  },
  suggestionRow: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 12,
    gap: 12,
  },
  suggestionBorder: {
    borderBottomWidth: 1,
    borderBottomColor: '#F1F5F9',
  },
  suggestionTextWrap: {
    flex: 1,
  },
  suggestionTitle: {
    fontSize: 15,
  },
  suggestionSubtitle: {
    fontSize: 12,
    marginTop: 2,
  },
  filtersScroll: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 10,
  },
  badgeRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 4,
  },
  userBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingHorizontal: 12,
    paddingVertical: 5,
    borderRadius: 12,
  },
  userBadgeText: {
    fontSize: 13,
    fontWeight: '700',
  },
  locatedBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 12,
    marginVertical: 4,
  },
  locatedNoticeText: {
    fontSize: 13,
    fontWeight: '700',
    color: '#15803D',
  },
  mapFlex: {
    flex: 1,
    minHeight: 220,
    position: 'relative',
  },
  bottomSheetOverlay: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    zIndex: 100,
  },
  restoreSheetPill: {
    position: 'absolute',
    bottom: 20,
    alignSelf: 'center',
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    paddingHorizontal: 18,
    paddingVertical: 10,
    borderRadius: 24,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 3 },
    shadowOpacity: 0.2,
    shadowRadius: 6,
    elevation: 6,
    zIndex: 90,
    cursor: 'pointer' as any,
  },
  restoreSheetText: {
    fontSize: 14,
  },
  routePreviewCard: {
    position: 'absolute',
    top: 12,
    left: 14,
    right: 14,
    borderRadius: 20,
    padding: 14,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.18,
    shadowRadius: 10,
    elevation: 12,
    zIndex: 110,
  },
  routePreviewHeader: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    justifyContent: 'space-between',
    marginBottom: 10,
  },
  routePreviewTitleCol: {
    flex: 1,
    marginRight: 8,
  },
  routeHeaderBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    marginBottom: 3,
  },
  routeHeaderBadgeText: {
    fontSize: 15,
  },
  routePreviewMetrics: {
    fontSize: 14,
  },
  closeRouteBtn: {
    width: 32,
    height: 32,
    borderRadius: 16,
    backgroundColor: '#F1F5F9',
    justifyContent: 'center',
    alignItems: 'center',
  },
  routeChipsScroll: {
    flexDirection: 'row',
    marginBottom: 10,
  },
  routeAltChip: {
    paddingHorizontal: 12,
    paddingVertical: 7,
    borderRadius: 16,
    marginRight: 8,
  },
  routeAltChipText: {
    fontSize: 12,
  },
  navStartBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    paddingVertical: 12,
    borderRadius: 14,
    borderWidth: 1,
  },
  navStartBtnText: {
    color: '#FFFFFF',
    fontSize: 15,
    fontWeight: '800',
  },
});
