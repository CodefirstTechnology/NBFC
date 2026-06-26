const { withNativeFederation, shareAll } = require('@angular-architects/native-federation/config');

module.exports = withNativeFederation({
  name: 'shell',
  remotes: {
    'mfe-member': 'http://localhost:4201/remoteEntry.json',
    'mfe-deposit': 'http://localhost:4202/remoteEntry.json',
    'mfe-loan': 'http://localhost:4203/remoteEntry.json',
    'mfe-collection': 'http://localhost:4204/remoteEntry.json',
    'mfe-recovery': 'http://localhost:4205/remoteEntry.json',
    'mfe-accounting': 'http://localhost:4206/remoteEntry.json',
    'mfe-reports': 'http://localhost:4207/remoteEntry.json',
    'mfe-admin': 'http://localhost:4208/remoteEntry.json',
  },
  shared: {
    ...shareAll({
      singleton: true,
      strictVersion: true,
      requiredVersion: 'auto',
    }),
  },
  skip: ['rxjs/ajax', 'rxjs/fetch', 'rxjs/testing', 'rxjs/webSocket'],
});
