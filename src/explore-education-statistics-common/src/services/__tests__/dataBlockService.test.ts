/* eslint-disable prefer-destructuring */
import dataBlockService, {
  DataBlockGeoJSON,
  DataBlockRequest,
  GeographicLevel,
} from '@common/services/dataBlockService';
import testData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';

import { dataApi } from '@common/services/api';

jest.mock('@common/services/api', () => {
  return {
    dataApi: {
      post: jest.fn(),
    },
  };
});

describe('dataBlockService', () => {
  beforeEach(() => {
    // @ts-ignore
    dataApi.post.mockImplementation(() => {
      return Promise.resolve(testData.response);
    });
  });

  const dataBlockRequest: DataBlockRequest = {
    subjectId: 1,
    geographicLevel: GeographicLevel.National,
    startYear: '2014',
    endYear: '2015',
    filters: ['1', '2'],
    indicators: ['23', '26', '28'],
  };

  test('Call datablock service', async () => {
    const result = await dataBlockService.getDataBlockForSubject(
      dataBlockRequest,
    );

    expect(dataApi.post).toBeCalledWith('/Data', dataBlockRequest);

    expect(result).toMatchSnapshot(result);
  });

  test('DataBlock service returns geojson data', async () => {
    const result = await dataBlockService.getDataBlockForSubject(
      dataBlockRequest,
    );

    expect(dataApi.post).toBeCalledWith('/Data', dataBlockRequest);

    expect(result.metaData.locations).not.toBeUndefined();

    expect(result.metaData.locations.E92000001).not.toBeUndefined();

    // @ts-ignore
    const geoJson: DataBlockGeoJSON[] =
      result.metaData.locations.E92000001.geoJson;

    expect(geoJson.length).toBe(1);

    expect(geoJson[0].geometry).not.toBeUndefined();

    expect(geoJson[0].properties).not.toBeUndefined();
  });
});
