import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import _releaseService, { Release } from '@admin/services/releaseService';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/contexts/ManageReleaseContext';
import noop from 'lodash/noop';
import { MemoryRouter } from 'react-router';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import ReleaseStatusPage from '../ReleaseStatusPage';

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

const releaseData: Release = {
  id: 'release-1',
  slug: 'release-1-slug',
  status: 'Draft',
  latestRelease: false,
  live: false,
  amendment: false,
  releaseName: 'Release 1',
  publicationId: 'publication-1',
  publicationTitle: 'Publication 1',
  publicationSlug: 'publication-1-slug',
  timePeriodCoverage: { value: 'W51', label: 'Week 51' },
  title: 'Release Title',
  type: {
    id: '9d333457-9132-4e55-ae78-c55cb3673d7c',
    title: 'Official Statistics',
  },
  contact: {
    id: '69416b29-8a20-4de1-11de-08d85fc33fc0',
    teamName: 'Test name',
    teamEmail: 'test@test.com',
    contactName: 'Test contact name',
    contactTelNo: '1111 1111 1111',
  },
  nextReleaseDate: {
    day: 1,
    month: 1,
    year: 2020,
  },
  previousVersionId: '',
  preReleaseAccessList: '',
};

describe('ReleaseStatusPage', () => {
  const testRelease: ManageRelease = {
    releaseId: 'release-1',
    onChangeReleaseStatus: noop,
    publication: {
      id: 'publication-1',
      themeId: 'theme-1',
      topicId: 'topic-1',
      title: 'Test publication',
      legacyReleases: [],
    },
  };

  test('renders public release link correctly', async () => {
    releaseService.getRelease.mockResolvedValue(releaseData);

    render(
      <MemoryRouter>
        <TestConfigContextProvider>
          <ManageReleaseContext.Provider value={testRelease}>
            <ReleaseStatusPage />
          </ManageReleaseContext.Provider>
        </TestConfigContextProvider>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.queryByTestId('public-release-url')).toBeInTheDocument();
    });

    expect(screen.getByTestId('public-release-url')).toHaveTextContent(
      'http://localhost/find-statistics/publication-1-slug/release-1-slug',
    );
  });
});
