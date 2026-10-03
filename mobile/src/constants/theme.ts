import '@/global.css';
import { Platform } from 'react-native';

export const BrandColors = {
  primary: '#004C97',        // Dominant Krakow Blue
  primaryDark: '#162C41',    // Dark navy for high contrast
  primaryLight: '#E8F1FC',   
  accentTeal: '#009692',     // Accessibility Teal
  accentTealLight: '#E6F5F3',
  danger: '#D32F2F',         
  warning: '#965E00',        // Warning orange
  success: '#967D00',        // Yellow/Gold 
  unverified: '#757575',
} as const;

export const Colors = {
  light: {
    text: '#0F172A',
    textSecondary: '#475569',
    background: '#F8FAFC',
    backgroundElement: '#F1F5F9',
    backgroundSelected: '#E2E8F0',
    border: '#E2E8F0',
    card: '#FFFFFF',
    cardBorder: '#E2E8F0',
    inputBg: '#FFFFFF',
    tint: BrandColors.primary,
    accent: BrandColors.accentTeal,
  },
  dark: {
    text: '#F8FAFC',
    textSecondary: '#94A3B8',
    background: '#0F172A',
    backgroundElement: '#1E293B',
    backgroundSelected: '#334155',
    border: '#334155',
    card: '#1E293B',
    cardBorder: '#334155',
    inputBg: '#1E293B',
    tint: '#38BDF8',
    accent: '#2DD4BF',
  },
  highContrastLight: {
    text: '#000000',
    textSecondary: '#1A1A1A',
    background: '#FFFFFF',
    backgroundElement: '#F8F9FA',
    backgroundSelected: '#D0E2FF',
    border: '#000000',
    card: '#FFFFFF',
    cardBorder: '#000000',
    inputBg: '#FFFFFF',
    tint: '#003399',
    accent: '#005A4E',
  },
  highContrastDark: {
    text: '#FFFFFF',
    textSecondary: '#EEEEEE',
    background: '#000000',
    backgroundElement: '#121212',
    backgroundSelected: '#1E3A8A',
    border: '#FFFFFF',
    card: '#0A0A0A',
    cardBorder: '#FFFFFF',
    inputBg: '#121212',
    tint: '#60A5FA',
    accent: '#5EEAD4',
  },
} as const;

export type ThemeMode = 'light' | 'dark' | 'highContrastLight' | 'highContrastDark';
export type ThemeColor = keyof typeof Colors.light;
export type ThemeColors = Record<ThemeColor, string>;

export const Fonts = Platform.select({
  ios: {
    sans: 'system-ui',
    serif: 'ui-serif',
    rounded: 'ui-rounded',
    mono: 'ui-monospace',
  },
  default: {
    sans: 'normal',
    serif: 'serif',
    rounded: 'normal',
    mono: 'monospace',
  },
  web: {
    sans: 'var(--font-display)',
    serif: 'var(--font-serif)',
    rounded: 'var(--font-rounded)',
    mono: 'var(--font-mono)',
  },
});

export const Spacing = {
  half: 2,
  one: 4,
  two: 8,
  three: 16,
  four: 24,
  five: 32,
  six: 64,
} as const;

export const Typography = {
  default: {
    title: 26,
    subtitle: 20,
    headline: 18,
    body: 16,
    small: 14,
    caption: 12,
  },
  large: {
    title: 32,
    subtitle: 24,
    headline: 22,
    body: 19,
    small: 16,
    caption: 14,
  },
  extraLarge: {
    title: 38,
    subtitle: 28,
    headline: 25,
    body: 22,
    small: 19,
    caption: 16,
  },
} as const;

export const MinTouchTargetSize = 48; // WCAG 2.5.5 AAA touch target compliance
export const BottomTabInset = Platform.select({ ios: 64, android: 80 }) ?? 60;
export const MaxContentWidth = 800;
