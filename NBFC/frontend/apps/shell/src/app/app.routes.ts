import { Routes } from '@angular/router';
import { authGuard, guestGuard } from '@patsanstha/auth';
import { loadFederatedRoutes } from '../federation-remotes';
import { ShellLayoutComponent } from './layout/shell-layout.component';

export const appRoutes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./pages/login/login.page').then((m) => m.LoginPageComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    component: ShellLayoutComponent,
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/dashboard/dashboard.page').then((m) => m.DashboardPageComponent),
      },
      {
        path: 'members',
        loadChildren: () => loadFederatedRoutes('mfe-member', 'memberRoutes'),
      },
      {
        path: 'deposits',
        loadChildren: () => loadFederatedRoutes('mfe-deposit', 'depositRoutes'),
      },
      {
        path: 'loans',
        loadChildren: () => loadFederatedRoutes('mfe-loan', 'loanRoutes'),
      },
      {
        path: 'collections',
        loadChildren: () => loadFederatedRoutes('mfe-collection', 'collectionRoutes'),
      },
      {
        path: 'recovery',
        loadChildren: () => loadFederatedRoutes('mfe-recovery', 'recoveryRoutes'),
      },
      {
        path: 'accounting',
        loadChildren: () => loadFederatedRoutes('mfe-accounting', 'accountingRoutes'),
      },
      {
        path: 'reports',
        loadChildren: () => loadFederatedRoutes('mfe-reports', 'reportsRoutes'),
      },
      {
        path: 'admin',
        loadChildren: () => loadFederatedRoutes('mfe-admin', 'adminRoutes'),
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
