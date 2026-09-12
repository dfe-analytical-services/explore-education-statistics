import { FindStatisticsPageQuery } from '@frontend/modules/find-statistics/FindStatisticsPage';
import createPublicationListRequest, {
  createPublicationSuggestRequest,
} from '@frontend/modules/find-statistics/utils/createPublicationListRequest';

describe('createPublicationListRequest', () => {
  test('does not include a raw OData filter when release type or theme filters are provided', () => {
    const testQuery: FindStatisticsPageQuery = {
      releaseType: 'AdHocStatistics',
      themeId: 'theme-1',
    };

    expect(createPublicationListRequest(testQuery)).toEqual({
      orderBy: 'published desc',
      page: 1,
      releaseType: 'AdHocStatistics',
      search: '',
      themeId: 'theme-1',
    });
  });

  test('does not include a raw OData filter in suggest requests', () => {
    const testQuery: FindStatisticsPageQuery = {
      releaseType: 'AdHocStatistics',
      themeId: 'theme-1',
    };

    expect(createPublicationSuggestRequest(testQuery, 'absence')).toEqual({
      releaseType: 'AdHocStatistics',
      search: 'absence',
      themeId: 'theme-1',
    });
  });
});
