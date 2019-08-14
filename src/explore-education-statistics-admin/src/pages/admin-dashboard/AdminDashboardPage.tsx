import Link from '@admin/components/Link';
import { LoginContext } from '@admin/components/Login';
import Page from '@admin/components/Page';
import { IdTitlePair } from '@admin/services/common/types';
import dashboardService from '@admin/services/dashboard/service';
import {
  AdminDashboardPublication,
  ThemeAndTopics,
} from '@admin/services/dashboard/types';
import loginService from '@admin/services/sign-in/service';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import orderBy from 'lodash/orderBy';
import React, { useContext, useEffect, useState } from 'react';
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
  >();

  const [themes, setThemes] = useState<ThemeAndTopicsIdsAndTitles[]>();

  const [selectedThemeAndTopic, setSelectedThemeAndTopic] = useState<{
    theme: ThemeAndTopicsIdsAndTitles;
    topic: IdTitlePair;
  }>();

  useEffect(() => {
    dashboardService
      .getMyThemesAndTopics()
      .then(themeList =>
        setThemes(themeList.map(themeToThemeWithIdTitleAndTopics)),
      );
  }, []);

  useEffect(() => {
    if (themes) {
      setSelectedThemeAndTopic({
        theme: themes[0],
        topic: orderBy(themes[0].topics, topic => topic.title)[0],
      });
    }
  }, [themes]);

  useEffect(() => {
    if (selectedThemeAndTopic) {
      dashboardService
        .getMyPublicationsByTopic(selectedThemeAndTopic.topic.id)
        .then(setMyPublications);
    }
  }, [selectedThemeAndTopic]);

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
          {themes && selectedThemeAndTopic && myPublications && (
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
                  topic: orderBy(
                    findThemeById(themeId, themes).topics,
                    topic => topic.title,
                  )[0],
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
        <TabsSection id="draft-releases" title="View draft releases (0)">
          <div className="govuk-inset-text">
            There are currently no releases ready for you to review
          </div>
        </TabsSection>
        <TabsSection
          id="scheduled-releases"
          title="View scheduled releases (0)"
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
