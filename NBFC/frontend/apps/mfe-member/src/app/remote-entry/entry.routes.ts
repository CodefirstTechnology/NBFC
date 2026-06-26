import { Routes } from '@angular/router';

export const memberRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../pages/member-list/member-list.page').then((m) => m.MemberListPageComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('../pages/member-create/member-create.page').then((m) => m.MemberCreatePageComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('../pages/member-detail/member-detail.page').then((m) => m.MemberDetailPageComponent),
  },
];
