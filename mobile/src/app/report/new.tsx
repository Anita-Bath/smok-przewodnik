import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, Pressable, Alert } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import { AccessibilityToggle } from '@/components/AccessibilityToggle';
import { AccessibleInput } from '@/components/AccessibleInput';
import { AccessibleButton } from '@/components/AccessibleButton';
import { BrandColors, Spacing, MaxContentWidth } from '@/constants/theme';
import { KRAKOW_PLACES } from '@/services/krakowData';
import { useAccessibility } from '@/context/AccessibilityContext';
import { supabase } from '@/services/supabase';

type MaterialIconName = keyof typeof MaterialCommunityIcons.glyphMap;

export default function NewReportScreen() {
  const router = useRouter();
  const { placeId } = useLocalSearchParams<{ placeId?: string }>();
  const { isDark, isHighContrast, addPoints } = useAccessibility();

  const place = KRAKOW_PLACES.find((p) => p.id === placeId);

  const [selectedType, setSelectedType] = useState('elevator_broken');
  const [description, setDescription] = useState('');
  const [hasPhoto, setHasPhoto] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const reportTypes = [
    { key: 'elevator_broken', label: 'Awaria windy', icon: 'elevator-down' },
    { key: 'stairs_blocked', label: 'Schody bez podjazdu', icon: 'stairs' },
    { key: 'sidewalk_blocked', label: 'Zastawiony chodnik / koperta', icon: 'car-off' },
    { key: 'rough_surface', label: 'Remont nawierzchni / dziury', icon: 'alert' },
    { key: 'sound_missing', label: 'Awaria sygnalizacji dźwiękowej', icon: 'volume-off' },
  ];

  const handleSubmit = async () => {
    try {
      const { data: { session } } = await supabase.auth.getSession();
      if (!session) {
        alert('Musisz być zalogowany aby dodać zgłoszenie');
        return;
      }
      if (placeId) {
        await fetch(`${process.env.EXPO_PUBLIC_API_URL}/spatials/places/${placeId}/accessibility`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${session.access_token}`
          },
          body: JSON.stringify({
            AttributeCode: selectedType,
            Value: true
          })
        });
      }

      addPoints(15);
      setSubmitted(true);
      setTimeout(() => {
        router.back();
      }, 1800);
    } catch (e) {
      console.error(e);
      alert('Wystąpił błąd podczas wysyłania zgłoszenia.');
    }
  };

  const handlePickPhoto = async () => {
    if (hasPhoto) {
      setHasPhoto(false);
      return;
    }
    
    try {
      const result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: ['images'],
        allowsEditing: true,
        aspect: [4, 3],
        quality: 1,
      });

      if (!result.canceled) {
        setHasPhoto(true);
      }
    } catch (e) {
      console.error('Błąd podczas wybierania zdjęcia:', e);
      Alert.alert('Błąd', 'Nie udało się otworzyć galerii.');
    }
  };

  const cardBg = isHighContrast
    ? (isDark ? '#000000' : '#FFFFFF')
    : (isDark ? '#1E293B' : '#FFFFFF');
  const cardBorder = isHighContrast
    ? (isDark ? '#FFFFFF' : '#000000')
    : (isDark ? '#334155' : '#E2E8F0');

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
          accessibilityLabel="Anuluj"
          style={styles.backBtn}>
          <MaterialCommunityIcons
            name="close"
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
                  backgroundColor: isDark ? '#064E3B' : '#DCFCE7',
                  borderColor: isHighContrast
                    ? (isDark ? '#34D399' : '#000000')
                    : (isDark ? '#059669' : '#86EFAC'),
                  borderWidth: isHighContrast ? 3 : 1,
                },
              ]}>
              <MaterialCommunityIcons
                name="check-decagram"
                size={54}
                color={isDark ? '#34D399' : '#15803D'}
              />
              <Text
                style={[
                  styles.successTitle,
                  { color: isDark ? '#86EFAC' : '#15803D' },
                ]}>
                Dziękujemy za zgłoszenie!
              </Text>
              <Text
                style={[
                  styles.successSubtitle,
                  { color: isDark ? '#D1FAE5' : '#166534' },
                ]}>
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
                      backgroundColor: cardBg,
                      borderColor: cardBorder,
                      borderWidth: isHighContrast ? 2 : 1,
                    },
                  ]}>
                  <Text
                    style={[
                      styles.targetLabel,
                      { color: isDark ? '#94A3B8' : '#64748B' },
                    ]}>
                    Lokalizacja zgłoszenia:
                  </Text>
                  <Text
                    style={[
                      styles.targetName,
                      {
                        color: isHighContrast
                          ? (isDark ? '#FFFFFF' : '#000000')
                          : (isDark ? '#F8FAFC' : '#0F172A'),
                      },
                    ]}>
                    {place.name} ({place.address})
                  </Text>
                </View>
              )}

              {/* Type selection */}
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
                            ? (isHighContrast
                                ? (isDark ? '#0284C7' : '#000000')
                                : (isDark ? '#0369A1' : BrandColors.primaryLight))
                            : cardBg,
                          borderColor: isSelected
                            ? (isHighContrast ? '#38BDF8' : BrandColors.primary)
                            : cardBorder,
                          borderWidth: isSelected ? 2.5 : (isHighContrast ? 2 : 1),
                        },
                      ]}>
                      <MaterialCommunityIcons
                        name={type.icon as MaterialIconName}
                        size={22}
                        color={
                          isSelected
                            ? (isDark || isHighContrast ? '#FFFFFF' : BrandColors.primary)
                            : (isHighContrast
                                ? (isDark ? '#FFFFFF' : '#000000')
                                : (isDark ? '#94A3B8' : '#475569'))
                        }
                      />
                      <Text
                        style={[
                          styles.typeLabel,
                          {
                            color: isSelected
                              ? (isDark || isHighContrast ? '#FFFFFF' : BrandColors.primaryDark)
                              : (isHighContrast
                                  ? (isDark ? '#FFFFFF' : '#000000')
                                  : (isDark ? '#F8FAFC' : '#1E293B')),
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
                onPress={handlePickPhoto}
                accessible
                accessibilityRole="button"
                accessibilityLabel="Dołącz zdjęcie przeszkody"
                style={[
                  styles.photoButton,
                  {
                    backgroundColor: hasPhoto
                      ? (isDark ? '#064E3B' : '#E6F5F3')
                      : cardBg,
                    borderColor: hasPhoto
                      ? (isDark ? '#34D399' : BrandColors.accentTeal)
                      : cardBorder,
                    borderWidth: isHighContrast ? 2 : 1.5,
                  },
                ]}>
                <MaterialCommunityIcons
                  name={hasPhoto ? 'camera' : 'camera-outline'}
                  size={24}
                  color={
                    hasPhoto
                      ? (isDark ? '#34D399' : BrandColors.accentTeal)
                      : (isDark ? '#94A3B8' : '#64748B')
                  }
                />
                <Text
                  style={[
                    styles.photoText,
                    {
                      color: hasPhoto
                        ? (isDark ? '#86EFAC' : BrandColors.accentTeal)
                        : (isHighContrast
                            ? (isDark ? '#FFFFFF' : '#000000')
                            : (isDark ? '#CBD5E1' : '#475569')),
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
