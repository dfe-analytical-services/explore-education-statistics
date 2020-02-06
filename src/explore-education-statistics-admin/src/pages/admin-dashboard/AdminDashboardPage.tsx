import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import LoginContext from '@admin/components/Login';
import Page from '@admin/components/Page';
import PrereleaseAccessManagement from '@admin/pages/admin-dashboard/components/PrereleaseAccessManagement';
import ReleasesTab from '@admin/pages/admin-dashboard/components/ReleasesByStatusTab';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import { PrereleaseContactDetails } from '@admin/services/common/types';
import dashboardService from '@admin/services/dashboard/service';
import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import loginService from '@admin/services/sign-in/service';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { Dictionary } from '@common/types';
import React, { useContext, useEffect, useState } from 'react';
import ManagePublicationsAndReleasesTab from './components/ManagePublicationsAndReleasesTab';

interface Model {
  draftReleases: AdminDashboardRelease[];
  scheduledReleases: AdminDashboardRelease[];
}

const AdminDashboardPage = ({ handleApiErrors }: ErrorControlProps) => {
  const { user } = useContext(LoginContext);
  const [model, setModel] = useState<Model>();
  useEffect(() => {
    Promise.all([
      dashboardService.getDraftReleases(),
      dashboardService.getScheduledReleases(),
    ])
      .then(([draftReleases, scheduledReleases]) => {
        const contactResultsByRelease = scheduledReleases.map(release =>
          dashboardService
            .getPreReleaseContactsForRelease(release.id)
            .then(contacts => ({
              releaseId: release.id,
              contacts,
            })),
        );

        return Promise.all(contactResultsByRelease).then(contactResults => {
          const preReleaseContactsByScheduledRelease: Dictionary<
            PrereleaseContactDetails[]
          > = {};
          contactResults.forEach(result => {
            const { releaseId, contacts } = result;
            preReleaseContactsByScheduledRelease[releaseId] = contacts;
          });
          setModel({
            draftReleases,
            scheduledReleases,
          });
        });
      })
      .catch(handleApiErrors);
  }, [handleApiErrors]);

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
                  <Link to="/methodology">manage methodology</Link>
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
              id="draft-releases"
              title={`View draft releases (${model.draftReleases.length})`}
            >
              <ReleasesTab
                releases={model.draftReleases}
                noReleasesMessage="There are currently no draft releases"
                actions={release => (
                  <ButtonLink
                    to={summaryRoute.generateLink(
                      release.publicationId,
                      release.id,
                    )}
                  >
                    View and edit release
                  </ButtonLink>
                )}
              />
            </TabsSection>
            <TabsSection
              id="scheduled-releases"
              title={`View scheduled releases (${model.scheduledReleases.length})`}
            >
              <ReleasesTab
                releases={model.scheduledReleases}
                noReleasesMessage="There are currently no scheduled releases"
                actions={release => (
                  <ButtonLink
                    to={summaryRoute.generateLink(
                      release.publicationId,
                      release.id,
                    )}
                  >
                    Preview release
                  </ButtonLink>
                )}
              >
                {release => <PrereleaseAccessManagement release={release} />}
              </ReleasesTab>
            </TabsSection>
          </Tabs>
        </Page>
      )}
    </>
  );
};

export default withErrorControl(AdminDashboardPage);
