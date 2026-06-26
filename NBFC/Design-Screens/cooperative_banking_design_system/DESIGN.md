---
name: Cooperative Banking Design System
colors:
  surface: '#faf8ff'
  surface-dim: '#dad9e0'
  surface-bright: '#faf8ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f4f3fa'
  surface-container: '#eeedf4'
  surface-container-high: '#e9e7ee'
  surface-container-highest: '#e3e2e9'
  on-surface: '#1a1b20'
  on-surface-variant: '#43474f'
  inverse-surface: '#2f3035'
  inverse-on-surface: '#f1f0f7'
  outline: '#747780'
  outline-variant: '#c4c6d0'
  surface-tint: '#405e92'
  primary: '#002653'
  on-primary: '#ffffff'
  primary-container: '#1a3c6e'
  on-primary-container: '#8aa8e0'
  inverse-primary: '#abc7ff'
  secondary: '#006d38'
  on-secondary: '#ffffff'
  secondary-container: '#8af6a9'
  on-secondary-container: '#00723b'
  tertiary: '#392200'
  on-tertiary: '#ffffff'
  tertiary-container: '#563600'
  on-tertiary-container: '#e0992c'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d7e3ff'
  primary-fixed-dim: '#abc7ff'
  on-primary-fixed: '#001b3f'
  on-primary-fixed-variant: '#264679'
  secondary-fixed: '#8df9ac'
  secondary-fixed-dim: '#71dc92'
  on-secondary-fixed: '#00210d'
  on-secondary-fixed-variant: '#005229'
  tertiary-fixed: '#ffddb5'
  tertiary-fixed-dim: '#ffb956'
  on-tertiary-fixed: '#2a1800'
  on-tertiary-fixed-variant: '#633f00'
  background: '#faf8ff'
  on-background: '#1a1b20'
  surface-variant: '#e3e2e9'
  error-npa: '#D64545'
  surface-muted: '#F8FAFC'
  border-subtle: '#E2E8F0'
  text-secondary: '#404040'
typography:
  display-lg:
    fontFamily: Manrope
    fontSize: 48px
    fontWeight: '700'
    lineHeight: '1.2'
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Manrope
    fontSize: 32px
    fontWeight: '600'
    lineHeight: '1.3'
  headline-lg-mobile:
    fontFamily: Manrope
    fontSize: 24px
    fontWeight: '600'
    lineHeight: '1.3'
  title-md:
    fontFamily: Manrope
    fontSize: 20px
    fontWeight: '600'
    lineHeight: '1.4'
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.6'
  body-sm:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1.5'
  label-md:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '600'
    lineHeight: '1.2'
    letterSpacing: 0.01em
  label-sm:
    fontFamily: Inter
    fontSize: 11px
    fontWeight: '500'
    lineHeight: '1.2'
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base: 8px
  container-margin-desktop: 32px
  container-margin-mobile: 16px
  gutter: 24px
  section-gap: 48px
---

## Brand & Style

This design system is engineered for the financial sector, specifically targeting the operational needs of cooperative credit societies. The brand personality is rooted in **Stability, Modernity, and Radical Clarity**. It aims to evoke a sense of institutional trust while remaining accessible to staff who manage complex financial data daily.

The chosen design style is **Corporate / Modern** with a focus on high-information density handled through **Minimalism**. By utilizing generous whitespace and a strictly governed grid, the interface reduces the cognitive load associated with ledger management and loan processing. The aesthetic is clean and professional, ensuring that bilingual labels (English and Marathi) coexist without visual clutter.

## Colors

The palette is led by **Deep Blue (#1A3C6E)**, a color chosen for its historical association with banking stability and professional integrity. 

- **Success Green (#2E9E5B)** is reserved for positive financial growth, completed transactions, and healthy account statuses.
- **Warning Amber (#F2A93B)** acts as a critical indicator for overdue installments or administrative alerts.
- **NPA Red (#D64545)** is a high-visibility semantic color specifically used for Non-Performing Asset flags and critical system errors.

The background uses a pure white base with **Surface Muted (#F8FAFC)** for container backgrounds to create subtle layering. Text utilizes a near-black neutral (#191A1F) for maximum legibility.

## Typography

The typography strategy employs **Manrope** for headlines and **Inter** for all UI and body elements. Manrope provides a modern, geometric touch to headers and financial figures, while Inter offers world-class legibility for dense data tables and bilingual text.

**Bilingual Handling:** To accommodate Marathi scripts, the line height for all body and label styles is set to a minimum of 1.5x. This ensures that Marathi diacritics (matras) do not overlap with the lines above or below. When displaying English and Marathi side-by-side, the Marathi label should be set at a 10% smaller font size or a slightly lighter weight to maintain visual equilibrium with the Latin characters.

## Layout & Spacing

This design system utilizes a **Fixed-Fluid hybrid grid**. On desktop, content is housed within a 12-column grid with a maximum width of 1440px. On mobile, it collapses to a single-column flow with 16px margins.

The spacing rhythm is based on an **8px base unit**. Dashboards use a "Card-on-Surface" model where the background is a light grey-blue tint, and the primary content resides in white cards. This creates a clear distinction between the system navigation and the user's workspace. Padding within cards should be generous (24px to 32px) to prevent financial data from feeling cramped.

## Elevation & Depth

To maintain a professional banking aesthetic, this design system avoids heavy shadows in favor of **Tonal Layers** and **Soft Ambient Depth**. 

- **Level 0 (Base):** Used for the main application background.
- **Level 1 (Surface):** Default card state. Uses a very soft, diffused shadow (0px 4px 12px rgba(0, 0, 0, 0.05)) and a 1px border in `border-subtle`.
- **Level 2 (Interaction/Hover):** Applied when a user interacts with a card or button. The shadow deepens slightly, and the border color shifts to the primary blue at a low opacity.
- **Level 3 (Modals):** High elevation with a backdrop blur (12px) to ensure focus is maintained on critical financial confirmations.

## Shapes

The shape language is defined by **Softened Precision**. UI components utilize a standard **8px (0.5rem)** radius to appear modern and approachable. Larger containers, such as Dashboard Cards and Loan Summaries, use a **16px (1rem)** radius to create a distinct, modular look that feels solid and secure. 

Buttons follow the 8px rule, except for specialized "Action Pills" in tables which may use a fully rounded (pill-shaped) style to differentiate them from static labels.

## Components

### Buttons
Primary buttons use the Deep Blue (#1A3C6E) with white text. Secondary actions use a ghost style with a Deep Blue border. Success/Positive actions (e.g., "Disburse Loan") may use the Green (#2E9E5B) but only when they are the final step in a flow.

### Cards
Cards are the primary organizational unit. Every card must have a 16px border-radius. For financial summaries, cards should include a 4px accent border on the top edge using the status colors (Blue for active, Amber for overdue, Red for NPA).

### Input Fields
Inputs use a white background with a 1px border. On focus, the border transitions to a 2px Deep Blue stroke. Labels for inputs should always support English/Marathi stacking, with the Marathi translation appearing in a smaller `label-sm` style directly below the English title.

### Status Chips
Chips are used extensively for account statuses. They utilize a low-saturation background of the status color with high-saturation text (e.g., a light green background with dark green text for "Active").

### Data Tables
Tables are the heart of the system. They should be "zebra-striped" with `surface-muted` and use `body-sm` for high information density. Row heights should be fixed at 48px to ensure touch-targets are accessible while maintaining professional compactness.