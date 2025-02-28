import { testRelease } from '@admin/pages/release/__data__/testRelease';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import _releaseVersionService from '@admin/services/releaseVersionService';
import _permissionService from '@admin/services/permissionService';
import _preReleaseUserService from '@admin/services/preReleaseUserService';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import { MemoryRouter } from 'react-router';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import ReleasePreReleaseAccessPage from '@admin/pages/release/pre-release/ReleasePreReleaseAccessPage';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/releaseVersionService');
const releaseVersionService = _releaseVersionService as jest.Mocked<
  typeof _releaseVersionService
>;
jest.mock('@admin/services/permissionService');
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;
jest.mock('@admin/services/preReleaseUserService');
const preReleaseUserService = _preReleaseUserService as jest.Mocked<
  typeof _preReleaseUserService
>;

// Mock useMedia as tests for tabs can be flaky due to the
// role being added only on desktop.
jest.mock('@common/hooks/useMedia', () => ({
  useDesktopMedia: () => {
    return {
      onMedia: (value: string) => value,
    };
  },
}));

describe('ReleasePreReleaseAccessPage', () => {
  test('renders the pre-release users tab', async () => {
    releaseVersionService.getReleaseVersion.mockResolvedValue(testRelease);
    permissionService.canUpdateRelease.mockResolvedValue(true);
    preReleaseUserService.getUsers.mockResolvedValue([]);

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByText('Manage pre-release user access'),
      ).toBeInTheDocument();
    });

    expect(await screen.findByText('Pre-release users')).toBeInTheDocument();

    expect(
      screen.getByRole('tab', { name: 'Pre-release users' }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('URL')).toBeInTheDocument();

    expect(
      await screen.findByLabelText('Invite new users by email'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Invite new users' }),
    ).toBeInTheDocument();
  });

  test('renders pre-release page link correctly', async () => {
    releaseVersionService.getReleaseVersion.mockResolvedValue(testRelease);
    permissionService.canUpdateRelease.mockResolvedValue(true);

    renderPage();

    await waitFor(() => {
      expect(screen.getByLabelText('URL')).toBeInTheDocument();
    });

    expect(screen.getByLabelText('URL')).toHaveValue(
      'http://localhost/publication/publication-1/release/release-1/prerelease/content',
    );
  });

  test('renders the public access list tab', async () => {
    releaseVersionService.getReleaseVersion.mockResolvedValue(testRelease);
    permissionService.canUpdateRelease.mockResolvedValue(true);
    preReleaseUserService.getUsers.mockResolvedValue([]);

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByText('Manage pre-release user access'),
      ).toBeInTheDocument();
    });

    await userEvent.click(
      screen.getByRole('tab', { name: 'Public access list' }),
    );

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
        <ReleaseVersionContextProvider releaseVersion={testRelease}>
          <ReleasePreReleaseAccessPage />
        </ReleaseVersionContextProvider>
      </TestConfigContextProvider>
    </MemoryRouter>,
  );
}
