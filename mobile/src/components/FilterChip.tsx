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
  const { isHighContrast } = useAccessibility();

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
          backgroundColor: selected
            ? (isHighContrast ? '#000000' : BrandColors.primaryLight)
            : '#FFFFFF',
          borderColor: selected
            ? (isHighContrast ? '#000000' : BrandColors.primary)
            : (isHighContrast ? '#333333' : '#CBD5E1'),
          borderWidth: isHighContrast ? 2.5 : 1.5,
          opacity: pressed ? 0.75 : 1,
        },
      ]}>
      {selected ? (
        <MaterialCommunityIcons
          name="check"
          size={18}
          color={isHighContrast ? '#FFFFFF' : BrandColors.primary}
        />
      ) : (
        icon && (
          <MaterialCommunityIcons
            name={icon}
            size={18}
            color={isHighContrast ? '#000000' : '#475569'}
          />
        )
      )}
      <Text
        style={[
          styles.text,
          {
            color: selected
              ? (isHighContrast ? '#FFFFFF' : BrandColors.primary)
              : (isHighContrast ? '#000000' : '#334155'),
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
