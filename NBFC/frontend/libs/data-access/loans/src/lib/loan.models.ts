export enum LoanProductType {
  Personal = 0,
  Gold = 1,
  Business = 2,
  Vehicle = 3,
}

export enum LoanApplicationStatus {
  Submitted = 0,
  UnderReview = 1,
  Approved = 2,
  Rejected = 3,
  Disbursed = 4,
  Closed = 5,
  Npa = 6,
}

export interface LoanApplicationSummary {
  id: string;
  loanNumber: string;
  memberName: string;
  memberNumber: string;
  productType: LoanProductType;
  requestedAmount: number;
  approvedAmount: number | null;
  emiAmount: number | null;
  status: LoanApplicationStatus;
  appliedOn: string;
}

export interface LoanApplicationDetail {
  id: string;
  memberId: string;
  memberNumber: string;
  memberName: string;
  branchId: string;
  loanNumber: string;
  productType: LoanProductType;
  requestedAmount: number;
  approvedAmount: number | null;
  interestRate: number;
  tenureMonths: number;
  emiAmount: number | null;
  outstandingPrincipal: number | null;
  purpose: string;
  status: LoanApplicationStatus;
  rejectionReason: string | null;
  appliedOn: string;
  approvedOn: string | null;
  disbursedOn: string | null;
  createdAt: string;
  modifiedAt: string | null;
}

export interface PagedLoansResponse {
  items: LoanApplicationSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ListLoansParams {
  page?: number;
  pageSize?: number;
  search?: string;
  productType?: LoanProductType;
  status?: LoanApplicationStatus;
  branchId?: string;
  memberId?: string;
}

export interface CreateLoanApplicationRequest {
  memberId: string;
  memberNumber: string;
  memberName: string;
  branchId: string;
  productType: LoanProductType;
  requestedAmount: number;
  tenureMonths: number;
  purpose: string;
}

export interface LoanProductInfo {
  productType: LoanProductType;
  label: string;
  labelMr: string;
  rate: number;
  maxTenureMonths: number;
  icon: string;
  iconTone: 'blue' | 'gold' | 'green';
}

export const LOAN_PRODUCTS: LoanProductInfo[] = [
  {
    productType: LoanProductType.Personal,
    label: 'Personal Loan',
    labelMr: 'व्यक्तिगत कर्ज',
    rate: 12,
    maxTenureMonths: 60,
    icon: 'person',
    iconTone: 'blue',
  },
  {
    productType: LoanProductType.Gold,
    label: 'Gold Loan',
    labelMr: 'सोने कर्ज',
    rate: 9,
    maxTenureMonths: 12,
    icon: 'savings',
    iconTone: 'gold',
  },
  {
    productType: LoanProductType.Business,
    label: 'Business Loan',
    labelMr: 'व्यवसाय कर्ज',
    rate: 10.5,
    maxTenureMonths: 84,
    icon: 'storefront',
    iconTone: 'blue',
  },
  {
    productType: LoanProductType.Vehicle,
    label: 'Agricultural Loan',
    labelMr: 'कृषी कर्ज',
    rate: 7,
    maxTenureMonths: 36,
    icon: 'agriculture',
    iconTone: 'green',
  },
];

export function getLoanProductMaxTenure(type: LoanProductType): number {
  return LOAN_PRODUCTS.find((p) => p.productType === type)?.maxTenureMonths ?? 60;
}

export function getLoanProductInfo(type: LoanProductType): LoanProductInfo | undefined {
  return LOAN_PRODUCTS.find((p) => p.productType === type);
}

export function formatInr(amount: number): string {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 2,
  }).format(amount);
}

export function loanProductLabel(type: LoanProductType): string {
  return LOAN_PRODUCTS.find((p) => p.productType === type)?.label ?? 'Unknown';
}

export function loanStatusLabel(status: LoanApplicationStatus): string {
  return [
    'Submitted',
    'Under Review',
    'Approved',
    'Rejected',
    'Disbursed',
    'Closed',
    'NPA',
  ][status] ?? 'Unknown';
}

export function loanStatusVariant(
  status: LoanApplicationStatus
): 'active' | 'inactive' | 'pending' | 'warning' | 'error' {
  switch (status) {
    case LoanApplicationStatus.Submitted:
    case LoanApplicationStatus.UnderReview:
      return 'pending';
    case LoanApplicationStatus.Approved:
    case LoanApplicationStatus.Disbursed:
      return 'active';
    case LoanApplicationStatus.Rejected:
    case LoanApplicationStatus.Npa:
      return 'error';
    case LoanApplicationStatus.Closed:
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
