import ProtectedRoute from '@admin/components/ProtectedRoute';
import { ThemeTopicContextProvider } from '@admin/contexts/ThemeTopicContext';
import {
  publicationAssignMethodologyRoute,
  publicationCreateRoute,
} from '@admin/routes/themeTopicRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';
import { RouteComponentProps, Switch } from 'react-router';
import PageNotFoundPage from '../errors/PageNotFoundPage';

const routes = [publicationCreateRoute, publicationAssignMethodologyRoute];

interface Params {
  themeId: string;
  topicId: string;
}

const ThemeTopicPageContainer = ({ match }: RouteComponentProps<Params>) => {
  const { themeId, topicId } = match.params;

  return (
    <ThemeTopicContextProvider themeId={themeId} topicId={topicId}>
      {({ value, isLoading }) => (
        <LoadingSpinner loading={isLoading}>
          <Switch>
            {value ? (
              routes.map(route => (
                <ProtectedRoute key={route.path} {...route} />
              ))
            ) : (
              <ProtectedRoute
                path="*"
                allowAnonymousUsers
                component={PageNotFoundPage}
              />
            )}
          </Switch>
        </LoadingSpinner>
      )}
    </ThemeTopicContextProvider>
  );
};

export default ThemeTopicPageContainer;
