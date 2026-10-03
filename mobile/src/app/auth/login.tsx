import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, Pressable } from 'react-native';
import { useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { DragonLogo } from '@/components/DragonLogo';
import { AccessibilityToggle } from '@/components/AccessibilityToggle';
import { AccessibleInput } from '@/components/AccessibleInput';
import { AccessibleButton } from '@/components/AccessibleButton';
import { BrandColors, Spacing, MaxContentWidth } from '@/constants/theme';
import { useAccessibility } from '@/context/AccessibilityContext';

export default function LoginScreen() {
  const router = useRouter();
  const { isDark, isHighContrast, login, loginAsGuest } = useAccessibility();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleLogin = () => {
    login(email || 'jan.kowalski@krakow.pl', 'Jan Kowalski');
    router.replace('/(tabs)');
  };

  const handleGuestLogin = () => {
    loginAsGuest();
    router.replace('/(tabs)');
  };

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
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled">
        <View style={styles.card}>
          {/* Header */}
          <View style={styles.header}>
            <DragonLogo size={48} showText={true} />
          </View>

          {/* Accessibility Toggle */}
          <AccessibilityToggle />

          {/* Graphic / Route Preview Placeholder */}
          <View
            style={[
              styles.illustrationBox,
              {
                backgroundColor: isHighContrast
                  ? (isDark ? '#082F49' : '#E5EBF2')
                  : (isDark ? '#1E293B' : '#E8F4F1'),
                borderColor: isHighContrast
                  ? (isDark ? '#38BDF8' : '#000000')
                  : (isDark ? '#334155' : '#D1E6E1'),
                borderWidth: isHighContrast ? 2 : 1,
              },
            ]}>
            <View style={styles.routePinStart} />
            <View
              style={[
                styles.routeLine,
                {
                  backgroundColor: isHighContrast
                    ? (isDark ? '#38BDF8' : '#000000')
                    : BrandColors.primary,
                },
              ]}
            />
            <View style={styles.routePinEnd}>
              <View style={styles.routePinDot} />
            </View>
            <View
              style={[
                styles.badgeStepFree,
                {
                  backgroundColor: isHighContrast
                    ? (isDark ? '#000000' : '#FFFFFF')
                    : (isDark ? '#0F172A' : '#FFFFFF'),
                  borderColor: isHighContrast
                    ? (isDark ? '#34D399' : '#000000')
                    : (isDark ? '#115E59' : '#B2DFDB'),
                  borderWidth: isHighContrast ? 2 : 1,
                },
              ]}>
              <Text
                style={[
                  styles.badgeStepFreeText,
                  {
                    color: isHighContrast
                      ? (isDark ? '#34D399' : '#000000')
                      : (isDark ? '#2DD4BF' : BrandColors.accentTeal),
                  },
                ]}>
                ✓ Wejście bez schodów
              </Text>
            </View>
          </View>

          {/* Main Title & Subtitle */}
          <Text
            style={[
              styles.title,
              {
                color: isHighContrast
                  ? (isDark ? '#FFFFFF' : '#000000')
                  : (isDark ? '#F8FAFC' : '#0F172A'),
                fontWeight: isHighContrast ? '900' : '800',
              },
            ]}>
            Droga dopasowana do Twoich potrzeb
          </Text>

          <Text
            style={[
              styles.subtitle,
              {
                color: isHighContrast
                  ? (isDark ? '#CBD5E1' : '#1E293B')
                  : (isDark ? '#94A3B8' : '#475569'),
                fontWeight: isHighContrast ? '600' : '400',
              },
            ]}>
            Ty wybierasz miejsce — Smok prowadzi Cię do celu. Bezpiecznie oraz w Twoim tempie.
          </Text>

          {/* Form */}
          <View style={styles.form}>
            <AccessibleInput
              label="Adres e-mail"
              placeholder="mail@example.com"
              keyboardType="email-address"
              autoCapitalize="none"
              value={email}
              onChangeText={setEmail}
            />

            <AccessibleInput
              label="Hasło"
              placeholder="Hasło"
              isPassword
              value={password}
              onChangeText={setPassword}
            />

            <Pressable
              onPress={() => {}}
              accessible
              accessibilityRole="button"
              accessibilityLabel="Przypomnij hasło"
              style={styles.forgotPassword}>
              <Text
                style={[
                  styles.forgotPasswordText,
                  {
                    color: isHighContrast
                      ? (isDark ? '#38BDF8' : '#003366')
                      : (isDark ? '#2DD4BF' : BrandColors.accentTeal),
                  },
                ]}>
                Zapomniałeś hasła?
              </Text>
            </Pressable>

            {/* Submit Action */}
            <AccessibleButton
              label="Logowanie"
              variant="primary"
              onPress={handleLogin}
              style={styles.actionBtn}
            />

            {/* Register Link */}
            <Pressable
              onPress={() => router.push('/auth/register')}
              accessible
              accessibilityRole="button"
              accessibilityLabel="Utwórz nowe konto"
              style={styles.registerLink}>
              <Text
                style={[
                  styles.registerLinkText,
                  {
                    color: isHighContrast
                      ? (isDark ? '#38BDF8' : '#003366')
                      : (isDark ? '#2DD4BF' : BrandColors.accentTeal),
                  },
                ]}>
                Utwórz konto
              </Text>
            </Pressable>

            {/* Guest Login Button */}
            <AccessibleButton
              label="Zaloguj jako gość"
              icon="book-open-outline"
              variant="guest"
              onPress={handleGuestLogin}
              style={styles.guestBtn}
            />

            <Text
              style={[
                styles.guestHint,
                {
                  color: isHighContrast
                    ? (isDark ? '#CBD5E1' : '#1E293B')
                    : (isDark ? '#94A3B8' : '#64748B'),
                },
              ]}>
              Przeglądaj mapę, konto niepotrzebne.
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
  scrollContent: {
    flexGrow: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: Spacing.three,
  },
  card: {
    width: '100%',
    maxWidth: MaxContentWidth,
    borderRadius: 24,
    paddingHorizontal: 8,
  },
  header: {
    marginBottom: Spacing.two,
  },
  illustrationBox: {
    height: 120,
    borderRadius: 20,
    position: 'relative',
    overflow: 'hidden',
    marginVertical: Spacing.two,
    justifyContent: 'center',
    paddingHorizontal: 20,
  },
  routePinStart: {
    position: 'absolute',
    left: 30,
    bottom: 30,
    width: 14,
    height: 14,
    borderRadius: 7,
    backgroundColor: '#0052B4',
    borderWidth: 2,
    borderColor: '#FFFFFF',
  },
  routeLine: {
    position: 'absolute',
    left: 44,
    bottom: 35,
    width: '60%',
    height: 4,
    borderRadius: 2,
  },
  routePinEnd: {
    position: 'absolute',
    right: 50,
    top: 25,
    width: 22,
    height: 22,
    borderRadius: 11,
    backgroundColor: '#0052B4',
    justifyContent: 'center',
    alignItems: 'center',
  },
  routePinDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: '#FFFFFF',
  },
  badgeStepFree: {
    position: 'absolute',
    right: 25,
    bottom: 20,
    paddingHorizontal: 12,
    paddingVertical: 5,
    borderRadius: 14,
  },
  badgeStepFreeText: {
    fontSize: 12,
    fontWeight: '700',
  },
  title: {
    fontSize: 28,
    lineHeight: 34,
    letterSpacing: -0.5,
    marginTop: Spacing.two,
  },
  subtitle: {
    fontSize: 16,
    lineHeight: 22,
    marginTop: 8,
    marginBottom: Spacing.two,
  },
  form: {
    marginTop: 8,
  },
  forgotPassword: {
    alignSelf: 'flex-end',
    paddingVertical: 8,
  },
  forgotPasswordText: {
    fontSize: 14,
    fontWeight: '700',
  },
  actionBtn: {
    marginTop: 12,
  },
  registerLink: {
    alignItems: 'center',
    paddingVertical: 14,
  },
  registerLinkText: {
    fontSize: 16,
    fontWeight: '700',
  },
  guestBtn: {
    marginTop: 4,
  },
  guestHint: {
    textAlign: 'center',
    fontSize: 13,
    marginTop: 8,
    marginBottom: 16,
  },
});
