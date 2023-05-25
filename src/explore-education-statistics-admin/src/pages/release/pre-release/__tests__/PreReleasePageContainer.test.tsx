import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import PreReleasePageContainer from '@admin/pages/release/pre-release/PreReleasePageContainer';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import { preReleaseRoute } from '@admin/routes/routes';
import _permissionService from '@admin/services/permissionService';
import _preReleaseService, {
  PreReleaseSummary,
} from '@admin/services/preReleaseService';
import { render, screen, waitFor, within } from '@testing-library/react';
import { addHours, subHours } from 'date-fns';
import React from 'react';
import { generatePath, MemoryRouter } from 'react-router';
import { Route } from 'react-router-dom';

jest.mock('@admin/services/permissionService');
jest.mock('@admin/services/preReleaseService');

const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;
const preReleaseService = _preReleaseService as jest.Mocked<
  typeof _preReleaseService
>;

describe('PreReleasePageContainer', () => {
  const testPreReleaseSummary: PreReleaseSummary = {
    publicationTitle: 'Test publication',
    publicationSlug: 'test-publication',
    releaseTitle: 'Calendar year 2018',
    releaseSlug: '2018',
    contactEmail: 'test@test.com',
    contactTeam: 'Test team',
  };

  test('renders correctly when pre-release has ended', async () => {
    permissionService.getPreReleaseWindowStatus.mockResolvedValue({
      access: 'After',
      start: new Date('2018-12-12T09:00:00Z'),
      end: new Date('2018-12-13T00:00:00Z'),
    });

    preReleaseService.getPreReleaseSummary.mockResolvedValue(
      testPreReleaseSummary,
    );

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Pre-release access has ended' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('link', { name: 'View this release' }),
      ).toHaveAttribute(
        'href',
        'http://localhost/find-statistics/test-publication/2018',
      );
    });
  });

  test('renders correctly when pre-release has not started', async () => {
    permissionService.getPreReleaseWindowStatus.mockResolvedValue({
      access: 'Before',
      start: new Date('3000-12-12T09:00:00Z'),
      end: new Date('3000-12-13T00:00:00Z'),
    });

    preReleaseService.getPreReleaseSummary.mockResolvedValue(
      testPreReleaseSummary,
    );

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByRole('heading', {
          name: 'Pre-release access is not yet available',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByText(
          'Pre-release access will be available from 12 December 3000 at 09:00 until 13 December 3000 at 00:00.',
        ),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('heading', { name: 'Pre-release access has ended' }),
      ).not.toBeInTheDocument();
    });
  });

  test('renders correctly when within pre-release window', async () => {
    const now = new Date();

    permissionService.getPreReleaseWindowStatus.mockResolvedValue({
      access: 'Within',
      start: subHours(now, 6),
      end: addHours(now, 18),
    });

    preReleaseService.getPreReleaseSummary.mockResolvedValue(
      testPreReleaseSummary,
    );

    renderPage();

    await waitFor(() => {
      const banner = within(screen.getByRole('region', { name: 'Contact' }));
      expect(
        banner.getByText('If you have an enquiry about this release contact:'),
      ).toBeInTheDocument();

      expect(banner.getByText('Test team:')).toBeInTheDocument();
      expect(
        banner.getByRole('link', { name: 'test@test.com' }),
      ).toBeInTheDocument();

      expect(screen.getByRole('link', { name: 'Content' })).toBeInTheDocument();

      expect(
        screen.queryByRole('heading', { name: 'Pre-release access has ended' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('heading', {
          name: 'Pre-release access is not yet available',
        }),
      ).not.toBeInTheDocument();
    });
  });

  const renderPage = () => {
    return render(
      <TestConfigContextProvider>
        <MemoryRouter
          initialEntries={[
            generatePath<ReleaseRouteParams>(preReleaseRoute.path, {
              publicationId: 'publication-1',
              releaseId: 'release-1',
            }),
          ]}
        >
          <Route
            path={preReleaseRoute.path}
            component={PreReleasePageContainer}
          />
        </MemoryRouter>
      </TestConfigContextProvider>,
    );
  };
});
