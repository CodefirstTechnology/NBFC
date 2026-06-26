import { Routes } from '@angular/router';
import { loadRemoteModule } from '@angular-architects/native-federation';
import { authGuard, guestGuard } from '@patsanstha/auth';
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
        loadChildren: () =>
          loadRemoteModule('mfe-member', './Routes').then((m) => m.memberRoutes),
      },
      {
        path: 'deposits',
        loadChildren: () =>
          loadRemoteModule('mfe-deposit', './Routes').then((m) => m.depositRoutes),
      },
      {
        path: 'loans',
        loadChildren: () =>
          loadRemoteModule('mfe-loan', './Routes').then((m) => m.loanRoutes),
      },
      {
        path: 'collections',
        loadChildren: () =>
          loadRemoteModule('mfe-collection', './Routes').then((m) => m.collectionRoutes),
      },
      {
        path: 'recovery',
        loadChildren: () =>
          loadRemoteModule('mfe-recovery', './Routes').then((m) => m.recoveryRoutes),
      },
      {
        path: 'accounting',
        loadChildren: () =>
          loadRemoteModule('mfe-accounting', './Routes').then((m) => m.accountingRoutes),
      },
      {
        path: 'reports',
        loadChildren: () =>
          loadRemoteModule('mfe-reports', './Routes').then((m) => m.reportsRoutes),
      },
      {
        path: 'admin',
        loadChildren: () =>
          loadRemoteModule('mfe-admin', './Routes').then((m) => m.adminRoutes),
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
