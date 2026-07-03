export enum MemberStatus {
  Pending = 0,
  Active = 1,
  Inactive = 2,
  Suspended = 3,
  Closed = 4,
}

export enum KycVerificationStatus {
  Pending = 0,
  Verified = 1,
  Failed = 2,
}

export enum SharePaymentMode {
  Cash = 0,
  BankTransfer = 1,
  Cheque = 2,
}

export enum EmploymentType {
  Salaried = 0,
  SelfEmployed = 1,
  Business = 2,
  Retired = 3,
  Other = 4,
}

export enum MemberDocumentType {
  Photo = 0,
  AadhaarCard = 1,
  PanCard = 2,
}

export interface MemberDocument {
  id: string;
  documentType: MemberDocumentType;
  fileName: string;
  contentType: string;
  storageKey: string;
  fileUrl: string;
  fileSizeBytes: number;
  createdAt: string;
}

export interface MemberSummary {
  id: string;
  memberNumber: string;
  fullName: string;
  mobileNumber: string;
  branchId: string;
  status: MemberStatus;
  joinedOn: string;
}

export interface MemberDetail {
  id: string;
  memberNumber: string;
  fullName: string;
  dateOfBirth: string | null;
  gender: string;
  mobileNumber: string;
  email: string | null;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  state: string;
  pinCode: string;
  aadhaarMasked: string | null;
  panMasked: string | null;
  photoUrl: string | null;
  aadhaarVerificationStatus: KycVerificationStatus;
  panVerificationStatus: KycVerificationStatus;
  panVerifiedName: string | null;
  nomineeName: string | null;
  nomineeRelation: string | null;
  nomineeDateOfBirth: string | null;
  nomineeSharePercent: number;
  nomineeAddressSameAsMember: boolean;
  nomineeAddressLine1: string | null;
  nomineeAddressLine2: string | null;
  nomineeCity: string | null;
  nomineeState: string | null;
  nomineePinCode: string | null;
  numberOfShares: number | null;
  shareFaceValue: number;
  shareTotalAmount: number | null;
  sharePaymentMode: SharePaymentMode | null;
  employmentType: EmploymentType | null;
  occupation: string | null;
  employerName: string | null;
  monthlyIncome: number | null;
  onboardingStep: number;
  documents: MemberDocument[];
  branchId: string;
  status: MemberStatus;
  joinedOn: string;
  createdAt: string;
  modifiedAt: string | null;
}

export interface PagedMembersResponse {
  items: MemberSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ListMembersParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: MemberStatus;
  branchId?: string;
}

export interface CreateMemberRequest {
  branchId: string;
  fullName: string;
  dateOfBirth: string;
  gender: string;
  mobileNumber: string;
  email?: string | null;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state: string;
  pinCode: string;
  aadhaar: string;
  pan: string;
  nomineeName?: string | null;
  nomineeRelation?: string | null;
  nomineeDateOfBirth?: string | null;
  nomineeSharePercent?: number;
  nomineeAddressSameAsMember?: boolean;
  numberOfShares?: number | null;
  shareFaceValue?: number;
  sharePaymentMode?: SharePaymentMode | null;
  employmentType?: EmploymentType | null;
  occupation?: string | null;
  employerName?: string | null;
  monthlyIncome?: number | null;
}

export interface SaveOnboardingDraftRequest {
  memberId?: string | null;
  branchId: string;
  onboardingStep: number;
  fullName?: string | null;
  dateOfBirth?: string | null;
  gender?: string | null;
  mobileNumber?: string | null;
  email?: string | null;
  addressLine1?: string | null;
  addressLine2?: string | null;
  city?: string | null;
  state?: string | null;
  pinCode?: string | null;
  aadhaar?: string | null;
  pan?: string | null;
  nomineeName?: string | null;
  nomineeRelation?: string | null;
  nomineeDateOfBirth?: string | null;
  nomineeSharePercent?: number | null;
  nomineeAddressSameAsMember?: boolean | null;
  numberOfShares?: number | null;
  shareFaceValue?: number | null;
  sharePaymentMode?: SharePaymentMode | null;
  employmentType?: EmploymentType | null;
  occupation?: string | null;
  employerName?: string | null;
  monthlyIncome?: number | null;
}

export interface KycVerificationResult {
  status: KycVerificationStatus;
  verifiedName: string | null;
  message: string;
}

export interface ApiProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
}
