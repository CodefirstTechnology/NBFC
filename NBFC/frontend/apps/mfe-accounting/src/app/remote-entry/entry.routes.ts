import { Routes } from '@angular/router';

export const accountingRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../pages/journal-list/journal-list.page').then((m) => m.JournalListPageComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('../pages/journal-create/journal-create.page').then((m) => m.JournalCreatePageComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('../pages/journal-detail/journal-detail.page').then((m) => m.JournalDetailPageComponent),
  },
];
