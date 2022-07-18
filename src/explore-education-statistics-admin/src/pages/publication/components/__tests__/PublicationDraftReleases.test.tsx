import PublicationDraftReleases from '@admin/pages/publication/components/PublicationDraftReleases';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import {
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import _releaseContentService, {
  EditableRelease,
} from '@admin/services/releaseContentService';
import _releaseService, { MyRelease } from '@admin/services/releaseService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
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

describe('PublicationDraftReleases', () => {
  const testContact: PublicationContactDetails = {
    id: 'contact-1',
    contactName: 'John Smith',
    contactTelNo: '0777777777',
    teamEmail: 'john.smith@test.com',
    teamName: 'Team Smith',
  };

  const testRelease1: MyRelease = {
    amendment: false,
    approvalStatus: 'Draft',
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
    approvalStatus: 'HigherLevelReview',
    id: 'release-2',
    published: '2022-01-02T00:00:00',
    slug: 'release-2-slug',
    title: 'Release 2',
  };

  const testRelease3: MyRelease = {
    ...testRelease1,
    amendment: true,
    id: 'release-3',
    permissions: {
      canAddPrereleaseUsers: false,
      canUpdateRelease: true,
      canDeleteRelease: true,
      canMakeAmendmentOfRelease: true,
    },
    published: '2022-01-03T00:00:00',
    slug: 'release-3-slug',
    title: 'Release 3',
  };

  const testPublication: MyPublication = {
    id: 'publication-1',
    title: 'Publication 1',
    contact: testContact,
    releases: [testRelease1, testRelease2, testRelease3],
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
    releaseService.getReleaseChecklist.mockResolvedValue({
      errors: [
        {
          code: 'DataFileImportsMustBeCompleted',
        },
        {
          code: 'EmptyContentSectionExists',
        },
      ],
      valid: false,
      warnings: [{ code: 'NoMethodology' }],
    });

    releaseContentService.getContent.mockResolvedValue({
      release: testEditableRelease,
      availableDataBlocks: [],
    });
  });

  test('renders the draft releases table correctly', async () => {
    setUp(testPublication);

    expect(screen.getByText('Draft releases')).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(4);

    // Draft
    const row1cells = within(rows[1]).getAllByRole('cell');
    expect(within(row1cells[0]).getByText('Release 1')).toBeInTheDocument();
    expect(within(row1cells[1]).getByText('Draft')).toBeInTheDocument();

    await waitFor(() => {
      // Errors
      expect(within(row1cells[2]).getByText('2')).toBeInTheDocument();
      // Warnings
      expect(within(row1cells[3]).getByText('1')).toBeInTheDocument();
      // Unresolved comments
      expect(within(row1cells[4]).getByText('0')).toBeInTheDocument();
    });

    expect(
      within(row1cells[5]).getByRole('link', { name: 'Edit Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/summary',
    );

    expect(
      within(row1cells[5]).queryByRole('link', {
        name: 'View original for Release 1',
      }),
    ).not.toBeInTheDocument();

    expect(
      within(row1cells[5]).queryByRole('button', {
        name: 'Cancel amendment for Release 1',
      }),
    ).not.toBeInTheDocument();

    // In review
    const row2cells = within(rows[2]).getAllByRole('cell');
    expect(within(row2cells[0]).getByText('Release 2')).toBeInTheDocument();
    expect(within(row2cells[1]).getByText('In Review')).toBeInTheDocument();

    await waitFor(() => {
      // Errors
      expect(within(row2cells[2]).getByText('2')).toBeInTheDocument();
      // Warnings
      expect(within(row2cells[3]).getByText('1')).toBeInTheDocument();
      // Unresolved comments
      expect(within(row2cells[4]).getByText('0')).toBeInTheDocument();
    });

    expect(
      within(row2cells[5]).getByRole('link', { name: 'Edit Release 2' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2/summary',
    );

    expect(
      within(row2cells[5]).queryByRole('link', {
        name: 'View original for Release 2',
      }),
    ).not.toBeInTheDocument();

    expect(
      within(row2cells[5]).queryByRole('button', {
        name: 'Cancel amendment for Release 2',
      }),
    ).not.toBeInTheDocument();

    // Amendment
    const row3cells = within(rows[3]).getAllByRole('cell');
    expect(within(row3cells[0]).getByText('Release 3')).toBeInTheDocument();
    expect(within(row3cells[1]).getByText('Amendment')).toBeInTheDocument();

    await waitFor(() => {
      // Errors
      expect(within(row3cells[2]).getByText('2')).toBeInTheDocument();
      // Warnings
      expect(within(row3cells[3]).getByText('1')).toBeInTheDocument();
      // Unresolved comments
      expect(within(row3cells[4]).getByText('0')).toBeInTheDocument();
    });

    expect(
      within(row3cells[5]).getByRole('button', {
        name: 'Cancel amendment for Release 3',
      }),
    ).toBeInTheDocument();

    expect(
      within(row3cells[5]).getByRole('link', {
        name: 'View original for Release 3',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-previous-id/summary',
    );

    expect(
      within(row3cells[5]).getByRole('link', { name: 'Edit Release 3' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-3/summary',
    );
  });

  test('shows a view instead of edit link if you do not have permission to edit the release', () => {
    setUp(
      produce(testPublication, draft => {
        draft.releases[0].permissions.canUpdateRelease = false;
      }),
    );

    const rows = screen.getAllByRole('row');
    const row1cells = within(rows[1]).getAllByRole('cell');

    expect(
      within(row1cells[5]).getByRole('link', { name: 'View Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/summary',
    );
  });

  test('does not show the cancel button if you do not have permission to cancel the amendment', () => {
    setUp(
      produce(testPublication, draft => {
        draft.releases[2].permissions.canDeleteRelease = false;
      }),
    );

    const rows = screen.getAllByRole('row');
    const row3cells = within(rows[3]).getAllByRole('cell');
    expect(
      within(row3cells[5]).queryByRole('button', {
        name: 'Cancel amendment for Release 3',
      }),
    ).not.toBeInTheDocument();
  });

  test('handles cancelling an amendment correctly', async () => {
    releaseService.getDeleteReleasePlan.mockResolvedValue({
      scheduledMethodologies: [
        {
          id: 'methodology-1',
          title: 'Methodology 1',
        },
        {
          id: 'methodology-2',
          title: 'Methodology 2',
        },
      ],
    });

    setUp(testPublication);

    const rows = screen.getAllByRole('row');
    const row3cells = within(rows[3]).getAllByRole('cell');

    userEvent.click(
      within(row3cells[5]).getByRole('button', {
        name: 'Cancel amendment for Release 3',
      }),
    );

    await waitFor(() => {
      expect(releaseService.getDeleteReleasePlan).toHaveBeenCalledWith(
        testRelease3.id,
      );
    });

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByText('Confirm you want to cancel this amended release'),
    ).toBeInTheDocument();
    expect(modal.getByText('Methodology 1')).toBeInTheDocument();
    expect(modal.getByText('Methodology 2')).toBeInTheDocument();

    userEvent.click(
      modal.getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(releaseService.deleteRelease).toHaveBeenCalledWith(
        testRelease3.id,
      );
    });

    expect(
      screen.queryByText('Confirm you want to cancel this amended release'),
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
        <PublicationDraftReleases />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
