import PublicationReleasesPage from '@admin/pages/publication/PublicationReleasesPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import {
  testContact,
  testPublication as baseTestPublication,
} from '@admin/pages/publication/__data__/testPublication';
import _publicationService, {
  MyPublication,
} from '@admin/services/publicationService';
import _releaseService, { Release } from '@admin/services/releaseService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationReleasesPage', () => {
  const testRelease1: Release = {
    amendment: false,
    approvalStatus: 'Approved',
    id: 'release-1',
    latestRelease: false,
    live: false,
    permissions: {
      canAddPrereleaseUsers: false,
      canUpdateRelease: true,
      canDeleteRelease: false,
      canMakeAmendmentOfRelease: true,
    },
    previousVersionId: 'release-previous-id',
    publicationId: 'publication-1',
    publicationTitle: 'Publication 1',
    publicationSummary: 'Publication 1 summary',
    publicationSlug: 'publication-slug-1',
    published: '2022-01-01T00:00:00',
    publishScheduled: '2022-01-01T00:00:00',
    releaseName: 'Release name',
    slug: 'release-1-slug',
    title: 'Release 1',
    timePeriodCoverage: {
      label: 'Academic year',
      value: 'AY',
    },
    type: 'AdHocStatistics',
    contact: testContact,
    preReleaseAccessList: '',
  };

  const testRelease2: Release = {
    ...testRelease1,
    approvalStatus: 'Draft',
    id: 'release-2',
    published: '2022-01-02T00:00:00',
    slug: 'release-2-slug',
    title: 'Release 2',
  };

  const testRelease3: Release = {
    ...testRelease1,
    approvalStatus: 'Approved',
    id: 'release-3',
    live: true,
    published: '2022-01-03T00:00:00',
    slug: 'release-3-slug',
    title: 'Release 3',
  };

  const testRelease4: Release = {
    ...testRelease1,
    approvalStatus: 'Approved',
    id: 'release-4',
    live: true,
    published: '2022-01-04T00:00:00',
    slug: 'release-4-slug',
    title: 'Release 4',
  };

  const testPublication: MyPublication = {
    ...baseTestPublication,
    releases: [testRelease1, testRelease2, testRelease3, testRelease4],
  };

  beforeAll(() => {
    jest.useFakeTimers();
  });

  afterAll(() => {
    jest.useRealTimers();
  });

  beforeEach(() => {
    releaseService.getReleaseStatus.mockResolvedValue({
      overallStage: 'Complete',
    });
    releaseService.getReleaseChecklist.mockResolvedValue({
      errors: [],
      valid: true,
      warnings: [],
    });
  });

  test('renders the releases page correctly', async () => {
    publicationService.getMyPublication.mockResolvedValue(testPublication);
    renderPage(testPublication);

    expect(screen.getByText('Manage releases')).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText('Scheduled releases')).toBeInTheDocument();
    });
    expect(
      screen.getByRole('link', { name: 'Create new release' }),
    ).toBeInTheDocument();
    const scheduledTable = screen.getByTestId('publication-scheduled-releases');
    const scheduledRows = within(scheduledTable).getAllByRole('row');
    expect(scheduledRows).toHaveLength(2);
    const scheduledRow1cells = within(scheduledRows[1]).getAllByRole('cell');
    expect(
      within(scheduledRow1cells[0]).getByText('Release 1'),
    ).toBeInTheDocument();

    expect(screen.getByText('Draft releases')).toBeInTheDocument();
    const draftTable = screen.getByTestId('publication-draft-releases');
    const draftRows = within(draftTable).getAllByRole('row');
    expect(draftRows).toHaveLength(2);
    const draftRow1cells = within(draftRows[1]).getAllByRole('cell');
    expect(
      within(draftRow1cells[0]).getByText('Release 2'),
    ).toBeInTheDocument();

    expect(screen.getByText('Published releases (2 of 2)')).toBeInTheDocument();
    const publishedTable = screen.getByTestId('publication-published-releases');
    const publishedRows = within(publishedTable).getAllByRole('row');
    expect(publishedRows).toHaveLength(3);
    const publishedRow1cells = within(publishedRows[1]).getAllByRole('cell');
    const publishedRow2cells = within(publishedRows[2]).getAllByRole('cell');
    expect(
      within(publishedRow1cells[0]).getByText('Release 3'),
    ).toBeInTheDocument();
    expect(
      within(publishedRow2cells[0]).getByText('Release 4'),
    ).toBeInTheDocument();
  });

  test('does not show the create release button if you do not have permission', async () => {
    const publication = {
      ...testPublication,
      permissions: {
        ...testPublication.permissions,
        canCreateReleases: false,
      },
    };
    publicationService.getMyPublication.mockResolvedValue(publication);

    renderPage(publication);

    await waitFor(() => {
      expect(screen.getByText('Manage releases')).toBeInTheDocument();
    });

    expect(
      screen.queryByRole('link', { name: 'Create new release' }),
    ).not.toBeInTheDocument();
  });

  test('show a message if there are no releases', async () => {
    const publication = {
      ...testPublication,
      releases: [],
    };
    publicationService.getMyPublication.mockResolvedValue(publication);
    renderPage(publication);

    await waitFor(() => {
      expect(
        screen.getByText('There are no releases for this publication yet.'),
      );
    });

    expect(
      screen.queryByTestId('publication-scheduled-releases'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('publication-draft-releases'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('publication-published-releases'),
    ).not.toBeInTheDocument();
  });
});

function renderPage(publication: MyPublication) {
  render(
    <MemoryRouter>
      <PublicationContextProvider
        publication={publication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <PublicationReleasesPage />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
