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
