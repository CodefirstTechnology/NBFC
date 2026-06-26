import { Routes } from '@angular/router';

export const adminRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../pages/user-list/user-list.page').then((m) => m.UserListPageComponent),
  },
  {
    path: 'users/new',
    loadComponent: () =>
      import('../pages/user-create/user-create.page').then((m) => m.UserCreatePageComponent),
  },
  {
    path: 'roles',
    loadComponent: () =>
      import('../pages/roles/roles.page').then((m) => m.RolesPageComponent),
  },
];
