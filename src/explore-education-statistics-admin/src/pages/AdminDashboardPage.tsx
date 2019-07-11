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
import Link from '../components/Link';
import Page from '../components/Page';

const AdminDashboardPage = () => {
  const [myPublications, setMyPublications] = useState<Publication[]>([]);

  const [inProgressPublications, setInProgressPublications] = useState<
    Publication[]
  >([]);

  const [themes, setThemes] = useState<ThemeAndTopics[]>();

  const [selectedThemeAndTopicIds, setSelectedThemeAndTopicIds] = useState<{
    themeId: string;
    topicId: string;
  }>();

  const authentication = useContext(LoginContext);

  useEffect(() => {
    const { user } = authentication;
    const loggedInUserId = user ? user.id : null;

    if (loggedInUserId) {
      if (!themes) {
        const themeList = DummyPublicationsData.themesAndTopics;

        const firstTheme = themeList[0];
        const topicsList = themeList[0].topics;

        setThemes(themeList);

        const firstTopic = topicsList[0];

        setSelectedThemeAndTopicIds({
          themeId: firstTheme.id,
          topicId: firstTopic.id,
        });
      }

      if (selectedThemeAndTopicIds) {
        const fetchedMyPublications = DummyPublicationsData.allPublications.filter(
          publication =>
            publication.owner.id === loggedInUserId &&
            publication.topic.id === selectedThemeAndTopicIds.topicId,
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
  }, [authentication, selectedThemeAndTopicIds]);

  const findThemeById = (themeId: string, availableThemes: ThemeAndTopics[]) =>
    availableThemes.find(theme => theme.id === themeId) || availableThemes[0];

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
          {themes && selectedThemeAndTopicIds && (
            <AdminDashboardPublicationsTab
              publications={myPublications}
              noResultsMessage="You have not yet created any publications"
              themes={themes.map(theme => ({
                id: theme.id,
                label: theme.title,
              }))}
              topics={findThemeById(
                selectedThemeAndTopicIds.themeId,
                themes,
              ).topics.map(topic => ({ id: topic.id, label: topic.title }))}
              selectedThemeId={selectedThemeAndTopicIds.themeId}
              selectedTopicId={selectedThemeAndTopicIds.topicId}
              onThemeChange={themeId =>
                setSelectedThemeAndTopicIds({
                  themeId,
                  topicId: findThemeById(themeId, themes).topics[0].id,
                })
              }
              onTopicChange={topicId =>
                setSelectedThemeAndTopicIds({
                  themeId: selectedThemeAndTopicIds.themeId,
                  topicId,
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
