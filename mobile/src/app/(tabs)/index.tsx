import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TextInput,
  ScrollView,
  Pressable,
} from 'react-native';
import { useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { AccessibilityToggle } from '@/components/AccessibilityToggle';
import { FilterChip } from '@/components/FilterChip';
import { MapViewer } from '@/components/MapViewer';
import { PlaceBottomSheet } from '@/components/PlaceBottomSheet';
import { BrandColors, Spacing } from '@/constants/theme';
import { KRAKOW_PLACES, KrakowPlace } from '@/services/krakowData';
import { useAccessibility } from '@/context/AccessibilityContext';

export default function ExploreScreen() {
  const router = useRouter();
  const { isHighContrast, isGuest, user } = useAccessibility();

  const [searchQuery, setSearchQuery] = useState('');
  const [activeFilter, setActiveFilter] = useState<string>('no_stairs');
  const [selectedPlace, setSelectedPlace] = useState<KrakowPlace>(KRAKOW_PLACES[0]); // default to Wawel
  const [filteredPlaces, setFilteredPlaces] = useState<KrakowPlace[]>(KRAKOW_PLACES);
  const [locatedNotice, setLocatedNotice] = useState<string | null>(null);

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

  const handleSearch = (text: string) => {
    setSearchQuery(text);
    if (!text.trim()) {
      setFilteredPlaces(KRAKOW_PLACES);
      return;
    }
    const filtered = KRAKOW_PLACES.filter(
      (p) =>
        p.name.toLowerCase().includes(text.toLowerCase()) ||
        p.address.toLowerCase().includes(text.toLowerCase())
    );
    setFilteredPlaces(filtered);
    if (filtered.length > 0) {
      setSelectedPlace(filtered[0]);
    }
  };

  const handleNavigateToRoute = (place: KrakowPlace) => {
    router.push({
      pathname: '/route-planner',
      params: { placeId: place.id },
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
        { backgroundColor: isHighContrast ? '#FFFFFF' : '#F8FAFC' },
      ]}>
      {/* Top Search Bar */}
      <View style={styles.topContainer}>
        <View
          style={[
            styles.searchBar,
            {
              backgroundColor: '#FFFFFF',
              borderColor: isHighContrast ? '#000000' : '#E2E8F0',
              borderWidth: isHighContrast ? 2.5 : 1,
            },
          ]}>
          <MaterialCommunityIcons
            name="magnify"
            size={22}
            color={isHighContrast ? '#000000' : '#64748B'}
          />
          <TextInput
            placeholder="Szukaj miejsc i adresów"
            placeholderTextColor="#94A3B8"
            value={searchQuery}
            onChangeText={handleSearch}
            accessible
            accessibilityLabel="Pole wyszukiwania miejsc i adresów w Krakowie"
            style={[
              styles.searchInput,
              {
                color: isHighContrast ? '#000000' : '#0F172A',
                fontWeight: isHighContrast ? '700' : '500',
              },
            ]}
          />
          {searchQuery.length > 0 && (
            <Pressable
              onPress={() => handleSearch('')}
              accessible
              accessibilityRole="button"
              accessibilityLabel="Wyczyść wyszukiwanie">
              <MaterialCommunityIcons
                name="close-circle"
                size={18}
                color="#94A3B8"
              />
            </Pressable>
          )}
        </View>

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
                backgroundColor: isHighContrast ? '#E2F1EE' : '#E6F5F3',
                borderColor: isHighContrast ? '#005A4E' : '#B2DFDB',
                borderWidth: isHighContrast ? 2 : 1,
              },
            ]}>
            <MaterialCommunityIcons
              name="account-outline"
              size={16}
              color={BrandColors.accentTeal}
            />
            <Text
              style={[
                styles.userBadgeText,
                { color: isHighContrast ? '#004D40' : '#00796B' },
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
                backgroundColor: isHighContrast ? '#E8F5E9' : '#DCFCE7',
                borderColor: isHighContrast ? '#000000' : '#86EFAC',
                borderWidth: isHighContrast ? 2 : 1,
              },
            ]}>
            <MaterialCommunityIcons name="crosshairs-gps" size={16} color="#15803D" />
            <Text style={styles.locatedNoticeText}>{locatedNotice}</Text>
          </View>
        )}

        <AccessibilityToggle />
      </View>

      {/* Main Map Viewer with Leaflet & OpenStreetMap */}
      <View style={styles.mapFlex}>
        <MapViewer
          places={filteredPlaces}
          selectedPlace={selectedPlace}
          onSelectPlace={(p) => setSelectedPlace(p)}
          onLocateMe={() => {
            setLocatedNotice('Zlokalizowano pozycję GPS w Krakowie');
            setTimeout(() => setLocatedNotice(null), 3500);
          }}
        />
      </View>

      {/* Bottom Place Card */}
      {selectedPlace && (
        <PlaceBottomSheet
          place={selectedPlace}
          onNavigateToRoute={handleNavigateToRoute}
          onViewDetails={handleViewDetails}
          onAddReport={handleAddReport}
        />
      )}
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
  },
});
