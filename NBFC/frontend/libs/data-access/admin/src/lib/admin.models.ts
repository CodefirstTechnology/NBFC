export interface UserSummary {
  id: string;
  email: string;
  fullName: string;
  tenantId: string;
  branchId: string | null;
  isActive: boolean;
  roles: string[];
}

export interface PagedUsersResponse {
  items: UserSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ListUsersParams {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  fullName: string;
  branchId?: string | null;
  roles: string[];
}

export interface UpdateUserRequest {
  fullName?: string | null;
  branchId?: string | null;
  isActive?: boolean | null;
}

export interface AssignRolesRequest {
  roles: string[];
}

export interface RoleSummary {
  id: string;
  name: string;
  permissions: string[];
}

export function extractApiErrorMessage(error: unknown, fallback: string): string {
  if (typeof error === 'object' && error !== null && 'error' in error) {
    const problem = (error as { error?: { detail?: string; title?: string } }).error;
    return problem?.detail ?? problem?.title ?? fallback;
  }

  return fallback;
}
