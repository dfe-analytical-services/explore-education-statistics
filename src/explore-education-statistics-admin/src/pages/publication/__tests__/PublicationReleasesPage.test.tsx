import PublicationReleasesPage from '@admin/pages/publication/PublicationReleasesPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import {
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import _releaseContentService, {
  EditableRelease,
} from '@admin/services/releaseContentService';
import _releaseService, { MyRelease } from '@admin/services/releaseService';
import { render, screen, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';
import produce from 'immer';

jest.mock('@admin/services/releaseService');
jest.mock('@admin/services/releaseContentService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;
const releaseContentService = _releaseContentService as jest.Mocked<
  typeof _releaseContentService
>;

describe('PublicationReleasesPage', () => {
  const testContact: PublicationContactDetails = {
    id: 'contact-1',
    contactName: 'John Smith',
    contactTelNo: '0777777777',
    teamEmail: 'john.smith@test.com',
    teamName: 'Team Smith',
  };

  const testRelease1: MyRelease = {
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
    publicationSlug: 'publication-slug-1',
    publicationTitle: 'Publication 1',
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

  const testRelease2: MyRelease = {
    ...testRelease1,
    approvalStatus: 'Draft',
    id: 'release-2',
    published: '2022-01-02T00:00:00',
    slug: 'release-2-slug',
    title: 'Release 2',
  };

  const testRelease3: MyRelease = {
    ...testRelease1,
    approvalStatus: 'Approved',
    id: 'release-3',
    live: true,
    published: '2022-01-03T00:00:00',
    slug: 'release-3-slug',
    title: 'Release 3',
  };

  const testRelease4: MyRelease = {
    ...testRelease1,
    approvalStatus: 'Approved',
    id: 'release-4',
    live: true,
    published: '2022-01-04T00:00:00',
    slug: 'release-4-slug',
    title: 'Release 4',
  };

  const testPublication: MyPublication = {
    id: 'publication-1',
    title: 'Publication 1',
    contact: testContact,
    releases: [testRelease1, testRelease2, testRelease3, testRelease4],
    legacyReleases: [],
    methodologies: [],
    themeId: 'theme-1',
    topicId: 'topic-1',
    permissions: {
      canAdoptMethodologies: true,
      canCreateReleases: true,
      canUpdatePublication: true,
      canUpdatePublicationTitle: true,
      canUpdatePublicationSupersededBy: true,
      canCreateMethodologies: true,
      canManageExternalMethodology: true,
    },
  };

  const testEditableRelease: EditableRelease = {
    approvalStatus: 'Draft',
    id: 'release-1',
    latestRelease: false,

    publicationId: 'publication-1',
    published: '2022-01-01T00:00:00',
    releaseName: 'Release name',
    slug: 'release-1-slug',
    title: 'Release 1',

    type: 'AdHocStatistics',
    content: [],
    coverageTitle: '',
    dataLastPublished: '',
    downloadFiles: [],
    hasDataGuidance: false,
    hasPreReleaseAccessList: false,
    headlinesSection: {
      id: '',
      order: 0,
      content: [],
      heading: '',
    },
    keyStatisticsSection: {
      id: '',
      order: 0,
      content: [],
      heading: '',
    },
    keyStatisticsSecondarySection: {
      id: '',
      order: 0,
      content: [],
      heading: '',
    },
    publication: {
      ...testPublication,
      contact: {
        teamName: 'Explore Education Statistics',
        teamEmail: 'explore.statistics@education.gov.uk',
        contactName: 'Cameron Race',
        contactTelNo: '07780991976',
      },
      methodologies: [],
      slug: '',
      topic: { theme: { title: '' } },
    },
    relatedDashboardsSection: {
      id: '',
      order: 0,
      content: [],
      heading: '',
    },
    relatedInformation: [],
    summarySection: {
      id: '',
      order: 0,
      content: [],
      heading: '',
    },
    updates: [],
    yearTitle: '',
  };

  beforeEach(() => {
    releaseService.getReleaseStatus.mockResolvedValue({
      overallStage: 'Scheduled',
    });
    releaseService.getReleaseChecklist.mockResolvedValue({
      errors: [],
      valid: true,
      warnings: [],
    });

    releaseContentService.getContent.mockResolvedValue({
      release: testEditableRelease,
      availableDataBlocks: [],
    });
  });

  test('renders the releases page correctly', async () => {
    setUp(testPublication);

    expect(screen.getByText('Manage releases')).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Create new release' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Scheduled releases')).toBeInTheDocument();
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

  test('does not show the create release button if you do not have permission', () => {
    setUp(
      produce(testPublication, draft => {
        draft.permissions.canCreateReleases = false;
      }),
    );

    expect(
      screen.queryByRole('link', { name: 'Create new release' }),
    ).not.toBeInTheDocument();
  });

  test('show a message if there are no releases', () => {
    setUp(
      produce(testPublication, draft => {
        draft.releases = [];
      }),
    );

    expect(screen.getByText('There are no releases for this publication yet.'));

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

function setUp(publication: MyPublication) {
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
