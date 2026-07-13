import { roundMoney } from './money';

export enum DepositCalcProductType {
  Savings = 0,
  RecurringDeposit = 1,
  FixedDeposit = 2,
}

export interface DepositMaturityInput {
  productType: DepositCalcProductType;
  /** FD/Savings: lump-sum principal. RD: monthly installment. */
  amount: number;
  annualRatePercent: number;
  tenureMonths: number | null;
}

export interface DepositMaturityResult {
  /** Total interest over the period (or 1 year for Savings). */
  interest: number;
  /** Maturity / projected value. For Savings = principal + 1y interest. */
  maturity: number;
  /** Total amount invested (FD/Savings = principal; RD = installment × months). */
  totalInvested: number;
  formulaLabel: string;
}

/**
 * Product-specific deposit maturity / interest preview.
 *
 * FD  — simple interest: I = P × r × n / 1200 ; M = P + I
 * RD  — standard bank formula:
 *       M = P×n + P×n×(n+1)/2 × r/1200
 *       (P = monthly installment, n = months)
 * Savings — estimated annual interest: I = P × r / 100 ; M = P + I
 */
export function estimateDepositMaturity(input: DepositMaturityInput): DepositMaturityResult {
  const amount = roundMoney(Number(input.amount) || 0);
  const rate = Number(input.annualRatePercent) || 0;
  const months = Math.trunc(Number(input.tenureMonths) || 0);

  if (amount <= 0 || rate < 0) {
    return emptyResult();
  }

  switch (input.productType) {
    case DepositCalcProductType.FixedDeposit: {
      if (months <= 0) {
        return emptyResult();
      }

      const interest = roundMoney((amount * rate * months) / 1200);
      return {
        interest,
        maturity: roundMoney(amount + interest),
        totalInvested: amount,
        formulaLabel: `FD simple interest · ${rate}% p.a. · ${months} months`,
      };
    }

    case DepositCalcProductType.RecurringDeposit: {
      if (months <= 0) {
        return emptyResult();
      }

      // M = P*n + P*n*(n+1)/2 * r/1200
      const totalInvested = roundMoney(amount * months);
      const interest = roundMoney((amount * months * (months + 1) * rate) / (2 * 1200));
      return {
        interest,
        maturity: roundMoney(totalInvested + interest),
        totalInvested,
        formulaLabel: `RD maturity · ₹${amount}/month × ${months} · ${rate}% p.a.`,
      };
    }

    case DepositCalcProductType.Savings:
    default: {
      const interest = roundMoney((amount * rate) / 100);
      return {
        interest,
        maturity: roundMoney(amount + interest),
        totalInvested: amount,
        formulaLabel: `Savings estimated annual interest · ${rate}% p.a.`,
      };
    }
  }
}

function emptyResult(): DepositMaturityResult {
  return {
    interest: 0,
    maturity: 0,
    totalInvested: 0,
    formulaLabel: 'Enter amount to see projection',
  };
}
