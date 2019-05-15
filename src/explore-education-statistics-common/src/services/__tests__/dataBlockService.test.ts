import dataBlockService, {
  DataBlockRequest,
  GeographicLevel,
} from '@common/services/dataBlockService';
import testData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';

import { dataApi } from '@common/services/api';
import { AxiosRequestConfig } from 'axios';

jest.mock('@common/services/api', () => {
  return {
    dataApi: {
      get: jest.fn(),
      post: jest.fn(),
    },
  };
});

describe('dataBlockService', () => {
  const dataBlockRequest: DataBlockRequest = {
    subjectId: 1,
    geographicLevel: GeographicLevel.National,
    startYear: '2014',
    endYear: '2015',
    filters: ['1', '2'],
    indicators: ['23', '26', '28'],
  };

  test('Call datablock service', async () => {
    // @ts-ignore
    dataApi.get.mockImplementation(() => {
      return Promise.resolve(testData.testBlockMetaData);
    });

    // @ts-ignore
    dataApi.post.mockImplementation(() => {
      return Promise.resolve(testData.testBlockData);
    });

    const result = await dataBlockService.getDataBlockForSubject(
      dataBlockRequest,
    );

    expect(dataApi.get).toBeCalledWith('/meta/subject/1');
    expect(dataApi.post).toBeCalledWith('/tablebuilder', dataBlockRequest);

    expect(result).toMatchSnapshot(result);
  });
});
