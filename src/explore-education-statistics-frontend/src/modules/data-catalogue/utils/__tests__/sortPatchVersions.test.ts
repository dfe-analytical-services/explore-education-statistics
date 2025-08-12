import { ApiDataSetVersionChanges } from '@common/services/types/apiDataSetChanges';
import sortVersionChanges from '@frontend/modules/data-catalogue/utils/sortVersionChanges';

describe('change log versions sorting order using sortVersionChanges utiltiy function', () => {
  test('sorts a mix of versions in descending order', () => {
    const versions: ApiDataSetVersionChanges[] = [
      {
        versionNumber: '3.0.2',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '3.0.4',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '3.0.1',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '3.0.3',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '1.0',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '3.0',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '2.0',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '1.3.3',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '1.3.2',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '1.3.4',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '1.3.1',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '1.3',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '1.2',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
      {
        versionNumber: '1.1',
        majorChanges: {},
        minorChanges: {},
        patchHistory: [],
      },
    ];
    const sorted = sortVersionChanges(versions);

    expect(sorted.map(v => v.versionNumber)).toEqual([
      '3.0.4',
      '3.0.3',
      '3.0.2',
      '3.0.1',
      '3.0',
      '2.0',
      '1.3.4',
      '1.3.3',
      '1.3.2',
      '1.3.1',
      '1.3',
      '1.2',
      '1.1',
      '1.0',
    ]);
  });
});
