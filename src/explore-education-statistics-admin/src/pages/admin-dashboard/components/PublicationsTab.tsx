import FormThemeTopicSelect from '@admin/components/form/FormThemeTopicSelect';
import useQueryParams from '@admin/hooks/useQueryParams';
import TopicPublications from '@admin/pages/admin-dashboard/components/TopicPublications';
import { ThemeTopicParams, dashboardRoute } from '@admin/routes/routes';
import themeService, { Theme } from '@admin/services/themeService';
import { Topic } from '@admin/services/topicService';
import appendQuery from '@common/utils/url/appendQuery';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useStorageItem from '@common/hooks/useStorageItem';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useMemo } from 'react';
import { useHistory, useLocation } from 'react-router';

interface Props {
  isBauUser: boolean;
}

const PublicationsTab = ({ isBauUser }: Props) => {
  const { themeId, topicId } = useQueryParams<ThemeTopicParams>();
  const location = useLocation();
  const history = useHistory();

  const [savedThemeTopic, setSavedThemeTopic] = useStorageItem<
    ThemeTopicParams
  >('dashboardThemeTopic', undefined);

  const { value: themes, isLoading: loadingThemes } = useAsyncHandledRetry(
    themeService.getThemes,
  );

  const selectedTheme = useMemo<Theme | undefined>(() => {
    // Only select a theme for BAU users.
    // Analysts see all their publications for all themes.
    if (!isBauUser) {
      return undefined;
    }
    const selectedThemeId = themeId || savedThemeTopic?.themeId;

    return (
      themes?.find(t => t.id === selectedThemeId) ??
      orderBy(themes, t => t.title)[0]
    );
  }, [isBauUser, savedThemeTopic, themeId, themes]);

  const selectedTopic = useMemo<Topic | undefined>(() => {
    if (!selectedTheme) {
      return undefined;
    }

    const selectedTopicId = topicId || savedThemeTopic?.topicId;

    return (
      selectedTheme.topics.find(t => t.id === selectedTopicId) ??
      orderBy(selectedTheme.topics, t => t.title)[0]
    );
  }, [savedThemeTopic, selectedTheme, topicId]);

  useEffect(() => {
    if (savedThemeTopic) {
      return;
    }

    // Set default theme/topic in storage if it hasn't been set yet (e.g. first time
    // visiting dashboard).
    if (selectedTheme && selectedTopic) {
      setSavedThemeTopic({
        themeId: selectedTheme.id,
        topicId: selectedTopic.id,
      });
    }
  }, [
    isBauUser,
    savedThemeTopic,
    selectedTheme,
    selectedTopic,
    setSavedThemeTopic,
  ]);

  useEffect(() => {
    if (!selectedTheme || !selectedTopic) {
      return;
    }

    // Update query params to reflect the chosen
    // theme/topic if they haven't already been set.
    if (selectedTheme?.id !== themeId || selectedTopic?.id !== topicId) {
      history.replace(
        appendQuery<ThemeTopicParams>(location.pathname, {
          themeId: selectedTheme?.id,
          topicId: selectedTopic?.id,
        }),
      );
    }
  }, [
    isBauUser,
    history,
    location.pathname,
    savedThemeTopic,
    selectedTheme,
    selectedTopic,
    themeId,
    topicId,
  ]);

  return (
    <LoadingSpinner
      hideText
      loading={loadingThemes}
      text="Loading your publications"
    >
      <h2>View and manage your publications</h2>
      <p>Select a publication to:</p>

      <ul>
        <li>create new releases and methodologies</li>
        <li>edit exiting releases and methodologies</li>
        <li>view and sign-off releases and methodologies</li>
      </ul>

      {isBauUser && (
        <>
          {themes && themes.length > 0 && (
            <FormThemeTopicSelect
              id="publicationsReleases-themeTopic"
              legend="Choose a theme and topic to view publications for"
              legendHidden
              themes={themes}
              topicId={topicId}
              onChange={(nextTopicId, nextThemeId) => {
                setSavedThemeTopic({
                  themeId: nextThemeId,
                  topicId: nextTopicId,
                });

                history.replace(
                  appendQuery<ThemeTopicParams>(dashboardRoute.path, {
                    themeId: nextThemeId,
                    topicId: nextTopicId,
                  }),
                );
              }}
            />
          )}
        </>
      )}

      <hr className="govuk-!-margin-bottom-0" />

      {themes?.length === 0 ? (
        <>
          <h3
            className="govuk-heading-s"
            data-testid="no-permission-to-access-releases"
          >
            You do not currently have permission to edit any releases within the
            service. To view a prerelease, please use the link provided to you
            by email.
          </h3>
          <p>
            To request access to a release, contact your team leader or the
            Explore education statistics team at{' '}
            <a href="mailto:explore.statistics@education.gov.uk">
              explore.statistics@education.gov.uk
            </a>
          </p>
        </>
      ) : (
        <>
          {selectedTheme && selectedTopic ? (
            <TopicPublications
              key={selectedTopic.id}
              themeTitle={selectedTheme.title}
              topic={selectedTopic}
            />
          ) : (
            <>
              {themes?.map(theme => {
                return theme.topics.map(topic => (
                  <TopicPublications
                    key={topic.id}
                    themeTitle={theme.title}
                    topic={topic}
                  />
                ));
              })}
            </>
          )}
        </>
      )}
    </LoadingSpinner>
  );
};

export default PublicationsTab;
