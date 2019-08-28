import publicationRoutes from '@admin/routes/edit-publication/routes';
import dashboardService from '@admin/services/dashboard/service';
import {
  AdminDashboardPublication,
  ThemeAndTopics,
} from '@admin/services/dashboard/types';
import FormSelect from '@common/components/form/FormSelect';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useState } from 'react';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import { IdTitlePair } from '@admin/services/common/types';
import Link from '@admin/components/Link';
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
) => availableThemes.find(theme => theme.id === themeId) || availableThemes[0];

const findTopicById = (topicId: string, theme: ThemeAndTopicsIdsAndTitles) =>
  theme.topics.find(topic => topic.id === topicId) || theme.topics[0];

const MyPublicationsTab = () => {
  const [myPublications, setMyPublications] = useState<
    AdminDashboardPublication[]
  >();

  const [themes, setThemes] = useState<ThemeAndTopicsIdsAndTitles[]>();

  const [selectedThemeAndTopic, setSelectedThemeAndTopic] = useState<{
    theme: ThemeAndTopicsIdsAndTitles;
    topic: IdTitlePair;
  }>();

  const onThemeChange = (
    themeId: string,
    availableThemes: ThemeAndTopicsIdsAndTitles[],
  ) =>
    setSelectedThemeAndTopic({
      theme: findThemeById(themeId, availableThemes),
      topic: orderBy(
        findThemeById(themeId, availableThemes).topics,
        topic => topic.title,
      )[0],
    });

  const onTopicChange = (
    topicId: string,
    selectedTheme: ThemeAndTopicsIdsAndTitles,
  ) =>
    setSelectedThemeAndTopic({
      theme: selectedTheme,
      topic: findTopicById(topicId, selectedTheme),
    });

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
    <>
      {themes && selectedThemeAndTopic && myPublications && (
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
                value={selectedThemeAndTopic.theme.id}
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
                value={selectedThemeAndTopic.topic.id}
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
              {myPublications.map(publication => (
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
          {myPublications.length === 0 && (
            <div className="govuk-inset-text">
              You have not yet created any publications
            </div>
          )}
          <Link
            to={publicationRoutes.createPublication.generateLink(
              selectedThemeAndTopic.topic.id,
            )}
            className="govuk-button"
          >
            Create new publication
          </Link>
        </section>
      )}
    </>
  );
};

export default MyPublicationsTab;
