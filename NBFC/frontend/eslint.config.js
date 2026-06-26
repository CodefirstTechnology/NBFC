const nx = require('@nx/eslint-plugin');

module.exports = [
  ...nx.configs['flat/base'],
  ...nx.configs['flat/typescript'],
  ...nx.configs['flat/javascript'],
  {
    files: ['**/*.ts', '**/*.tsx', '**/*.js', '**/*.jsx'],
    rules: {
      '@nx/enforce-module-boundaries': [
        'error',
        {
          enforceBuildableLibDependency: true,
          allow: [],
          depConstraints: [
            {
              sourceTag: 'type:app',
              onlyDependOnLibsWithTags: ['type:ui', 'type:auth', 'type:util', 'type:tokens', 'type:data-access']
            },
            {
              sourceTag: 'type:mfe',
              onlyDependOnLibsWithTags: ['type:ui', 'type:auth', 'type:util', 'type:tokens', 'type:data-access']
            },
            {
              sourceTag: 'type:data-access',
              onlyDependOnLibsWithTags: ['type:auth', 'type:tokens', 'type:util']
            },
            {
              sourceTag: 'type:ui',
              onlyDependOnLibsWithTags: ['type:tokens', 'type:util']
            },
            {
              sourceTag: 'type:auth',
              onlyDependOnLibsWithTags: ['type:util', 'type:tokens']
            },
            {
              sourceTag: 'type:util',
              onlyDependOnLibsWithTags: ['type:tokens']
            },
            {
              sourceTag: 'type:tokens',
              onlyDependOnLibsWithTags: []
            },
            {
              sourceTag: 'scope:shell',
              onlyDependOnLibsWithTags: ['scope:shared']
            },
            {
              sourceTag: 'scope:member',
              onlyDependOnLibsWithTags: ['scope:shared']
            },
            {
              sourceTag: 'scope:deposit',
              onlyDependOnLibsWithTags: ['scope:shared']
            },
            {
              sourceTag: 'scope:loan',
              onlyDependOnLibsWithTags: ['scope:shared']
            },
            {
              sourceTag: 'scope:collection',
              onlyDependOnLibsWithTags: ['scope:shared']
            },
            {
              sourceTag: 'scope:recovery',
              onlyDependOnLibsWithTags: ['scope:shared']
            },
            {
              sourceTag: 'scope:accounting',
              onlyDependOnLibsWithTags: ['scope:shared']
            },
            {
              sourceTag: 'scope:reports',
              onlyDependOnLibsWithTags: ['scope:shared']
            },
            {
              sourceTag: 'scope:admin',
              onlyDependOnLibsWithTags: ['scope:shared']
            },
            {
              sourceTag: 'scope:shared',
              onlyDependOnLibsWithTags: ['scope:shared']
            }
          ]
        }
      ]
    }
  },
  {
    ignores: ['**/dist', '**/node_modules', '**/.nx']
  }
];
