export enum RecoveryCaseStatus {
  Open = 0,
  InProgress = 1,
  Resolved = 2,
  WrittenOff = 3,
}

export interface RecoveryCaseSummary {
  id: string;
  caseNumber: string;
  loanNumber: string;
  memberName: string;
  memberNumber: string;
  outstandingAmount: number;
  daysPastDue: number;
  status: RecoveryCaseStatus;
  openedOn: string;
  assignedToUserId: string | null;
}

export interface RecoveryCaseDetail {
  id: string;
  loanApplicationId: string;
  loanNumber: string;
  memberId: string;
  memberNumber: string;
  memberName: string;
  branchId: string;
  caseNumber: string;
  outstandingAmount: number;
  daysPastDue: number;
  status: RecoveryCaseStatus;
  notes: string | null;
  assignedToUserId: string | null;
  openedOn: string;
  resolvedOn: string | null;
  createdAt: string;
  modifiedAt: string | null;
}

export interface PagedRecoveryCasesResponse {
  items: RecoveryCaseSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ListRecoveryCasesParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: RecoveryCaseStatus;
  branchId?: string;
  memberId?: string;
  assignedToUserId?: string;
}

export interface CreateRecoveryCaseRequest {
  loanApplicationId: string;
  loanNumber: string;
  memberId: string;
  memberNumber: string;
  memberName: string;
  branchId: string;
  outstandingAmount: number;
  daysPastDue: number;
  notes?: string | null;
}

export interface UpdateRecoveryCaseRequest {
  status?: RecoveryCaseStatus;
  assignedToUserId?: string | null;
  notes?: string | null;
}

export function formatInr(amount: number): string {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 2,
  }).format(amount);
}

export function recoveryStatusLabel(status: RecoveryCaseStatus): string {
  return ['Open', 'In Progress', 'Resolved', 'Written Off'][status] ?? 'Unknown';
}

export function recoveryStatusVariant(
  status: RecoveryCaseStatus
): 'active' | 'inactive' | 'pending' | 'warning' | 'error' {
  switch (status) {
    case RecoveryCaseStatus.Open:
      return 'warning';
    case RecoveryCaseStatus.InProgress:
      return 'pending';
    case RecoveryCaseStatus.Resolved:
      return 'active';
    case RecoveryCaseStatus.WrittenOff:
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
