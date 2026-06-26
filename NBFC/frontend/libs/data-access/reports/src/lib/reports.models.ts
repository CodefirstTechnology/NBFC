export enum ReportType {
  BranchSummary = 0,
  LoanPortfolio = 1,
  CollectionsDaily = 2,
  NpaSummary = 3,
}

export enum ReportSnapshotStatus {
  Pending = 0,
  Completed = 1,
  Failed = 2,
}

export interface ReportSnapshotSummary {
  id: string;
  reportType: ReportType;
  title: string;
  status: ReportSnapshotStatus;
  generatedAt: string;
  generatedByUserId: string | null;
}

export interface ReportSnapshotDetail {
  id: string;
  reportType: ReportType;
  title: string;
  parametersJson: string;
  resultJson: string;
  status: ReportSnapshotStatus;
  generatedAt: string;
  generatedByUserId: string | null;
  createdAt: string;
  modifiedAt: string | null;
}

export interface PagedReportSnapshotsResponse {
  items: ReportSnapshotSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ListReportSnapshotsParams {
  page?: number;
  pageSize?: number;
  reportType?: ReportType;
  status?: ReportSnapshotStatus;
}

export interface GenerateReportRequest {
  reportType: ReportType;
  title: string;
  parametersJson?: string;
}

export const REPORT_TYPES = [
  { reportType: ReportType.BranchSummary, label: 'Branch Summary' },
  { reportType: ReportType.LoanPortfolio, label: 'Loan Portfolio' },
  { reportType: ReportType.CollectionsDaily, label: 'Collections Daily' },
  { reportType: ReportType.NpaSummary, label: 'NPA Summary' },
] as const;

export function reportTypeLabel(type: ReportType): string {
  return REPORT_TYPES.find((r) => r.reportType === type)?.label ?? 'Unknown';
}

export function reportStatusLabel(status: ReportSnapshotStatus): string {
  return ['Pending', 'Completed', 'Failed'][status] ?? 'Unknown';
}

export function reportStatusVariant(
  status: ReportSnapshotStatus
): 'active' | 'inactive' | 'pending' | 'warning' | 'error' {
  switch (status) {
    case ReportSnapshotStatus.Pending:
      return 'pending';
    case ReportSnapshotStatus.Completed:
      return 'active';
    case ReportSnapshotStatus.Failed:
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
