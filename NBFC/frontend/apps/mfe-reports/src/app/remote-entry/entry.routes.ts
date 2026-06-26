import { Routes } from '@angular/router';

export const reportsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../pages/report-list/report-list.page').then((m) => m.ReportListPageComponent),
  },
  {
    path: 'generate',
    loadComponent: () =>
      import('../pages/report-generate/report-generate.page').then((m) => m.ReportGeneratePageComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('../pages/report-detail/report-detail.page').then((m) => m.ReportDetailPageComponent),
  },
];
