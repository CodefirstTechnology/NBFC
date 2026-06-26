---
name: TrustPoint Agent
colors:
  surface: '#faf9fd'
  surface-dim: '#dad9de'
  surface-bright: '#faf9fd'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f4f3f7'
  surface-container: '#eeedf2'
  surface-container-high: '#e9e7ec'
  surface-container-highest: '#e3e2e6'
  on-surface: '#1a1b1f'
  on-surface-variant: '#43474f'
  inverse-surface: '#2f3034'
  inverse-on-surface: '#f1f0f5'
  outline: '#747780'
  outline-variant: '#c4c6d0'
  surface-tint: '#405e92'
  primary: '#00122d'
  on-primary: '#ffffff'
  primary-container: '#002653'
  on-primary-container: '#738ec1'
  inverse-primary: '#abc7fd'
  secondary: '#0060ab'
  on-secondary: '#ffffff'
  secondary-container: '#6eaeff'
  on-secondary-container: '#004076'
  tertiary: '#0f1215'
  on-tertiary: '#ffffff'
  tertiary-container: '#24272a'
  on-tertiary-container: '#8c8e91'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d6e3ff'
  primary-fixed-dim: '#abc7fd'
  on-primary-fixed: '#001b3e'
  on-primary-fixed-variant: '#2a4775'
  secondary-fixed: '#d3e3ff'
  secondary-fixed-dim: '#a3c9ff'
  on-secondary-fixed: '#001c39'
  on-secondary-fixed-variant: '#004883'
  tertiary-fixed: '#e1e2e6'
  tertiary-fixed-dim: '#c5c6ca'
  on-tertiary-fixed: '#191c1f'
  on-tertiary-fixed-variant: '#44474a'
  background: '#faf9fd'
  on-background: '#1a1b1f'
  surface-variant: '#e3e2e6'
  glass-bg: rgba(255, 255, 255, 0.8)
  glass-border: rgba(26, 60, 110, 0.05)
  shadow-tint: rgba(26, 60, 110, 0.05)
  success-container: '#d4e3ff'
  on-success-container: '#001c39'
typography:
  headline-lg:
    fontFamily: Manrope
    fontSize: 28px
    fontWeight: '700'
    lineHeight: 36px
    letterSpacing: -0.02em
  headline-lg-mobile:
    fontFamily: Manrope
    fontSize: 24px
    fontWeight: '700'
    lineHeight: 32px
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Manrope
    fontSize: 22px
    fontWeight: '600'
    lineHeight: 28px
    letterSpacing: -0.01em
  headline-md-mobile:
    fontFamily: Manrope
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 26px
    letterSpacing: -0.01em
  body-lg:
    fontFamily: Manrope
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-md:
    fontFamily: Manrope
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  numeral-lg:
    fontFamily: Manrope
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
    letterSpacing: -0.03em
  numeral-md:
    fontFamily: Manrope
    fontSize: 18px
    fontWeight: '600'
    lineHeight: 24px
  label-md:
    fontFamily: Manrope
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.01em
  label-md-mobile:
    fontFamily: Manrope
    fontSize: 11px
    fontWeight: '600'
    lineHeight: 14px
    letterSpacing: 0.01em
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  container-margin: 20px
  stack-sm: 8px
  stack-md: 16px
  stack-lg: 24px
  gutter: 16px
  touch-target-min: 48px
---

## Brand & Style

TrustPoint Agent is a high-fidelity fintech design system tailored for field agents in high-trust banking environments. The aesthetic balances **Corporate Reliability** with **Modern Glassmorphism**, conveying both authority and approachability. 

The visual style utilizes a "soft-professional" approach: clean, high-whitespace layouts of Minimalism combined with the sophisticated depth of Glassmorphism. The goal is to reduce cognitive load for agents on the move while maintaining a premium, stable feel that builds confidence during financial interactions. Visual cues like subtle blurs and tinted shadows create a lightweight, airy feel without sacrificing the structured density required for financial data.

## Colors

The palette is anchored in a deep "Midnight Trust" blue (#002653), providing a stable foundation for primary actions and headings. A secondary "Digital Azure" (#0060ac) is used for emphasis and interactive elements, while a grayscale tertiary palette manages meta-information.

The background uses a slightly cool-toned off-white (#faf9fe) to reduce glare. Key to this system is the use of semi-transparent "Glass" surfaces for cards, allowing for subtle depth and a modern, high-fidelity appearance. Status colors (like the assigned badge) utilize high-chroma containers with high-contrast text for immediate legibility in outdoor environments.

## Typography

Manrope is used exclusively across the system for its modern, geometric construction and exceptional legibility at various weights. 

- **Headlines:** Use Bold (700) or Semi-Bold (600) with slight negative letter-spacing to create a compact, authoritative "impact" for page titles and names.
- **Numerals:** Financial data is prioritized using specific numeral scales with tighter tracking to ensure large currency values remain legible on a single line.
- **Labels:** Micro-copy (labels/badges) uses Semi-Bold at small sizes with 1% letter-spacing to ensure crisp rendering on mobile displays.

## Layout & Spacing

The system follows a contextual mobile-first layout with a focus on "one-handed" ergonomics. 

- **Margins:** A generous 20px container margin ensures content is centered away from physical device edges.
- **Rhythm:** An 8px baseline grid drives vertical rhythm (stack-sm/md/lg). 
- **Touch Ergonomics:** Interactive elements adhere to a strict 48px minimum touch target height. 
- **Navigation:** A fixed 80px bottom navigation bar provides a permanent anchor for primary app sections, while the 72px top app bar handles branding and utility actions.

## Elevation & Depth

Hierarchy is achieved through a combination of **Glassmorphism** and **Tinted Ambient Shadows**. 

1. **Base Layer:** The flat background (#faf9fe).
2. **Interactive Cards:** "Glass" surfaces using 80% opacity white with an 8px backdrop blur. These cards use a specific "Ambient Tint" shadow (110, 0.05 opacity) with a large 20px spread to create a soft, floating effect without the harshness of black shadows.
3. **Overlays:** Navigation bars use a standard `shadow-sm` and a subtle 1px top border to define boundaries.
4. **Active States:** Subtle scale transforms (0.98) are used instead of heavy elevation changes to provide tactile feedback during interactions.

## Shapes

The shape language is "Generously Rounded," using a systematic approach to corner radii:

- **Standard Elements:** 0.5rem (8px) for buttons and smaller containers.
- **Primary Cards:** 1.5rem (24px) for list cards to emphasize the "friendly corporate" feel.
- **Badges/Icons:** Full pills (9999px) for status indicators and profile containers.
- **Navigation:** The bottom bar features unique 1.5rem top-only rounding to "cradle" the app content.

## Components

### Buttons
- **Primary:** Solid #002653 background, #ffffff text. 48px height. 8px radius.
- **Icon-Only:** 48x48px, 1.5px border weight (outline-variant), centered material icon.
- **Navigation Tabs:** Pill-shaped background for the active state (#1a3c6e), centered vertical stack of icon and label.

### Cards
- **Visit Card:** Glassmorphic background, 24px corner radius, 16px internal padding. Uses a split header (Left: Name/Distance; Right: Due Amount).
- **Map Container:** 160px height, 24px radius, incorporates a bottom-anchored "floating" action chip.

### Input & Feedback
- **Badges:** Small 20-24px height pills. Uses container colors (e.g., secondary-container) with matching high-contrast text.
- **Lists:** Vertical stacks with 16px (stack-md) spacing between items.

### Profile/Avatars
- **Standard:** 32px or 48px circles with a slight primary-container background to act as a placeholder during loading.