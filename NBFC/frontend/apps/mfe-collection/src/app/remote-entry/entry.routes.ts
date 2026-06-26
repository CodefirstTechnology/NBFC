import { Routes } from '@angular/router';

export const collectionRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../pages/collection-list/collection-list.page').then((m) => m.CollectionListPageComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('../pages/collection-create/collection-create.page').then((m) => m.CollectionCreatePageComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('../pages/collection-detail/collection-detail.page').then((m) => m.CollectionDetailPageComponent),
  },
];
