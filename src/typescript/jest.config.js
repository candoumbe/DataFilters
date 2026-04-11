/** @type {import('jest').Config} */
const config = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  roots: ['<rootDir>/test'],
  testMatch: ['**/*.test.ts'],
  collectCoverageFrom: ['src/**/*.ts', '!src/**/*.d.ts'],
  coverageDirectory: 'coverage',
  coverageReporters: ['text', 'lcov', 'clover'],
  // chevrotain is a pure-ESM package. We pre-bundle it to CJS in the global
  // setup step and redirect imports to the generated bundle.
  globalSetup: '<rootDir>/jest.global-setup.cjs',
  moduleNameMapper: {
    '^chevrotain$': '<rootDir>/.jest-cache/chevrotain-cjs.js',
  },
};

module.exports = config;
