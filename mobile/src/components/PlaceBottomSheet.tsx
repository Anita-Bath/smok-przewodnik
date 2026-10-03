import React from 'react';
import { View, Text, StyleSheet, Pressable } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { BrandColors, Spacing } from '@/constants/theme';
import { KrakowPlace } from '@/services/krakowData';
import { useAccessibility } from '@/context/AccessibilityContext';
import { AccessibleButton } from './AccessibleButton';

interface PlaceBottomSheetProps {
  place: KrakowPlace;
  onNavigateToRoute: (place: KrakowPlace) => void;
  onViewDetails: (place: KrakowPlace) => void;
  onAddReport: (place: KrakowPlace) => void;
}

export function PlaceBottomSheet({
  place,
  onNavigateToRoute,
  onViewDetails,
  onAddReport,
}: PlaceBottomSheetProps) {
  const { isHighContrast, isGuest, savedPlaceIds, toggleSavePlace } = useAccessibility();
  const isSaved = savedPlaceIds.includes(place.id);

  return (
    <View
      style={[
        styles.sheetContainer,
        {
          backgroundColor: '#FFFFFF',
          borderColor: isHighContrast ? '#000000' : '#E2E8F0',
          borderWidth: isHighContrast ? 3 : 1,
        },
      ]}
      accessible
      accessibilityRole="summary"
      accessibilityLabel={`Wybrane miejsce: ${place.name}`}>
      {/* Handle */}
      <View style={styles.handleContainer}>
        <View
          style={[
            styles.handle,
            { backgroundColor: isHighContrast ? '#000000' : '#CBD5E1' },
          ]}
        />
      </View>

      {/* Header: Title + Bookmark */}
      <View style={styles.headerRow}>
        <View style={styles.titleArea}>
          <Text
            style={[
              styles.placeTitle,
              {
                color: isHighContrast ? '#000000' : '#0F172A',
                fontWeight: isHighContrast ? '900' : '800',
              },
            ]}>
            {place.name}
          </Text>
          <Text
            style={[
              styles.placeSubtitle,
              {
                color: isHighContrast ? '#1E293B' : '#64748B',
                fontWeight: isHighContrast ? '600' : '400',
              },
            ]}>
            {place.distanceFromUserMeters} m stąd · {place.address}
          </Text>
        </View>

        <Pressable
          onPress={() => toggleSavePlace(place.id)}
          accessible
          accessibilityRole="button"
          accessibilityLabel={isSaved ? 'Usuń z zapisanych' : 'Zapisz miejsce'}
          style={[
            styles.saveButton,
            {
              borderColor: isHighContrast ? '#000000' : BrandColors.primary,
              borderWidth: isHighContrast ? 2 : 1.5,
              backgroundColor: isSaved
                ? (isHighContrast ? '#000000' : BrandColors.primaryLight)
                : '#FFFFFF',
            },
          ]}>
          <MaterialCommunityIcons
            name={isSaved ? 'bookmark' : 'bookmark-outline'}
            size={22}
            color={
              isSaved && isHighContrast
                ? '#FFFFFF'
                : isHighContrast
                ? '#000000'
                : BrandColors.primary
            }
          />
          <Text
            style={[
              styles.saveText,
              {
                color:
                  isSaved && isHighContrast
                    ? '#FFFFFF'
                    : isHighContrast
                    ? '#000000'
                    : BrandColors.primary,
                fontWeight: isHighContrast ? '800' : '700',
              },
            ]}>
            {isSaved ? 'Zapisano' : 'Zapisz'}
          </Text>
        </Pressable>
      </View>

      {/* Confidence status banner */}
      <View
        style={[
          styles.statusBanner,
          {
            backgroundColor: isHighContrast ? '#E2F1EE' : '#E6F5F3',
            borderColor: isHighContrast ? '#005A4E' : '#B2DFDB',
            borderWidth: isHighContrast ? 2 : 1,
          },
        ]}>
        <MaterialCommunityIcons
          name="account-group-outline"
          size={18}
          color={BrandColors.accentTeal}
        />
        <Text
          style={[
            styles.statusText,
            {
              color: isHighContrast ? '#004D40' : '#00796B',
              fontWeight: isHighContrast ? '800' : '700',
            },
          ]}>
          {place.confidenceLabel}
        </Text>
      </View>

      {/* Facts check list */}
      <View style={styles.factsGrid}>
        {place.facts.map((fact) => (
          <View key={fact.id} style={styles.factRow}>
            <MaterialCommunityIcons
              name={
                fact.status === 'verified'
                  ? 'check-circle-outline'
                  : fact.status === 'to_check'
                  ? 'radiobox-blank'
                  : 'help-circle-outline'
              }
              size={18}
              color={
                isHighContrast
                  ? '#000000'
                  : fact.status === 'verified'
                  ? BrandColors.success
                  : BrandColors.accentTeal
              }
            />
            <Text
              style={[
                styles.factLabel,
                {
                  color: isHighContrast ? '#000000' : '#1E293B',
                  fontWeight: isHighContrast ? '700' : '500',
                },
              ]}>
              {fact.label}
            </Text>
          </View>
        ))}
      </View>

      {/* General disclaimer */}
      <Text
        style={[
          styles.disclaimerText,
          {
            color: isHighContrast ? '#111827' : '#64748B',
            fontWeight: isHighContrast ? '600' : '400',
          },
        ]}>
        {place.generalNote ||
          'Dostępność może się zmieniać. Przed wizytą sprawdź informacje u miejsca.'}
      </Text>

      {/* Action buttons: Trasa & Szczegóły */}
      <View style={styles.buttonsRow}>
        <View style={styles.buttonFlex}>
          <AccessibleButton
            label="Trasa"
            icon="navigation-variant"
            variant="primary"
            onPress={() => onNavigateToRoute(place)}
          />
        </View>
        <View style={styles.buttonFlex}>
          <AccessibleButton
            label="Szczegóły"
            variant="secondary"
            onPress={() => onViewDetails(place)}
          />
        </View>
      </View>

      {/* Community update action */}
      <Pressable
        onPress={() => onAddReport(place)}
        accessible
        accessibilityRole="button"
        accessibilityLabel={
          isGuest
            ? 'Zaloguj się, aby dodać aktualizację dostępności'
            : 'Dodaj aktualizację dostępności'
        }
        style={styles.addUpdateRow}>
        <MaterialCommunityIcons
          name="pencil-outline"
          size={18}
          color={isHighContrast ? '#000000' : BrandColors.accentTeal}
        />
        <Text
          style={[
            styles.addUpdateText,
            {
              color: isHighContrast ? '#000000' : BrandColors.accentTeal,
              fontWeight: isHighContrast ? '800' : '700',
            },
          ]}>
          {isGuest
            ? 'Zaloguj się, aby dodać aktualizację dostępności'
            : 'Dodaj aktualizację dostępności'}
        </Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  sheetContainer: {
    borderTopLeftRadius: 28,
    borderTopRightRadius: 28,
    paddingHorizontal: 20,
    paddingBottom: 24,
    paddingTop: 10,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: -4 },
    shadowOpacity: 0.1,
    shadowRadius: 8,
    elevation: 8,
  },
  handleContainer: {
    alignItems: 'center',
    paddingVertical: 4,
  },
  handle: {
    width: 44,
    height: 5,
    borderRadius: 3,
    marginBottom: 8,
  },
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 10,
  },
  titleArea: {
    flex: 1,
    marginRight: 12,
  },
  placeTitle: {
    fontSize: 24,
    letterSpacing: -0.3,
  },
  placeSubtitle: {
    fontSize: 14,
    marginTop: 2,
  },
  saveButton: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 12,
    minWidth: 64,
  },
  saveText: {
    fontSize: 11,
    marginTop: 2,
  },
  statusBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: 12,
    gap: 8,
    marginVertical: 8,
  },
  statusText: {
    fontSize: 13,
  },
  factsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
    marginVertical: 6,
  },
  factRow: {
    flexDirection: 'row',
    alignItems: 'center',
    width: '48%',
    gap: 6,
    paddingVertical: 2,
  },
  factLabel: {
    fontSize: 13,
    flexShrink: 1,
  },
  disclaimerText: {
    fontSize: 12,
    lineHeight: 16,
    marginVertical: 10,
  },
  buttonsRow: {
    flexDirection: 'row',
    gap: 12,
    marginVertical: 8,
  },
  buttonFlex: {
    flex: 1,
  },
  addUpdateRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    paddingVertical: 10,
    marginTop: 4,
  },
  addUpdateText: {
    fontSize: 14,
  },
});
