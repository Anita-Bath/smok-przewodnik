import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import { useAccessibility } from '@/context/AccessibilityContext';

export function useTheme() {
  const scheme = useColorScheme();
  const { isHighContrast } = useAccessibility();
  const baseTheme = scheme === 'dark' ? 'dark' : 'light';

  if (isHighContrast) {
    return baseTheme === 'dark' ? Colors.highContrastDark : Colors.highContrastLight;
  }

  return Colors[baseTheme];
}
