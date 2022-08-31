import { render, screen, within } from '@testing-library/react';
import React from 'react';
import { noop } from 'lodash';
import PublicationSummary from '@admin/pages/admin-dashboard/components/PublicationSummary';
import {
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import _releaseService, {
  Release,
  ReleaseStageStatuses,
} from '@admin/services/releaseService';
import { MemoryRouter } from 'react-router-dom';
import { MethodologyVersionSummary } from '@admin/services/methodologyService';

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

describe('PublicationSummary', () => {
  const testTopicId = 'test-topic';

  const testContact: PublicationContactDetails = {
    id: 'contact-1',
    contactName: 'John Smith',
    contactTelNo: '0777777777',
    teamEmail: 'john.smith@test.com',
    teamName: 'Team Smith',
  };

  const fullPermissions: MyPublication['permissions'] = {
    canAdoptMethodologies: true,
    canCreateReleases: true,
    canUpdatePublication: true,
    canUpdatePublicationTitle: true,
    canUpdatePublicationSupersededBy: true,
    canCreateMethodologies: true,
    canManageExternalMethodology: true,
  };

  const testPublication: MyPublication = {
    id: 'publication-1',
    title: 'Publication 1',
    summary: 'Publication 1 summary',
    contact: testContact,
    releases: [],
    methodologies: [],
    themeId: 'theme-1',
    topicId: 'topic-1',
    permissions: {
      canAdoptMethodologies: false,
      canCreateReleases: false,
      canUpdatePublication: false,
      canUpdatePublicationTitle: false,
      canUpdatePublicationSupersededBy: false,
      canCreateMethodologies: false,
      canManageExternalMethodology: false,
    },
  };

  const testReleases: Release[] = [
    {
      id: 'rel-1',
      latestRelease: true,
      published: '2021-06-30T11:21:17',
      slug: 'rel-1-slug',
      title: 'Release 1',
      publicationId: testPublication.id,
      permissions: {
        canUpdateRelease: false,
      },
      approvalStatus: 'Draft',
    } as Release,
    {
      id: 'rel-3',
      latestRelease: false,
      published: '2021-01-01T11:21:17',
      slug: 'rel-3-slug',
      title: 'Amendment Release',
      amendment: true,
      previousVersionId: 'rel-2',
      publicationId: testPublication.id,
      permissions: {
        canUpdateRelease: false,
        canDeleteRelease: true,
      },
      approvalStatus: 'Draft',
    } as Release,
    {
      id: 'rel-2',
      latestRelease: true,
      published: '2021-05-30T11:21:17',
      slug: 'rel-2-slug',
      title: 'Release 2',
      publicationId: testPublication.id,
      permissions: {
        canUpdateRelease: false,
      },
      approvalStatus: 'Approved',
    } as Release,
  ];

  const testMethodologies: MethodologyVersionSummary[] = [
    {
      amendment: false,
      id: 'methodology-v1',
      methodologyId: 'methodology-1',
      previousVersionId: 'methodology-previous-version-1',
      owned: true,
      published: '2021-06-08T09:04:17.9805585',
      status: 'Approved',
      title: 'Methodology 1',
      permissions: {
        canApproveMethodology: false,
        canUpdateMethodology: false,
        canDeleteMethodology: false,
        canMakeAmendmentOfMethodology: false,
        canMarkMethodologyAsDraft: false,
        canRemoveMethodologyLink: false,
      },
    },
  ];

  const testExternalMethodology: MyPublication['externalMethodology'] = {
    title: 'External methodology',
    url: 'https://example.com',
  };

  const completeReleaseStatus: ReleaseStageStatuses = {
    overallStage: 'Complete',
  };

  beforeEach(() => {
    releaseService.getReleaseStatus.mockResolvedValue(completeReleaseStatus);
  });

  test('renders correctly with a publication with no releases, methodologies or permissions', async () => {
    render(
      <MemoryRouter>
        <PublicationSummary
          publication={testPublication}
          topicId={testTopicId}
          onChangePublication={noop}
        />
      </MemoryRouter>,
    );

    expect(screen.getByText('No releases created')).toBeInTheDocument();

    expect(screen.getByText('Methodologies')).toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Create methodology',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Use an external methodology',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Manage publication',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Create new release',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders contact details correctly', async () => {
    render(
      <MemoryRouter>
        <PublicationSummary
          publication={{
            ...testPublication,
            contact: testContact,
          }}
          topicId={testTopicId}
          onChangePublication={noop}
        />
      </MemoryRouter>,
    );

    expect(screen.queryByText('No team name')).not.toBeInTheDocument();
    expect(screen.queryByText('No contact name')).not.toBeInTheDocument();

    expect(screen.getByText('Team Smith')).toBeInTheDocument();

    const teamEmailLink = screen.getByRole('link', {
      name: 'john.smith@test.com',
    });
    expect(teamEmailLink).toBeInTheDocument();
    expect(teamEmailLink).toHaveAttribute('href', 'mailto:john.smith@test.com');

    expect(
      screen.getByText(testContact.contactName as string),
    ).toBeInTheDocument();

    const contactTelNoLink = screen.getByRole('link', {
      name: '0777777777',
    });
    expect(contactTelNoLink).toBeInTheDocument();
    expect(contactTelNoLink).toHaveAttribute(
      'href',
      `tel:${testContact.contactTelNo}`,
    );
  });

  test('renders correct controls with full permissions', async () => {
    render(
      <MemoryRouter>
        <PublicationSummary
          publication={{
            ...testPublication,
            permissions: fullPermissions,
          }}
          topicId={testTopicId}
          onChangePublication={noop}
        />
      </MemoryRouter>,
    );

    expect(screen.getByText('No releases created')).toBeInTheDocument();

    expect(screen.getByText('Methodologies')).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Create methodology',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Use an external methodology',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Manage publication',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Create new release',
      }),
    ).toBeInTheDocument();
  });

  test('renders correctly with releases', async () => {
    render(
      <MemoryRouter>
        <PublicationSummary
          publication={{
            ...testPublication,
            releases: testReleases,
          }}
          topicId={testTopicId}
          onChangePublication={noop}
        />
      </MemoryRouter>,
    );

    expect(screen.queryByText('No releases created')).not.toBeInTheDocument();

    const releaseList = screen.getByTestId(
      `Releases for ${testPublication.title}`,
    );
    expect(releaseList).toBeInTheDocument();

    const releaseExpandButtons = within(releaseList)
      .getAllByRole('button')
      .filter(b => b.hasAttribute('aria-expanded'));

    // We expect only 2 Releases to be visible here, as the "rel-3" Amendment is hiding the "rel-2" Release that
    // it's an Amendment of.
    expect(releaseExpandButtons).toHaveLength(2);

    // Not exhaustively testing the contents of each Release, as this is handled in a separate component used by
    // PublicationSummary.
    const release1 = releaseExpandButtons[0];
    expect(release1.textContent).toEqual(
      `${testReleases[0].title} (not Live) Draft`,
    );

    const release2 = releaseExpandButtons[1];
    expect(release2.textContent).toEqual(
      `${testReleases[1].title} (not Live) Draft Amendment`,
    );
  });

  test('renders correctly with methodologies', async () => {
    render(
      <MemoryRouter>
        <PublicationSummary
          publication={{
            ...testPublication,
            methodologies: testMethodologies,
            externalMethodology: testExternalMethodology,
          }}
          topicId={testTopicId}
          onChangePublication={noop}
        />
      </MemoryRouter>,
    );

    // Not exhaustively testing the Methodology details, as this is handled in a separate component used by
    // PublicationSummary.
    expect(
      screen.getByRole('button', {
        name: `${testMethodologies[0].title} (Owned) Published`,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: `${testExternalMethodology.title} (External)`,
      }),
    ).toBeInTheDocument();
  });
});
