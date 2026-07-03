/** Mirrors server rules in CreateMemberCommandValidator. */
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

const VALIDATION_MESSAGES: Record<string, string> = {
  'Members.Mobile.Invalid': 'Mobile must be 10 digits and start with 6, 7, 8, or 9.',
  'Members.PinCode.Invalid': 'PIN code must be exactly 6 digits.',
  'Members.Aadhaar.Invalid': 'Aadhaar must be exactly 12 digits.',
  'Members.Pan.Invalid': 'PAN must match format ABCDE1234F.',
  'Members.DateOfBirth.Invalid': 'Date of birth must be in the past.',
  'Members.Aadhaar.Exists': 'A member with this Aadhaar already exists.',
  'Members.Email.Invalid': 'Enter a valid email address or leave it blank.',
  validation_error: 'Check the highlighted fields and try again.',
  invalid_request_body: 'Invalid request format. Check date and field formats.',
};

export function memberApiErrorMessage(error: unknown, fallback: string): string {
  if (typeof error === 'object' && error !== null && 'error' in error) {
    const problem = (error as { error?: { detail?: string; title?: string } }).error;
    if (problem?.title && VALIDATION_MESSAGES[problem.title]) {
      return VALIDATION_MESSAGES[problem.title];
    }
    return problem?.detail ?? problem?.title ?? fallback;
  }

  return fallback;
}
