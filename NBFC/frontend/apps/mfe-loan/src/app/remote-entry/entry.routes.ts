import { Routes } from '@angular/router';

export const loanRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../pages/loan-list/loan-list.page').then((m) => m.LoanListPageComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('../pages/loan-create/loan-create.page').then((m) => m.LoanCreatePageComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('../pages/loan-detail/loan-detail.page').then((m) => m.LoanDetailPageComponent),
  },
];
