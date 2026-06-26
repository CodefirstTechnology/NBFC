import { Routes } from '@angular/router';

export const recoveryRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../pages/recovery-list/recovery-list.page').then((m) => m.RecoveryListPageComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('../pages/recovery-create/recovery-create.page').then((m) => m.RecoveryCreatePageComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('../pages/recovery-detail/recovery-detail.page').then((m) => m.RecoveryDetailPageComponent),
  },
];
