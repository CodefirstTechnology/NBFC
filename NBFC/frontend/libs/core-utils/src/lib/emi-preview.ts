/**
 * Client-side EMI preview helpers only — backend is always source of truth.
 */
export interface EmiPreviewInput {
  principal: number;
  annualRatePercent: number;
  tenureMonths: number;
}

export interface EmiPreviewResult {
  emiAmount: number;
  totalInterest: number;
  totalPayment: number;
}

export function previewEmi(input: EmiPreviewInput): EmiPreviewResult {
  const principal = roundMoney(input.principal);
  const monthlyRate = input.annualRatePercent / 12 / 100;
  const months = input.tenureMonths;

  if (principal <= 0 || months <= 0) {
    return { emiAmount: 0, totalInterest: 0, totalPayment: 0 };
  }

  if (monthlyRate === 0) {
    const emiAmount = roundMoney(principal / months);
    return {
      emiAmount,
      totalInterest: 0,
      totalPayment: roundMoney(emiAmount * months),
    };
  }

  const factor = Math.pow(1 + monthlyRate, months);
  const emiAmount = roundMoney((principal * monthlyRate * factor) / (factor - 1));
  const totalPayment = roundMoney(emiAmount * months);
  const totalInterest = roundMoney(totalPayment - principal);

  return { emiAmount, totalInterest, totalPayment };
}

export function roundMoney(value: number): number {
  return Math.round(value * 100) / 100;
}

export function formatInr(value: number): string {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}
