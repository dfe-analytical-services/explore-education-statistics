import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import _permissionService, {
  ReleaseStatusPermissions,
} from '@admin/services/permissionService';
import _releaseVersionService, {
  ReleaseVersion,
} from '@admin/services/releaseVersionService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { MemoryRouter } from 'react-router';
import ReleaseStatusPage from '../ReleaseStatusPage';

jest.mock('@admin/services/permissionService');
jest.mock('@admin/services/releaseVersionService');

const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;
const releaseVersionService = _releaseVersionService as jest.Mocked<
  typeof _releaseVersionService
>;

describe('ReleaseStatusPage', () => {
  const testStatusPermissions: ReleaseStatusPermissions = {
    canMarkDraft: true,
    canMarkHigherLevelReview: true,
    canMarkApproved: true,
  };

  test('renders public release link correctly', async () => {
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.getByLabelText('URL')).toHaveValue(
      'http://localhost/find-statistics/publication-1-slug/release-1-slug',
    );
  });

  test('renders Draft status details correctly', async () => {
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.getByTestId('Current status-value')).toHaveTextContent(
      'In draft',
    );
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
    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
      overallStage: 'Scheduled',
    });

    renderPage({
      ...testRelease,
      approvalStatus: 'Approved',
      publishScheduled: '2021-01-15',
      nextReleaseDate: {
        month: 10,
        year: 2022,
      },
    });

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.getByTestId('Current status')).toHaveTextContent('Approved');
    expect(
      await screen.findByTestId('Release process status'),
    ).toHaveTextContent('Scheduled');

    expect(screen.getByTestId('Scheduled release')).toHaveTextContent(
      '15 January 2021',
    );
    expect(screen.getByTestId('Next release expected')).toHaveTextContent(
      'October 2022',
    );
  });

  test('renders status form when Edit button is clicked', async () => {
    permissionService.getReleaseStatusPermissions.mockResolvedValue(
      testStatusPermissions,
    );

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Edit release status' }),
      ).toBeInTheDocument();
    });

    await userEvent.click(
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
    permissionService.getReleaseStatusPermissions.mockResolvedValue({
      canMarkDraft: false,
      canMarkHigherLevelReview: false,
      canMarkApproved: false,
    });

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.queryByText('Edit release status')).not.toBeInTheDocument();
  });

  function renderPage(releaseVersion: ReleaseVersion = testRelease) {
    return render(
      <MemoryRouter>
        <TestConfigContextProvider>
          <ReleaseVersionContextProvider releaseVersion={releaseVersion}>
            <ReleaseStatusPage />
          </ReleaseVersionContextProvider>
        </TestConfigContextProvider>
      </MemoryRouter>,
    );
  }
});
