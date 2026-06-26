import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  MemberApiService,
  MemberSummary,
  extractApiErrorMessage,
  memberStatusLabel,
} from '@patsanstha/members-data-access';
import { PatsButtonComponent, PatsTableComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-member-list-page',
  standalone: true,
  imports: [
    FormsModule,
    HasPermissionDirective,
    PatsButtonComponent,
    PatsTableComponent,
  ],
  template: `
    <section class="members-page">
      <header class="members-page__header">
        <div>
          <h1>Member Directory <span class="members-page__subtitle">/ सभासद नोंदणी</span></h1>
          <p>Search and manage cooperative society members.</p>
        </div>
        <pats-button
          *patsHasPermission="'members.create'"
          icon="person_add"
          (clicked)="createMember()">
          Add Member
        </pats-button>
      </header>

      <div class="members-page__toolbar">
        <input
          class="members-page__search"
          type="search"
          placeholder="Search by name, member no., mobile…"
          [ngModel]="search()"
          (ngModelChange)="onSearchChange($event)" />
      </div>

      @if (errorMessage()) {
        <p class="members-page__error">{{ errorMessage() }}</p>
      }

      <pats-table
        [columns]="columns"
        [rows]="tableRows()"
        [loading]="loading()"
        [rowClickable]="true"
        emptyMessage="No members found."
        (rowClicked)="openMember($event)" />

      <footer class="members-page__pagination">
        <span>{{ totalCount() }} members</span>
        <div class="members-page__pagination-actions">
          <pats-button
            variant="ghost"
            size="sm"
            [disabled]="page() <= 1 || loading()"
            (clicked)="goToPage(page() - 1)">
            Previous
          </pats-button>
          <span>Page {{ page() }}</span>
          <pats-button
            variant="ghost"
            size="sm"
            [disabled]="!hasNextPage() || loading()"
            (clicked)="goToPage(page() + 1)">
            Next
          </pats-button>
        </div>
      </footer>
    </section>
  `,
  styles: [
    `
      .members-page {
        display: flex;
        flex-direction: column;
        gap: 24px;
      }

      .members-page__header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 16px;
      }

      .members-page__header h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
        color: var(--pats-color-primary);
      }

      .members-page__subtitle {
        font-size: 20px;
        font-weight: 400;
        color: var(--pats-color-on-surface-variant);
      }

      .members-page__header p {
        margin: 8px 0 0;
        color: var(--pats-color-text-secondary);
      }

      .members-page__toolbar {
        display: flex;
        gap: 12px;
      }

      .members-page__search {
        flex: 1;
        min-height: var(--pats-touch-target);
        padding: 0 16px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-container-lowest);
        font-size: 16px;
      }

      .members-page__search:focus {
        outline: none;
        border-color: var(--pats-color-primary-container);
        box-shadow: 0 0 0 2px rgba(26, 60, 110, 0.12);
      }

      .members-page__error {
        margin: 0;
        color: var(--pats-color-error);
      }

      .members-page__pagination {
        display: flex;
        align-items: center;
        justify-content: space-between;
        color: var(--pats-color-text-secondary);
        font-size: 14px;
      }

      .members-page__pagination-actions {
        display: flex;
        align-items: center;
        gap: 12px;
      }
    `,
  ],
})
export class MemberListPageComponent implements OnInit {
  private readonly memberApi = inject(MemberApiService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly members = signal<MemberSummary[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly search = signal('');
  readonly hasNextPage = signal(false);

  readonly columns = [
    { key: 'memberNumber', header: 'Member No.' },
    { key: 'fullName', header: 'Name' },
    { key: 'mobileNumber', header: 'Mobile' },
    { key: 'statusLabel', header: 'Status' },
    { key: 'joinedOn', header: 'Joined' },
  ];

  readonly tableRows = signal<
    Array<MemberSummary & { statusLabel: string; joinedOn: string }>
  >([]);

  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    void this.loadMembers();
  }

  onSearchChange(value: string): void {
    this.search.set(value);

    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }

    this.searchTimer = setTimeout(() => {
      this.page.set(1);
      void this.loadMembers();
    }, 300);
  }

  async goToPage(nextPage: number): Promise<void> {
    this.page.set(nextPage);
    await this.loadMembers();
  }

  openMember(row: MemberSummary & { statusLabel?: string }): void {
    void this.router.navigate(['/members', row.id]);
  }

  createMember(): void {
    void this.router.navigate(['/members/new']);
  }

  private async loadMembers(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      const response = await this.memberApi.list({
        page: this.page(),
        pageSize: this.pageSize(),
        search: this.search().trim() || undefined,
      });

      this.members.set(response.items);
      this.totalCount.set(response.totalCount);
      this.hasNextPage.set(this.page() * this.pageSize() < response.totalCount);

      this.tableRows.set(
        response.items.map((member) => ({
          ...member,
          statusLabel: memberStatusLabel(member.status),
          joinedOn: member.joinedOn,
        }))
      );
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Unable to load members.'));
      this.tableRows.set([]);
    } finally {
      this.loading.set(false);
    }
  }
}
