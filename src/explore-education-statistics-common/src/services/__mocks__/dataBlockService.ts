import mockObject from '@common-test/mockObject';

const actual = jest.requireActual('../dataBlockService');

const { GeographicLevel } = actual;

const service = actual.default;

const mock = mockObject(service);

export default mock;

export { GeographicLevel };
