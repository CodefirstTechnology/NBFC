import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  MemberApiService,
  MemberDetail,
  extractApiErrorMessage,
  memberStatusLabel,
  memberStatusVariant,
} from '@patsanstha/members-data-access';
import { PatsStatusPillComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-member-detail-page',
  standalone: true,
  imports: [RouterLink, PatsStatusPillComponent],
  template: `
    <section class="detail-page">
      <a routerLink="/members" class="detail-page__back">
        <span class="material-symbols-outlined">arrow_back</span>
        Back to directory
      </a>

      @if (loading()) {
        <p>Loading member…</p>
      } @else if (errorMessage()) {
        <p class="detail-page__error">{{ errorMessage() }}</p>
      } @else {
        @if (member(); as m) {
        <header class="detail-page__header">
          <div>
            <p class="detail-page__eyebrow">{{ m.memberNumber }}</p>
            <h1>{{ m.fullName }}</h1>
            <p>{{ m.city }}, {{ m.state }}</p>
          </div>
          <pats-status-pill [label]="statusLabel()" [variant]="statusVariant()" />
        </header>

        <div class="detail-page__grid">
          <article class="detail-page__card">
            <h2>Contact</h2>
            <dl>
              <dt>Mobile</dt><dd>{{ m.mobileNumber }}</dd>
              <dt>Email</dt><dd>{{ m.email || '—' }}</dd>
            </dl>
          </article>

          <article class="detail-page__card">
            <h2>Identity</h2>
            <dl>
              <dt>Aadhaar</dt><dd>{{ m.aadhaarMasked }}</dd>
              <dt>PAN</dt><dd>{{ m.panMasked }}</dd>
              <dt>Date of Birth</dt><dd>{{ m.dateOfBirth }}</dd>
              <dt>Gender</dt><dd>{{ m.gender }}</dd>
            </dl>
          </article>

          <article class="detail-page__card">
            <h2>Address</h2>
            <dl>
              <dt>Line 1</dt><dd>{{ m.addressLine1 }}</dd>
              <dt>Line 2</dt><dd>{{ m.addressLine2 || '—' }}</dd>
              <dt>City</dt><dd>{{ m.city }}</dd>
              <dt>State</dt><dd>{{ m.state }}</dd>
              <dt>PIN</dt><dd>{{ m.pinCode }}</dd>
            </dl>
          </article>

          <article class="detail-page__card">
            <h2>Nominee</h2>
            <dl>
              <dt>Name</dt><dd>{{ m.nomineeName || '—' }}</dd>
              <dt>Relation</dt><dd>{{ m.nomineeRelation || '—' }}</dd>
              <dt>Joined</dt><dd>{{ m.joinedOn }}</dd>
              <dt>Branch ID</dt><dd>{{ m.branchId }}</dd>
            </dl>
          </article>
        </div>
        }
      }
    </section>
  `,
  styles: [
    `
      .detail-page {
        display: flex;
        flex-direction: column;
        gap: 24px;
      }

      .detail-page__back {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        color: var(--pats-color-primary-container);
        font-size: 14px;
        font-weight: 600;
      }

      .detail-page__header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 16px;
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        border-top: 4px solid var(--pats-color-primary-container);
        box-shadow: var(--pats-shadow-card);
      }

      .detail-page__eyebrow {
        margin: 0 0 4px;
        font-size: 13px;
        font-weight: 600;
        color: var(--pats-color-text-secondary);
        text-transform: uppercase;
        letter-spacing: 0.04em;
      }

      .detail-page__header h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
      }

      .detail-page__header p {
        margin: 8px 0 0;
        color: var(--pats-color-text-secondary);
      }

      .detail-page__grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
        gap: 16px;
      }

      .detail-page__card {
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
      }

      .detail-page__card h2 {
        margin: 0 0 16px;
        font-family: var(--pats-font-display);
        font-size: 18px;
        color: var(--pats-color-primary-container);
      }

      dl {
        margin: 0;
        display: grid;
        gap: 12px;
      }

      dt {
        font-size: 11px;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.04em;
        color: var(--pats-color-text-secondary);
      }

      dd {
        margin: 0;
        font-size: 15px;
      }

      .detail-page__error {
        color: var(--pats-color-error);
      }
    `,
  ],
})
export class MemberDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly memberApi = inject(MemberApiService);

  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly member = signal<MemberDetail | null>(null);
  readonly statusLabel = signal('');
  readonly statusVariant = signal<'active' | 'inactive' | 'pending' | 'warning' | 'error'>('active');

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage.set('Member id is missing.');
      this.loading.set(false);
      return;
    }

    void this.loadMember(id);
  }

  private async loadMember(id: string): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      const detail = await this.memberApi.getById(id);
      this.member.set(detail);
      this.statusLabel.set(memberStatusLabel(detail.status));
      this.statusVariant.set(memberStatusVariant(detail.status));
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Member not found.'));
    } finally {
      this.loading.set(false);
    }
  }
}
