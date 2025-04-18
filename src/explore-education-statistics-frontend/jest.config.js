const nextJest = require('next/jest.js');

const createJestConfig = nextJest({
  dir: './',
});

/** @type {import('jest').Config} */
const config = {
  collectCoverageFrom: ['src/**/*.{ts,tsx}', '!src/**/*.d.ts'],
  setupFilesAfterEnv: ['<rootDir>/test/setupTests.js'],
  testMatch: [
    '<rootDir>/src/**/*.(spec|test).{js,jsx,ts,tsx}',
    '<rootDir>/test/**/*.(spec|test).{js,jsx,ts,tsx}',
  ],
  testEnvironment: 'jsdom',
  testEnvironmentOptions: {
    url: 'http://localhost/',
  },
  transform: {
    '^.+\\.css$':
      '<rootDir>/../explore-education-statistics-common/test/cssTransform.js',
    '^(?!.*\\.(js|jsx|ts|tsx|css|json)$)':
      '<rootDir>/../explore-education-statistics-common/test/fileTransform.js',
  },
  transformIgnorePatterns: [
    '[/\\\\]node_modules[/\\\\](?!react-leaflet)[/\\\\]',
  ],
  moduleNameMapper: {
    '^.+\\.module\\.(css|sass|scss)$': 'identity-obj-proxy',
    '^@frontend/(.*)$': '<rootDir>/src/$1',
    '^@frontend-test/(.*)$': '<rootDir>/test/$1',
    '^@common/(.*)$': '<rootDir>/../explore-education-statistics-common/src/$1',
    '^@common-test/(.*)$':
      '<rootDir>/../explore-education-statistics-common/test/$1',
    '^axios$': '<rootDir>/node_modules/axios/dist/axios.js',
    'react-markdown':
      '<rootDir>/node_modules/react-markdown/react-markdown.min.js',
  },
  moduleFileExtensions: [
    'web.js',
    'js',
    'web.ts',
    'ts',
    'web.tsx',
    'tsx',
    'json',
    'web.jsx',
    'jsx',
    'node',
  ],
  watchPlugins: [
    'jest-watch-typeahead/filename',
    'jest-watch-typeahead/testname',
  ],
  resetMocks: true,
  snapshotSerializers: ['jest-serializer-html'],
};

module.exports = createJestConfig(config);
