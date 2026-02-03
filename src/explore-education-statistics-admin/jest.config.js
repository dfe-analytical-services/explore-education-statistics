/** @type {import('jest').Config} */
const config = {
  collectCoverageFrom: ['src/**/*.{js,jsx,ts,tsx}', '!src/**/*.d.ts'],
  verbose: true,
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
    '^.+\\.(t|j)sx?$': 'babel-jest',
    '^.+\\.css$': '<rootDir>/config/jest/cssTransform.js',
    '^(?!.*\\.(js|jsx|ts|tsx|css|json)$)':
      '<rootDir>/config/jest/fileTransform.js',
  },
  transformIgnorePatterns: [
    '[/\\\\]node_modules[/\\\\](?!react-leaflet)[/\\\\]',
    '^.+\\.module\\.(css|sass|scss)$',
  ],
  moduleNameMapper: {
    '^.+\\.module\\.(css|sass|scss)$': 'identity-obj-proxy',
    '^@admin/(.*)$': '<rootDir>/src/$1',
    '^@admin-test/(.*)$':
      '<rootDir>/../explore-education-statistics-admin/test/$1',
    '^@common/(.*)$': '<rootDir>/../explore-education-statistics-common/src/$1',
    '^@common-test/(.*)$':
      '<rootDir>/../explore-education-statistics-common/test/$1',
    '^axios$': '<rootDir>/node_modules/axios/dist/axios.js',
    '^marked$': '<rootDir>/node_modules/marked/lib/marked.umd.js',
    '^@hello-pangea/dnd$':
      '<rootDir>/node_modules/@hello-pangea/dnd/dist/dnd.js',
    // Stub out CKEditor to prevent errors from its CSS build polluting the
    // test output.
    '^explore-education-statistics-ckeditor$': '<rootDir>/test/stub.js',
    // Stub out MSAL React library.
    '@azure/msal-react': '<rootDir>/test/stub.js',
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
  resetMocks: true,
  snapshotSerializers: ['jest-serializer-html'],
  watchPlugins: [
    'jest-watch-typeahead/filename',
    'jest-watch-typeahead/testname',
  ],
};

module.exports = config;
