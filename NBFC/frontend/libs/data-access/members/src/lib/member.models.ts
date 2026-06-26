export enum MemberStatus {
  Pending = 0,
  Active = 1,
  Inactive = 2,
  Suspended = 3,
  Closed = 4,
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
  dateOfBirth: string;
  gender: string;
  mobileNumber: string;
  email: string | null;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  state: string;
  pinCode: string;
  aadhaarMasked: string;
  panMasked: string;
  nomineeName: string | null;
  nomineeRelation: string | null;
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
}

export interface ApiProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
}
