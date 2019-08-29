import ButtonLink from "@admin/components/ButtonLink";
import Link from '@admin/components/Link';
import { LoginContext } from '@admin/components/Login';
import Page from '@admin/components/Page';
import ReleasesTab from '@admin/pages/admin-dashboard/components/ReleasesByStatusTab';
import {summaryRoute} from "@admin/routes/edit-release/routes";
import dashboardService from '@admin/services/dashboard/service';
import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import loginService from '@admin/services/sign-in/service';
import RelatedInformation from '@common/components/RelatedInformation';
import SummaryListItem from "@common/components/SummaryListItem";
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, { useContext, useEffect, useState } from 'react';
import MyPublicationsTab from './components/MyPublicationsTab';

interface Model {
  draftReleases: AdminDashboardRelease[];
  scheduledReleases: AdminDashboardRelease[];
}

const AdminDashboardPage = () => {
  const { user } = useContext(LoginContext);

  const [model, setModel] = useState<Model>();

  useEffect(() => {
    Promise.all([
      dashboardService.getDraftReleases(),
      dashboardService.getScheduledReleases(),
    ]).then(([draft, scheduled]) => {
      setModel({
        draftReleases: draft,
        scheduledReleases: scheduled,
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
                  <a
                    className="govuk-link"
                    href={loginService.getSignOutLink()}
                  >
                    Sign out
                  </a>
                </span>
              </h1>
            </div>
            <div className="govuk-grid-column-one-third">
              <RelatedInformation heading="Help and guidance">
                <ul className="govuk-list">
                  <li>
                    <Link to="/prototypes/methodology-home">
                      Administrators' guide{' '}
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
              <MyPublicationsTab />
            </TabsSection>
            <TabsSection
              id="draft-releases"
              title={`View draft releases (${model.draftReleases.length})`}
            >
              <ReleasesTab
                releases={model.draftReleases}
                noReleasesMessage="There are currently no draft releases"
                actions={release => (
                  <ButtonLink to={summaryRoute.generateLink(release.publicationId, release.id)}>
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
                  <ButtonLink to={summaryRoute.generateLink(release.publicationId, release.id)}>
                    Preview release
                  </ButtonLink>
                )}
                afterCommentsSection={release => (
                  <SummaryListItem term="Pre release access">
                    hello
                  </SummaryListItem>
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
