import useQueryParams from '@admin/hooks/useQueryParams';
import { ThemeTopicParams } from '@admin/routes/routes';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { useAuthContext } from '@admin/contexts/AuthContext';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import DraftReleasesTab from '@admin/pages/admin-dashboard/components/DraftReleasesTab';
import PublicationsTab from '@admin/pages/admin-dashboard/components/PublicationsTab';
import ScheduledReleasesTab from '@admin/pages/admin-dashboard/components/ScheduledReleasesTab';
import loginService from '@admin/services/loginService';
import releaseService from '@admin/services/releaseService';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, { useState } from 'react';
import ManagePublicationsAndReleasesTab from './components/ManagePublicationsAndReleasesTab';

const AdminDashboardPage = () => {
  const { showNewDashboard } = useQueryParams<ThemeTopicParams>(); // TODO EES-3217 - remove when ready to go live
  const { user } = useAuthContext();
  const isBauUser = user?.permissions.canAccessUserAdministrationPages ?? false;

  const [totalDraftReleases, setTotalDraftReleases] = useState<number>(0);

  const {
    value: draftReleases = [],
    isLoading: isLoadingDraftReleases,
    retry: reloadDraftReleases,
  } = useAsyncHandledRetry(async () => {
    const releases = await releaseService.getDraftReleases();
    // Store the total so it doesn't flicker in the tab while reloading.
    setTotalDraftReleases(releases.length);
    return releases;
  });
  const {
    value: scheduledReleases = [],
    isLoading: isLoadingScheduledReleases,
  } = useAsyncHandledRetry(releaseService.getScheduledReleases);

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

          {showNewDashboard ? (
            <>
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
            </>
          ) : (
            <>
              <p>This is your administration dashboard - here you can:</p>

              <ul className="govuk-!-margin-bottom-6">
                <li>
                  <Link to="/dashboard">manage publications and releases</Link>
                </li>
                {user?.permissions.canManageAllTaxonomy && (
                  <li>
                    <Link to="/themes">manage themes and topics</Link>
                  </li>
                )}
              </ul>
            </>
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
          if (showNewDashboard && section.id === 'draft-releases') {
            reloadDraftReleases();
          }
        }}
      >
        <TabsSection
          id={showNewDashboard ? 'publications' : 'publicationsReleases'}
          title={
            showNewDashboard
              ? 'Your publications'
              : 'Manage publications and releases'
          }
        >
          {showNewDashboard ? (
            <PublicationsTab isBauUser={isBauUser} />
          ) : (
            <ManagePublicationsAndReleasesTab />
          )}
        </TabsSection>
        <TabsSection
          lazy
          id="draft-releases"
          title={
            showNewDashboard
              ? `Draft releases (${totalDraftReleases})`
              : `View draft releases (${totalDraftReleases})`
          }
        >
          <DraftReleasesTab
            isBauUser={isBauUser}
            isLoading={isLoadingDraftReleases}
            releases={draftReleases}
            showNewDashboard={!!showNewDashboard}
            onChangeRelease={reloadDraftReleases}
          />
        </TabsSection>
        <TabsSection
          lazy
          id="scheduled-releases"
          title={
            showNewDashboard
              ? `Approved scheduled releases (${scheduledReleases?.length})`
              : `View scheduled releases (${scheduledReleases?.length})`
          }
        >
          <ScheduledReleasesTab
            isLoading={isLoadingScheduledReleases}
            releases={scheduledReleases}
            showNewDashboard={!!showNewDashboard}
          />
        </TabsSection>
      </Tabs>
    </Page>
  );
};

export default AdminDashboardPage;
