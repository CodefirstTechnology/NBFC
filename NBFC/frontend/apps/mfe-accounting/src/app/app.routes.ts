import { Routes } from '@angular/router';

export const appRoutes: Routes = [
  {
    path: '',
    loadChildren: () => import('./remote-entry/entry.routes').then((m) => m.accountingRoutes),
  },
];
