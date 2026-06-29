/** Local copy — avoids native-federation re-export issues from shared libs. */
export const MEMBER_MOBILE_PATTERN = /^[6-9]\d{9}$/;
export const MEMBER_PIN_PATTERN = /^\d{6}$/;
export const MEMBER_AADHAAR_PATTERN = /^\d{12}$/;
export const MEMBER_PAN_PATTERN = /^[A-Za-z]{5}\d{4}[A-Za-z]$/;

export function normalizeMobileNumber(value: string): string {
  let digits = value.replace(/\D/g, '');
  if (digits.length === 12 && digits.startsWith('91')) {
    digits = digits.slice(2);
  }
  return digits;
}

export function normalizeDigits(value: string): string {
  return value.replace(/\D/g, '');
}

export function normalizePan(value: string): string {
  return value.trim().toUpperCase();
}
