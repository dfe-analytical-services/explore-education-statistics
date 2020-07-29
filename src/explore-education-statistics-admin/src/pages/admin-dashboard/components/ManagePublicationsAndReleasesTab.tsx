import ButtonLink from '@admin/components/ButtonLink';
import useQueryParams from '@admin/hooks/useQueryParams';
import { dashboardRoute } from '@admin/routes/routes';
import {
  publicationCreateRoute,
  ThemeTopicParams,
} from '@admin/routes/themeTopicRoutes';
import dashboardService, {
  Theme,
  Topic,
} from '@admin/services/dashboardService';
import permissionService from '@admin/services/permissionService';
import appendQuery from '@admin/utils/url/appendQuery';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import FormSelect from '@common/components/form/FormSelect';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import orderBy from 'lodash/orderBy';
import React, { useMemo, useEffect } from 'react';
import { generatePath, useHistory } from 'react-router';
import PublicationSummary from './PublicationSummary';

const ManagePublicationsAndReleasesTab = () => {
  const { themeId, topicId } = useQueryParams<ThemeTopicParams>();
  const history = useHistory();

  const { value: themes, isLoading: loadingThemes } = useAsyncHandledRetry(
    dashboardService.getMyThemesAndTopics,
  );

  const selectedTheme = useMemo<Theme | undefined>(() => {
    if (!themes) {
      return undefined;
    }

    return (
      themes.find(t => t.id === themeId) ?? orderBy(themes, t => t.title)[0]
    );
  }, [themeId, themes]);

  const selectedTopic = useMemo<Topic | undefined>(() => {
    if (!themes || !selectedTheme) {
      return undefined;
    }

    return (
      selectedTheme.topics.find(t => t.id === topicId) ??
      orderBy(selectedTheme.topics, t => t.title)[0]
    );
  }, [selectedTheme, themes, topicId]);

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
    if (!selectedTheme || !selectedTopic) {
      return;
    }

    if (!themeId || !topicId) {
      history.replace(
        appendQuery<ThemeTopicParams>(dashboardRoute.path, {
          themeId: selectedTheme.id,
          topicId: selectedTopic.id,
        }),
      );
    }
  }, [history, selectedTheme, selectedTopic, themeId, topicId]);

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
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-one-half">
                <FormSelect
                  id="selectTheme"
                  label="Select theme"
                  name="selectTheme"
                  value={selectedTheme?.id}
                  options={themes.map(theme => ({
                    label: theme.title,
                    value: theme.id,
                  }))}
                  onChange={event => {
                    const newTheme = themes.find(
                      t => t.id === event.target.value,
                    );

                    if (!newTheme) {
                      return;
                    }

                    history.push(
                      appendQuery<ThemeTopicParams>(dashboardRoute.path, {
                        themeId: event.target.value,
                        topicId: orderBy(
                          newTheme.topics,
                          topic => topic.title,
                        )[0]?.id,
                      }),
                    );
                  }}
                />
              </div>
              <div className="govuk-grid-column-one-half">
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
                  value={selectedTopic?.id}
                  onChange={event => {
                    history.push(
                      appendQuery<ThemeTopicParams>(dashboardRoute.path, {
                        themeId,
                        topicId: event.target.value,
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
                            themeId={selectedTheme.id}
                            topicId={selectedTopic.id}
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
                      to={generatePath<ThemeTopicParams>(
                        publicationCreateRoute.path,
                        {
                          themeId: selectedTheme.id,
                          topicId: selectedTopic.id,
                        },
                      )}
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
