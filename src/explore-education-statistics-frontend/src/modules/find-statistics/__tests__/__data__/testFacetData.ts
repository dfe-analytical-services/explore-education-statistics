import { FacetResult } from '@azure/search-documents';

const testFacetThemeResults: FacetResult[] = [
  {
    value: 'theme-1',
    count: 1,
  },
  {
    value: 'theme-2',
    count: 1,
  },
  {
    value: 'theme-3',
    count: 1,
  },
];
const testReleaseTypeFacetResults: FacetResult[] = [
  {
    value: 'AccreditedOfficialStatistics',
    count: 1,
  },
  {
    value: 'ExperimentalStatistics',
    count: 1,
  },
  {
    value: 'AdHocStatistics',
    count: 1,
  },
];

// eslint-disable-next-line import/prefer-default-export
export const testFacetResults = {
  releaseType: testReleaseTypeFacetResults,
  themeId: testFacetThemeResults,
};
// eslint-disable-next-line import/prefer-default-export
export const testFacetResultsSearched = {
  releaseType: [testReleaseTypeFacetResults[0], testReleaseTypeFacetResults[1]],
  themeId: [testFacetThemeResults[1], testFacetThemeResults[2]],
};
// eslint-disable-next-line import/prefer-default-export
export const testFacetNoResults = {
  releaseType: [],
  themeId: [],
};
