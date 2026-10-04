import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, Pressable } from 'react-native';
import { useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { DragonLogo } from '@/components/DragonLogo';
import { AccessibilityToggle } from '@/components/AccessibilityToggle';
import { AccessibleInput } from '@/components/AccessibleInput';
import { AccessibleButton } from '@/components/AccessibleButton';
import { BrandColors, Spacing, MaxContentWidth } from '@/constants/theme';
import { useAccessibility } from '@/context/AccessibilityContext';

export default function RegisterScreen() {
  const router = useRouter();
  const { isDark, isHighContrast, register, loginAsGuest } = useAccessibility();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);

  const handleRegister = async () => {
    try {
      setLoading(true);
      await register(email, password, name || 'Użytkownik');
      router.replace('/(tabs)');
    } catch (err: any) {
      alert(err.message || 'Rejestracja nie powiodła się');
    } finally {
      setLoading(false);
    }
  };

  const handleGuest = () => {
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

          {/* Title & Subtitle */}
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
            Zrób mapę Krakowa dla siebie
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
            Zapisuj przydatne miejsca i udostępniaj informacje o dostępności bez zbędnych danych.
          </Text>

          {/* Accessibility Toggle */}
          <AccessibilityToggle />

          {/* Form */}
          <View style={styles.form}>
            <AccessibleInput
              label="Imię"
              placeholder="Jan Kowalski"
              value={name}
              onChangeText={setName}
            />

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
              placeholder="Utwórz hasło"
              hint="Użyj co najmniej 8 znaków."
              isPassword
              value={password}
              onChangeText={setPassword}
            />

            {/* Privacy Shield Box */}
            <View
              style={[
                styles.privacyBox,
                {
                  backgroundColor: isHighContrast
                    ? (isDark ? '#064E3B' : '#E8F5E9')
                    : (isDark ? '#1E293B' : '#E6F5F3'),
                  borderColor: isHighContrast
                    ? (isDark ? '#34D399' : '#000000')
                    : (isDark ? '#334155' : '#B2DFDB'),
                  borderWidth: isHighContrast ? 2 : 1,
                },
              ]}>
              <MaterialCommunityIcons
                name="shield-check-outline"
                size={24}
                color={
                  isHighContrast
                    ? (isDark ? '#34D399' : '#000000')
                    : (isDark ? '#2DD4BF' : BrandColors.accentTeal)
                }
              />
              <View style={styles.privacyContent}>
                <Text
                  style={[
                    styles.privacyTitle,
                    {
                      color: isHighContrast
                        ? (isDark ? '#FFFFFF' : '#000000')
                        : (isDark ? '#F8FAFC' : '#0F172A'),
                      fontWeight: isHighContrast ? '800' : '700',
                    },
                  ]}>
                  Twoje potrzeby są prywatne.
                </Text>
                <Text
                  style={[
                    styles.privacyText,
                    {
                      color: isHighContrast
                        ? (isDark ? '#CBD5E1' : '#111827')
                        : (isDark ? '#94A3B8' : '#475569'),
                      fontWeight: isHighContrast ? '600' : '400',
                    },
                  ]}>
                  Nie potrzebujemy danych medycznych. Ustawienia mapy możesz zmienić później.
                </Text>
              </View>
            </View>

            {/* Terms */}
            <Text
              style={[
                styles.termsText,
                {
                  color: isHighContrast
                    ? (isDark ? '#CBD5E1' : '#1E293B')
                    : (isDark ? '#94A3B8' : '#64748B'),
                },
              ]}>
              Tworząc konto, akceptujesz nasze{' '}
              <Text style={styles.termsLink}>warunki korzystania</Text> oraz{' '}
              <Text style={styles.termsLink}>politykę prywatności</Text>.
            </Text>

            {/* Create Account Action */}
            <AccessibleButton
              label={loading ? "Rejestracja..." : "Utwórz konto"}
              variant="primary"
              onPress={handleRegister}
              style={styles.actionBtn}
            />

            {/* Login Link */}
            <View style={styles.loginLinkRow}>
              <Text
                style={[
                  styles.haveAccountText,
                  {
                    color: isHighContrast
                      ? (isDark ? '#FFFFFF' : '#000000')
                      : (isDark ? '#94A3B8' : '#64748B'),
                  },
                ]}>
                Masz już konto?{' '}
              </Text>
              <Pressable
                onPress={() => router.push('/auth/login')}
                accessible
                accessibilityRole="button"
                accessibilityLabel="Przejdź do logowania">
                <Text
                  style={[
                    styles.loginLink,
                    {
                      color: isHighContrast
                        ? (isDark ? '#38BDF8' : '#003366')
                        : (isDark ? '#2DD4BF' : BrandColors.accentTeal),
                    },
                  ]}>
                  Zaloguj się
                </Text>
              </Pressable>
            </View>

            {/* Guest Action */}
            <AccessibleButton
              label="Zaloguj się jako gość"
              icon="book-open-outline"
              variant="guest"
              onPress={handleGuest}
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
              Konto możesz utworzyć w dowolnym momencie.
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
    paddingHorizontal: 8,
  },
  header: {
    marginBottom: Spacing.two,
  },
  title: {
    fontSize: 28,
    lineHeight: 34,
    letterSpacing: -0.5,
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
  privacyBox: {
    flexDirection: 'row',
    borderRadius: 16,
    padding: 14,
    gap: 12,
    marginVertical: 12,
    alignItems: 'flex-start',
  },
  privacyContent: {
    flex: 1,
  },
  privacyTitle: {
    fontSize: 15,
  },
  privacyText: {
    fontSize: 13,
    lineHeight: 18,
    marginTop: 2,
  },
  termsText: {
    fontSize: 13,
    lineHeight: 18,
    marginVertical: 8,
    textAlign: 'center',
  },
  termsLink: {
    textDecorationLine: 'underline',
    fontWeight: '600',
  },
  actionBtn: {
    marginTop: 8,
  },
  loginLinkRow: {
    flexDirection: 'row',
    justifyContent: 'center',
    paddingVertical: 14,
  },
  haveAccountText: {
    fontSize: 15,
  },
  loginLink: {
    fontSize: 15,
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
