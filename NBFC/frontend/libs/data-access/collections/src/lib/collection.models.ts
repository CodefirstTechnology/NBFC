export enum PaymentMode {
  Cash = 0,
  Cheque = 1,
  Upi = 2,
  BankTransfer = 3,
}

export enum CollectionReceiptStatus {
  Collected = 0,
  Reversed = 1,
}

export interface CollectionReceiptSummary {
  id: string;
  receiptNumber: string;
  loanNumber: string;
  memberName: string;
  memberNumber: string;
  amount: number;
  paymentMode: PaymentMode;
  status: CollectionReceiptStatus;
  collectedOn: string;
}

export interface CollectionReceiptDetail {
  id: string;
  memberId: string;
  memberNumber: string;
  memberName: string;
  loanApplicationId: string;
  loanNumber: string;
  branchId: string;
  receiptNumber: string;
  amount: number;
  paymentMode: PaymentMode;
  referenceNumber: string | null;
  collectedOn: string;
  status: CollectionReceiptStatus;
  createdAt: string;
  modifiedAt: string | null;
}

export interface PagedCollectionsResponse {
  items: CollectionReceiptSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ListCollectionsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  branchId?: string;
  memberId?: string;
  loanNumber?: string;
}

export interface CreateCollectionReceiptRequest {
  memberId: string;
  memberNumber: string;
  memberName: string;
  loanApplicationId: string;
  loanNumber: string;
  branchId: string;
  amount: number;
  paymentMode: PaymentMode;
  referenceNumber?: string | null;
  collectedOn: string;
}

export const PAYMENT_MODES = [
  { mode: PaymentMode.Cash, label: 'Cash', icon: 'payments' },
  { mode: PaymentMode.Cheque, label: 'Cheque', icon: 'receipt_long' },
  { mode: PaymentMode.Upi, label: 'UPI', icon: 'qr_code_2' },
  { mode: PaymentMode.BankTransfer, label: 'Bank Transfer', icon: 'account_balance' },
] as const;

export function formatInr(amount: number): string {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 2,
  }).format(amount);
}

export function paymentModeLabel(mode: PaymentMode): string {
  return PAYMENT_MODES.find((p) => p.mode === mode)?.label ?? 'Unknown';
}

export function collectionStatusLabel(status: CollectionReceiptStatus): string {
  return ['Collected', 'Reversed'][status] ?? 'Unknown';
}

export function collectionStatusVariant(
  status: CollectionReceiptStatus
): 'active' | 'inactive' | 'pending' | 'warning' | 'error' {
  switch (status) {
    case CollectionReceiptStatus.Collected:
      return 'active';
    case CollectionReceiptStatus.Reversed:
      return 'error';
    default:
      return 'inactive';
  }
}

export function extractApiErrorMessage(error: unknown, fallback: string): string {
  if (typeof error === 'object' && error !== null && 'error' in error) {
    const problem = (error as { error?: { detail?: string; title?: string } }).error;
    return problem?.detail ?? problem?.title ?? fallback;
  }

  return fallback;
}
