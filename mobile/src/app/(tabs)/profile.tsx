import React from 'react';
import { View, Text, StyleSheet, ScrollView, Pressable, Switch } from 'react-native';
import { useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { AccessibleButton } from '@/components/AccessibleButton';
import { BrandColors, Spacing, MaxContentWidth } from '@/constants/theme';
import {
  useAccessibility,
  ACCESSIBILITY_PRESETS,
  ConstraintLevel,
} from '@/context/AccessibilityContext';

type MaterialIconName = keyof typeof MaterialCommunityIcons.glyphMap;

export default function ProfileScreen() {
  const router = useRouter();
  const {
    isDark,
    toggleDarkTheme,
    isHighContrast,
    toggleHighContrast,
    isGuest,
    user,
    points,
    activePresetId,
    constraints,
    feedbackChannels,
    applyPreset,
    setConstraint,
    toggleFeedbackChannel,
    logout,
  } = useAccessibility();

  const constraintItems = [
    { key: 'stairs', label: 'Schody i stopnie' },
    { key: 'curb', label: 'Wysokie krawężniki' },
    { key: 'rough_surface', label: 'Nierówny bruk / kocie łby' },
    { key: 'steep_slope', label: 'Strome podjazdy (>6%)' },
    { key: 'elevator_unavailable', label: 'Brak windy na peronach' },
    { key: 'crowding', label: 'Zatłoczone przejścia' },
    { key: 'missing_audio_signal', label: 'Brak sygnalizacji dźwiękowej' },
  ];

  const cycleConstraint = (key: string) => {
    const current = constraints[key] || 'allowed';
    let next: ConstraintLevel = 'allowed';
    if (current === 'allowed') next = 'prefer_avoid';
    else if (current === 'prefer_avoid') next = 'must_avoid';
    else next = 'allowed';

    setConstraint(key, next);
  };

  const getConstraintBadge = (level: ConstraintLevel) => {
    if (level === 'must_avoid') {
      return {
        text: 'Niedozwolone',
        bg: isDark ? '#7F1D1D' : '#FEE2E2',
        color: isDark ? '#FCA5A5' : '#DC2626',
        icon: 'cancel' as MaterialIconName,
      };
    }
    if (level === 'prefer_avoid') {
      return {
        text: 'Unikaj',
        bg: isDark ? '#78350F' : '#FEF3C7',
        color: isDark ? '#FCD34D' : '#D97706',
        icon: 'alert-circle-outline' as MaterialIconName,
      };
    }
    return {
      text: 'Dozwolone',
      bg: isDark ? '#14532D' : '#DCFCE7',
      color: isDark ? '#86EFAC' : '#16A34A',
      icon: 'check-circle-outline' as MaterialIconName,
    };
  };

  const cardBorderColor = isHighContrast
    ? (isDark ? '#FFFFFF' : '#000000')
    : (isDark ? '#334155' : '#E2E8F0');
  const cardBgColor = isHighContrast
    ? (isDark ? '#000000' : '#FFFFFF')
    : (isDark ? '#1E293B' : '#FFFFFF');
  const dividerColor = isDark ? '#334155' : '#F1F5F9';

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
      <ScrollView contentContainerStyle={styles.container}>
        <View style={styles.contentWrapper}>
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
            Profil i dostępność
          </Text>

          {/* User Account Card */}
          <View
            style={[
              styles.userCard,
              {
                backgroundColor: cardBgColor,
                borderColor: cardBorderColor,
                borderWidth: isHighContrast ? 2.5 : 1,
              },
            ]}>
            <View style={styles.userInfoRow}>
              <View
                style={[
                  styles.avatarCircle,
                  {
                    backgroundColor: isHighContrast
                      ? (isDark ? '#38BDF8' : '#000000')
                      : BrandColors.primary,
                  },
                ]}>
                <MaterialCommunityIcons name="account" size={32} color="#FFFFFF" />
              </View>
              <View style={styles.userNameBlock}>
                <Text
                  style={[
                    styles.userName,
                    {
                      color: isHighContrast
                        ? (isDark ? '#FFFFFF' : '#000000')
                        : (isDark ? '#F8FAFC' : '#0F172A'),
                      fontWeight: isHighContrast ? '800' : '700',
                    },
                  ]}>
                  {isGuest ? 'Użytkownik Gość' : user?.name}
                </Text>
                <Text
                  style={[
                    styles.userEmail,
                    {
                      color: isHighContrast
                        ? (isDark ? '#CBD5E1' : '#1E293B')
                        : (isDark ? '#94A3B8' : '#64748B'),
                    },
                  ]}>
                  {isGuest
                    ? 'Ustawienia zapisywane tylko na urządzeniu'
                    : user?.email}
                </Text>
              </View>
            </View>

            {isGuest ? (
              <AccessibleButton
                label="Zaloguj się lub utwórz konto"
                variant="guest"
                onPress={() => router.push('/auth/login')}
                style={styles.authBtn}
              />
            ) : (
              <AccessibleButton
                label="Wyloguj się"
                variant="secondary"
                onPress={logout}
                style={styles.authBtn}
              />
            )}
          </View>

          {/* Theme & Appearance Section */}
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
            Wygląd i motyw
          </Text>

          <View
            style={[
              styles.channelsCard,
              {
                backgroundColor: cardBgColor,
                borderColor: cardBorderColor,
                borderWidth: isHighContrast ? 2.5 : 1,
              },
            ]}>
            {/* Dark Mode Switch */}
            <View style={styles.channelRow}>
              <View style={styles.themeRowLeft}>
                <View
                  style={[
                    styles.themeIconCircle,
                    { backgroundColor: isDark ? '#334155' : '#FEF3C7' },
                  ]}>
                  <MaterialCommunityIcons
                    name={isDark ? 'weather-night' : 'white-balance-sunny'}
                    size={20}
                    color={isDark ? '#38BDF8' : '#D97706'}
                  />
                </View>
                <View style={styles.themeTextWrap}>
                  <Text
                    style={[
                      styles.channelLabel,
                      {
                        color: isHighContrast
                          ? (isDark ? '#FFFFFF' : '#000000')
                          : (isDark ? '#F8FAFC' : '#1E293B'),
                      },
                    ]}>
                    Tryb ciemny
                  </Text>
                  <Text
                    style={[
                      styles.themeSubtitle,
                      {
                        color: isHighContrast
                          ? (isDark ? '#CBD5E1' : '#1E293B')
                          : (isDark ? '#94A3B8' : '#64748B'),
                      },
                    ]}>
                    {isDark ? 'Włączony — oszczędza wzrok i baterię' : 'Wyłączony — jasny motyw'}
                  </Text>
                </View>
              </View>
              <Switch
                value={isDark}
                onValueChange={toggleDarkTheme}
                trackColor={{
                  false: isDark ? '#334155' : '#D1D5DB',
                  true: isHighContrast ? '#38BDF8' : BrandColors.primary,
                }}
                thumbColor="#FFFFFF"
                accessibilityLabel="Przełącz ciemny motyw"
              />
            </View>

            {/* High Contrast Switch */}
            <View
              style={[
                styles.channelRow,
                { borderTopWidth: 1, borderTopColor: dividerColor },
              ]}>
              <View style={styles.themeRowLeft}>
                <View
                  style={[
                    styles.themeIconCircle,
                    { backgroundColor: isDark ? '#334155' : '#E6F5F3' },
                  ]}>
                  <MaterialCommunityIcons
                    name="eye-outline"
                    size={20}
                    color={isDark ? '#2DD4BF' : '#008779'}
                  />
                </View>
                <View style={styles.themeTextWrap}>
                  <Text
                    style={[
                      styles.channelLabel,
                      {
                        color: isHighContrast
                          ? (isDark ? '#FFFFFF' : '#000000')
                          : (isDark ? '#F8FAFC' : '#1E293B'),
                      },
                    ]}>
                    Wysoki kontrast (WCAG AAA)
                  </Text>
                  <Text
                    style={[
                      styles.themeSubtitle,
                      {
                        color: isHighContrast
                          ? (isDark ? '#CBD5E1' : '#1E293B')
                          : (isDark ? '#94A3B8' : '#64748B'),
                      },
                    ]}>
                    {isHighContrast ? 'Włączony — wyraziste ramki i gruby tekst' : 'Wyłączony'}
                  </Text>
                </View>
              </View>
              <Switch
                value={isHighContrast}
                onValueChange={toggleHighContrast}
                trackColor={{
                  false: isDark ? '#334155' : '#D1D5DB',
                  true: '#008779',
                }}
                thumbColor="#FFFFFF"
                accessibilityLabel="Przełącz wysoki kontrast"
              />
            </View>
          </View>

          {/* Points & Community Rewards Card */}
          <View
            style={[
              styles.rewardsCard,
              {
                backgroundColor: isHighContrast
                  ? (isDark ? '#451A03' : '#FFFBEB')
                  : (isDark ? '#1E293B' : '#FEF3C7'),
                borderColor: isHighContrast
                  ? (isDark ? '#F59E0B' : '#000000')
                  : (isDark ? '#B45309' : '#FDE68A'),
                borderWidth: isHighContrast ? 2.5 : 1,
              },
            ]}>
            <View style={styles.rewardsHeader}>
              <MaterialCommunityIcons name="star-circle" size={26} color="#F59E0B" />
              <Text
                style={[
                  styles.rewardsTitle,
                  {
                    color: isHighContrast
                      ? (isDark ? '#FDE68A' : '#000000')
                      : (isDark ? '#FCD34D' : '#78350F'),
                    fontWeight: isHighContrast ? '800' : '700',
                  },
                ]}>
                Punkty Społeczności: {points} pkt
              </Text>
            </View>
            <Text
              style={[
                styles.rewardsDesc,
                {
                  color: isHighContrast
                    ? (isDark ? '#FDE68A' : '#1E293B')
                    : (isDark ? '#CBD5E1' : '#92400E'),
                },
              ]}>
              Zdobywaj punkty za zgłaszanie barier i potwierdzanie dostępności.
              Wymieniaj na cyfrowe zniżki MPK oraz bilety partnerów w Krakowie.
            </Text>
          </View>

          {/* Accessibility Presets */}
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
            Szybkie profile potrzeb
          </Text>

          <View style={styles.presetsGrid}>
            {ACCESSIBILITY_PRESETS.map((preset) => {
              const isSelected = activePresetId === preset.id;
              return (
                <Pressable
                  key={preset.id}
                  onPress={() => applyPreset(preset.id)}
                  accessible
                  accessibilityRole="button"
                  accessibilityLabel={`Profil: ${preset.name}, ${isSelected ? 'aktywny' : 'wybierz'}`}
                  style={[
                    styles.presetCard,
                    {
                      backgroundColor: isSelected
                        ? (isHighContrast ? (isDark ? '#0284C7' : '#000000') : (isDark ? '#0369A1' : BrandColors.primaryLight))
                        : cardBgColor,
                      borderColor: isSelected
                        ? (isHighContrast ? '#38BDF8' : BrandColors.primary)
                        : cardBorderColor,
                      borderWidth: isSelected ? 2.5 : (isHighContrast ? 2 : 1),
                    },
                  ]}>
                  <MaterialCommunityIcons
                    name={preset.icon as MaterialIconName}
                    size={24}
                    color={
                      isSelected
                        ? (isDark || isHighContrast ? '#FFFFFF' : BrandColors.primary)
                        : (isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#94A3B8' : '#475569'))
                    }
                  />
                  <Text
                    style={[
                      styles.presetName,
                      {
                        color: isSelected
                          ? (isDark || isHighContrast ? '#FFFFFF' : BrandColors.primaryDark)
                          : (isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#F8FAFC' : '#1E293B')),
                        fontWeight: isSelected || isHighContrast ? '800' : '600',
                      },
                    ]}>
                    {preset.name}
                  </Text>
                </Pressable>
              );
            })}
          </View>

          {/* Fine-grained Routing Constraints (Tri-state) */}
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
            Ograniczenia trasy (trójstanowe)
          </Text>
          <Text
            style={[
              styles.sectionSubtitle,
              {
                color: isHighContrast
                  ? (isDark ? '#CBD5E1' : '#1E293B')
                  : (isDark ? '#94A3B8' : '#64748B'),
              },
            ]}>
            Kliknij na status, aby przełączać: Dozwolone → Unikaj → Niedozwolone.
          </Text>

          <View
            style={[
              styles.constraintsList,
              {
                backgroundColor: cardBgColor,
                borderColor: cardBorderColor,
                borderWidth: isHighContrast ? 2.5 : 1,
              },
            ]}>
            {constraintItems.map((item, index) => {
              const level = constraints[item.key] || 'allowed';
              const badge = getConstraintBadge(level);

              return (
                <View
                  key={item.key}
                  style={[
                    styles.constraintRow,
                    index < constraintItems.length - 1 && {
                      borderBottomWidth: 1,
                      borderBottomColor: dividerColor,
                    },
                  ]}>
                  <Text
                    style={[
                      styles.constraintLabel,
                      {
                        color: isHighContrast
                          ? (isDark ? '#FFFFFF' : '#000000')
                          : (isDark ? '#F8FAFC' : '#1E293B'),
                        fontWeight: isHighContrast ? '700' : '500',
                      },
                    ]}>
                    {item.label}
                  </Text>

                  <Pressable
                    onPress={() => cycleConstraint(item.key)}
                    accessible
                    accessibilityRole="button"
                    accessibilityLabel={`${item.label}: ${badge.text}. Kliknij, aby zmienić.`}
                    style={[
                      styles.badgePressable,
                      {
                        backgroundColor: isHighContrast
                          ? (isDark ? '#000000' : '#FFFFFF')
                          : badge.bg,
                        borderColor: isHighContrast
                          ? (isDark ? '#FFFFFF' : '#000000')
                          : badge.color,
                        borderWidth: isHighContrast ? 2 : 1,
                      },
                    ]}>
                    <MaterialCommunityIcons
                      name={badge.icon}
                      size={16}
                      color={isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : badge.color}
                    />
                    <Text
                      style={[
                        styles.badgeText,
                        {
                          color: isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : badge.color,
                          fontWeight: isHighContrast ? '800' : '700',
                        },
                      ]}>
                      {badge.text}
                    </Text>
                  </Pressable>
                </View>
              );
            })}
          </View>

          {/* Feedback Channels */}
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
            Kanały powiadomień i asysty
          </Text>

          <View
            style={[
              styles.channelsCard,
              {
                backgroundColor: cardBgColor,
                borderColor: cardBorderColor,
                borderWidth: isHighContrast ? 2.5 : 1,
              },
            ]}>
            <View style={styles.channelRow}>
              <Text
                style={[
                  styles.channelLabel,
                  {
                    color: isHighContrast
                      ? (isDark ? '#FFFFFF' : '#000000')
                      : (isDark ? '#F8FAFC' : '#1E293B'),
                  },
                ]}>
                Wskazówki wizualne (wysoki kontrast / piktogramy)
              </Text>
              <Switch
                value={feedbackChannels.includes('visual')}
                onValueChange={() => toggleFeedbackChannel('visual')}
                trackColor={{
                  false: isDark ? '#334155' : '#D1D5DB',
                  true: BrandColors.primary,
                }}
              />
            </View>

            <View
              style={[
                styles.channelRow,
                { borderTopWidth: 1, borderTopColor: dividerColor },
              ]}>
              <Text
                style={[
                  styles.channelLabel,
                  {
                    color: isHighContrast
                      ? (isDark ? '#FFFFFF' : '#000000')
                      : (isDark ? '#F8FAFC' : '#1E293B'),
                  },
                ]}>
                Wskazówki dźwiękowe i lektor (Audio)
              </Text>
              <Switch
                value={feedbackChannels.includes('audio')}
                onValueChange={() => toggleFeedbackChannel('audio')}
                trackColor={{
                  false: isDark ? '#334155' : '#D1D5DB',
                  true: BrandColors.primary,
                }}
              />
            </View>

            <View
              style={[
                styles.channelRow,
                { borderTopWidth: 1, borderTopColor: dividerColor },
              ]}>
              <Text
                style={[
                  styles.channelLabel,
                  {
                    color: isHighContrast
                      ? (isDark ? '#FFFFFF' : '#000000')
                      : (isDark ? '#F8FAFC' : '#1E293B'),
                  },
                ]}>
                Wibracje i haptyka (Haptic)
              </Text>
              <Switch
                value={feedbackChannels.includes('haptic')}
                onValueChange={() => toggleFeedbackChannel('haptic')}
                trackColor={{
                  false: isDark ? '#334155' : '#D1D5DB',
                  true: BrandColors.primary,
                }}
              />
            </View>
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
  container: {
    padding: Spacing.three,
    alignItems: 'center',
  },
  contentWrapper: {
    width: '100%',
    maxWidth: MaxContentWidth,
  },
  headerTitle: {
    fontSize: 28,
    marginBottom: 4,
  },
  userCard: {
    borderRadius: 20,
    padding: 18,
    marginVertical: 10,
  },
  userInfoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
  },
  avatarCircle: {
    width: 52,
    height: 52,
    borderRadius: 26,
    justifyContent: 'center',
    alignItems: 'center',
  },
  userNameBlock: {
    flex: 1,
  },
  userName: {
    fontSize: 18,
  },
  userEmail: {
    fontSize: 13,
    marginTop: 2,
  },
  authBtn: {
    marginTop: 14,
  },
  rewardsCard: {
    borderRadius: 18,
    padding: 16,
    marginVertical: 8,
  },
  rewardsHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    marginBottom: 4,
  },
  rewardsTitle: {
    fontSize: 16,
  },
  rewardsDesc: {
    fontSize: 13,
    lineHeight: 18,
  },
  sectionTitle: {
    fontSize: 18,
    marginTop: 18,
    marginBottom: 4,
  },
  sectionSubtitle: {
    fontSize: 13,
    marginBottom: 10,
  },
  presetsGrid: {
    gap: 8,
    marginBottom: 8,
  },
  presetCard: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 14,
    borderRadius: 14,
    gap: 12,
  },
  presetName: {
    fontSize: 15,
  },
  constraintsList: {
    borderRadius: 18,
    overflow: 'hidden',
    marginBottom: 10,
  },
  constraintRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 12,
  },
  constraintLabel: {
    fontSize: 14,
    flex: 1,
    marginRight: 8,
  },
  badgePressable: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 12,
  },
  badgeText: {
    fontSize: 12,
  },
  borderBottom: {
    borderBottomWidth: 1,
    borderBottomColor: '#F1F5F9',
  },
  borderTop: {
    borderTopWidth: 1,
    borderTopColor: '#F1F5F9',
  },
  channelsCard: {
    borderRadius: 18,
    paddingHorizontal: 16,
    marginBottom: 24,
  },
  channelRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 14,
  },
  channelLabel: {
    fontSize: 14,
    flex: 1,
    marginRight: 10,
    fontWeight: '500',
  },
  themeRowLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    flex: 1,
    marginRight: 10,
  },
  themeIconCircle: {
    width: 36,
    height: 36,
    borderRadius: 18,
    justifyContent: 'center',
    alignItems: 'center',
  },
  themeTextWrap: {
    flex: 1,
  },
  themeSubtitle: {
    fontSize: 12,
    marginTop: 2,
  },
});
