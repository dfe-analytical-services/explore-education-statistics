import React, { useContext, useEffect, useState } from 'react';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { LoginContext } from '@admin/components/Login';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import { IdLabelPair, Publication } from '@admin/services/types/types';
import AdminDashboardPublicationsTab from '@admin/components/AdminDashboardPublicationsTab';
import Link from '../components/Link';
import Page from '../components/Page';

const AdminDashboardPage = () => {
  const [myPublications, setMyPublications] = useState<Publication[]>([]);

  const [inProgressPublications, setInProgressPublications] = useState<
    Publication[]
  >([]);

  const [themes, setThemes] = useState<IdLabelPair[]>();

  const [topics, setTopics] = useState<IdLabelPair[]>();

  const [selectedThemeId, setSelectedThemeId] = useState<string>();

  const [selectedTopicId, setSelectedTopicId] = useState<string>();

  const authentication = useContext(LoginContext);

  useEffect(() => {
    const { user } = authentication;
    const loggedInUserId = user ? user.id : null;

    if (loggedInUserId) {
      if (!themes) {
        const { themesAndTopics } = DummyPublicationsData;
        const listOfJustTheThemes = themesAndTopics.map(
          themeTopicsPair => themeTopicsPair.theme,
        );

        setThemes(listOfJustTheThemes);
        setSelectedThemeId(listOfJustTheThemes[0].id);
      }

      if (selectedThemeId) {
        const { themesAndTopics } = DummyPublicationsData;
        const matchingThemeTopicsPair =
          themesAndTopics.find(
            themeTopicsPair => themeTopicsPair.theme.id === selectedThemeId,
          ) || themesAndTopics[0];
        const justThisThemesTopics = matchingThemeTopicsPair.topics;
        setTopics(justThisThemesTopics);
        setSelectedTopicId(justThisThemesTopics[0].id);
      }

      if (selectedTopicId) {
        const fetchedMyPublications = DummyPublicationsData.allPublications.filter(
          publication => publication.owner.id === loggedInUserId,
        );

        const fetchedInProgressPublications = DummyPublicationsData.allPublications.filter(
          publication => publication.owner.id !== loggedInUserId,
        );

        setMyPublications(fetchedMyPublications);
        setInProgressPublications(fetchedInProgressPublications);
      }
    } else {
      setMyPublications([]);
      setInProgressPublications([]);
    }
  }, [authentication, selectedThemeId, selectedTopicId, themes, topics]);

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
          {themes && topics && selectedThemeId && selectedTopicId && (
            <AdminDashboardPublicationsTab
              publications={myPublications}
              noResultsMessage="You have not yet created any publications"
              themes={themes}
              topics={topics}
              selectedThemeId={selectedThemeId}
              selectedTopicId={selectedTopicId}
              onThemeChange={themeId => setSelectedThemeId(themeId)}
              onTopicChange={topicId => setSelectedThemeId(topicId)}
            />
          )}
        </TabsSection>
        <TabsSection id="in-progress-publications" title="In progress">
          Hi
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
