import Link from '@admin/components/Link';
import ThemeAndTopicContext from '@admin/components/ThemeAndTopicContext';
import { generateAdminDashboardThemeTopicLink } from '@admin/routes/dashboard/routes';
import publicationRoutes from '@admin/routes/edit-publication/routes';
import { IdTitlePair } from '@admin/services/common/types';
import dashboardService from '@admin/services/dashboard/service';
import {
  AdminDashboardPublication,
  AdminDashboardRelease,
} from '@admin/services/dashboard/types';
import permissionService from '@admin/services/permissions/permissionService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import FormSelect from '@common/components/form/FormSelect';
import orderBy from 'lodash/orderBy';
import React, { useContext, useEffect } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import PublicationSummary from './PublicationSummary';

interface ThemeAndTopicsIdsAndTitles extends IdTitlePair {
  topics: IdTitlePair[];
}

const findThemeById = (
  themeId: string,
  availableThemes: ThemeAndTopicsIdsAndTitles[],
) =>
  availableThemes.find(
    theme => theme.id === themeId,
  ) as ThemeAndTopicsIdsAndTitles;

const findTopicById = (topicId: string, theme: ThemeAndTopicsIdsAndTitles) =>
  theme.topics.find(topic => topic.id === topicId) as IdTitlePair;

interface Props {
  onChangePublication: () => void;
  nonLiveReleases: AdminDashboardRelease[];
}

const ManagePublicationsAndReleasesTab = ({
  match,
  onChangePublication,
  nonLiveReleases,
}: Props &
  RouteComponentProps<{
    themeId?: string;
    topicId?: string;
  }>) => {
  const {
    selectedThemeAndTopic: { theme: selectedTheme, topic: selectedTopic },
    setSelectedThemeAndTopic,
  } = useContext(ThemeAndTopicContext);
  const { themeId, topicId } = match.params;

  const {
    value: themes,
    isLoading: loadingThemes,
  } = useAsyncRetry(async () => {
    return dashboardService.getMyThemesAndTopics();
  }, []);

  const {
    value = [[], false],
    isLoading: loadingPublications,
  } = useAsyncRetry(async () => {
    if (selectedTopic.id) {
      return Promise.all([
        dashboardService.getMyPublicationsByTopic(selectedTopic.id),
        permissionService.canCreatePublicationForTopic(selectedTopic.id),
      ]);
    }
    return undefined;
  }, [selectedTopic.id, nonLiveReleases]);

  const [myPublications, canCreatePublication] = value as [
    AdminDashboardPublication[],
    boolean,
  ];

  useEffect(() => {
    if (themes && themes.length && !selectedTheme.id && !selectedTopic.id) {
      if (themeId && topicId) {
        const theme = themes.find(t => t.id === themeId);
        const topic = theme && theme.topics.find(t => t.id === topicId);
        if (theme && topic) {
          setSelectedThemeAndTopic({ theme, topic });
          return;
        }
      }
      const newSelectedTheme = themes[0];
      const newSelectedTopic = orderBy(
        themes[0].topics,
        topic => topic.title,
      )[0];
      setSelectedThemeAndTopic({
        theme: newSelectedTheme,
        topic: newSelectedTopic,
      });
    }
  }, [
    themes,
    selectedTheme.id,
    selectedTopic.id,
    themeId,
    topicId,
    setSelectedThemeAndTopic,
  ]);

  useEffect(() => {
    if (selectedTheme.id && selectedTopic.id) {
      // eslint-disable-next-line
      history.replaceState(
        {},
        '',
        generateAdminDashboardThemeTopicLink(
          selectedTheme.id,
          selectedTopic.id,
        ),
      );
    }
  }, [selectedTheme, selectedTopic, nonLiveReleases]);

  return (
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

      <LoadingSpinner loading={loadingThemes}>
        {themes && themes.length && selectedTheme && selectedTopic ? (
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
                  value={selectedTheme ? selectedTheme.id : undefined}
                  onChange={event => {
                    const newTheme = findThemeById(event.target.value, themes);
                    setSelectedThemeAndTopic({
                      theme: newTheme,
                      topic: orderBy(newTheme.topics, topic => topic.title)[0],
                    });
                  }}
                />
              </div>
              <div className="govuk-grid-column-one-half">
                <FormSelect
                  id="selectTopic"
                  label="Select topic"
                  name="selectTopic"
                  options={selectedTheme.topics.map(topic => ({
                    label: topic.title,
                    value: topic.id,
                  }))}
                  value={selectedTopic ? selectedTopic.id : undefined}
                  onChange={event => {
                    setSelectedThemeAndTopic({
                      theme: selectedTheme,
                      topic: findTopicById(event.target.value, selectedTheme),
                    });
                  }}
                />
              </div>
            </div>
            <hr />
            <h2>{selectedTheme.title}</h2>
            <h3>{selectedTopic.title}</h3>
            <LoadingSpinner loading={loadingPublications}>
              {myPublications && myPublications.length > 0 && (
                <Accordion id="publications">
                  {orderBy(myPublications, pub => pub.title.toUpperCase()).map(
                    publication => (
                      <AccordionSection
                        key={publication.id}
                        heading={publication.title}
                        headingTag="h3"
                      >
                        <PublicationSummary
                          publication={publication}
                          onChangePublication={onChangePublication}
                        />
                      </AccordionSection>
                    ),
                  )}
                </Accordion>
              )}
              {canCreatePublication &&
                myPublications &&
                myPublications.length === 0 && (
                  <div className="govuk-inset-text">
                    You have not yet created any publications
                  </div>
                )}
              {canCreatePublication && (
                <Link
                  to={publicationRoutes.createPublication.generateLink(
                    selectedTheme.id,
                    selectedTopic.id,
                  )}
                  className="govuk-button"
                >
                  Create new publication
                </Link>
              )}
            </LoadingSpinner>
          </>
        ) : (
          <>
            <h3 className="govuk-heading-s">
              You do not currently have permission to view any releases within
              the service
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

export default withRouter(ManagePublicationsAndReleasesTab);
