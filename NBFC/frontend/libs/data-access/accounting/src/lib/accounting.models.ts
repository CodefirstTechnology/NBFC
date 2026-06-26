export enum JournalEntryStatus {
  Draft = 0,
  Posted = 1,
  Reversed = 2,
}

export interface JournalEntrySummary {
  id: string;
  entryNumber: string;
  description: string;
  entryDate: string;
  debitAccountCode: string;
  creditAccountCode: string;
  amount: number;
  status: JournalEntryStatus;
}

export interface JournalEntryDetail {
  id: string;
  entryNumber: string;
  description: string;
  entryDate: string;
  debitAccountCode: string;
  creditAccountCode: string;
  amount: number;
  status: JournalEntryStatus;
  referenceType: string | null;
  referenceId: string | null;
  createdAt: string;
  modifiedAt: string | null;
}

export interface PagedJournalEntriesResponse {
  items: JournalEntrySummary[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ListJournalEntriesParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: JournalEntryStatus;
  fromDate?: string;
  toDate?: string;
}

export interface CreateJournalEntryRequest {
  description: string;
  entryDate: string;
  debitAccountCode: string;
  creditAccountCode: string;
  amount: number;
  referenceType?: string | null;
  referenceId?: string | null;
}

export function formatInr(amount: number): string {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 2,
  }).format(amount);
}

export function journalStatusLabel(status: JournalEntryStatus): string {
  return ['Draft', 'Posted', 'Reversed'][status] ?? 'Unknown';
}

export function journalStatusVariant(
  status: JournalEntryStatus
): 'active' | 'inactive' | 'pending' | 'warning' | 'error' {
  switch (status) {
    case JournalEntryStatus.Draft:
      return 'pending';
    case JournalEntryStatus.Posted:
      return 'active';
    case JournalEntryStatus.Reversed:
      return 'inactive';
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
