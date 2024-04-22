const nextJest = require('next/jest.js');

const createJestConfig = nextJest({
  // Provide the path to your Next.js app to load next.config.js and .env files in your test environment
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
    '^.+\\.(t|j)sx?$': '@swc/jest',
    '^.+\\.css$':
      '<rootDir>/../explore-education-statistics-common/test/cssTransform.js',
    '^(?!.*\\.(js|jsx|ts|tsx|css|json)$)':
      '<rootDir>/../explore-education-statistics-common/test/fileTransform.js',
  },
  transformIgnorePatterns: [
    '[/\\\\]node_modules[/\\\\].+\\.(js|jsx|ts|tsx)$',
    '^.+\\.module\\.(css|sass|scss)$',
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
    'react-leaflet': '<rootDir>/__mocks__/reactLeafletMock.ts',
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
