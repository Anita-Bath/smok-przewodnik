import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, Pressable, Alert } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { AccessibilityToggle } from '@/components/AccessibilityToggle';
import { AccessibleInput } from '@/components/AccessibleInput';
import { AccessibleButton } from '@/components/AccessibleButton';
import { BrandColors, Spacing, MaxContentWidth } from '@/constants/theme';
import { KRAKOW_PLACES } from '@/services/krakowData';
import { useAccessibility } from '@/context/AccessibilityContext';

type MaterialIconName = keyof typeof MaterialCommunityIcons.glyphMap;

export default function NewReportScreen() {
  const router = useRouter();
  const { placeId } = useLocalSearchParams<{ placeId?: string }>();
  const { isHighContrast, addPoints } = useAccessibility();

  const place = KRAKOW_PLACES.find((p) => p.id === placeId);

  const [selectedType, setSelectedType] = useState('elevator_broken');
  const [description, setDescription] = useState('');
  const [hasPhoto, setHasPhoto] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const reportTypes = [
    { key: 'elevator_broken', label: 'Awaria windy', icon: 'elevator-down' },
    { key: 'stairs_blocked', label: 'Schody bez podjazdu', icon: 'stairs' },
    { key: 'sidewalk_blocked', label: 'Zastawiony chodnik / koperta', icon: 'car-off' },
    { key: 'rough_surface', label: 'Remont nawierzchni / dziury', icon: 'alert-triangle' },
    { key: 'sound_missing', label: 'Awaria sygnalizacji dźwiękowej', icon: 'volume-off' },
  ];

  const handleSubmit = () => {
    addPoints(15);
    setSubmitted(true);
    setTimeout(() => {
      router.back();
    }, 1800);
  };

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
          accessibilityLabel="Anuluj"
          style={styles.backBtn}>
          <MaterialCommunityIcons
            name="close"
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
          Zgłoś przeszkodę
        </Text>

        <View style={{ width: 24 }} />
      </View>

      <ScrollView contentContainerStyle={styles.scrollContainer}>
        <View style={styles.content}>
          <AccessibilityToggle />

          {submitted ? (
            <View
              style={[
                styles.successCard,
                {
                  backgroundColor: '#DCFCE7',
                  borderColor: isHighContrast ? '#000000' : '#86EFAC',
                  borderWidth: isHighContrast ? 3 : 1,
                },
              ]}>
              <MaterialCommunityIcons
                name="check-decagram"
                size={54}
                color="#15803D"
              />
              <Text style={styles.successTitle}>Dziękujemy za zgłoszenie!</Text>
              <Text style={styles.successSubtitle}>
                Twoja obserwacja pomoże innym mieszkańcom Krakowa.
                Otrzymujesz +15 Punktów Społeczności!
              </Text>
            </View>
          ) : (
            <>
              {/* Context target banner */}
              {place && (
                <View
                  style={[
                    styles.targetCard,
                    {
                      backgroundColor: '#FFFFFF',
                      borderColor: isHighContrast ? '#000000' : '#E2E8F0',
                      borderWidth: isHighContrast ? 2 : 1,
                    },
                  ]}>
                  <Text style={styles.targetLabel}>Lokalizacja zgłoszenia:</Text>
                  <Text style={styles.targetName}>
                    {place.name} ({place.address})
                  </Text>
                </View>
              )}

              {/* Type selection */}
              <Text
                style={[
                  styles.sectionTitle,
                  {
                    color: isHighContrast ? '#000000' : '#0F172A',
                    fontWeight: isHighContrast ? '900' : '800',
                  },
                ]}>
                Czego dotyczy zgłoszenie?
              </Text>

              <View style={styles.typesList}>
                {reportTypes.map((type) => {
                  const isSelected = selectedType === type.key;
                  return (
                    <Pressable
                      key={type.key}
                      onPress={() => setSelectedType(type.key)}
                      accessible
                      accessibilityRole="radio"
                      accessibilityState={{ selected: isSelected }}
                      style={[
                        styles.typeItem,
                        {
                          backgroundColor: isSelected
                            ? (isHighContrast ? '#000000' : BrandColors.primaryLight)
                            : '#FFFFFF',
                          borderColor: isSelected
                            ? (isHighContrast ? '#000000' : BrandColors.primary)
                            : (isHighContrast ? '#000000' : '#CBD5E1'),
                          borderWidth: isSelected ? 2.5 : (isHighContrast ? 2 : 1),
                        },
                      ]}>
                      <MaterialCommunityIcons
                        name={type.icon as MaterialIconName}
                        size={22}
                        color={
                          isSelected
                            ? (isHighContrast ? '#FFFFFF' : BrandColors.primary)
                            : (isHighContrast ? '#000000' : '#475569')
                        }
                      />
                      <Text
                        style={[
                          styles.typeLabel,
                          {
                            color: isSelected
                              ? (isHighContrast ? '#FFFFFF' : BrandColors.primary)
                              : (isHighContrast ? '#000000' : '#1E293B'),
                            fontWeight: isSelected || isHighContrast ? '800' : '600',
                          },
                        ]}>
                        {type.label}
                      </Text>
                    </Pressable>
                  );
                })}
              </View>

              {/* Description Input */}
              <AccessibleInput
                label="Dodatkowy opis sytuacji"
                placeholder="Np. Winda przy peronie 2 nie reaguje na przycisk..."
                multiline
                numberOfLines={3}
                value={description}
                onChangeText={setDescription}
              />

              {/* Photo Simulation */}
              <Pressable
                onPress={() => setHasPhoto(!hasPhoto)}
                accessible
                accessibilityRole="button"
                accessibilityLabel="Dołącz zdjęcie przeszkody"
                style={[
                  styles.photoButton,
                  {
                    backgroundColor: hasPhoto ? '#E6F5F3' : '#FFFFFF',
                    borderColor: hasPhoto
                      ? BrandColors.accentTeal
                      : (isHighContrast ? '#000000' : '#CBD5E1'),
                    borderWidth: isHighContrast ? 2 : 1.5,
                  },
                ]}>
                <MaterialCommunityIcons
                  name={hasPhoto ? 'camera' : 'camera-outline'}
                  size={24}
                  color={hasPhoto ? BrandColors.accentTeal : '#64748B'}
                />
                <Text
                  style={[
                    styles.photoText,
                    {
                      color: hasPhoto ? BrandColors.accentTeal : '#475569',
                      fontWeight: hasPhoto || isHighContrast ? '700' : '500',
                    },
                  ]}>
                  {hasPhoto ? '✓ Zdjęcie dołączone (dowód)' : 'Dołącz zdjęcie (opcjonalnie)'}
                </Text>
              </Pressable>

              {/* Submit Button */}
              <AccessibleButton
                label="Wyślij zgłoszenie (+15 pkt)"
                variant="primary"
                onPress={handleSubmit}
                style={styles.submitBtn}
              />
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
  targetCard: {
    borderRadius: 14,
    padding: 12,
    marginVertical: 8,
  },
  targetLabel: {
    fontSize: 12,
    color: '#64748B',
  },
  targetName: {
    fontSize: 15,
    fontWeight: '700',
    marginTop: 2,
    color: '#0F172A',
  },
  sectionTitle: {
    fontSize: 18,
    marginTop: 14,
    marginBottom: 10,
  },
  typesList: {
    gap: 8,
    marginBottom: 14,
  },
  typeItem: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 14,
    borderRadius: 16,
    gap: 12,
  },
  typeLabel: {
    fontSize: 15,
  },
  photoButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 10,
    padding: 16,
    borderRadius: 16,
    marginVertical: 12,
  },
  photoText: {
    fontSize: 15,
  },
  submitBtn: {
    marginTop: 8,
    marginBottom: 24,
  },
  successCard: {
    alignItems: 'center',
    padding: 28,
    borderRadius: 24,
    marginTop: 40,
  },
  successTitle: {
    fontSize: 22,
    fontWeight: '800',
    color: '#15803D',
    marginTop: 14,
    textAlign: 'center',
  },
  successSubtitle: {
    fontSize: 15,
    color: '#166534',
    textAlign: 'center',
    marginTop: 8,
    lineHeight: 22,
  },
});
