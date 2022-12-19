import mockObject from '@common-test/mockObject';

const { default: service, ...actual } = jest.requireActual(
  '../publicationService',
);

const mock = mockObject(service);

module.exports = {
  ...actual,
  default: mock,
  __esModule: true,
};
