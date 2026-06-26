/**
 * Design tokens extracted from Design-Screens/cooperative_banking_design_system/DESIGN.md
 * and cross-referenced HTML reference screens.
 */
export const colors = {
  primary: '#002653',
  onPrimary: '#ffffff',
  primaryContainer: '#1a3c6e',
  onPrimaryContainer: '#8aa8e0',
  secondary: '#006d38',
  onSecondary: '#ffffff',
  secondaryContainer: '#8af6a9',
  onSecondaryContainer: '#00723b',
  tertiary: '#392200',
  onTertiary: '#ffffff',
  tertiaryContainer: '#563600',
  onTertiaryContainer: '#e0992c',
  error: '#ba1a1a',
  onError: '#ffffff',
  errorContainer: '#ffdad6',
  onErrorContainer: '#93000a',
  errorNpa: '#D64545',
  success: '#2E9E5B',
  warning: '#F2A93B',
  background: '#faf8ff',
  onBackground: '#1a1b20',
  surface: '#faf8ff',
  onSurface: '#1a1b20',
  onSurfaceVariant: '#43474f',
  surfaceMuted: '#F8FAFC',
  surfaceContainerLowest: '#ffffff',
  surfaceContainerLow: '#f4f3fa',
  surfaceContainer: '#eeedf4',
  surfaceContainerHigh: '#e9e7ee',
  surfaceContainerHighest: '#e3e2e9',
  borderSubtle: '#E2E8F0',
  outline: '#747780',
  outlineVariant: '#c4c6d0',
  textSecondary: '#404040',
  inverseSurface: '#2f3035',
  inverseOnSurface: '#f1f0f7',
} as const;

export const spacing = {
  base: 8,
  xs: 4,
  sm: 8,
  md: 16,
  lg: 24,
  xl: 32,
  gutter: 24,
  sectionGap: 48,
  containerMarginMobile: 16,
  containerMarginDesktop: 32,
  touchTarget: 44,
  tableRowHeight: 48,
} as const;

export const radius = {
  sm: 4,
  md: 8,
  lg: 16,
  xl: 24,
  full: 9999,
} as const;

export const shadow = {
  card: '0 4px 12px rgba(0, 0, 0, 0.05)',
  cardHover: '0 8px 24px rgba(0, 0, 0, 0.08)',
  modal: '0 16px 48px rgba(0, 0, 0, 0.12)',
} as const;

export const typography = {
  fontDisplay: "'Manrope', sans-serif",
  fontBody: "'Inter', sans-serif",
  displayLg: { size: '48px', weight: 700, lineHeight: 1.2, letterSpacing: '-0.02em' },
  headlineLg: { size: '32px', weight: 600, lineHeight: 1.3 },
  headlineLgMobile: { size: '24px', weight: 600, lineHeight: 1.3 },
  titleMd: { size: '20px', weight: 600, lineHeight: 1.4 },
  bodyLg: { size: '18px', weight: 400, lineHeight: 1.6 },
  bodyMd: { size: '16px', weight: 400, lineHeight: 1.6 },
  bodySm: { size: '14px', weight: 400, lineHeight: 1.5 },
  labelMd: { size: '13px', weight: 600, lineHeight: 1.2, letterSpacing: '0.01em' },
  labelSm: { size: '11px', weight: 500, lineHeight: 1.2 },
} as const;

export const layout = {
  sidebarWidth: 256,
  maxContentWidth: 1440,
} as const;

export type StatusPillVariant = 'active' | 'inactive' | 'pending' | 'warning' | 'error' | 'info';

export const statusPillColors: Record<
  StatusPillVariant,
  { background: string; text: string; border: string }
> = {
  active: { background: 'rgba(46, 158, 91, 0.12)', text: '#00723b', border: 'rgba(46, 158, 91, 0.24)' },
  inactive: { background: 'rgba(116, 119, 128, 0.12)', text: '#43474f', border: 'rgba(116, 119, 128, 0.24)' },
  pending: { background: 'rgba(64, 94, 146, 0.12)', text: '#1a3c6e', border: 'rgba(64, 94, 146, 0.24)' },
  warning: { background: 'rgba(242, 169, 59, 0.14)', text: '#633f00', border: 'rgba(242, 169, 59, 0.28)' },
  error: { background: 'rgba(214, 69, 69, 0.12)', text: '#93000a', border: 'rgba(214, 69, 69, 0.24)' },
  info: { background: 'rgba(26, 60, 110, 0.08)', text: '#1a3c6e', border: 'rgba(26, 60, 110, 0.16)' },
};
