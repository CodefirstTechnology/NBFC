import { Routes } from '@angular/router';

export const depositRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../pages/deposit-list/deposit-list.page').then((m) => m.DepositListPageComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('../pages/deposit-create/deposit-create.page').then((m) => m.DepositCreatePageComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('../pages/deposit-detail/deposit-detail.page').then((m) => m.DepositDetailPageComponent),
  },
];
