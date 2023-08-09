import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { useAuthContext } from '@admin/contexts/AuthContext';
import ApprovalsTab from '@admin/pages/admin-dashboard/components/ApprovalsTab';
import DraftReleasesTab from '@admin/pages/admin-dashboard/components/DraftReleasesTab';
import PublicationsTab from '@admin/pages/admin-dashboard/components/PublicationsTab';
import ScheduledReleasesTab from '@admin/pages/admin-dashboard/components/ScheduledReleasesTab';
import releaseQueries from '@admin/queries/releaseQueries';
import loginService from '@admin/services/loginService';
import { MethodologyVersion } from '@admin/services/methodologyService';
import { Release } from '@admin/services/releaseService';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import React from 'react';
import { useQuery } from '@tanstack/react-query';

const testMethodologies: MethodologyVersion[] = [
  {
    id: 'c8c911e3-39c1-452b-801f-25bb79d1deb7',
    methodologyId: 'b8bd000c-f9d8-4319-a2b3-6bc18675e5ac',
    owningPublication: {
      id: 'bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9',
      title: 'Permanent and fixed-period exclusions in England',
    },
    otherPublications: [],
    published: '2018-08-25T00:00:00',
    publishingStrategy: 'Immediately',
    slug: 'permanent-and-fixed-period-exclusions-in-england',
    status: 'Approved',
    title: 'Pupil exclusion statistics: methodology',
    amendment: false,
  },
];

const testReleases: Release[] = [
  {
    id: 'test-id',
    title: 'Academic year 2016/17',
    slug: '2024-25',
    publicationId: 'bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9',
    publicationTitle: 'Permanent and fixed-period exclusions in England',
    publicationSummary: '',
    publicationSlug: 'pub-slug',
    year: 2024,
    yearTitle: '2024/25',
    nextReleaseDate: {
      year: '2200',
      month: '1',
      day: '',
    },
    live: false,
    timePeriodCoverage: {
      value: 'AY',
      label: 'Academic year',
    },
    preReleaseAccessList: '<p>Test public access list</p>',
    preReleaseUsersOrInvitesAdded: false,
    previousVersionId: 'f',
    latestRelease: false,
    type: 'NationalStatistics',
    contact: {
      teamName: 'UI test team name',
      teamEmail: 'ui_test@test.com',
      contactName: 'UI test contact name',
      contactTelNo: '1234 1234',
    },
    approvalStatus: 'Approved',
    notifySubscribers: false,
    latestInternalReleaseNote: 'Approved by UI tests',
    amendment: false,
    permissions: {
      canAddPrereleaseUsers: true,
      canViewRelease: true,
      canUpdateRelease: true,
      canDeleteRelease: false,
      canMakeAmendmentOfRelease: false,
    },
    updatePublishedDate: false,
  },
  {
    id: '86d868cf-ff4b-4325-ef26-08d93c9b5089',
    title: 'Academic year 2024/25',
    slug: '2024-25',
    publicationId: '959bd40c-4685-46ff-396d-08d93c9b5159',
    publicationTitle:
      'UI tests - Publication and Release UI Permissions Publication Owner',
    publicationSummary: '',
    publicationSlug:
      'ui-tests-publication-and-release-ui-permissions-publication-owner',
    year: 2024,
    yearTitle: '2024/25',
    nextReleaseDate: {
      year: '2200',
      month: '1',
      day: '',
    },
    publishScheduled: '2048-11-16',
    live: false,
    timePeriodCoverage: {
      value: 'AY',
      label: 'Academic year',
    },
    preReleaseAccessList: '<p>Test public access list</p>',
    preReleaseUsersOrInvitesAdded: false,
    previousVersionId: 'f',
    latestRelease: false,
    type: 'NationalStatistics',
    contact: {
      teamName: 'UI test team name',
      teamEmail: 'ui_test@test.com',
      contactName: 'UI test contact name',
      contactTelNo: '1234 1234',
    },
    approvalStatus: 'Approved',
    notifySubscribers: false,
    latestInternalReleaseNote: 'Approved by UI tests',
    amendment: false,
    permissions: {
      canAddPrereleaseUsers: true,
      canViewRelease: true,
      canUpdateRelease: true,
      canDeleteRelease: false,
      canMakeAmendmentOfRelease: false,
    },
    updatePublishedDate: false,
  },
];

const AdminDashboardPage = () => {
  // TO DO EES-4448 replace with real permission
  const isApprover = true;
  const { user } = useAuthContext();
  const isBauUser = user?.permissions.isBauUser ?? false;

  const {
    data: draftReleases = [],
    isLoading: isLoadingDraftReleases,
    refetch: reloadDraftReleases,
  } = useQuery(releaseQueries.listDraftReleases);
  const {
    data: scheduledReleases = [],
    isLoading: isLoadingScheduledReleases,
  } = useQuery(releaseQueries.listScheduledReleases);

  // TO DO EES-4448 fetch approvals data here and remove test data
  // const {
  //   data: methodologyApprovals = [],
  //   isLoading: isLoadingMethodologyApprovals,
  // } = useQuery(TBC);
  // const {
  //   data: releaseApprovals = [],
  //   isLoading: isLoadingReleaseApprovals,
  // } = useQuery(TBC);
  const methodologyApprovals = testMethodologies;
  const releaseApprovals = testReleases;
  const isLoadingApprovals = false;

  const totalApprovals = methodologyApprovals.length + releaseApprovals.length;

  return (
    <Page wide breadcrumbs={[{ name: 'Administrator dashboard' }]}>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle
            caption={`Welcome ${user?.name}`}
            title="Dashboard"
            className="govuk-!-margin-bottom-4"
          />

          <p className="govuk-body-s">
            {user && (
              <>
                Logged in as <strong>{user?.name}</strong>. Not you?{' '}
              </>
            )}
            <Link className="govuk-link" to={loginService.getSignOutLink()}>
              Sign out
            </Link>
          </p>

          {isApprover && totalApprovals > 0 && (
            <WarningMessage>
              You have outstanding <Link to="#approvals">approvals</Link>
            </WarningMessage>
          )}

          <p>
            This is your administration dashboard, here you can manage
            publications, releases and methodologies.
          </p>
          {isBauUser && (
            <ul className="govuk-!-margin-bottom-6">
              <li>
                <Link to="/themes">manage themes and topics</Link>
              </li>
            </ul>
          )}
        </div>

        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/contact-us" target="_blank">
                  Contact us
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

      <Tabs
        id="dashboardTabs"
        onToggle={section => {
          if (section.id === 'draft-releases') {
            reloadDraftReleases();
          }
        }}
      >
        <TabsSection id="publications" title="Your publications">
          <PublicationsTab isBauUser={isBauUser} />
        </TabsSection>
        <TabsSection
          lazy
          id="draft-releases"
          data-testid="publication-draft-releases"
          title={`Draft releases (${draftReleases.length})`}
        >
          <DraftReleasesTab
            isBauUser={isBauUser}
            isLoading={isLoadingDraftReleases}
            releases={draftReleases}
            onChangeRelease={reloadDraftReleases}
          />
        </TabsSection>

        {isApprover && (
          <TabsSection
            lazy
            id="approvals"
            data-testid="publication-approvals"
            title={`My approvals (${totalApprovals})`}
          >
            <ApprovalsTab
              isLoading={isLoadingApprovals}
              methodologyApprovals={methodologyApprovals}
              releaseApprovals={releaseApprovals}
            />
          </TabsSection>
        )}

        <TabsSection
          lazy
          id="scheduled-releases"
          title={`Approved scheduled releases (${scheduledReleases?.length})`}
        >
          <ScheduledReleasesTab
            isLoading={isLoadingScheduledReleases}
            releases={scheduledReleases}
          />
        </TabsSection>
      </Tabs>
    </Page>
  );
};

export default AdminDashboardPage;
