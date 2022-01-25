import { testRelease } from '@admin/pages/release/__data__/testRelease';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import _releaseService, { Release } from '@admin/services/releaseService';
import { ReleaseContextProvider } from '@admin/pages/release/contexts/ReleaseContext';
import { MemoryRouter } from 'react-router';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import ReleasePreReleaseAccessPage from '@admin/pages/release/pre-release/ReleasePreReleaseAccessPage';

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

const releaseData: Release = {
  id: 'release-1',
  slug: 'release-1-slug',
  approvalStatus: 'Draft',
  latestRelease: false,
  live: false,
  amendment: false,
  releaseName: 'Release 1',
  publicationId: 'publication-1',
  publicationTitle: 'Publication 1',
  publicationSlug: 'publication-1-slug',
  timePeriodCoverage: { value: 'W51', label: 'Week 51' },
  title: 'Release Title',
  type: 'OfficialStatistics',
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

describe('ReleasePreReleaseAccessPage', () => {
  test('renders prerelease page link correctly', async () => {
    releaseService.getRelease.mockResolvedValue(releaseData);

    render(
      <MemoryRouter>
        <TestConfigContextProvider>
          <ReleaseContextProvider release={testRelease}>
            <ReleasePreReleaseAccessPage />
          </ReleaseContextProvider>
        </TestConfigContextProvider>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.queryByTestId('prerelease-url')).toBeInTheDocument();
    });

    expect(screen.getByLabelText('Url')).toHaveValue(
      'http://localhost/publication/publication-1/release/release-1/prerelease/content',
    );
  });
});
