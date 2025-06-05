// eslint-disable-next-line import/no-import-module-exports
import mockObject from '@common-test/mockObject';

const { default: service, ...actual } = jest.requireActual(
  '../azurePublicationService',
);

const mock = mockObject(service);

module.exports = {
  ...actual,
  default: mock,
  __esModule: true,
};
