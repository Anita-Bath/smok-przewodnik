import React from 'react';
import { Pressable, Text, StyleSheet, View, ActivityIndicator, ViewStyle, TextStyle } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { BrandColors, MinTouchTargetSize } from '@/constants/theme';
import { useAccessibility } from '@/context/AccessibilityContext';

type MaterialIconName = keyof typeof MaterialCommunityIcons.glyphMap;

interface AccessibleButtonProps {
  label: string;
  onPress: () => void;
  variant?: 'primary' | 'secondary' | 'guest' | 'danger';
  icon?: MaterialIconName;
  iconPosition?: 'left' | 'right';
  disabled?: boolean;
  loading?: boolean;
  accessibilityLabel?: string;
  accessibilityHint?: string;
  style?: ViewStyle;
  textStyle?: TextStyle;
}

export function AccessibleButton({
  label,
  onPress,
  variant = 'primary',
  icon,
  iconPosition = 'left',
  disabled = false,
  loading = false,
  accessibilityLabel,
  accessibilityHint,
  style,
  textStyle,
}: AccessibleButtonProps) {
  const { isHighContrast } = useAccessibility();

  // Compute styles based on variant & contrast
  const getContainerStyle = (pressed: boolean): ViewStyle => {
    let base: ViewStyle = {
      minHeight: Math.max(MinTouchTargetSize, 50),
      borderRadius: 25,
      paddingHorizontal: 20,
      paddingVertical: 12,
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 10,
    };

    if (variant === 'primary') {
      base.backgroundColor = isHighContrast ? '#000000' : BrandColors.primary;
      if (isHighContrast) {
        base.borderWidth = 3;
        base.borderColor = '#000000';
      }
      if (pressed) base.opacity = 0.85;
    } else if (variant === 'secondary') {
      base.backgroundColor = '#FFFFFF';
      base.borderWidth = isHighContrast ? 2.5 : 1.5;
      base.borderColor = isHighContrast ? '#000000' : BrandColors.primary;
      if (pressed) base.backgroundColor = '#F1F5F9';
    } else if (variant === 'guest') {
      base.backgroundColor = '#FFFFFF';
      base.borderWidth = isHighContrast ? 2.5 : 1.5;
      base.borderColor = isHighContrast ? '#005A4E' : BrandColors.accentTeal;
      if (pressed) base.backgroundColor = '#E6F5F3';
    } else if (variant === 'danger') {
      base.backgroundColor = BrandColors.danger;
      if (isHighContrast) {
        base.borderWidth = 3;
        base.borderColor = '#000000';
      }
      if (pressed) base.opacity = 0.85;
    }

    if (disabled) {
      base.opacity = 0.5;
    }

    return base;
  };

  const getTextColor = (): string => {
    if (variant === 'primary' || variant === 'danger') return '#FFFFFF';
    if (variant === 'secondary') return isHighContrast ? '#000000' : BrandColors.primary;
    if (variant === 'guest') return isHighContrast ? '#005A4E' : BrandColors.accentTeal;
    return '#000000';
  };

  const textColor = getTextColor();

  return (
    <Pressable
      onPress={onPress}
      disabled={disabled || loading}
      accessible
      accessibilityRole="button"
      accessibilityLabel={accessibilityLabel || label}
      accessibilityHint={accessibilityHint}
      accessibilityState={{ disabled: disabled || loading }}
      style={({ pressed }) => [getContainerStyle(pressed), style]}>
      {loading ? (
        <ActivityIndicator color={textColor} />
      ) : (
        <>
          {icon && iconPosition === 'left' && (
            <MaterialCommunityIcons name={icon} size={22} color={textColor} />
          )}
          <Text
            style={[
              styles.text,
              {
                color: textColor,
                fontWeight: isHighContrast ? '800' : '700',
                fontSize: isHighContrast ? 17 : 16,
              },
              textStyle,
            ]}>
            {label}
          </Text>
          {icon && iconPosition === 'right' && (
            <MaterialCommunityIcons name={icon} size={22} color={textColor} />
          )}
        </>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  text: {
    letterSpacing: -0.2,
    textAlign: 'center',
  },
});
