import Link from '@admin/components/Link';
import {LoginContext} from '@admin/components/Login';
import Page from '@admin/components/Page';
import ReleasesTab from "@admin/pages/admin-dashboard/components/ReleasesByStatusTab";
import dashboardService from "@admin/services/dashboard/service";
import loginService from '@admin/services/sign-in/service';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, {useContext, useEffect, useState} from 'react';
import MyPublicationsTab from './components/MyPublicationsTab';

const AdminDashboardPage = () => {

  const [draftReleaseCount, setDraftReleaseCount] = useState<number>();

  const [scheduledReleaseCount, setScheduledReleaseCount] = useState<number>();

  useEffect(() => {
    setDraftReleaseCount(1);
    setScheduledReleaseCount(2);
  }, []);

  return (
    <Page wide breadcrumbs={[{ name: 'Administrator dashboard' }]}>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <UserGreeting />
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
          {(
            <MyPublicationsTab />
          )}
        </TabsSection>
        <TabsSection
          id="draft-releases"
          title={`View draft releases (${draftReleaseCount})`}
        >
          <ReleasesTab
            getReleasesFn={dashboardService.getDraftReleases}
            noReleasesMessage='There are currently no draft releases'
          />
        </TabsSection>
        <TabsSection
          id="scheduled-releases"
          title={`View scheduled releases (${scheduledReleaseCount})`}
        >
          <ReleasesTab
            getReleasesFn={dashboardService.getScheduledReleases}
            noReleasesMessage='There are currently no scheduled releases'
          />
        </TabsSection>
      </Tabs>
    </Page>
  );
};

const UserGreeting = () => {
  const { user } = useContext(LoginContext);

  return (
    <>
      <span className="govuk-caption-xl">Welcome</span>

      <h1 className="govuk-heading-xl">
        {user ? user.name : ''}{' '}
        <span className="govuk-body-s">
          Not you?{' '}
          <a className="govuk-link" href={loginService.getSignOutLink()}>
            Sign out
          </a>
        </span>
      </h1>
    </>
  );
};

export default AdminDashboardPage;
