/**
 * Shared money helpers — FE previews must match backend
 * MidpointRounding.AwayFromZero to 2 decimal places.
 */
export function roundMoney(value: number): number {
  if (!Number.isFinite(value)) {
    return 0;
  }

  // abs + Math.round then re-apply sign = AwayFromZero for both signs
  return ((Math.sign(value) || 1) * Math.round(Math.abs(value) * 100)) / 100;
}
