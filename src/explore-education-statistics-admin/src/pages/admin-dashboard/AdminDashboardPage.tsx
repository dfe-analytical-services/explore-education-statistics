import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import { useAuthContext } from '@admin/contexts/AuthContext';
import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import DraftReleasesTab from '@admin/pages/admin-dashboard/components/DraftReleasesTab';
import NonScheduledReleaseSummary from '@admin/pages/admin-dashboard/components/NonScheduledReleaseSummary';
import PrereleaseAccessManagement from '@admin/pages/admin-dashboard/components/PrereleaseAccessManagement';
import ReleasesTab from '@admin/pages/admin-dashboard/components/ReleasesByStatusTab';
import ReleaseSummary from '@admin/pages/admin-dashboard/components/ReleaseSummary';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import { PrereleaseContactDetails } from '@admin/services/common/types';
import dashboardService from '@admin/services/dashboard/service';
import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import service from '@admin/services/release/create-release/service';
import loginService from '@admin/services/sign-in/service';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { Dictionary } from '@common/types';
import React, { useEffect, useState } from 'react';
import ManagePublicationsAndReleasesTab from './components/ManagePublicationsAndReleasesTab';

interface Model {
  draftReleases: AdminDashboardRelease[];
  scheduledReleases: AdminDashboardRelease[];
}

const AdminDashboardPage = () => {
  const { user } = useAuthContext();
  const [model, setModel] = useState<Model>();
  const [cancelAmendmentReleaseId, setCancelAmendmentReleaseId] = useState<
    string
  >();
  useEffect(() => {
    Promise.all([
      dashboardService.getDraftReleases(),
      dashboardService.getScheduledReleases(),
    ]).then(([draftReleases, scheduledReleases]) => {
      const contactResultsByRelease = scheduledReleases.map(release =>
        dashboardService
          .getPreReleaseContactsForRelease(release.id)
          .then(contacts => ({
            releaseId: release.id,
            contacts,
          })),
      );

      return Promise.all(contactResultsByRelease).then(contactResults => {
        const preReleaseContactsByScheduledRelease: Dictionary<PrereleaseContactDetails[]> = {};
        contactResults.forEach(result => {
          const { releaseId, contacts } = result;
          preReleaseContactsByScheduledRelease[releaseId] = contacts;
        });
        setModel({
          draftReleases,
          scheduledReleases,
        });
      });
    });
  }, []);

  return (
    <>
      {model && (
        <Page wide breadcrumbs={[{ name: 'Administrator dashboard' }]}>
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <span className="govuk-caption-xl">Welcome</span>
              <h1 className="govuk-heading-xl">
                {user ? user.name : ''}{' '}
                <span className="govuk-body-s">
                  Not you?{' '}
                  <li className="govuk-header__navigation-item">
                    <Link
                      className="govuk-link"
                      to={loginService.getSignOutLink()}
                    >
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
                    <Link
                      to="/documentation/create-new-release"
                      target="_blank"
                    >
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

          <Tabs id="publicationTabs">
            <TabsSection
              id="my-publications"
              title="Manage publications and releases"
            >
              <ManagePublicationsAndReleasesTab />
            </TabsSection>
            <TabsSection
              lazy
              id="draft-releases"
              title={`View draft releases (${model.draftReleases.length})`}
            >
              <DraftReleasesTab initialReleases={model.draftReleases} />
            </TabsSection>
            <TabsSection
              lazy
              id="scheduled-releases"
              title={`View scheduled releases (${model.scheduledReleases.length})`}
            >
              <ReleasesTab
                releases={model.scheduledReleases}
                noReleasesMessage="There are currently no scheduled releases"
                releaseSummaryRenderer={release => (
                  <ReleaseSummary
                    key={release.id}
                    release={release}
                    actions={
                      <ButtonLink
                        to={summaryRoute.generateLink(
                          release.publicationId,
                          release.id,
                        )}
                      >
                        Preview release
                      </ButtonLink>
                    }
                  >
                    <PrereleaseAccessManagement release={release} />
                  </ReleaseSummary>
                )}
              />
            </TabsSection>
          </Tabs>
        </Page>
      )}
    </>
  );
};

export default AdminDashboardPage;
