import React, { useContext, useEffect, useState } from 'react';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { LoginContext } from '@admin/components/Login';
import DummyPublicationsData, {
  ThemeAndTopics,
} from '@admin/pages/DummyPublicationsData';
import { IdLabelPair, Publication } from '@admin/services/types/types';
import AdminDashboardPublicationsTab from '@admin/components/AdminDashboardPublicationsTab';
import themeService from '@admin/services/themeService';
import Link from '../components/Link';
import Page from '../components/Page';

const themeToThemeWithIdLabelAndTopics = (theme: ThemeAndTopics) => ({
  id: theme.id,
  label: theme.title,
  topics: theme.topics.map(topic => ({
    id: topic.id,
    label: topic.title,
  })),
});

const findThemeById = (
  themeId: string,
  availableThemes: ThemeAndTopicsIdsAndLabels[],
) => availableThemes.find(theme => theme.id === themeId) || availableThemes[0];

const findTopicById = (topicId: string, theme: ThemeAndTopicsIdsAndLabels) =>
  theme.topics.find(topic => topic.id === topicId) || theme.topics[0];

interface ThemeAndTopicsIdsAndLabels extends IdLabelPair {
  topics: IdLabelPair[];
}

const AdminDashboardPage = () => {
  const [myPublications, setMyPublications] = useState<Publication[]>([]);

  const [themes, setThemes] = useState<ThemeAndTopicsIdsAndLabels[]>();

  const [selectedThemeAndTopic, setSelectedThemeAndTopic] = useState<{
    theme: ThemeAndTopicsIdsAndLabels;
    topic: IdLabelPair;
  }>();

  const authentication = useContext(LoginContext);

  useEffect(() => {
    const loggedInUser = authentication.user;

    if (!loggedInUser) {
      setMyPublications([]);
      return;
    }

    if (!themes) {
      themeService.getThemesAndTopics(loggedInUser.id).then(themeList => {
        const themesAsIdLabelPairs = themeList.map(
          themeToThemeWithIdLabelAndTopics,
        );

        setThemes(themesAsIdLabelPairs);

        setSelectedThemeAndTopic({
          theme: themesAsIdLabelPairs[0],
          topic: themesAsIdLabelPairs[0].topics[0],
        });
      });
    }

    if (selectedThemeAndTopic) {
      const fetchedMyPublications = DummyPublicationsData.allPublications.filter(
        publication =>
          publication.owner.id === loggedInUser.id &&
          publication.topic.id === selectedThemeAndTopic.topic.id,
      );

      setMyPublications(fetchedMyPublications);
    }
  }, [authentication, selectedThemeAndTopic, themes]);

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
          {themes && selectedThemeAndTopic && (
            <AdminDashboardPublicationsTab
              publications={myPublications}
              noResultsMessage="You have not yet created any publications"
              themes={themes}
              topics={selectedThemeAndTopic.theme.topics}
              selectedThemeId={selectedThemeAndTopic.theme.id}
              selectedTopicId={selectedThemeAndTopic.topic.id}
              onThemeChange={themeId =>
                setSelectedThemeAndTopic({
                  theme: findThemeById(themeId, themes),
                  topic: findThemeById(themeId, themes).topics[0],
                })
              }
              onTopicChange={topicId =>
                setSelectedThemeAndTopic({
                  theme: selectedThemeAndTopic.theme,
                  topic: findTopicById(topicId, selectedThemeAndTopic.theme),
                })
              }
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
