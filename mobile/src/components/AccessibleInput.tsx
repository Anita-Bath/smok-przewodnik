import React, { useState } from 'react';
import { View, Text, TextInput, StyleSheet, Pressable, TextInputProps } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { BrandColors, Spacing } from '@/constants/theme';
import { useAccessibility } from '@/context/AccessibilityContext';

interface AccessibleInputProps extends TextInputProps {
  label: string;
  isPassword?: boolean;
  hint?: string;
  error?: string;
}

export function AccessibleInput({
  label,
  isPassword = false,
  hint,
  error,
  style,
  ...rest
}: AccessibleInputProps) {
  const { isHighContrast, isDark } = useAccessibility();
  const [isPasswordVisible, setIsPasswordVisible] = useState(false);
  const [isFocused, setIsFocused] = useState(false);

  return (
    <View style={styles.wrapper}>
      <View
        style={[
          styles.container,
          {
            borderColor: error
              ? BrandColors.danger
              : isFocused
              ? (isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#38BDF8' : BrandColors.primary))
              : (isHighContrast ? (isDark ? '#888888' : '#333333') : (isDark ? '#334155' : '#CBD5E1')),
            borderWidth: isHighContrast ? 2.5 : 1.5,
            backgroundColor: isDark ? '#1E293B' : '#FFFFFF',
          },
        ]}>
        <Text
          style={[
            styles.label,
            {
              color: error
                ? BrandColors.danger
                : isHighContrast
                ? (isDark ? '#FFFFFF' : '#000000')
                : (isDark ? '#94A3B8' : '#64748B'),
              fontWeight: isHighContrast ? '700' : '600',
            },
          ]}>
          {label}
        </Text>

        <View style={styles.inputRow}>
          <TextInput
            {...rest}
            secureTextEntry={isPassword && !isPasswordVisible}
            onFocus={() => setIsFocused(true)}
            onBlur={() => setIsFocused(false)}
            placeholderTextColor={isDark ? '#64748B' : '#94A3B8'}
            style={[
              styles.input,
              {
                color: isHighContrast ? (isDark ? '#FFFFFF' : '#000000') : (isDark ? '#F8FAFC' : '#0F172A'),
                fontSize: isHighContrast ? 17 : 16,
              },
              style,
            ]}
          />

          {isPassword && (
            <Pressable
              onPress={() => setIsPasswordVisible(!isPasswordVisible)}
              accessible
              accessibilityRole="button"
              accessibilityLabel={isPasswordVisible ? 'Ukryj hasło' : 'Pokaż hasło'}
              style={styles.eyeButton}>
              <MaterialCommunityIcons
                name={isPasswordVisible ? 'eye-off-outline' : 'eye-outline'}
                size={22}
                color={isHighContrast ? '#000000' : '#64748B'}
              />
            </Pressable>
          )}
        </View>
      </View>

      {hint && !error && (
        <Text
          style={[
            styles.hintText,
            { color: isHighContrast ? '#111827' : '#64748B' },
          ]}>
          {hint}
        </Text>
      )}

      {error && (
        <Text style={styles.errorText} accessibilityRole="alert">
          {error}
        </Text>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  wrapper: {
    marginVertical: 8,
  },
  container: {
    borderRadius: 16,
    paddingHorizontal: 16,
    paddingTop: 8,
    paddingBottom: 8,
  },
  label: {
    fontSize: 12,
    marginBottom: 2,
    letterSpacing: 0.2,
  },
  inputRow: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  input: {
    flex: 1,
    paddingVertical: 4,
    minHeight: 48,
  },
  eyeButton: {
    padding: 6,
    marginLeft: 6,
  },
  hintText: {
    fontSize: 13,
    marginTop: 4,
    marginLeft: 4,
  },
  errorText: {
    fontSize: 13,
    color: BrandColors.danger,
    marginTop: 4,
    marginLeft: 4,
    fontWeight: '600',
  },
});
