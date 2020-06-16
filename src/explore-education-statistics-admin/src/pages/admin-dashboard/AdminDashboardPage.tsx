import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import { useAuthContext } from '@admin/contexts/AuthContext';
import DraftReleasesTab from '@admin/pages/admin-dashboard/components/DraftReleasesTab';
import PrereleaseAccessManagement from '@admin/pages/admin-dashboard/components/PrereleaseAccessManagement';
import ReleasesTab from '@admin/pages/admin-dashboard/components/ReleasesByStatusTab';
import ReleaseSummary from '@admin/pages/admin-dashboard/components/ReleaseSummary';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import loginService from '@admin/services/loginService';
import releaseService, { Release } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import ManagePublicationsAndReleasesTab from './components/ManagePublicationsAndReleasesTab';

const AdminDashboardPage = () => {
  const { user } = useAuthContext();
  const {
    value = [[], []],
    isLoading: loadingReleases,
    retry: reloadDashboard,
  } = useAsyncRetry(async () => {
    const [draftReleases, scheduledReleases] = await Promise.all([
      releaseService.getDraftReleases(),
      releaseService.getScheduledReleases(),
    ]);

    return [draftReleases, scheduledReleases];
  }, []);

  const [draftReleases, scheduledReleases] = value as [Release[], Release[]];

  return (
    <Page wide breadcrumbs={[{ name: 'Administrator dashboard' }]}>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <span className="govuk-caption-xl">Welcome</span>
          <h1 className="govuk-heading-xl">
            {user ? user.name : ''}{' '}
            <span className="govuk-body-s">
              Not you?{' '}
              <li className="govuk-header__navigation-item">
                <Link className="govuk-link" to={loginService.getSignOutLink()}>
                  Sign out
                </Link>
              </li>
            </span>
          </h1>

          <p>This is your administration dashboard - here you can:</p>

          <ul className="govuk-bullet--list govuk-!-margin-bottom-9">
            <li>
              <Link to="/dashboard">manage publications and releases</Link>
            </li>
            <li>
              <Link to="/methodologies">manage methodologies</Link>
            </li>
          </ul>
        </div>

        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/documentation/using-dashboard" target="_blank">
                  Using your administration dashboard{' '}
                </Link>
              </li>
              <li>
                <Link to="/documentation/create-new-release" target="_blank">
                  Creating a new release{' '}
                </Link>
              </li>
              <li>
                <Link to="/contact-us" target="_blank">
                  Contact us
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

      <LoadingSpinner loading={loadingReleases}>
        <Tabs id="publicationTabs">
          <TabsSection
            id="my-publications"
            title="Manage publications and releases"
          >
            <ManagePublicationsAndReleasesTab
              nonLiveReleases={draftReleases.concat(scheduledReleases)}
              onChangePublication={reloadDashboard}
            />
          </TabsSection>
          <TabsSection
            lazy
            id="draft-releases"
            title={`View draft releases (${draftReleases.length})`}
          >
            <DraftReleasesTab
              releases={draftReleases}
              onChangeRelease={reloadDashboard}
            />
          </TabsSection>
          <TabsSection
            lazy
            id="scheduled-releases"
            title={`View scheduled releases (${scheduledReleases.length})`}
          >
            <ReleasesTab
              releases={scheduledReleases}
              noReleasesMessage="There are currently no scheduled releases"
              releaseSummaryRenderer={release => (
                <ReleaseSummary
                  key={release.id}
                  release={release}
                  actions={
                    <ButtonLink
                      to={summaryRoute.generateLink({
                        publicationId: release.publicationId,
                        releaseId: release.id,
                      })}
                    >
                      Preview release
                    </ButtonLink>
                  }
                >
                  {release.permissions.canUpdateRelease && (
                    <PrereleaseAccessManagement release={release} />
                  )}
                </ReleaseSummary>
              )}
            />
          </TabsSection>
        </Tabs>
      </LoadingSpinner>
    </Page>
  );
};

export default AdminDashboardPage;
