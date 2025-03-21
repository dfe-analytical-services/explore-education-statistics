import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { useAuthContext } from '@admin/contexts/AuthContext';
import ApprovalsTab from '@admin/pages/admin-dashboard/components/ApprovalsTab';
import DraftReleasesTab from '@admin/pages/admin-dashboard/components/DraftReleasesTab';
import PublicationsTab from '@admin/pages/admin-dashboard/components/PublicationsTab';
import ScheduledReleasesTab from '@admin/pages/admin-dashboard/components/ScheduledReleasesTab';
import releaseQueries from '@admin/queries/releaseQueries';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import React from 'react';
import { useQuery } from '@tanstack/react-query';
import methodologyQueries from '@admin/queries/methodologyQueries';
import { handleLogout } from '@admin/auth/msal';
import ButtonText from '@common/components/ButtonText';

const AdminDashboardPage = () => {
  const { user } = useAuthContext();
  const isBauUser = user?.permissions.isBauUser ?? false;
  const isApprover = user?.permissions.isApprover ?? false;

  const {
    data: draftReleases = [],
    isLoading: isLoadingDraftReleases,
    refetch: reloadDraftReleases,
  } = useQuery(releaseQueries.listDraftReleaseVersions);

  const {
    data: scheduledReleases = [],
    isLoading: isLoadingScheduledReleases,
  } = useQuery(releaseQueries.listScheduledReleaseVersions);

  const { data: releaseApprovals = [], isLoading: isLoadingReleaseApprovals } =
    useQuery({
      ...releaseQueries.listReleaseVersionsForApproval,
      enabled: isApprover,
    });

  const {
    data: methodologyApprovals = [],
    isLoading: isLoadingMethodologyApprovals,
  } = useQuery({
    ...methodologyQueries.listMethodologiesForApproval,
    enabled: isApprover,
  });

  const isLoadingApprovals =
    isLoadingReleaseApprovals || isLoadingMethodologyApprovals;

  const totalApprovals = !isLoadingApprovals
    ? methodologyApprovals.length + releaseApprovals.length
    : 0;

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
                Logged in as <strong>{user.name}</strong>. Not you?{' '}
              </>
            )}
            <ButtonText
              onClick={() => handleLogout()}
              testId="dashboard-sign-out-button"
            >
              Sign out
            </ButtonText>
          </p>

          {isApprover && totalApprovals > 0 && (
            <WarningMessage testId="outstanding-approvals-warning">
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
                <Link to="/themes">manage themes</Link>
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

      <Tabs id="dashboardTabs">
        <TabsSection id="publications" title="Your publications">
          <PublicationsTab isBauUser={isBauUser} />
        </TabsSection>
        <TabsSection
          lazy
          id="draft-releases"
          data-testid="publication-draft-releases"
          title={`Draft releases ${
            !isLoadingDraftReleases ? `(${draftReleases.length})` : ''
          }`}
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
            title={`Your approvals ${
              !isLoadingApprovals ? `(${totalApprovals})` : ''
            }`}
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
          title={`Approved scheduled releases ${
            !isLoadingScheduledReleases ? `(${scheduledReleases.length})` : ''
          }`}
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
