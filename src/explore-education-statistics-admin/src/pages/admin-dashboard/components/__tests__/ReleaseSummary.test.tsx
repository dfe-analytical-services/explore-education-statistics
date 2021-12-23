import ReleaseSummary from '@admin/pages/admin-dashboard/components/ReleaseSummary';
import _releaseService, {
  MyRelease,
  ReleaseStageStatuses,
} from '@admin/services/releaseService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { forceVisible } from 'react-lazyload';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

describe('ReleaseSummary', () => {
  const testRelease: MyRelease = {
    amendment: false,
    approvalStatus: 'Approved',
    contact: {
      contactName: 'Test contact name',
      contactTelNo: 'Test contact tel',
      id: 'test-contact-idc',
      teamEmail: 'test-contact@hiveit.co.uk',
      teamName: 'Test team name',
    },
    id: 'rel-3',
    latestInternalReleaseNote: 'Test internal release note',
    latestRelease: false,
    live: true,
    nextReleaseDate: {
      day: '01',
      month: '01',
      year: '2022',
    },
    permissions: {
      canAddPrereleaseUsers: false,
      canUpdateRelease: false,
      canDeleteRelease: false,
      canMakeAmendmentOfRelease: false,
    },
    preReleaseAccessList: '',
    previousVersionId: 'rel-2',
    publicationId: 'publication-id',
    publicationSlug: 'test-publication',
    publicationTitle: 'Test publication title',
    published: '2021-01-01T11:21:17',
    releaseName: 'Test release name',
    slug: 'rel-3-slug',
    timePeriodCoverage: {
      label: 'Academic year',
      value: 'AY',
    },
    title: 'Test release title',
    type: {
      id: 'test-type-id',
      title: 'Ad hoc',
    },
  };
  const testActions = <button type="button">Test action</button>;

  const completeReleaseStatus: ReleaseStageStatuses = {
    overallStage: 'Complete',
  };

  beforeEach(() => {
    releaseService.getReleaseStatus.mockResolvedValue(completeReleaseStatus);
  });

  test('renders approved and published releases correctly', async () => {
    render(<ReleaseSummary release={testRelease} actions={testActions} />);

    forceVisible(); // Force the lazyloaded stats to be visible.

    expect(
      screen.getByRole('button', { name: 'Test release title (Live)' }),
    ).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText('Published')).toBeInTheDocument();
    });

    expect(
      within(screen.getByTestId('Publish date')).getByText('1 January 2021'),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('Next release date')).getByText(
        '1 January 2022',
      ),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('Release process status')).getByText(
        'Complete',
      ),
    ).toBeInTheDocument();

    const leadStatistician = screen.getByTestId('Lead statistician')
      .textContent;
    expect(leadStatistician?.includes('Test contact name')).toBeTruthy();
    expect(
      leadStatistician?.includes('test-contact@hiveit.co.uk'),
    ).toBeTruthy();
    expect(leadStatistician?.includes('Test contact tel')).toBeTruthy();

    expect(
      within(screen.getByTestId('Internal note')).getByText(
        'Test internal release note',
      ),
    ).toBeInTheDocument();
  });

  test('renders draft releases correctly', () => {
    render(
      <ReleaseSummary
        release={{
          ...testRelease,
          approvalStatus: 'Draft',
          live: false,
          nextReleaseDate: undefined,
          published: undefined,
        }}
        actions={testActions}
      />,
    );

    expect(
      screen.getByRole('button', {
        name: 'Test release title (not Live) Draft',
      }),
    ).toBeInTheDocument();

    expect(screen.getByText('Draft')).toBeInTheDocument();

    expect(
      within(screen.getByTestId('Publish date')).getByText('Not yet published'),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('Next release date')).not.toBeInTheDocument();

    expect(
      screen.queryByTestId('Release process status'),
    ).not.toBeInTheDocument();

    const leadStatistician = screen.getByTestId('Lead statistician')
      .textContent;
    expect(leadStatistician?.includes('Test contact name')).toBeTruthy();
    expect(
      leadStatistician?.includes('test-contact@hiveit.co.uk'),
    ).toBeTruthy();
    expect(leadStatistician?.includes('Test contact tel')).toBeTruthy();

    expect(
      within(screen.getByTestId('Internal note')).getByText(
        'Test internal release note',
      ),
    ).toBeInTheDocument();
  });

  test('shows the scheduled publication date for scheduled releases', () => {
    render(
      <ReleaseSummary
        release={{
          ...testRelease,
          published: undefined,
          publishScheduled: '2022-01-01T11:21:17',
        }}
        actions={testActions}
      />,
    );

    expect(
      within(screen.getByTestId('Publish date')).getByText('1 January 2022'),
    ).toBeInTheDocument();
  });

  test('shows the amendment tag for amendments', () => {
    render(
      <ReleaseSummary
        release={{
          ...testRelease,
          amendment: true,
        }}
        actions={testActions}
      />,
    );

    expect(screen.getByText('Amendment')).toBeInTheDocument();
  });

  test('renders actions correctly', () => {
    render(<ReleaseSummary release={testRelease} actions={testActions} />);

    userEvent.click(
      screen.getByRole('button', { name: 'Test release title (Live)' }),
    );

    expect(
      screen.getByRole('button', { name: 'Test action' }),
    ).toBeInTheDocument();
  });

  test('renders children correctly', () => {
    render(
      <ReleaseSummary release={testRelease} actions={testActions}>
        <p>Test children</p>
      </ReleaseSummary>,
    );
    expect(screen.getByText('Test children')).toBeInTheDocument();
  });

  test('renders closed by default', () => {
    render(<ReleaseSummary release={testRelease} actions={testActions} />);
    expect(screen.getByText('Publish date')).not.toBeVisible();
  });

  test('renders open when open is set to true', () => {
    render(<ReleaseSummary release={testRelease} actions={testActions} open />);
    expect(screen.getByText('Publish date')).toBeVisible();
  });
});
