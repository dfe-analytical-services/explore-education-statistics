import React, { useContext, useEffect, useState } from 'react';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { Dictionary } from '@common/types';
import { LoginContext } from '@admin/components/Login';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import { Publication } from '@admin/services/publicationService';
import AdminDashboardPublicationsTab from '@admin/components/AdminDashboardPublicationsTab';
import groupBy from 'lodash/groupBy';
import Link from '../components/Link';
import Page from '../components/Page';

const AdminDashboardPage = () => {
  const emptyPublicationsArray: Publication[] = [];

  const [myPublications, setMyPublications] = useState(emptyPublicationsArray);

  const [inProgressPublications, setInProgressPublications] = useState(
    emptyPublicationsArray,
  );

  const authentication = useContext(LoginContext);

  const loadInitialData = () => {
    const { user } = authentication;
    const loggedInUserId = user ? user.id : null;

    const fetchedMyPublications =
      loggedInUserId === null
        ? []
        : DummyPublicationsData.allPublications.filter(
            _ => _.owner.id === loggedInUserId,
          );

    const fetchedInProgressPublications =
      loggedInUserId === null
        ? []
        : DummyPublicationsData.allPublications.filter(
            _ => _.owner.id !== loggedInUserId,
          );

    setMyPublications(fetchedMyPublications);
    setInProgressPublications(fetchedInProgressPublications);
  };

  useEffect(loadInitialData, []);

  const createThemeTopicTitleLabel = (_: Publication) =>
    `${_.topic.theme.title}, ${_.topic.title}`;

  const myPublicationsByThemeAndTopic: Dictionary<Publication[]> = groupBy(
    myPublications,
    createThemeTopicTitleLabel,
  );

  const inProgressPublicationsByThemeAndTopic: Dictionary<
    Publication[]
  > = groupBy(inProgressPublications, createThemeTopicTitleLabel);

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
        <TabsSection id="my-publications" title="Publications">
          <AdminDashboardPublicationsTab
            publicationsByThemeAndTopic={myPublicationsByThemeAndTopic}
            noResultsMessage="You have not yet created any publications"
          />
        </TabsSection>
        <TabsSection id="in-progress-publications" title="In progress">
          <AdminDashboardPublicationsTab
            publicationsByThemeAndTopic={inProgressPublicationsByThemeAndTopic}
            noResultsMessage="There are currently no releases in progress"
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
          Not you? <Link to="#">Sign out</Link>
        </span>
      </h1>
    </>
  );
};

export default AdminDashboardPage;
