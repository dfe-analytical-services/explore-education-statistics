import Page from '@admin/components/Page';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import ThemeAndTopicContext from '@admin/components/ThemeAndTopicContext';
import { ErrorControlState } from '@admin/contexts/ErrorControlContext';
import withErrorControl from '@admin/hocs/withErrorControl';
import CreatePublicationPage from '@admin/pages/theme/topic/CreatePublicationPage';
import dashboardService from '@admin/services/dashboard/service';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { useContext, useEffect, useState } from 'react';
import { Route, RouteComponentProps, Switch } from 'react-router';
import PageNotFoundPage from '../errors/PageNotFoundPage';
import PublicationAssignMethodologyPage from './topic/publication/PublicationAssignMethodologyPage';

export const themeTopicPath = '/theme/:themeId/topic/:topicId';

interface Props
  extends RouteComponentProps<{
      themeId?: string;
      topicId?: string;
    }>,
    ErrorControlState {}

const ThemeTopicWrapper = ({ match, handleApiErrors }: Props) => {
  const { selectedThemeAndTopic, setSelectedThemeAndTopic } = useContext(
    ThemeAndTopicContext,
  );

  const [state, setState] = useState<'404' | 'OKAY' | 'LOADING'>();

  useEffect(() => {
    if (!match.params.themeId || !match.params.topicId) {
      setState('404');
    } else if (
      selectedThemeAndTopic.theme.id === match.params.themeId &&
      selectedThemeAndTopic.topic.id === match.params.topicId
    ) {
      setState('OKAY');
    } else {
      setState('LOADING');
      dashboardService
        .getMyThemesAndTopics()
        .then(themeList => {
          const { themeId, topicId } = match.params;
          const theme = themeList.find(({ id }) => id === themeId);
          if (!theme) return setState('404');
          const topic = theme.topics.find(({ id }) => id === topicId);
          if (!topic) return setState('404');
          return setSelectedThemeAndTopic({ theme, topic });
        })
        .catch(handleApiErrors);
    }
  }, [
    match.params,
    selectedThemeAndTopic,
    setSelectedThemeAndTopic,
    handleApiErrors,
  ]);

  const page404 = (
    <ProtectedRoute allowAnonymousUsers component={PageNotFoundPage} />
  );

  switch (state) {
    case 'OKAY':
      return (
        <Switch>
          <Route
            path={`${themeTopicPath}/create-publication`}
            component={CreatePublicationPage}
            exact
          />
          <Route
            path={`${themeTopicPath}/publication/:publicationId/assign-methodology`}
            component={PublicationAssignMethodologyPage}
            exact
          />
          {page404}
        </Switch>
      );
    case '404':
      return page404;
    default:
      return (
        <Page>
          <LoadingSpinner />
        </Page>
      );
  }
};

export default withErrorControl(ThemeTopicWrapper);
