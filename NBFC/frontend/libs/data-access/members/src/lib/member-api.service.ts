import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AUTH_API_BASE_URL } from '@patsanstha/auth';
import {
  CreateMemberRequest,
  KycVerificationResult,
  ListMembersParams,
  MemberDetail,
  MemberDocumentType,
  PagedMembersResponse,
  SaveOnboardingDraftRequest,
} from './member.models';

const VALIDATION_MESSAGES: Record<string, string> = {
  'Members.Mobile.Invalid': 'Mobile must be 10 digits and start with 6, 7, 8, or 9.',
  'Members.PinCode.Invalid': 'PIN code must be exactly 6 digits.',
  'Members.Aadhaar.Invalid': 'Aadhaar must be exactly 12 digits.',
  'Members.Pan.Invalid': 'PAN must match format ABCDE1234F.',
  'Members.DateOfBirth.Invalid': 'Date of birth must be in the past.',
  'Members.Aadhaar.Exists': 'A member with this Aadhaar already exists.',
  'Members.Email.Invalid': 'Enter a valid email address or leave it blank.',
  'Members.Submit.Invalid': 'Complete all required steps before submitting.',
  validation_error: 'Check the highlighted fields and try again.',
  invalid_request_body: 'Invalid request format. Check date and field formats.',
};

@Injectable({ providedIn: 'root' })
export class MemberApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(AUTH_API_BASE_URL);

  private get membersUrl(): string {
    return `${this.apiBaseUrl}/members`;
  }

  list(params: ListMembersParams = {}): Promise<PagedMembersResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page ?? 1))
      .set('pageSize', String(params.pageSize ?? 20));

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params.status !== undefined) {
      httpParams = httpParams.set('status', String(params.status));
    }

    if (params.branchId) {
      httpParams = httpParams.set('branchId', params.branchId);
    }

    return firstValueFrom(
      this.http.get<PagedMembersResponse>(this.membersUrl, { params: httpParams })
    );
  }

  getById(id: string): Promise<MemberDetail> {
    return firstValueFrom(this.http.get<MemberDetail>(`${this.membersUrl}/${id}`));
  }

  create(request: CreateMemberRequest): Promise<MemberDetail> {
    return firstValueFrom(
      this.http.post<MemberDetail>(this.membersUrl, request, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      })
    );
  }

  saveDraft(request: SaveOnboardingDraftRequest): Promise<MemberDetail> {
    return firstValueFrom(
      this.http.post<MemberDetail>(`${this.membersUrl}/onboarding/draft`, request)
    );
  }

  submitOnboarding(memberId: string): Promise<MemberDetail> {
    return firstValueFrom(
      this.http.post<MemberDetail>(`${this.membersUrl}/${memberId}/submit`, {})
    );
  }

  uploadDocument(
    memberId: string,
    documentType: MemberDocumentType,
    file: File
  ): Promise<MemberDetail> {
    const formData = new FormData();
    formData.append('documentType', String(documentType));
    formData.append('file', file, file.name);

    return firstValueFrom(
      this.http.post<MemberDetail>(`${this.membersUrl}/${memberId}/documents`, formData)
    );
  }

  resolveMemberFileUrl(fileUrl: string | null | undefined): string | null {
    if (!fileUrl) {
      return null;
    }

    if (fileUrl.startsWith('http://') || fileUrl.startsWith('https://')) {
      return fileUrl;
    }

    const origin = this.apiBaseUrl.replace(/\/api\/v1\/?$/, '');
    return `${origin}${fileUrl.startsWith('/') ? fileUrl : `/${fileUrl}`}`;
  }

  verifyAadhaar(memberId: string, aadhaar: string): Promise<KycVerificationResult> {
    return firstValueFrom(
      this.http.post<KycVerificationResult>(
        `${this.membersUrl}/${memberId}/kyc/verify-aadhaar`,
        { aadhaar }
      )
    );
  }

  verifyPan(memberId: string, pan: string): Promise<KycVerificationResult> {
    return firstValueFrom(
      this.http.post<KycVerificationResult>(
        `${this.membersUrl}/${memberId}/kyc/verify-pan`,
        { pan }
      )
    );
  }
}

export function extractApiErrorMessage(error: unknown, fallback: string): string {
  if (typeof error === 'object' && error !== null && 'error' in error) {
    const problem = (error as { error?: { detail?: string; title?: string } }).error;
    if (problem?.title && VALIDATION_MESSAGES[problem.title]) {
      return VALIDATION_MESSAGES[problem.title];
    }
    return problem?.detail ?? problem?.title ?? fallback;
  }

  return fallback;
}

export function memberStatusLabel(status: number): string {
  return ['Pending', 'Active', 'Inactive', 'Suspended', 'Closed'][status] ?? 'Unknown';
}

export function memberStatusVariant(
  status: number
): 'active' | 'inactive' | 'pending' | 'warning' | 'error' {
  switch (status) {
    case 1:
      return 'active';
    case 0:
      return 'pending';
    case 2:
      return 'inactive';
    case 3:
      return 'warning';
    case 4:
      return 'error';
    default:
      return 'inactive';
  }
}

export function kycStatusLabel(status: number): string {
  return ['Pending', 'Verified', 'Failed'][status] ?? 'Unknown';
}
