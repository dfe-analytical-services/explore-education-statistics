import mockObject from '@common-test/mockObject';

const actual = jest.requireActual('../Client');

const service = actual.default;

const mock = mockObject(service);

export default mock;
