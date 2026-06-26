import { initFederation } from '@angular-architects/native-federation';

initFederation()
  .catch((err) => console.error('Native federation init failed', err))
  .then(() => import('./bootstrap'))
  .catch((err) => console.error('Bootstrap failed', err));
