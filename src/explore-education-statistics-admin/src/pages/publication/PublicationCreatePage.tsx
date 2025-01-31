import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PublicationForm from '@admin/pages/publication/components/PublicationForm';
import { dashboardRoute, ThemeParams } from '@admin/routes/routes';
import themeService from '@admin/services/themeService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import appendQuery from '@common/utils/url/appendQuery';
import React from 'react';
import { RouteComponentProps } from 'react-router';

export default function PublicationCreatePage({
  history,
  match,
}: RouteComponentProps<{ themeId: string }>) {
  const { themeId } = match.params;

  const { value: theme, isLoading } = useAsyncHandledRetry(
    () => themeService.getTheme(themeId),
    [themeId],
  );

  if (isLoading) {
    return <LoadingSpinner loading={isLoading} />;
  }

  if (!theme) {
    return null;
  }

  return (
    <Page breadcrumbs={[{ name: 'Create new publication' }]}>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle caption={theme.title} title="Create new publication" />
        </div>
      </div>

      <PublicationForm
        cancelButton={
          <Link
            unvisited
            to={appendQuery<ThemeParams>(dashboardRoute.path, {
              themeId: theme.id,
            })}
          >
            Cancel
          </Link>
        }
        themeId={theme.id}
        onSubmit={() => history.push(dashboardRoute.path)}
      />
    </Page>
  );
}
