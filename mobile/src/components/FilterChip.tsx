import React from 'react';
import { Pressable, Text, StyleSheet, View } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { BrandColors } from '@/constants/theme';
import { useAccessibility } from '@/context/AccessibilityContext';

type MaterialIconName = keyof typeof MaterialCommunityIcons.glyphMap;

interface FilterChipProps {
  label: string;
  selected?: boolean;
  onPress: () => void;
  icon?: MaterialIconName;
}

export function FilterChip({ label, selected = false, onPress, icon }: FilterChipProps) {
  const { isHighContrast, isDark } = useAccessibility();

  const getBgColor = () => {
    if (selected) {
      if (isHighContrast) return isDark ? '#FFFFFF' : '#000000';
      return isDark ? '#0369A1' : BrandColors.primaryLight;
    }
    if (isHighContrast) return isDark ? '#000000' : '#FFFFFF';
    return isDark ? '#1E293B' : '#FFFFFF';
  };

  const getBorderColor = () => {
    if (selected) {
      if (isHighContrast) return isDark ? '#FFFFFF' : '#000000';
      return isDark ? '#38BDF8' : BrandColors.primary;
    }
    if (isHighContrast) return isDark ? '#FFFFFF' : '#333333';
    return isDark ? '#334155' : '#CBD5E1';
  };

  const getTextColor = () => {
    if (selected) {
      if (isHighContrast) return isDark ? '#000000' : '#FFFFFF';
      return isDark ? '#FFFFFF' : BrandColors.primary;
    }
    if (isHighContrast) return isDark ? '#FFFFFF' : '#000000';
    return isDark ? '#E2E8F0' : '#334155';
  };

  const textColor = getTextColor();

  return (
    <Pressable
      onPress={onPress}
      accessible
      accessibilityRole="checkbox"
      accessibilityState={{ checked: selected }}
      accessibilityLabel={`Filtr: ${label}, ${selected ? 'aktywny' : 'nieaktywny'}`}
      style={({ pressed }) => [
        styles.chip,
        {
          backgroundColor: getBgColor(),
          borderColor: getBorderColor(),
          borderWidth: isHighContrast ? 2.5 : 1.5,
          opacity: pressed ? 0.75 : 1,
        },
      ]}>
      {selected ? (
        <MaterialCommunityIcons
          name="check"
          size={18}
          color={textColor}
        />
      ) : (
        icon && (
          <MaterialCommunityIcons
            name={icon}
            size={18}
            color={textColor}
          />
        )
      )}
      <Text
        style={[
          styles.text,
          {
            color: textColor,
            fontWeight: selected || isHighContrast ? '700' : '600',
          },
        ]}>
        {label}
      </Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  chip: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 14,
    paddingVertical: 8,
    borderRadius: 20,
    gap: 6,
    marginRight: 8,
    minHeight: 40,
  },
  text: {
    fontSize: 14,
  },
});
