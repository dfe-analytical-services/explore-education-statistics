import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { noop } from 'lodash';
import PublicationSummary from '@admin/pages/admin-dashboard/components/PublicationSummary';
import {
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import { MemoryRouter } from 'react-router-dom';
import _releaseService, { MyRelease } from '@admin/services/releaseService';
import { MyMethodology } from '@admin/services/methodologyService';
import userEvent from '@testing-library/user-event';

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
    canCreateReleases: true,
    canUpdatePublication: true,
    canCreateMethodologies: true,
    canManageExternalMethodology: true,
  };

  const testPublication: MyPublication = {
    id: 'publication-1',
    title: 'Publication 1',
    contact: undefined,
    releases: [],
    methodologies: [],
    permissions: {
      canCreateReleases: false,
      canUpdatePublication: false,
      canCreateMethodologies: false,
      canManageExternalMethodology: false,
    },
  };

  const testReleases: MyRelease[] = [
    {
      id: 'rel-1',
      latestRelease: true,
      published: '2021-06-30T11:21:17.7585345',
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
      published: '2021-01-01T11:21:17.7585345',
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
      published: '2021-05-30T11:21:17.7585345',
      slug: 'rel-2-slug',
      title: 'Release 2',
      publicationId: testPublication.id,
      permissions: {
        canUpdateRelease: false,
      },
      approvalStatus: 'Approved',
    } as MyRelease,
  ];

  const testMethodologies: MyMethodology[] = [
    {
      amendment: false,
      id: '1234',
      latestInternalReleaseNote: 'this is the release note',
      previousVersionId: 'lfkjdlfj',
      published: '2021-06-08T09:04:17.9805585',
      slug: 'meth-1',
      status: 'Approved',
      title: 'I am a methodology',
      owningPublication: {
        id: 'p1',
        title: 'Publication title',
      },
      permissions: {
        canApproveMethodology: false,
        canUpdateMethodology: false,
        canDeleteMethodology: false,
        canMakeAmendmentOfMethodology: false,
        canMarkMethodologyAsDraft: false,
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
      screen.queryByRole('button', {
        name: 'Link to an externally hosted methodology',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText('Manage this publication', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText('Create new release', {
        selector: 'a',
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

    expect(
      screen.getByText(testContact.teamName as string),
    ).toBeInTheDocument();

    const teamEmailLink = screen.getByText(testContact.teamEmail as string, {
      selector: 'a',
    });
    expect(teamEmailLink).toBeInTheDocument();
    expect(teamEmailLink).toHaveAttribute(
      'href',
      `mailto:${testContact.teamEmail}`,
    );

    expect(
      screen.getByText(testContact.contactName as string),
    ).toBeInTheDocument();

    const contactTelNoLink = screen.getByText(
      testContact.contactTelNo as string,
      {
        selector: 'a',
      },
    );
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
      screen.getByRole('button', {
        name: 'Link to an externally hosted methodology',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Manage this publication', {
        selector: 'a',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Create new release', {
        selector: 'a',
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
      `${testReleases[0].title} (not Live)Draft`,
    );

    const release2 = releaseExpandButtons[1];
    expect(release2.textContent).toEqual(
      `${testReleases[1].title} (not Live)DraftAmendment`,
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
        name: `${testMethodologies[0].title} Approved`,
      }),
    ).toBeInTheDocument();

    const externalMethodologyLink = screen.getByText(
      `${testExternalMethodology.title} (external methodology)`,
      {
        selector: 'a',
      },
    );
    expect(externalMethodologyLink).toBeInTheDocument();
    expect(externalMethodologyLink).toHaveAttribute(
      'href',
      testExternalMethodology.url,
    );
  });

  test('handles the cancelling of Release Amendments successfully', async () => {
    const onChangePublicationCallback = jest.fn();

    render(
      <MemoryRouter>
        <PublicationSummary
          publication={{
            ...testPublication,
            releases: testReleases,
            permissions: fullPermissions,
          }}
          topicId={testTopicId}
          onChangePublication={onChangePublicationCallback}
        />
      </MemoryRouter>,
    );

    // Expand the Release Amendment to see the controls within it.
    userEvent.click(
      screen.getByRole('button', {
        name: `${testReleases[1].title} (not Live) Draft Amendment`,
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', {
          name: 'Cancel amendment',
        }),
      ).toBeInTheDocument();
    });

    // Now click the "Cancel amendment" button and check that the warning modal appears.
    releaseService.getDeleteReleasePlan.mockResolvedValue({
      releaseId: testReleases[1].id,
      methodologiesScheduledWithRelease: [
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

    userEvent.click(
      screen.getByRole('button', {
        name: 'Cancel amendment',
      }),
    );

    await waitFor(() => {
      expect(releaseService.getDeleteReleasePlan).toHaveBeenCalledWith(
        testReleases[1].id,
      );
    });

    expect(
      screen.getByText('Confirm you want to cancel this amended release'),
    ).toBeInTheDocument();
    expect(screen.getByText('Methodology 1')).toBeInTheDocument();
    expect(screen.getByText('Methodology 2')).toBeInTheDocument();

    // Confirm the cancelling of the Release Amendment.
    userEvent.click(
      screen.getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(releaseService.deleteRelease).toHaveBeenCalledWith(
        testReleases[1].id,
      );
      expect(onChangePublicationCallback).toHaveBeenCalled();
    });

    expect(
      screen.queryByText('Confirm you want to cancel this amended release'),
    ).not.toBeInTheDocument();
  });
});
