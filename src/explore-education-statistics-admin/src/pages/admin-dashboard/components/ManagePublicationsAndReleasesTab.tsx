import Link from '@admin/components/Link';
import ThemeAndTopicContext from '@admin/components/ThemeAndTopicContext';
import { generateAdminDashboardThemeTopicLink } from '@admin/routes/dashboard/routes';
import publicationRoutes from '@admin/routes/edit-publication/routes';
import { IdTitlePair } from '@admin/services/common/types';
import dashboardService from '@admin/services/dashboard/service';
import {
  AdminDashboardPublication,
  ThemeAndTopics,
} from '@admin/services/dashboard/types';
import permissionService from '@admin/services/permissions/permissionService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ErrorSummary, {
  ErrorSummaryMessage,
} from '@common/components/ErrorSummary';
import FormSelect from '@common/components/form/FormSelect';
import LoadingSpinner from '@common/components/LoadingSpinner';
import orderBy from 'lodash/orderBy';
import React, { useContext, useEffect, useState } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';
import PublicationSummary from './PublicationSummary';

interface ThemeAndTopicsIdsAndTitles extends IdTitlePair {
  topics: IdTitlePair[];
}

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
) =>
  availableThemes.find(
    theme => theme.id === themeId,
  ) as ThemeAndTopicsIdsAndTitles;

const findTopicById = (topicId: string, theme: ThemeAndTopicsIdsAndTitles) =>
  theme.topics.find(topic => topic.id === topicId) as IdTitlePair;

const ManagePublicationsAndReleasesTab = ({
  match,
}: RouteComponentProps<{
  themeId?: string;
  topicId?: string;
}>) => {
  const { selectedThemeAndTopic, setSelectedThemeAndTopic } = useContext(
    ThemeAndTopicContext,
  );

  const [myPublications, setMyPublications] = useState<
    AdminDashboardPublication[]
  >();

  const [themes, setThemes] = useState<ThemeAndTopicsIdsAndTitles[]>();

  const [apiErrors, setApiErrors] = useState<ErrorSummaryMessage[]>([]);

  const [canCreatePublication, setCanCreatePublication] = useState(false);

  const { themeId, topicId } = match.params;

  const onThemeChange = (
    newThemeId: string,
    availableThemes: ThemeAndTopicsIdsAndTitles[],
  ) => {
    setSelectedThemeAndTopic({
      theme: findThemeById(newThemeId, availableThemes),
      topic: orderBy(
        findThemeById(newThemeId, availableThemes).topics,
        topic => topic.title,
      )[0],
    });
  };

  const onTopicChange = (
    newTopicId: string,
    selectedTheme: ThemeAndTopicsIdsAndTitles,
  ) => {
    setSelectedThemeAndTopic({
      theme: selectedTheme,
      topic: findTopicById(newTopicId, selectedTheme),
    });
  };

  const sortPublications = (
    a: AdminDashboardPublication,
    b: AdminDashboardPublication,
  ) => {
    const pubA = a.title.toUpperCase();
    const pubB = b.title.toUpperCase();

    let comparison = 0;
    if (pubA > pubB) {
      comparison = 1;
    } else if (pubA < pubB) {
      comparison = -1;
    }

    return comparison;
  };

  useEffect(() => {
    dashboardService
      .getMyThemesAndTopics()
      .then(themeList =>
        setThemes(themeList.map(themeToThemeWithIdTitleAndTopics)),
      );
  }, []);

  useEffect(() => {
    if (themes) {
      if (selectedThemeAndTopic.theme.id === '' && themes.length) {
        setSelectedThemeAndTopic({
          theme: themes[0],
          topic: orderBy(themes[0].topics, topic => topic.title)[0],
        });
      }
      if (themeId && topicId && topicId !== selectedThemeAndTopic.topic.id) {
        const theme = themes.find(t => t.id === themeId);

        const topic = theme && theme.topics.find(t => t.id === topicId);

        if (theme && topic) {
          setSelectedThemeAndTopic({ theme, topic });
        }
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [themeId, themes, topicId]);

  useEffect(() => {
    if (selectedThemeAndTopic.topic.id) {
      Promise.all([
        dashboardService
          .getMyPublicationsByTopic(selectedThemeAndTopic.topic.id)
          .then(setMyPublications),
        permissionService
          .canCreatePublicationForTopic(selectedThemeAndTopic.topic.id)
          .then(setCanCreatePublication),
      ])
        .then(_ => {
          setApiErrors([]);
          // eslint-disable-next-line
          history.replaceState(
            {},
            '',
            generateAdminDashboardThemeTopicLink(
              selectedThemeAndTopic.theme.id,
              selectedThemeAndTopic.topic.id,
            ),
          );
        })
        .catch(error => {
          setApiErrors([
            ...apiErrors,
            { id: error.data.traceId, message: error.data.title },
          ]);
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedThemeAndTopic]);

  return (
    <>
      <section>
        <p className="govuk-body">Select publications to:</p>
        <ul className="govuk-list--bullet">
          <li>create new releases and methodologies</li>
          <li>edit exiting releases and methodologies</li>
          <li>view and sign-off releases and methodologies</li>
        </ul>
        <p className="govuk-bo">
          To remove publications, releases and methodologies email{' '}
          <a href="mailto:explore.statistics@education.gov.uk">
            explore.statistics@education.gov.uk
          </a>
        </p>

        {themes && selectedThemeAndTopic && myPublications ? (
          <>
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-one-half">
                <FormSelect
                  id="selectTheme"
                  label="Select theme"
                  name="selectTheme"
                  options={themes.map(theme => ({
                    label: theme.title,
                    value: theme.id,
                  }))}
                  value={
                    selectedThemeAndTopic.theme
                      ? selectedThemeAndTopic.theme.id
                      : undefined
                  }
                  onChange={event => {
                    onThemeChange(event.target.value, themes);
                  }}
                />
              </div>
              <div className="govuk-grid-column-one-half">
                <FormSelect
                  id="selectTopic"
                  label="Select topic"
                  name="selectTopic"
                  options={selectedThemeAndTopic.theme.topics.map(topic => ({
                    label: topic.title,
                    value: topic.id,
                  }))}
                  value={
                    selectedThemeAndTopic.topic
                      ? selectedThemeAndTopic.topic.id
                      : undefined
                  }
                  onChange={event => {
                    onTopicChange(
                      event.target.value,
                      selectedThemeAndTopic.theme,
                    );
                  }}
                />
              </div>
            </div>
            <hr />
            <h2>{selectedThemeAndTopic.theme.title}</h2>
            <h3>{selectedThemeAndTopic.topic.title}</h3>
            {myPublications.length > 0 && (
              <Accordion id="publications">
                {myPublications.sort(sortPublications).map(publication => (
                  <AccordionSection
                    key={publication.id}
                    heading={publication.title}
                    headingTag="h3"
                  >
                    <PublicationSummary publication={publication} />
                  </AccordionSection>
                ))}
              </Accordion>
            )}
            {canCreatePublication && myPublications.length === 0 && (
              <div className="govuk-inset-text">
                You have not yet created any publications
              </div>
            )}
            {canCreatePublication && (
              <Link
                to={publicationRoutes.createPublication.generateLink(
                  selectedThemeAndTopic.theme.id,
                  selectedThemeAndTopic.topic.id,
                )}
                className="govuk-button"
              >
                Create new publication
              </Link>
            )}
          </>
        ) : (
          <>
            {apiErrors.length > 0 ? (
              <ErrorSummary id="publications-error" errors={apiErrors} />
            ) : (
              <LoadingSpinner />
            )}
          </>
        )}
      </section>
    </>
  );
};

export default withRouter(ManagePublicationsAndReleasesTab);
