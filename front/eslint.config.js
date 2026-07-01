import parser from '@typescript-eslint/parser';

export default [
  { ignores: ['dist/**', 'node_modules/**', 'coverage/**', 'e2e/**'] },
  {
    files: ['src/**/*.ts'],
    languageOptions: {
      parser,
    },
    rules: {
      'no-console': 'warn',
      'no-debugger': 'error',
    },
  },
];
