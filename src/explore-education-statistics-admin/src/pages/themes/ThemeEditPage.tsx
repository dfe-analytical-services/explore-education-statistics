import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import ThemeForm from '@admin/pages/themes/components/ThemeForm';
import { ThemeParams, themesRoute } from '@admin/routes/routes';
import themeService from '@admin/services/themeService';
import appendQuery from '@common/utils/url/appendQuery';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const ThemeEditPage = ({
  history,
  match,
}: RouteComponentProps<ThemeParams>) => {
  const { themeId } = match.params;

  const { value: theme, isLoading } = useAsyncHandledRetry(
    () => themeService.getTheme(themeId),
    [themeId],
  );

  const themesPath = appendQuery<ThemeParams>(themesRoute.path, {
    themeId,
  });

  return (
    <Page
      title="Edit theme"
      breadcrumbs={[
        { name: 'Manage themes', link: themesPath },
        { name: 'Edit theme' },
      ]}
    >
      <LoadingSpinner loading={isLoading}>
        {theme && (
          <ThemeForm
            initialValues={{
              title: theme?.title,
              summary: theme?.summary,
            }}
            cancelButton={
              <Link unvisited to={themesPath}>
                Cancel
              </Link>
            }
            onSubmit={async values => {
              await themeService.updateTheme(themeId, values);
              history.push(themesPath);
            }}
          />
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default ThemeEditPage;
