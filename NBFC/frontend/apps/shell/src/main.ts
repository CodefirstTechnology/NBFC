import { initFederation } from '@angular-architects/native-federation';

// Init without manifest — remotes load lazily when a route is visited (avoids OOM).
initFederation()
  .catch((err) => console.error('Native federation init failed', err))
  .then(() => import('./bootstrap'))
  .catch((err) => console.error('Bootstrap failed', err));
