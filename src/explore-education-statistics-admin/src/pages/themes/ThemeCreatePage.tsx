import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import ThemeForm from '@admin/pages/themes/components/ThemeForm';
import { ThemeParams, themesRoute } from '@admin/routes/routes';
import themeService from '@admin/services/themeService';
import appendQuery from '@common/utils/url/appendQuery';
import React from 'react';
import { useNavigate } from 'react-router';

const ThemeCreatePage = () => {
  const navigate = useNavigate();

  return (
    <Page
      title="Create theme"
      breadcrumbs={[
        { name: 'Manage themes', link: themesRoute.fullPath },
        { name: 'Create theme' },
      ]}
    >
      <ThemeForm
        cancelButton={
          <Link unvisited to={themesRoute.fullPath}>
            Cancel
          </Link>
        }
        onSubmit={async values => {
          const theme = await themeService.createTheme(values);

          navigate(
            appendQuery<ThemeParams>(themesRoute.fullPath, {
              themeId: theme.id,
            }),
          );
        }}
      />
    </Page>
  );
};

export default ThemeCreatePage;
