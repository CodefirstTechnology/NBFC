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

export interface DashboardKpi {
  totalMembers: number;
  newMembersThisWeek: number;
  membersTrendPercent: number | null;
  activeLoansAmount: number;
  activeLoansCount: number;
  activeLoansTrendPercent: number | null;
  totalDepositsAmount: number;
  depositsTrendPercent: number | null;
  recoveryRatePercent: number;
  recoveryTargetPercent: number;
}

export interface MonthlyPerformancePoint {
  monthLabel: string;
  month: number;
  year: number;
  targetCollection: number;
  actualRecovery: number;
}

export interface RecentActivityItem {
  id: string;
  activityType: string;
  memberName: string;
  referenceNumber: string;
  amount: number | null;
  status: string;
  occurredAt: string;
}

export interface SystemStatus {
  isOnline: boolean;
  message: string;
  lastProcessedAt: string | null;
}

export interface ExecutiveDashboard {
  generatedAt: string;
  branchName: string;
  kpis: DashboardKpi;
  monthlyPerformance: MonthlyPerformancePoint[];
  recentActivity: RecentActivityItem[];
  systemStatus: SystemStatus;
}

export function formatInr(amount: number): string {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
}

export function formatCompactInr(amount: number): string {
  if (amount >= 10_000_000) {
    return `₹ ${(amount / 10_000_000).toFixed(2)} Cr`;
  }

  if (amount >= 100_000) {
    return `₹ ${(amount / 100_000).toFixed(2)} L`;
  }

  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(amount);
}

export function formatCount(value: number): string {
  return new Intl.NumberFormat('en-IN').format(value);
}

export function formatTrendPercent(value: number | null): string | null {
  if (value === null || Number.isNaN(value)) {
    return null;
  }

  const prefix = value > 0 ? '+' : '';
  return `${prefix}${value.toFixed(1)}%`;
}

export function formatRelativeTime(isoDate: string): string {
  const date = new Date(isoDate);
  const diffMs = Date.now() - date.getTime();
  const minutes = Math.floor(diffMs / 60_000);

  if (minutes < 1) return 'Just now';
  if (minutes < 60) return `${minutes} min${minutes === 1 ? '' : 's'} ago`;

  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} hour${hours === 1 ? '' : 's'} ago`;

  const days = Math.floor(hours / 24);
  return `${days} day${days === 1 ? '' : 's'} ago`;
}

export function memberInitials(name: string): string {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('');
}

export function activityStatusVariant(status: string): 'active' | 'pending' | 'error' | 'inactive' {
  switch (status) {
    case 'Completed':
      return 'active';
    case 'Pending Verification':
    case 'In Progress':
      return 'pending';
    case 'Rejected':
    case 'Reversed':
      return 'error';
    default:
      return 'inactive';
  }
}
