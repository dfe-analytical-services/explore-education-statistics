import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import _permissionService, {
  ReleaseStatusPermissions,
} from '@admin/services/permissionService';
import _releaseVersionService, {
  ReleaseVersion,
} from '@admin/services/releaseVersionService';
import mockDate from '@common-test/mockDate';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
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

    expect(await screen.findByText('Sign off')).toBeInTheDocument();

    expect(screen.getByLabelText('URL')).toHaveValue(
      'http://localhost/find-statistics/publication-1-slug/release-1-slug',
    );
  });

  test('renders Draft status details correctly', async () => {
    renderPage();

    expect(await screen.findByText('Sign off')).toBeInTheDocument();

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

    expect(await screen.findByText('Sign off')).toBeInTheDocument();

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

    const { user } = renderPage();

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Edit release status' }),
      ).toBeInTheDocument();
    });

    await user.click(
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

    expect(await screen.findByText('Sign off')).toBeInTheDocument();

    expect(screen.queryByText('Edit release status')).not.toBeInTheDocument();
  });

  describe('scheduled release warning modal', () => {
    test('shows the warning modal if edit a pre-release on release day', async () => {
      mockDate.set('2045-07-18 09:29');
      permissionService.getReleaseStatusPermissions.mockResolvedValue(
        testStatusPermissions,
      );
      releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
        overallStage: 'Scheduled',
      });

      const { user } = renderPage({
        ...testRelease,
        approvalStatus: 'Approved',
        publishScheduled: '2045-07-18',
      });
      expect(await screen.findByText('Sign off')).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', { name: 'Edit release status' }),
      );

      expect(await screen.findByText('Important')).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));
      expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
      expect(
        modal.getByRole('button', { name: 'Continue' }),
      ).toBeInTheDocument();
    });

    test('renders the status form when click continue on the warning modal', async () => {
      mockDate.set('2045-07-18 09:29');
      permissionService.getReleaseStatusPermissions.mockResolvedValue(
        testStatusPermissions,
      );
      releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
        overallStage: 'Scheduled',
      });

      const { user } = renderPage({
        ...testRelease,
        approvalStatus: 'Approved',
        publishScheduled: '2045-07-18',
      });

      expect(await screen.findByText('Sign off')).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', { name: 'Edit release status' }),
      );

      expect(await screen.findByText('Important')).toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: 'Continue' }));

      expect(
        screen.getByRole('button', { name: 'Update status' }),
      ).toBeInTheDocument();
    });

    test('does not show the warning modal if edit a pre-release before release day', async () => {
      mockDate.set('2045-07-17 23:00');
      permissionService.getReleaseStatusPermissions.mockResolvedValue(
        testStatusPermissions,
      );
      releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
        overallStage: 'Scheduled',
      });

      const { user } = renderPage({
        ...testRelease,
        approvalStatus: 'Approved',
        publishScheduled: '2045-07-18',
      });

      expect(await screen.findByText('Sign off')).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', { name: 'Edit release status' }),
      );

      expect(screen.queryByText('Important')).not.toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Update status' }),
      ).toBeInTheDocument();
    });
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
