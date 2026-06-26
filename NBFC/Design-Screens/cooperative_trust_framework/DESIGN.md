---
name: Cooperative Trust Framework
colors:
  surface: '#f8f9ff'
  surface-dim: '#cbdbf5'
  surface-bright: '#f8f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#eff4ff'
  surface-container: '#e5eeff'
  surface-container-high: '#dce9ff'
  surface-container-highest: '#d3e4fe'
  on-surface: '#0b1c30'
  on-surface-variant: '#43474f'
  inverse-surface: '#213145'
  inverse-on-surface: '#eaf1ff'
  outline: '#747780'
  outline-variant: '#c4c6d0'
  surface-tint: '#405e92'
  primary: '#002653'
  on-primary: '#ffffff'
  primary-container: '#1a3c6e'
  on-primary-container: '#8aa8e0'
  inverse-primary: '#abc7ff'
  secondary: '#595f65'
  on-secondary: '#ffffff'
  secondary-container: '#dee3ea'
  on-secondary-container: '#5f656b'
  tertiary: '#461a00'
  on-tertiary: '#ffffff'
  tertiary-container: '#682a00'
  on-tertiary-container: '#ff863f'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d7e3ff'
  primary-fixed-dim: '#abc7ff'
  on-primary-fixed: '#001b3f'
  on-primary-fixed-variant: '#264679'
  secondary-fixed: '#dee3ea'
  secondary-fixed-dim: '#c2c7ce'
  on-secondary-fixed: '#171c21'
  on-secondary-fixed-variant: '#42474d'
  tertiary-fixed: '#ffdbca'
  tertiary-fixed-dim: '#ffb690'
  on-tertiary-fixed: '#341100'
  on-tertiary-fixed-variant: '#783200'
  background: '#f8f9ff'
  on-background: '#0b1c30'
  surface-variant: '#d3e4fe'
typography:
  display-lg:
    fontFamily: Manrope
    fontSize: 40px
    fontWeight: '800'
    lineHeight: 52px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Manrope
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
    letterSpacing: -0.01em
  headline-lg-mobile:
    fontFamily: Manrope
    fontSize: 24px
    fontWeight: '700'
    lineHeight: 32px
  title-md:
    fontFamily: Manrope
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
  body-lg:
    fontFamily: Manrope
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Manrope
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  label-md-bold:
    fontFamily: Manrope
    fontSize: 14px
    fontWeight: '700'
    lineHeight: 20px
    letterSpacing: 0.01em
  label-sm:
    fontFamily: Manrope
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
  marathi-supplement:
    fontFamily: Manrope
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 22px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base: 8px
  container-max-width: 1200px
  gutter: 24px
  margin-mobile: 16px
  margin-desktop: 40px
  section-gap: 48px
  form-element-gap: 20px
---

## Brand & Style
The design system is engineered for the high-stakes environment of cooperative banking, specifically for Sahakari Bank. It balances the authoritative weight of a traditional financial institution with the modern accessibility required for digital-first loan applications. The visual narrative is rooted in **Corporate Modernism**, prioritizing clarity, data density, and user confidence.

The interface must evoke a sense of reliability and transparency. Given the bilingual requirement (English and Marathi), the design system utilizes generous vertical rhythm and clear visual grouping to accommodate varying script heights and complexities without compromising the professional aesthetic.

## Colors
The palette is anchored by **Deep Blue (#1A3C6E)**, symbolizing stability and institutional trust. This is used for primary actions, headers, and key navigation elements. 

- **Primary:** Deep Blue is the dominant brand signal.
- **Secondary:** A soft blue-tinted light gray (#EBF0F7) is used for large background areas and container grouping, providing a sophisticated alternative to pure white.
- **Tertiary:** An energetic Orange (#F97316) is reserved exclusively for high-priority call-to-actions or "Attention" states within the loan wizard, such as "Apply Now" or "Submit Application."
- **Functional:** Success greens and Error reds are used with high saturation to ensure clear feedback during complex form entries.

## Typography
This design system utilizes **Manrope** as the sole typeface. Its modern, geometric construction maintains exceptional legibility for both Latin and Devanagari scripts, which is critical for bilingual Marathi/English labels.

- **Bilingual Strategy:** Marathi labels should be placed either directly below or to the right of English labels. Use `label-sm` in a slightly lighter neutral tone for the secondary language to maintain hierarchy.
- **Data Tables:** For EMI schedules and calculation previews, use `body-md` with tabular lining figures to ensure numerical alignment.
- **Hierarchy:** High-contrast weights (ExtraBold for displays, Medium for body) are used to guide the user through the linear loan application process.

## Layout & Spacing
The layout follows a **Fixed Grid** philosophy for the core application wizard to ensure that data-heavy forms remain readable and focused.

- **Desktop (1200px+):** A 12-column grid. The loan wizard is centered in an 8-column card to minimize eye-travel.
- **Tablet (768px - 1024px):** A 1-column layout for the wizard steps, utilizing the full width of the screen for data tables.
- **Mobile (<768px):** Steppers transition from horizontal labels to a simplified "Step X of Y" progress indicator.
- **Rhythm:** An 8px base unit (aligned with the ROUND_EIGHT principle) governs all padding and margins. Complex forms use a 20px (`form-element-gap`) vertical spacing to prevent visual clutter in bilingual contexts.

## Elevation & Depth
The design system employs **Tonal Layers** rather than aggressive shadows to maintain a clean, institutional feel.

- **The Canvas:** The primary background is white (#FFFFFF). 
- **The Container:** Wizard sections and data-heavy previews (EMI schedules) are housed in containers with a 1px border (#E2E8F0) and a subtle, high-diffusion ambient shadow (4px blur, 2% opacity).
- **Interactive Elements:** Buttons and active form fields use a subtle 2px bottom-shadow to suggest tactility without breaking the professional, flat aesthetic.
- **Focus States:** Active input fields use a 2px solid stroke of the Primary Deep Blue with a 4px soft outer glow in a 10% opacity version of the primary color.

## Shapes
Following the "ROUND_EIGHT" foundation, the design system utilizes a **Rounded** shape language.

- **Standard Elements:** Buttons, input fields, and checkboxes use `rounded` (0.5rem / 8px).
- **Surface Containers:** Large cards, loan summary sections, and the main wizard container use `rounded-lg` (1rem / 16px).
- **Informational Tags:** Status badges (e.g., "Verified," "Pending") use `rounded-xl` (1.5rem / 24px) to distinguish them from interactive buttons.

## Components
- **Buttons:** Primary buttons are Solid Deep Blue with white text. Secondary buttons are Ghost-style with a Deep Blue border. The "Submit" or "Calculate" action uses the Tertiary Orange.
- **Input Fields:** Stacked layout is preferred. English label on top (Bold), Marathi label immediately below (Regular, muted color). Error states must include an icon for accessibility.
- **Bilingual Chips:** Small, rounded indicators that allow users to toggle the primary/secondary language prominence throughout the wizard.
- **EMI Preview Card:** A dedicated container with a #EBF0F7 background. Key figures (Monthly Payment, Total Interest) are displayed in `headline-lg` for immediate impact.
- **Progress Stepper:** A horizontal bar at the top of the wizard. Completed steps are marked with a Deep Blue checkmark; the current step is indicated by a thick 4px underline.
- **Data Tables (Schedules):** Low-contrast zebra striping using #F8FAFC. Headers are sticky and use `label-md-bold` with a solid Deep Blue bottom border.