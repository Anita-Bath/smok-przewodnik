import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import { useAccessibility } from '@/context/AccessibilityContext';

export function useTheme() {
  const scheme = useColorScheme();
  const baseTheme = scheme === 'dark' ? 'dark' : 'light';
  
  try {
    const { isHighContrast } = useAccessibility();
    if (isHighContrast) {
      return baseTheme === 'dark' ? Colors.highContrastDark : Colors.highContrastLight;
    }
  } catch {
    // In case hook is called outside provider during early bootstrap
  }

  return Colors[baseTheme];
}
