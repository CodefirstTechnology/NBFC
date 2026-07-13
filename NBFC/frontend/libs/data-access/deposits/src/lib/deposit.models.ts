export enum DepositProductType {
  Savings = 0,
  RecurringDeposit = 1,
  FixedDeposit = 2,
}

export enum DepositAccountStatus {
  Active = 0,
  Matured = 1,
  Closed = 2,
  PrematureClosed = 3,
}

export enum InterestPayoutMode {
  OnMaturity = 0,
  Monthly = 1,
}

export interface DepositAccountSummary {
  id: string;
  accountNumber: string;
  memberName: string;
  memberNumber: string;
  productType: DepositProductType;
  currentBalance: number;
  interestRate: number;
  maturityDate: string | null;
  status: DepositAccountStatus;
  openedOn: string;
}

export interface DepositAccountDetail {
  id: string;
  memberId: string;
  memberNumber: string;
  memberName: string;
  branchId: string;
  accountNumber: string;
  productType: DepositProductType;
  principalAmount: number;
  currentBalance: number;
  interestRate: number;
  tenureMonths: number | null;
  interestPayoutMode: InterestPayoutMode;
  autoRenewal: boolean;
  openedOn: string;
  maturityDate: string | null;
  status: DepositAccountStatus;
  createdAt: string;
  modifiedAt: string | null;
}

export interface PagedDepositsResponse {
  items: DepositAccountSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface DepositSummary {
  totalDepositsAmount: number;
  depositsTrendPercent: number | null;
  totalActiveAccounts: number;
  activeSavingsCount: number;
  fixedDepositsBalance: number;
  dueThisMonthCount: number;
}

export interface ListDepositsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  productType?: DepositProductType;
  status?: DepositAccountStatus;
  branchId?: string;
  memberId?: string;
}

export interface CreateDepositAccountRequest {
  memberId: string;
  memberNumber: string;
  memberName: string;
  branchId: string;
  productType: DepositProductType;
  principalAmount: number;
  tenureMonths?: number | null;
  interestPayoutMode: InterestPayoutMode;
  autoRenewal: boolean;
}

export interface UpdateDepositAccountRequest {
  status?: DepositAccountStatus;
  autoRenewal?: boolean;
}

export interface ProductRateInfo {
  productType: DepositProductType;
  label: string;
  rate: number;
  description: string;
  icon: string;
}

export const DEPOSIT_PRODUCTS: ProductRateInfo[] = [
  {
    productType: DepositProductType.Savings,
    label: 'Savings Account',
    rate: 4.0,
    description: 'Flexible deposits & withdrawals',
    icon: 'savings',
  },
  {
    productType: DepositProductType.RecurringDeposit,
    label: 'Recurring Deposit',
    rate: 6.5,
    description: 'Monthly savings habit',
    icon: 'calendar_today',
  },
  {
    productType: DepositProductType.FixedDeposit,
    label: 'Fixed Deposit',
    rate: 7.2,
    description: 'Lump sum investment',
    icon: 'lock_clock',
  },
];

export function formatInr(amount: number): string {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 2,
  }).format(amount);
}

export function formatCompactInr(amount: number): string {
  if (amount >= 10_000_000) {
    return `₹ ${(amount / 10_000_000).toFixed(2)} Crores`;
  }

  if (amount >= 100_000) {
    return `₹ ${(amount / 100_000).toFixed(2)} Lakhs`;
  }

  return formatInr(amount);
}

export function formatTrendPercent(value: number | null | undefined): string | null {
  if (value === null || value === undefined) {
    return null;
  }

  const sign = value > 0 ? '+' : '';
  return `${sign}${value.toFixed(1)}%`;
}

export function formatDepositDate(value: string | null): string {
  if (!value) {
    return '—';
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat('en-IN', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  }).format(date);
}

export function depositProductShort(type: DepositProductType): string {
  switch (type) {
    case DepositProductType.Savings:
      return 'SB';
    case DepositProductType.RecurringDeposit:
      return 'RD';
    case DepositProductType.FixedDeposit:
      return 'FD';
    default:
      return '—';
  }
}

export function depositProductDotClass(type: DepositProductType): string {
  switch (type) {
    case DepositProductType.Savings:
      return 'dot--savings';
    case DepositProductType.RecurringDeposit:
      return 'dot--recurring';
    case DepositProductType.FixedDeposit:
      return 'dot--fixed';
    default:
      return 'dot--default';
  }
}

export function isMaturitySoon(maturityDate: string | null, withinDays = 30): boolean {
  if (!maturityDate) {
    return false;
  }

  const maturity = new Date(maturityDate);
  const now = new Date();
  const diffMs = maturity.getTime() - now.getTime();
  const diffDays = diffMs / (1000 * 60 * 60 * 24);

  return diffDays >= 0 && diffDays <= withinDays;
}

export function isMaturityPast(maturityDate: string | null): boolean {
  if (!maturityDate) {
    return false;
  }

  return new Date(maturityDate).getTime() < Date.now();
}

export function estimateMaturityAmount(
  principal: number,
  rate: number,
  tenureMonths: number
): { interest: number; maturity: number } {
  // Backward-compatible FD-style simple interest helper.
  const interest = Math.round(((principal * rate * tenureMonths) / 1200) * 100) / 100;
  return {
    interest,
    maturity: Math.round((principal + interest) * 100) / 100,
  };
}

export function depositProductLabel(type: DepositProductType): string {
  return DEPOSIT_PRODUCTS.find((p) => p.productType === type)?.label ?? 'Unknown';
}

export function depositStatusLabel(status: DepositAccountStatus): string {
  return ['Active', 'Matured', 'Closed', 'Premature Closed'][status] ?? 'Unknown';
}

export function depositStatusVariant(
  status: DepositAccountStatus
): 'active' | 'inactive' | 'pending' | 'warning' | 'error' {
  switch (status) {
    case DepositAccountStatus.Active:
      return 'active';
    case DepositAccountStatus.Matured:
      return 'pending';
    case DepositAccountStatus.Closed:
      return 'inactive';
    case DepositAccountStatus.PrematureClosed:
      return 'warning';
    default:
      return 'inactive';
  }
}

export function interestPayoutLabel(mode: InterestPayoutMode): string {
  return mode === InterestPayoutMode.Monthly ? 'Monthly' : 'On Maturity';
}

export function extractApiErrorMessage(error: unknown, fallback: string): string {
  if (typeof error === 'object' && error !== null && 'error' in error) {
    const problem = (error as { error?: { detail?: string; title?: string } }).error;
    return problem?.detail ?? problem?.title ?? fallback;
  }

  return fallback;
}
