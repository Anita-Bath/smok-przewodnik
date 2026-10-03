import React from 'react';
import { View, Text, StyleSheet, Pressable } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { BrandColors } from '@/constants/theme';
import { KrakowPlace } from '@/services/krakowData';
import { useAccessibility } from '@/context/AccessibilityContext';
import { calculateDistanceMeters, formatDistance } from '@/services/routingService';
import { AccessibleButton } from './AccessibleButton';

interface PlaceBottomSheetProps {
  place: KrakowPlace;
  isExpanded?: boolean;
  onToggleExpand?: () => void;
  onClose?: () => void;
  onNavigateToRoute: (place: KrakowPlace) => void;
  onViewDetails: (place: KrakowPlace) => void;
  onAddReport: (place: KrakowPlace) => void;
}

export function PlaceBottomSheet({
  place,
  isExpanded = true,
  onToggleExpand,
  onClose,
  onNavigateToRoute,
  onViewDetails,
  onAddReport,
}: PlaceBottomSheetProps) {
  const { isHighContrast, isDark, isGuest, savedPlaceIds, toggleSavePlace, userLocation } = useAccessibility();
  const isSaved = savedPlaceIds.includes(place.id);

  const realDistanceMeters = userLocation
    ? calculateDistanceMeters(userLocation, place.coordinates)
    : place.distanceFromUserMeters;
  const distanceText = formatDistance(realDistanceMeters);

  const handleToggle = () => {
    if (onToggleExpand) {
      onToggleExpand();
    }
  };

  return (
    <View
      style={[
        styles.sheetContainer,
        {
          backgroundColor: isHighContrast
            ? (isDark ? '#000000' : '#FFFFFF')
            : (isDark ? '#1E293B' : '#FFFFFF'),
          borderColor: isHighContrast
            ? (isDark ? '#FFFFFF' : '#000000')
            : (isDark ? '#334155' : '#E2E8F0'),
          borderWidth: isHighContrast ? 3 : 1,
        },
      ]}
      accessible
      accessibilityRole="summary"
      accessibilityLabel={`Informacje o miejscu: ${place.name}, ${isExpanded ? 'rozwinięte' : 'zwinięte'}`}>
      
      {/* Interactive Handle Bar - Generous touch target */}
      <Pressable
        onPress={handleToggle}
        accessible
        accessibilityRole="button"
        accessibilityLabel={isExpanded ? 'Zwiń kartę miejsca' : 'Rozwiń szczegóły miejsca'}
        style={({ pressed }) => [
          styles.handleContainer,
          {
            opacity: pressed ? 0.6 : 1,
            cursor: 'pointer' as any,
          },
        ]}>
        <View
          pointerEvents="none"
          style={[
            styles.handle,
            { backgroundColor: isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#64748B' : '#94A3B8') },
          ]}
        />
      </Pressable>

      {/* Header Row: Place Title, Distance, and Window Controls */}
      <View style={styles.headerRow}>
        <Pressable
          onPress={handleToggle}
          style={({ pressed }) => [
            styles.titleArea,
            {
              cursor: 'pointer' as any,
              opacity: pressed ? 0.8 : 1,
            },
          ]}
          accessible
          accessibilityRole="button"
          accessibilityLabel={`Miejsce ${place.name}, kliknij aby ${isExpanded ? 'zwinąć' : 'rozwinąć'}`}>
          <Text
            numberOfLines={1}
            style={[
              styles.placeTitle,
              {
                color: isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#F8FAFC' : '#0F172A'),
                fontWeight: isHighContrast ? '900' : '800',
              },
            ]}>
            {place.name}
          </Text>
          <Text
            numberOfLines={1}
            style={[
              styles.placeSubtitle,
              {
                color: isHighContrast ? (isDark ? '#E2E8F0' : '#1E293B') : (isDark ? '#94A3B8' : '#64748B'),
                fontWeight: isHighContrast ? '600' : '400',
              },
            ]}>
            {distanceText} stąd · {place.address}
          </Text>
        </Pressable>

        {/* Action icons / Controls */}
        <View style={styles.controlsRow}>
          <Pressable
            onPress={() => toggleSavePlace(place.id)}
            accessible
            accessibilityRole="button"
            accessibilityLabel={isSaved ? 'Usuń z zapisanych' : 'Zapisz miejsce'}
            style={({ pressed }) => [
              styles.iconControlBtn,
              {
                borderColor: isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#475569' : '#CBD5E1'),
                borderWidth: isHighContrast ? 2 : 1,
                backgroundColor: isSaved
                  ? (isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#0369A1' : BrandColors.primaryLight))
                  : (isDark ? '#334155' : '#FFFFFF'),
                cursor: 'pointer' as any,
                opacity: pressed ? 0.7 : 1,
              },
            ]}>
            <MaterialCommunityIcons
              name={isSaved ? 'bookmark' : 'bookmark-outline'}
              size={20}
              color={
                isSaved && isHighContrast
                  ? (isDark ? '#000000' : '#FFFFFF')
                  : isHighContrast
                  ? (isDark ? '#FFFFFF' : '#000000')
                  : (isSaved && isDark ? '#FFFFFF' : isDark ? '#94A3B8' : BrandColors.primary)
              }
            />
          </Pressable>

          {/* Toggle Expand / Collapse Chevron */}
          <Pressable
            onPress={handleToggle}
            accessible
            accessibilityRole="button"
            accessibilityLabel={isExpanded ? 'Zwiń kartę' : 'Rozwiń kartę'}
            style={({ pressed }) => [
              styles.iconControlBtn,
              {
                borderColor: isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#475569' : '#CBD5E1'),
                borderWidth: isHighContrast ? 2 : 1,
                backgroundColor: isDark ? '#334155' : '#F1F5F9',
                cursor: 'pointer' as any,
                opacity: pressed ? 0.7 : 1,
              },
            ]}>
            <MaterialCommunityIcons
              name={isExpanded ? 'chevron-down' : 'chevron-up'}
              size={24}
              color={isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#F8FAFC' : '#1E293B')}
            />
          </Pressable>

          {/* Close / Dismiss Button */}
          {onClose && (
            <Pressable
              onPress={onClose}
              accessible
              accessibilityRole="button"
              accessibilityLabel="Zamknij kartę miejsca"
              style={({ pressed }) => [
                styles.iconControlBtn,
                {
                  borderColor: isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#475569' : '#CBD5E1'),
                  borderWidth: isHighContrast ? 2 : 1,
                  backgroundColor: isDark ? '#334155' : '#F1F5F9',
                  cursor: 'pointer' as any,
                  opacity: pressed ? 0.7 : 1,
                },
              ]}>
              <MaterialCommunityIcons
                name="close"
                size={20}
                color={isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#F8FAFC' : '#0F172A')}
              />
            </Pressable>
          )}
        </View>
      </View>

      {/* COMPACT PEEK STATE: Quick Navigation Button */}
      {!isExpanded && (
        <View style={styles.compactActionsRow}>
          <AccessibleButton
            label="Trasa"
            icon="navigation-variant"
            variant="primary"
            onPress={() => onNavigateToRoute(place)}
            style={styles.compactBtn}
          />
          <AccessibleButton
            label="Rozwiń szczegóły"
            variant="secondary"
            onPress={handleToggle}
            style={styles.compactBtn}
          />
        </View>
      )}

      {/* EXPANDED CONTENT: Full scorecard, checklist & details */}
      {isExpanded && (
        <View style={styles.expandedContent}>
          {/* Confidence status banner */}
          <View
            style={[
              styles.statusBanner,
              {
                backgroundColor: isHighContrast
                  ? (isDark ? '#064E3B' : '#E2F1EE')
                  : (isDark ? '#064E3B' : '#E6F5F3'),
                borderColor: isHighContrast
                  ? (isDark ? '#5EEAD4' : '#005A4E')
                  : (isDark ? '#059669' : '#B2DFDB'),
                borderWidth: isHighContrast ? 2 : 1,
              },
            ]}>
            <MaterialCommunityIcons
              name="account-group-outline"
              size={18}
              color={isDark ? '#2DD4BF' : BrandColors.accentTeal}
            />
            <Text
              style={[
                styles.statusText,
                {
                  color: isHighContrast
                    ? (isDark ? '#FFFFFF' : '#004D40')
                    : (isDark ? '#6EE7B7' : '#00796B'),
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
                      ? (isDark ? '#FFFFFF' : '#000000')
                      : fact.status === 'verified'
                      ? (isDark ? '#4ADE80' : BrandColors.success)
                      : (isDark ? '#2DD4BF' : BrandColors.accentTeal)
                  }
                />
                <Text
                  style={[
                    styles.factLabel,
                    {
                      color: isHighContrast
                        ? (isDark ? '#FFFFFF' : '#000000')
                        : (isDark ? '#F1F5F9' : '#1E293B'),
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
                color: isHighContrast
                  ? (isDark ? '#CBD5E1' : '#111827')
                  : (isDark ? '#94A3B8' : '#64748B'),
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
              color={isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#2DD4BF' : BrandColors.accentTeal)}
            />
            <Text
              style={[
                styles.addUpdateText,
                {
                  color: isHighContrast
                    ? (isDark ? '#FFFFFF' : '#000000')
                    : (isDark ? '#2DD4BF' : BrandColors.accentTeal),
                  fontWeight: isHighContrast ? '800' : '700',
                },
              ]}>
              {isGuest
                ? 'Zaloguj się, aby dodać aktualizację dostępności'
                : 'Dodaj aktualizację dostępności'}
            </Text>
          </Pressable>
        </View>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  sheetContainer: {
    borderTopLeftRadius: 28,
    borderTopRightRadius: 28,
    paddingHorizontal: 20,
    paddingBottom: 20,
    paddingTop: 4,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: -4 },
    shadowOpacity: 0.15,
    shadowRadius: 10,
    elevation: 12,
  },
  handleContainer: {
    alignItems: 'center',
    justifyContent: 'center',
    width: '100%',
    paddingVertical: 14,
    cursor: 'pointer' as any,
  },
  handle: {
    width: 54,
    height: 6,
    borderRadius: 3,
  },
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 6,
  },
  titleArea: {
    flex: 1,
    marginRight: 8,
    cursor: 'pointer' as any,
  },
  placeTitle: {
    fontSize: 22,
    letterSpacing: -0.3,
  },
  placeSubtitle: {
    fontSize: 13,
    marginTop: 2,
  },
  controlsRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  iconControlBtn: {
    width: 40,
    height: 40,
    borderRadius: 12,
    justifyContent: 'center',
    alignItems: 'center',
    cursor: 'pointer' as any,
  },
  compactActionsRow: {
    flexDirection: 'row',
    gap: 10,
    marginTop: 8,
  },
  compactBtn: {
    flex: 1,
    minHeight: 44,
    paddingVertical: 8,
  },
  expandedContent: {
    marginTop: 4,
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
    marginVertical: 8,
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
    paddingVertical: 8,
    marginTop: 2,
  },
  addUpdateText: {
    fontSize: 14,
  },
});
