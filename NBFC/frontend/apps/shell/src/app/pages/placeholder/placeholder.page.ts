import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PatsStatusPillComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-placeholder-page',
  standalone: true,
  imports: [PatsStatusPillComponent],
  template: `
    <section class="placeholder">
      <pats-status-pill [label]="title + ' Remote'" variant="pending" />
      <h1>{{ title }}</h1>
      <p>
        The <code>{{ remote }}</code> microfrontend will load here in a later build step via native
        federation.
      </p>
    </section>
  `,
  styles: [
    `
      .placeholder {
        display: flex;
        flex-direction: column;
        gap: 16px;
        padding: 32px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
      }

      .placeholder h1 {
        margin: 0;
        font-family: var(--pats-font-display);
      }

      .placeholder p {
        margin: 0;
        color: var(--pats-color-text-secondary);
        max-width: 640px;
      }
    `,
  ],
})
export class PlaceholderPageComponent {
  private readonly route = inject(ActivatedRoute);

  readonly title = this.route.snapshot.data['title'] as string;
  readonly remote = this.route.snapshot.data['remote'] as string;
}
