import { roundMoney } from './money';

/**
 * Client-side EMI preview — mirrors backend LoanEmiCalculator.
 *
 * EMI = P × r × (1+r)^n / ((1+r)^n − 1)
 * where r = annualRate% / 12 / 100, n = tenure months
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
  monthlyRatePercent: number;
}

export function previewEmi(input: EmiPreviewInput): EmiPreviewResult {
  const principal = roundMoney(Number(input.principal) || 0);
  const annualRate = Number(input.annualRatePercent) || 0;
  const months = Math.trunc(Number(input.tenureMonths) || 0);
  const monthlyRate = annualRate / 12 / 100;

  if (principal <= 0 || months <= 0) {
    return { emiAmount: 0, totalInterest: 0, totalPayment: 0, monthlyRatePercent: 0 };
  }

  let emiAmount: number;
  if (monthlyRate === 0) {
    emiAmount = roundMoney(principal / months);
  } else {
    const factor = Math.pow(1 + monthlyRate, months);
    emiAmount = roundMoney((principal * monthlyRate * factor) / (factor - 1));
  }

  const totalPayment = roundMoney(emiAmount * months);
  const totalInterest = roundMoney(totalPayment - principal);

  return {
    emiAmount,
    totalInterest,
    totalPayment,
    monthlyRatePercent: roundMoney(monthlyRate * 100),
  };
}

export { roundMoney } from './money';

export function formatInr(value: number): string {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}
