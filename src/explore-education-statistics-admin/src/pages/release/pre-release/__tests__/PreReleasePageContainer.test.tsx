import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import PreReleasePageContainer, {
  calculatePraPeriodAdvice,
} from '@admin/pages/release/pre-release/PreReleasePageContainer';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import { preReleaseRoute } from '@admin/routes/routes';
import _permissionService from '@admin/services/permissionService';
import _preReleaseService, {
  PreReleaseSummary,
} from '@admin/services/preReleaseService';
import { render, screen, waitFor, within } from '@testing-library/react';
import { subHours } from 'date-fns';
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

  test('calculates PRA period advice during winter', () => {
    const result = calculatePraPeriodAdvice(
      new Date('2020-12-09T00:00:00Z'),
      new Date('2020-12-10T09:30:00Z'),
    );

    expect(result).toBe(
      'Pre-release access will be available from 9 December 2020 at 00:00 until it is published on 10 December 2020.',
    );
  });

  test('calculates PRA period advice during summer', () => {
    const result = calculatePraPeriodAdvice(
      new Date('2020-06-08T23:00:00Z'),
      new Date('2020-06-10T08:30:00Z'),
    );

    expect(result).toBe(
      'Pre-release access will be available from 9 June 2020 at 00:00 until it is published on 10 June 2020.',
    );
  });

  test('renders correctly when pre-release has ended', async () => {
    permissionService.getPreReleaseWindowStatus.mockResolvedValue({
      access: 'After',
      start: new Date('2018-12-12T09:00:00Z'),
      scheduledPublishDate: new Date('2018-12-13T00:00:00Z'),
    });

    preReleaseService.getPreReleaseSummary.mockResolvedValue(
      testPreReleaseSummary,
    );

    await renderPageAndAwaitForText('Pre-release access has ended');

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

  test('renders correctly when pre-release has not started', async () => {
    permissionService.getPreReleaseWindowStatus.mockResolvedValue({
      access: 'Before',
      start: new Date('3000-12-12T09:00:00Z'),
      scheduledPublishDate: new Date('3000-12-13T00:00:00Z'),
    });

    preReleaseService.getPreReleaseSummary.mockResolvedValue(
      testPreReleaseSummary,
    );

    await renderPageAndAwaitForText('Pre-release access is not yet available');

    expect(
      screen.getByRole('heading', {
        name: 'Pre-release access is not yet available',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        'Pre-release access will be available from 12 December 3000 at 09:00 until it is published on 13 December 3000.',
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Pre-release access has ended' }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly when within pre-release window', async () => {
    const now = new Date('2018-12-12T14:32:00Z');

    permissionService.getPreReleaseWindowStatus.mockResolvedValue({
      access: 'Within',
      start: subHours(now, 6),
      scheduledPublishDate: new Date('2018-12-13T00:00:00Z'),
    });

    preReleaseService.getPreReleaseSummary.mockResolvedValue(
      testPreReleaseSummary,
    );

    await renderPageAndAwaitForText(
      'If you have an enquiry about this release contact:',
    );

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

  test('renders correctly when on release day but release is yet to be published', async () => {
    const now = new Date(); // 2am on Publish Day

    permissionService.getPreReleaseWindowStatus.mockResolvedValue({
      access: 'Within',
      start: subHours(now, 26),
      scheduledPublishDate: subHours(now, 2),
    });

    preReleaseService.getPreReleaseSummary.mockResolvedValue(
      testPreReleaseSummary,
    );

    await renderPageAndAwaitForText(
      'If you have an enquiry about this release contact:',
    );

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
      screen.queryByRole('heading', {
        name: 'Pre-release access has ended',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', {
        name: 'Pre-release access is not yet available',
      }),
    ).not.toBeInTheDocument();
  });

  const renderPageAndAwaitForText = async (textToWaitFor: string) => {
    render(
      <TestConfigContextProvider>
        <MemoryRouter
          initialEntries={[
            generatePath<ReleaseRouteParams>(preReleaseRoute.path, {
              publicationId: 'publication-1',
              releaseVersionId: 'release-1',
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

    await waitFor(() => {
      expect(screen.getByText(textToWaitFor)).toBeInTheDocument();
    });
  };
});
