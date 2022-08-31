import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import ThemeForm from '@admin/pages/themes/components/ThemeForm';
import { ThemeParams, themesRoute } from '@admin/routes/routes';
import themeService from '@admin/services/themeService';
import appendQuery from '@common/utils/url/appendQuery';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const ThemeCreatePage = ({ history }: RouteComponentProps<ThemeParams>) => {
  return (
    <Page
      title="Create theme"
      breadcrumbs={[
        { name: 'Manage themes and topics', link: themesRoute.path },
        { name: 'Create theme' },
      ]}
    >
      <ThemeForm
        cancelButton={
          <Link unvisited to={themesRoute.path}>
            Cancel
          </Link>
        }
        onSubmit={async values => {
          const theme = await themeService.createTheme(values);

          history.push(
            appendQuery<ThemeParams>(themesRoute.path, {
              themeId: theme.id,
            }),
          );
        }}
      />
    </Page>
  );
};

export default ThemeCreatePage;
