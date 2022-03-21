import { render, screen, within } from '@testing-library/react';
import React from 'react';
import { noop } from 'lodash';
import PublicationSummary from '@admin/pages/admin-dashboard/components/PublicationSummary';
import {
  MyPublication,
  MyPublicationMethodology,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import { MemoryRouter } from 'react-router-dom';
import { MyRelease } from '@admin/services/releaseService';

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
    canCreateMethodologies: true,
    canManageExternalMethodology: true,
  };

  const testPublication: MyPublication = {
    id: 'publication-1',
    title: 'Publication 1',
    contact: undefined,
    releases: [],
    legacyReleases: [],
    methodologies: [],
    themeId: 'theme-1',
    topicId: 'topic-1',
    permissions: {
      canAdoptMethodologies: false,
      canCreateReleases: false,
      canUpdatePublication: false,
      canUpdatePublicationTitle: false,
      canCreateMethodologies: false,
      canManageExternalMethodology: false,
    },
  };

  const testReleases: MyRelease[] = [
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
    } as MyRelease,
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
    } as MyRelease,
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
    } as MyRelease,
  ];

  const testMethodologies: MyPublicationMethodology[] = [
    {
      owner: true,
      methodology: {
        amendment: false,
        id: 'methodology-v1',
        latestInternalReleaseNote: 'this is the release note',
        methodologyId: 'methodology-1',
        previousVersionId: 'methodology-previous-version-1',
        published: '2021-06-08T09:04:17.9805585',
        slug: 'methodology-slug-1',
        status: 'Approved',
        title: 'Methodology 1',
        owningPublication: {
          id: 'owning-publication-1',
          title: 'Owning publication title',
        },
        permissions: {
          canApproveMethodology: false,
          canUpdateMethodology: false,
          canDeleteMethodology: false,
          canMakeAmendmentOfMethodology: false,
          canMarkMethodologyAsDraft: false,
        },
      },
      permissions: {
        canDropMethodology: false,
      },
    },
  ];

  const testExternalMethodology: MyPublication['externalMethodology'] = {
    title: 'External methodology',
    url: 'https://example.com',
  };

  test('renders correctly with a publication with no releases, methodologies, contact details or permissions', async () => {
    render(
      <MemoryRouter>
        <PublicationSummary
          publication={testPublication}
          topicId={testTopicId}
          onChangePublication={noop}
        />
      </MemoryRouter>,
    );

    expect(screen.getByText('No team name')).toBeInTheDocument();
    expect(screen.getByText('No contact name')).toBeInTheDocument();

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

  test('renders correctly with team and contact details', async () => {
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
        name: `${testMethodologies[0].methodology.title} (Owned) Published`,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: `${testExternalMethodology.title} (External)`,
      }),
    ).toBeInTheDocument();
  });
});
