import Link from '@admin/components/Link';
import {LoginContext} from '@admin/components/Login';
import Page from '@admin/components/Page';
import loginService from '@admin/services/sign-in/service';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, {useContext, useEffect, useState} from 'react';
import AdminDashboardPublicationsTab from './components/AdminDashboardPublicationsTab';

const AdminDashboardPage = () => {

  const [draftReleaseCount, setDraftReleaseCount] = useState<number>();

  const [scheduledReleaseCount, setScheduledReleaseCount] = useState<number>();

  useEffect(() => {
    setDraftReleaseCount(1);
    setScheduledReleaseCount(2);
  });

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
            <AdminDashboardPublicationsTab />
          )}
        </TabsSection>
        <TabsSection id="draft-releases" title={`View draft releases (${draftReleaseCount})`}>
          <div className="govuk-inset-text">
            There are currently no releases ready for you to review
          </div>
        </TabsSection>
        <TabsSection
          id="scheduled-releases"
          title={`View scheduled releases (${scheduledReleaseCount})`}
        >
          <div className="govuk-inset-text">
            There are currently no unresolved comments
          </div>
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
