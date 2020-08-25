import ButtonLink from '@admin/components/ButtonLink';
import useQueryParams from '@admin/hooks/useQueryParams';
import {
  dashboardRoute,
  publicationCreateRoute,
  ThemeTopicParams,
} from '@admin/routes/routes';
import dashboardService from '@admin/services/dashboardService';
import permissionService from '@admin/services/permissionService';
import themeService, { Theme } from '@admin/services/themeService';
import { Topic } from '@admin/services/topicService';
import appendQuery from '@admin/utils/url/appendQuery';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import FormSelect from '@common/components/form/FormSelect';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useStorageItem from '@common/hooks/useStorageItem';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useMemo } from 'react';
import { generatePath, useHistory, useLocation } from 'react-router';
import PublicationSummary from './PublicationSummary';

const ManagePublicationsAndReleasesTab = () => {
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
    const selectedThemeId = themeId || savedThemeTopic?.themeId;

    return (
      themes?.find(t => t.id === selectedThemeId) ??
      orderBy(themes, t => t.title)[0]
    );
  }, [savedThemeTopic, themeId, themes]);

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

  const {
    value: myPublications,
    isLoading: loadingPublications,
    retry: reloadMyPublications,
  } = useAsyncHandledRetry(async () => {
    if (!selectedTopic?.id) {
      return undefined;
    }

    return dashboardService.getMyPublicationsByTopic(selectedTopic.id);
  }, [selectedTopic?.id]);

  const { value: canCreatePublication = false } = useAsyncRetry(async () => {
    if (!selectedTopic?.id) {
      return false;
    }

    return permissionService.canCreatePublicationForTopic(selectedTopic.id);
  }, [selectedTopic?.id]);

  useEffect(() => {
    if (savedThemeTopic) {
      return;
    }

    // Set default theme/topic in storage if it
    // hasn't been set yet (e.g. first time
    // visiting dashboard).
    if (selectedTheme && selectedTopic) {
      setSavedThemeTopic({
        themeId: selectedTheme.id,
        topicId: selectedTopic.id,
      });
    }
  }, [savedThemeTopic, selectedTheme, selectedTopic, setSavedThemeTopic]);

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
    history,
    savedThemeTopic,
    selectedTheme,
    selectedTopic,
    themeId,
    topicId,
  ]);

  return (
    <section>
      <p>Select publications to:</p>

      <ul>
        <li>create new releases and methodologies</li>
        <li>edit exiting releases and methodologies</li>
        <li>view and sign-off releases and methodologies</li>
      </ul>

      <p>
        To remove publications, releases and methodologies email{' '}
        <a href="mailto:explore.statistics@education.gov.uk">
          explore.statistics@education.gov.uk
        </a>
      </p>

      <LoadingSpinner loading={loadingThemes}>
        {themes && themes.length > 0 ? (
          <>
            <div className="dfe-flex dfe-flex-wrap">
              <div className="govuk-!-margin-right-4">
                <FormSelect
                  id="selectTheme"
                  label="Select theme"
                  name="selectTheme"
                  value={themeId}
                  options={themes.map(theme => ({
                    label: theme.title,
                    value: theme.id,
                  }))}
                  onChange={event => {
                    const nextTheme = themes.find(
                      theme => theme.id === event.target.value,
                    );

                    if (!nextTheme) {
                      return;
                    }

                    const nextTopic = orderBy(
                      nextTheme.topics,
                      topic => topic.title,
                    )[0];

                    if (!nextTopic) {
                      return;
                    }

                    setSavedThemeTopic({
                      themeId: nextTheme.id,
                      topicId: nextTopic.id,
                    });

                    history.replace(
                      appendQuery<ThemeTopicParams>(dashboardRoute.path, {
                        themeId: nextTheme.id,
                        topicId: nextTopic.id,
                      }),
                    );
                  }}
                />
              </div>
              <div>
                <FormSelect
                  id="selectTopic"
                  label="Select topic"
                  name="selectTopic"
                  options={
                    selectedTheme?.topics.map(topic => ({
                      label: topic.title,
                      value: topic.id,
                    })) ?? []
                  }
                  value={topicId}
                  onChange={event => {
                    if (!selectedTheme) {
                      return;
                    }

                    const nextTopic = selectedTheme.topics.find(
                      topic => topic.id === event.target.value,
                    );

                    if (!nextTopic) {
                      return;
                    }

                    setSavedThemeTopic({
                      themeId: selectedTheme.id,
                      topicId: nextTopic.id,
                    });

                    history.replace(
                      appendQuery<ThemeTopicParams>(dashboardRoute.path, {
                        themeId: selectedTheme.id,
                        topicId: nextTopic.id,
                      }),
                    );
                  }}
                />
              </div>
            </div>

            <hr />

            {selectedTheme && selectedTopic && (
              <>
                <h2 data-testid="selectedThemeTitle">{selectedTheme.title}</h2>

                <h3 data-testid="selectedTopicTitle">{selectedTopic.title}</h3>

                <LoadingSpinner loading={loadingPublications}>
                  {myPublications && myPublications.length > 0 ? (
                    <Accordion id="publications">
                      {orderBy(myPublications, pub =>
                        pub.title.toUpperCase(),
                      ).map(publication => (
                        <AccordionSection
                          key={publication.id}
                          heading={publication.title}
                          headingTag="h4"
                        >
                          <PublicationSummary
                            publication={publication}
                            onChangePublication={reloadMyPublications}
                          />
                        </AccordionSection>
                      ))}
                    </Accordion>
                  ) : (
                    <div className="govuk-inset-text">
                      No publications available
                    </div>
                  )}

                  {canCreatePublication && (
                    <ButtonLink
                      to={generatePath(publicationCreateRoute.path, {
                        topicId: selectedTopic.id,
                      })}
                    >
                      Create new publication
                    </ButtonLink>
                  )}
                </LoadingSpinner>
              </>
            )}
          </>
        ) : (
          <>
            <h3 className="govuk-heading-s">
              You do not currently have permission to edit any releases within
              the service. To view a prerelease, please use the link provided to
              you by email.
            </h3>
            <p>
              To request access to a release, contact your team leader or the
              Explore education statistics team at{' '}
              <a href="mailto:explore.statistics@education.gov.uk">
                explore.statistics@education.gov.uk
              </a>
            </p>
          </>
        )}
      </LoadingSpinner>
    </section>
  );
};

export default ManagePublicationsAndReleasesTab;
