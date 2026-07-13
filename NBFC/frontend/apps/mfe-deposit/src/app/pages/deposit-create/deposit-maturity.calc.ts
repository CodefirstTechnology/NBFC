/** Local copy — avoids native-federation re-export issues from shared libs. */

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
  interest: number;
  maturity: number;
  totalInvested: number;
  formulaLabel: string;
}

function roundMoney(value: number): number {
  if (!Number.isFinite(value)) {
    return 0;
  }

  return ((Math.sign(value) || 1) * Math.round(Math.abs(value) * 100)) / 100;
}

/**
 * FD  — I = P × r × n / 1200 ; M = P + I
 * RD  — M = P×n + P×n×(n+1)/2 × r/1200
 * Savings — I = P × r / 100 (1 year estimate)
 */
export function estimateDepositMaturity(input: DepositMaturityInput): DepositMaturityResult {
  const amount = roundMoney(Number(input.amount) || 0);
  const rate = Number(input.annualRatePercent) || 0;
  const months = Math.trunc(Number(input.tenureMonths) || 0);

  if (amount <= 0 || rate < 0) {
    return {
      interest: 0,
      maturity: 0,
      totalInvested: 0,
      formulaLabel: 'Enter amount to see projection',
    };
  }

  switch (input.productType) {
    case DepositCalcProductType.FixedDeposit: {
      if (months <= 0) {
        return {
          interest: 0,
          maturity: 0,
          totalInvested: 0,
          formulaLabel: 'Select tenure to see FD maturity',
        };
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
        return {
          interest: 0,
          maturity: 0,
          totalInvested: 0,
          formulaLabel: 'Select tenure to see RD maturity',
        };
      }

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
