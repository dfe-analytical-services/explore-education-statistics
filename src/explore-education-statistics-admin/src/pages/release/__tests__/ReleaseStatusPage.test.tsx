import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import { ReleaseContextProvider } from '@admin/pages/release/contexts/ReleaseContext';
import _permissionService, {
  ReleaseStatusPermissions,
} from '@admin/services/permissionService';
import _releaseService from '@admin/services/releaseService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { MemoryRouter } from 'react-router';
import ReleaseStatusPage from '../ReleaseStatusPage';

jest.mock('@admin/services/permissionService');
jest.mock('@admin/services/releaseService');

const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

describe('ReleaseStatusPage', () => {
  const testStatusPermissions: ReleaseStatusPermissions = {
    canMarkDraft: true,
    canMarkHigherLevelReview: true,
    canMarkApproved: true,
  };

  test('renders public release link correctly', async () => {
    releaseService.getRelease.mockResolvedValue(testRelease);

    render(
      <MemoryRouter>
        <TestConfigContextProvider>
          <ReleaseContextProvider release={testRelease}>
            <ReleaseStatusPage />
          </ReleaseContextProvider>
        </TestConfigContextProvider>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.getByTestId('public-release-url')).toHaveTextContent(
      'http://localhost/find-statistics/publication-1-slug/release-1-slug',
    );
  });

  test('renders Draft status details correctly', async () => {
    releaseService.getRelease.mockResolvedValue(testRelease);

    render(
      <MemoryRouter>
        <TestConfigContextProvider>
          <ReleaseContextProvider release={testRelease}>
            <ReleaseStatusPage />
          </ReleaseContextProvider>
        </TestConfigContextProvider>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.getByTestId('Current status')).toHaveTextContent('Draft');
    expect(
      screen.queryByTestId('Release process status'),
    ).not.toBeInTheDocument();
    expect(screen.getByTestId('Scheduled release')).toHaveTextContent(
      'Not scheduled',
    );
    expect(screen.getByTestId('Next release expected')).toHaveTextContent(
      'Not set',
    );
  });

  test('renders Approved status details correctly', async () => {
    releaseService.getReleaseStatus.mockResolvedValue({
      overallStage: 'Scheduled',
    });
    releaseService.getRelease.mockResolvedValue({
      ...testRelease,
      status: 'Approved',
      publishScheduled: '2021-01-15',
      nextReleaseDate: {
        month: 10,
        year: 2022,
      },
    });

    render(
      <MemoryRouter>
        <TestConfigContextProvider>
          <ReleaseContextProvider release={testRelease}>
            <ReleaseStatusPage />
          </ReleaseContextProvider>
        </TestConfigContextProvider>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.getByTestId('Current status')).toHaveTextContent('Approved');
    expect(screen.getByTestId('Release process status')).toHaveTextContent(
      'Scheduled',
    );
    expect(screen.getByTestId('Scheduled release')).toHaveTextContent(
      '15 January 2021',
    );
    expect(screen.getByTestId('Next release expected')).toHaveTextContent(
      'October 2022',
    );
  });

  test('renders status form when Edit button is clicked', async () => {
    releaseService.getRelease.mockResolvedValue(testRelease);
    permissionService.getReleaseStatusPermissions.mockResolvedValue(
      testStatusPermissions,
    );

    render(
      <MemoryRouter>
        <TestConfigContextProvider>
          <ReleaseContextProvider release={testRelease}>
            <ReleaseStatusPage />
          </ReleaseContextProvider>
        </TestConfigContextProvider>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Edit release status' }),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Edit release status' }),
    );

    await waitFor(() => {
      expect(screen.getByText('Edit release status')).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Update status' }),
      ).toBeInTheDocument();
    });
  });

  test('does not render Edit button if status cannot be changed', async () => {
    releaseService.getRelease.mockResolvedValue(testRelease);
    permissionService.getReleaseStatusPermissions.mockResolvedValue({
      canMarkDraft: false,
      canMarkHigherLevelReview: false,
      canMarkApproved: false,
    });

    render(
      <MemoryRouter>
        <TestConfigContextProvider>
          <ReleaseContextProvider release={testRelease}>
            <ReleaseStatusPage />
          </ReleaseContextProvider>
        </TestConfigContextProvider>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.queryByText('Edit release status')).not.toBeInTheDocument();
  });
});
