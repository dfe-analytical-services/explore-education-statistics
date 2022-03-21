import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { useAuthContext } from '@admin/contexts/AuthContext';
import DraftReleasesTab from '@admin/pages/admin-dashboard/components/DraftReleasesTab';
import ReleasesTab from '@admin/pages/admin-dashboard/components/ReleasesByStatusTab';
import ReleaseSummary from '@admin/pages/admin-dashboard/components/ReleaseSummary';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import loginService from '@admin/services/loginService';
import releaseService from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import { generatePath } from 'react-router';
import ManagePublicationsAndReleasesTab from './components/ManagePublicationsAndReleasesTab';

const AdminDashboardPage = () => {
  const { user } = useAuthContext();
  const {
    value,
    isLoading: loadingReleases,
    retry: reloadDashboard,
  } = useAsyncRetry(async () => {
    const [draftReleases, scheduledReleases] = await Promise.all([
      releaseService.getDraftReleases(),
      releaseService.getScheduledReleases(),
    ]);

    return {
      draftReleases,
      scheduledReleases,
    };
  }, []);

  const { draftReleases = [], scheduledReleases = [] } = value ?? {};

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
        </div>

        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              {/* EES-2464
              <li>
                <Link to="/documentation/using-dashboard" target="_blank">
                  Using your administration dashboard{' '}
                </Link>
              </li>
              <li>
                <Link to="/documentation/create-new-release" target="_blank">
                  Creating a new release{' '}
                </Link>
              </li> */}
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
            id="publicationsReleases"
            title="Manage publications and releases"
          >
            <ManagePublicationsAndReleasesTab />
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
                      to={generatePath<ReleaseRouteParams>(
                        releaseSummaryRoute.path,
                        {
                          publicationId: release.publicationId,
                          releaseId: release.id,
                        },
                      )}
                      variant="secondary"
                    >
                      View release
                    </ButtonLink>
                  }
                />
              )}
            />
          </TabsSection>
        </Tabs>
      </LoadingSpinner>
    </Page>
  );
};

export default AdminDashboardPage;
