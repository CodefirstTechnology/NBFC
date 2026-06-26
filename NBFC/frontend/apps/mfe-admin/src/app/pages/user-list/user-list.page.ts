import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  AdminApiService,
  UserSummary,
  extractApiErrorMessage,
} from '@patsanstha/admin-data-access';
import { PatsButtonComponent, PatsTableComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-user-list-page',
  standalone: true,
  imports: [FormsModule, RouterLink, HasPermissionDirective, PatsButtonComponent, PatsTableComponent],
  template: `
    <section class="page">
      <header class="page__header">
        <div>
          <h1>User Administration <span class="page__subtitle">/ वापरकर्ते</span></h1>
          <p>Manage platform users and their access.</p>
        </div>
        <div class="page__header-actions">
          <a routerLink="/admin/roles" class="page__link" *patsHasPermission="'admin.roles.manage'">Roles &amp; Permissions</a>
          <pats-button
            *patsHasPermission="'admin.users.manage'"
            icon="person_add"
            (clicked)="createUser()">
            New User
          </pats-button>
        </div>
      </header>

      <div class="page__toolbar">
        <input
          class="page__search"
          type="search"
          placeholder="Search by name or email…"
          [ngModel]="search()"
          (ngModelChange)="onSearchChange($event)" />
        <select
          class="page__filter"
          [ngModel]="activeFilter()"
          (ngModelChange)="onActiveFilterChange($event)">
          <option [ngValue]="null">All Users</option>
          <option [ngValue]="true">Active</option>
          <option [ngValue]="false">Inactive</option>
        </select>
      </div>

      @if (errorMessage()) {
        <p class="page__error">{{ errorMessage() }}</p>
      }

      <pats-table
        [columns]="columns"
        [rows]="tableRows()"
        [loading]="loading()"
        emptyMessage="No users found." />

      <footer class="page__pagination">
        <span>{{ totalCount() }} users</span>
        <div class="page__pagination-actions">
          <pats-button variant="ghost" size="sm" [disabled]="page() <= 1 || loading()" (clicked)="goToPage(page() - 1)">Previous</pats-button>
          <span>Page {{ page() }}</span>
          <pats-button variant="ghost" size="sm" [disabled]="!hasNextPage() || loading()" (clicked)="goToPage(page() + 1)">Next</pats-button>
        </div>
      </footer>
    </section>
  `,
  styles: [
    `
      .page { display: flex; flex-direction: column; gap: 24px; }
      .page__header { display: flex; align-items: flex-start; justify-content: space-between; gap: 16px; }
      .page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; color: var(--pats-color-primary); }
      .page__subtitle { font-size: 18px; color: var(--pats-color-text-secondary); font-weight: 500; }
      .page__header p { margin: 8px 0 0; color: var(--pats-color-text-secondary); }
      .page__header-actions { display: flex; align-items: center; gap: 12px; }
      .page__link { color: var(--pats-color-primary-container); font-weight: 600; font-size: 14px; text-decoration: none; }
      .page__toolbar { display: flex; flex-wrap: wrap; gap: 12px; }
      .page__search, .page__filter { min-height: 44px; padding: 0 16px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); background: var(--pats-color-surface-container-lowest); font-size: 14px; }
      .page__search { flex: 1; min-width: 240px; }
      .page__error { color: var(--pats-color-error); }
      .page__pagination { display: flex; align-items: center; justify-content: space-between; color: var(--pats-color-text-secondary); font-size: 14px; }
      .page__pagination-actions { display: flex; align-items: center; gap: 12px; }
    `,
  ],
})
export class UserListPageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly adminApi = inject(AdminApiService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly users = signal<UserSummary[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly search = signal('');
  readonly activeFilter = signal<boolean | null>(null);

  readonly columns = [
    { key: 'fullName', header: 'Name' },
    { key: 'email', header: 'Email' },
    { key: 'rolesLabel', header: 'Roles' },
    { key: 'activeLabel', header: 'Status' },
  ];

  readonly tableRows = computed(() =>
    this.users().map((user) => ({
      ...user,
      rolesLabel: user.roles.join(', ') || '—',
      activeLabel: user.isActive ? 'Active' : 'Inactive',
    }))
  );

  readonly hasNextPage = computed(() => this.page() * this.pageSize() < this.totalCount());

  ngOnInit(): void {
    void this.loadUsers();
  }

  onSearchChange(value: string): void {
    this.search.set(value);
    this.page.set(1);
    void this.loadUsers();
  }

  onActiveFilterChange(value: boolean | null): void {
    this.activeFilter.set(value);
    this.page.set(1);
    void this.loadUsers();
  }

  goToPage(nextPage: number): void {
    this.page.set(nextPage);
    void this.loadUsers();
  }

  createUser(): void {
    void this.router.navigate(['/admin/users/new']);
  }

  private async loadUsers(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);
    try {
      const response = await this.adminApi.listUsers({
        page: this.page(),
        pageSize: this.pageSize(),
        search: this.search() || undefined,
        isActive: this.activeFilter() ?? undefined,
      });
      this.users.set(response.items);
      this.totalCount.set(response.totalCount);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to load users.'));
    } finally {
      this.loading.set(false);
    }
  }
}
