import {
  AdminDashboardPublication,
  ThemeAndTopics,
} from '@admin/services/dashboard/types';
import React, { useContext, useEffect, useState } from 'react';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { LoginContext } from '@admin/components/Login';
import { IdTitlePair } from '@admin/services/common/types';
import dashboardService from '@admin/services/dashboard/service';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import AdminDashboardPublicationsTab from './components/AdminDashboardPublicationsTab';

const themeToThemeWithIdTitleAndTopics = (theme: ThemeAndTopics) => ({
  id: theme.id,
  title: theme.title,
  topics: theme.topics.map(topic => ({
    id: topic.id,
    title: topic.title,
  })),
});

const findThemeById = (
  themeId: string,
  availableThemes: ThemeAndTopicsIdsAndTitles[],
) => availableThemes.find(theme => theme.id === themeId) || availableThemes[0];

const findTopicById = (topicId: string, theme: ThemeAndTopicsIdsAndTitles) =>
  theme.topics.find(topic => topic.id === topicId) || theme.topics[0];

interface ThemeAndTopicsIdsAndTitles extends IdTitlePair {
  topics: IdTitlePair[];
}

const AdminDashboardPage = () => {
  const [myPublications, setMyPublications] = useState<
    AdminDashboardPublication[]
  >([]);

  const [themes, setThemes] = useState<ThemeAndTopicsIdsAndTitles[]>();

  const [selectedThemeAndTopic, setSelectedThemeAndTopic] = useState<{
    theme: ThemeAndTopicsIdsAndTitles;
    topic: IdTitlePair;
  }>();

  const authentication = useContext(LoginContext);

  useEffect(() => {
    if (!themes) {
      dashboardService.getMyThemesAndTopics().then(themeList => {
        const themesAsIdTitlePairs = themeList.map(
          themeToThemeWithIdTitleAndTopics,
        );

        setThemes(themesAsIdTitlePairs);

        setSelectedThemeAndTopic({
          theme: themesAsIdTitlePairs[0],
          topic: themesAsIdTitlePairs[0].topics[0],
        });
      });
    }

    if (selectedThemeAndTopic) {
      dashboardService
        .getMyPublicationsByTopic(selectedThemeAndTopic.topic.id)
        .then(setMyPublications);
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
          Not you?{' '}
          <a className="govuk-link" href="{loginService.getSignOutLink()}">
            Sign out
          </a>
        </span>
      </h1>
    </>
  );
};

export default AdminDashboardPage;
