import { Directive, Input, TemplateRef, ViewContainerRef, inject, effect } from '@angular/core';
import { AuthService } from '../services/auth.service';

@Directive({
  selector: '[patsHasPermission]',
  standalone: true,
})
export class HasPermissionDirective {
  private readonly templateRef = inject(TemplateRef<unknown>);
  private readonly viewContainer = inject(ViewContainerRef);
  private readonly auth = inject(AuthService);

  @Input({ alias: 'patsHasPermission' }) set permission(value: string | string[]) {
    this.required = Array.isArray(value) ? value : [value];
    this.render();
  }

  private required: string[] = [];

  constructor() {
    effect(() => {
      this.auth.permissions();
      this.render();
    });
  }

  private render(): void {
    const allowed = this.required.some((permission) => this.auth.hasPermission(permission));

    this.viewContainer.clear();
    if (allowed) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    }
  }
}
