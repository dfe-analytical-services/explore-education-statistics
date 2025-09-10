import {
  ApiDataSet,
  ApiDataSetDraftVersion,
} from '@admin/services/apiDataSetService';
import shouldShowDraftActions from '../shouldShowDraftActions';

describe('showDraftVersionActions', () => {
  const testDraftDataSet: ApiDataSetDraftVersion = {
    status: 'Draft',
    id: '',
    version: '2.0',
    type: 'Major',
    file: { id: '', title: '' },
    releaseVersion: { id: '', title: '' },
    totalResults: 0,
  };
  const testApiDatafile: ApiDataSet = {
    draftVersion: testDraftDataSet,
    id: '',
    title: '',
    summary: '',
    status: 'Draft',
    previousReleaseIds: [],
  };

  test.each`
    isPatch  | canUpdateRelease | testCase
    ${true}  | ${true}          | ${'both isPatch and canUpdateRelease are enabled'}
    ${true}  | ${false}         | ${'isPatch is enabled but canUpdateRelease is disabled'}
    ${false} | ${false}         | ${'both isPatch and canUpdateRelease are disabled'}
  `(
    'hides the actions section if draftVersion is undefined when $testCase',
    ({ isPatch, canUpdateRelease }) => {
      expect(
        shouldShowDraftActions(isPatch, canUpdateRelease, {
          ...testApiDatafile,
          draftVersion: undefined,
        }),
      ).toBe(false);
    },
  );

  it('hides the actions section if draftVersion.status is Processing', () => {
    expect(
      shouldShowDraftActions(false, true, {
        ...testApiDatafile,
        draftVersion: { ...testDraftDataSet, status: 'Processing' },
      }),
    ).toBe(false);
  });

  it('shows the actions section if draftVersion.status is not Draft and isPatch is false and canUpdateRelease is true', () => {
    expect(
      shouldShowDraftActions(false, true, {
        ...testApiDatafile,
        draftVersion: { ...testDraftDataSet, status: 'Mapping' },
      }),
    ).toBe(true);
  });

  it('hides the actions section if draftVersion.status is not Draft and isPatch is true', () => {
    expect(
      shouldShowDraftActions(true, true, {
        ...testApiDatafile,
        draftVersion: { ...testDraftDataSet, status: 'Mapping' },
      }),
    ).toBe(false);
  });

  it('shows the actions section if status is Draft, isPatch is false, canUpdateRelease is true', () => {
    expect(shouldShowDraftActions(false, true, testApiDatafile)).toBe(true);
  });

  it('shows the actions section if status is Draft, isPatch is false, canUpdateRelease is false', () => {
    expect(shouldShowDraftActions(false, false, testApiDatafile)).toBe(true);
  });
});
