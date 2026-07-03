import { loadRemoteModule } from '@angular-architects/native-federation';
import type { Route } from '@angular/router';

/** Remote entry URLs — loaded on demand per route, not at shell startup. */
export const REMOTE_ENTRIES = {
  'mfe-member': 'http://localhost:4201/remoteEntry.json',
  'mfe-deposit': 'http://localhost:4202/remoteEntry.json',
  'mfe-loan': 'http://localhost:4203/remoteEntry.json',
  'mfe-collection': 'http://localhost:4204/remoteEntry.json',
  'mfe-recovery': 'http://localhost:4205/remoteEntry.json',
  'mfe-accounting': 'http://localhost:4206/remoteEntry.json',
  'mfe-reports': 'http://localhost:4207/remoteEntry.json',
  'mfe-admin': 'http://localhost:4208/remoteEntry.json',
} as const;

export type RemoteName = keyof typeof REMOTE_ENTRIES;

export function loadFederatedRoutes<T extends Route[]>(
  remoteName: RemoteName,
  routesExport: string
): Promise<T> {
  return loadRemoteModule({
    remoteName,
    remoteEntry: REMOTE_ENTRIES[remoteName],
    exposedModule: './Routes',
  }).then((module) => module[routesExport] as T);
}
