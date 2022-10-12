import { testRelease } from '@admin/pages/release/__data__/testRelease';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import _releaseService, { Release } from '@admin/services/releaseService';
import _permissionService from '@admin/services/permissionService';
import _preReleaseUserService from '@admin/services/preReleaseUserService';
import { ReleaseContextProvider } from '@admin/pages/release/contexts/ReleaseContext';
import { MemoryRouter } from 'react-router';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import ReleasePreReleaseAccessPage from '@admin/pages/release/pre-release/ReleasePreReleaseAccessPage';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;
jest.mock('@admin/services/permissionService');
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;
jest.mock('@admin/services/preReleaseUserService');
const preReleaseUserService = _preReleaseUserService as jest.Mocked<
  typeof _preReleaseUserService
>;

const releaseData: Release = {
  id: 'release-1',
  slug: 'release-1-slug',
  approvalStatus: 'Draft',
  latestRelease: false,
  live: false,
  amendment: false,
  year: 2021,
  yearTitle: '2021/22',
  publicationId: 'publication-1',
  publicationTitle: 'Publication 1',
  publicationSummary: 'Publication 1 summary',
  publicationSlug: 'publication-1-slug',
  timePeriodCoverage: { value: 'W51', label: 'Week 51' },
  title: 'Release Title',
  type: 'OfficialStatistics',
  contact: {
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
  test('renders the pre-release users tab', async () => {
    releaseService.getRelease.mockResolvedValue(releaseData);
    permissionService.canUpdateRelease.mockResolvedValue(true);
    preReleaseUserService.getUsers.mockResolvedValue([]);

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByText('Manage pre-release user access'),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('tab', { name: 'Pre-release users' }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Url')).toBeInTheDocument();

    expect(
      screen.getByLabelText('Invite new users by email'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Invite new users' }),
    ).toBeInTheDocument();
  });

  test('renders pre-release page link correctly', async () => {
    releaseService.getRelease.mockResolvedValue(releaseData);
    permissionService.canUpdateRelease.mockResolvedValue(true);

    renderPage();

    await waitFor(() => {
      expect(screen.queryByTestId('prerelease-url')).toBeInTheDocument();
    });

    expect(screen.getByLabelText('Url')).toHaveValue(
      'http://localhost/publication/publication-1/release/release-1/prerelease/content',
    );
  });

  test('only renders the public access list for amendments', async () => {
    const amendmentRelease = { ...releaseData, amendment: true };
    releaseService.getRelease.mockResolvedValue(amendmentRelease);
    permissionService.canUpdateRelease.mockResolvedValue(true);

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByText('Public pre-release access list'),
      ).toBeInTheDocument();
    });

    expect(screen.queryByRole('tab')).not.toBeInTheDocument();
    expect(screen.queryByRole('tabpanel')).not.toBeInTheDocument();
    expect(screen.queryByText('Pre-release users')).not.toBeInTheDocument();
  });

  test('renders the public access list tab', async () => {
    releaseService.getRelease.mockResolvedValue(releaseData);
    permissionService.canUpdateRelease.mockResolvedValue(true);
    preReleaseUserService.getUsers.mockResolvedValue([]);

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByText('Manage pre-release user access'),
      ).toBeInTheDocument();
    });

    userEvent.click(screen.getByRole('tab', { name: 'Public access list' }));

    await waitFor(() => {
      expect(
        screen.getByText('Public pre-release access list'),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', {
        name: 'Create public pre-release access list',
      }),
    ).toBeInTheDocument();
  });
});

function renderPage() {
  render(
    <MemoryRouter>
      <TestConfigContextProvider>
        <ReleaseContextProvider release={testRelease}>
          <ReleasePreReleaseAccessPage />
        </ReleaseContextProvider>
      </TestConfigContextProvider>
    </MemoryRouter>,
  );
}
