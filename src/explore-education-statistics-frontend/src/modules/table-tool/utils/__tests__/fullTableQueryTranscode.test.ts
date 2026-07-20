import {
  encodeFullTableQueryToParams,
  decodeParamsToFullTableQuery,
} from '../fullTableQueryTranscode';

const testSubjectId = '12345678-abcd-ef01-2345-6789abcdef01';
const testSubjectIdEncoded = '12345678abcdef0123456789abcdef01';

const testFilter1 = 'abcdef01-2345-6789-abcd-ef0123456789';
const testFilter1Encoded = 'abcdef0123456789abcdef0123456789';
const testFilter2 = '23456789-abcd-ef01-2345-6789abcdef01';
const testFilter2Encoded = '23456789abcdef0123456789abcdef01';

const testIndicator1 = 'ef012345-6789-abcd-ef01-23456789abcd';
const testIndicator1Encoded = 'ef0123456789abcdef0123456789abcd';
const testIndicator2 = '6789abcd-ef01-2345-6789-abcdef012345';
const testIndicator2Encoded = '6789abcdef0123456789abcdef012345';

const testLocationId1 = '3456789a-bcde-f012-3456-789abcdef012';
const testLocationId1Encoded = '3456789abcdef0123456789abcdef012';
const testLocationId2 = 'bcdef012-3456-789a-bcde-f0123456789a';
const testLocationId2Encoded = 'bcdef0123456789abcdef0123456789a';

const testTimePeriod = {
  startYear: 2020,
  startCode: 'AY',
  endYear: 2021,
  endCode: 'AY',
};
const testTimePeriodEncoded = '2020|AY|2021|AY';

describe('fullTableQueryTranscode', () => {
  describe('encodeFullTableQueryToParams', () => {
    test('encodes a minimal query containing only subjectId', () => {
      const query = {
        subjectId: testSubjectId,
        filters: [],
        indicators: [],
        locationIds: [],
      };
      const result = encodeFullTableQueryToParams(query);
      expect(result).toBe(`sub=${testSubjectIdEncoded}`);
    });

    test('encodes a complete query containing all parameters', () => {
      const query = {
        subjectId: testSubjectId,
        timePeriod: testTimePeriod,
        filters: [testFilter1, testFilter2],
        indicators: [testIndicator1, testIndicator2],
        locationIds: [testLocationId1, testLocationId2],
      };
      const result = encodeFullTableQueryToParams(query);
      const expectedParams = new URLSearchParams({
        sub: testSubjectIdEncoded,
        tp: testTimePeriodEncoded,
        f: `${testFilter1Encoded},${testFilter2Encoded}`,
        ind: `${testIndicator1Encoded},${testIndicator2Encoded}`,
        loc: `${testLocationId1Encoded},${testLocationId2Encoded}`,
      }).toString();
      expect(result).toBe(expectedParams);
    });
  });

  describe('decodeParamsToFullTableQuery', () => {
    test('decodes minimal parameters containing only sub', () => {
      const params = new URLSearchParams({
        sub: testSubjectIdEncoded,
      });
      const result = decodeParamsToFullTableQuery(params);
      expect(result).toEqual({
        subjectId: testSubjectId,
        timePeriod: undefined,
        filters: [],
        indicators: [],
        locationIds: [],
      });
    });

    test('decodes complete parameters', () => {
      const params = new URLSearchParams({
        sub: testSubjectIdEncoded,
        tp: testTimePeriodEncoded,
        f: `${testFilter1Encoded},${testFilter2Encoded}`,
        ind: `${testIndicator1Encoded},${testIndicator2Encoded}`,
        loc: `${testLocationId1Encoded},${testLocationId2Encoded}`,
      });
      const result = decodeParamsToFullTableQuery(params);
      expect(result).toEqual({
        subjectId: testSubjectId,
        timePeriod: testTimePeriod,
        filters: [testFilter1, testFilter2],
        indicators: [testIndicator1, testIndicator2],
        locationIds: [testLocationId1, testLocationId2],
      });
    });

    test('handles missing parameters gracefully by providing empty arrays for list fields and undefined for timePeriod', () => {
      const params = new URLSearchParams();
      const result = decodeParamsToFullTableQuery(params);
      expect(result).toEqual({
        subjectId: '',
        timePeriod: undefined,
        filters: [],
        indicators: [],
        locationIds: [],
      });
    });
  });
});
