import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { useConfig } from '@admin/contexts/ConfigContext';
import {
  preReleaseNavRoutes,
  preReleaseRoutes,
} from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import permissionService, {
  PreReleaseWindowStatus,
} from '@admin/services/permissionService';
import preReleaseService, {
  PreReleaseSummary,
} from '@admin/services/preReleaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import NotificationBanner from '@common/components/NotificationBanner';
import { useErrorControl } from '@common/contexts/ErrorControlContext';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { format } from 'date-fns';
import React from 'react';
import { generatePath } from 'react-router';
import { Route, RouteComponentProps, Switch } from 'react-router-dom';

interface Model {
  preReleaseWindowStatus: PreReleaseWindowStatus;
  preReleaseSummary: PreReleaseSummary;
}

const PreReleasePageContainer = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { publicationId, releaseId } = match.params;

  const config = useConfig();
  const { user } = useAuthContext();

  const { errorPages, handleError } = useErrorControl();

  const { value: model, isLoading } = useAsyncRetry<
    Model | undefined
  >(async () => {
    try {
      const preReleaseWindowStatus = await permissionService.getPreReleaseWindowStatus(
        releaseId,
      );

      if (preReleaseWindowStatus.access === 'NoneSet') {
        errorPages.forbidden();
        return undefined;
      }

      const preReleaseSummary = await preReleaseService.getPreReleaseSummary(
        releaseId,
      );
      return {
        preReleaseWindowStatus,
        preReleaseSummary,
      };
    } catch (err) {
      handleError(err);
      return undefined;
    }
  }, [errorPages, releaseId]);

  const renderPage = () => {
    if (!model) {
      return null;
    }

    const {
      preReleaseWindowStatus: { access, start, end },
      preReleaseSummary: {
        contactEmail,
        contactTeam,
        releaseSlug,
        releaseTitle,
        publicationSlug,
        publicationTitle,
      },
    } = model;

    if (access === 'After') {
      return (
        <>
          <h1>Pre-release access has ended</h1>

          <>
            <p>
              The <strong>{releaseTitle}</strong> release of{' '}
              <strong>{publicationTitle}</strong> has now been published on the
              Explore Education Statistics service.
            </p>

            <a
              href={`${config.PublicAppUrl}/find-statistics/${publicationSlug}/${releaseSlug}`}
              rel="noopener noreferrer"
              data-testid="release-url"
            >
              View this release
            </a>
          </>
        </>
      );
    }

    if (access === 'Before') {
      return (
        <>
          <h1>Pre-release access is not yet available</h1>

          <p>
            Pre-release access for the <strong>{releaseTitle}</strong> release
            of <strong>{publicationTitle}</strong> is not yet available.
          </p>

          <p>
            {`Pre-release access will be available from ${format(
              start,
              'd MMMM yyyy',
            )} at ${format(start, 'HH:mm')} until ${format(
              end,
              'd MMMM yyyy',
            )} at ${format(end, 'HH:mm')}.`}
          </p>

          <p>
            If you believe that this release should be available and you are
            having problems accessing please contact the{' '}
            <a href={`mailto:${contactEmail}`}>production team</a>.
          </p>
        </>
      );
    }

    if (access === 'Within') {
      return (
        <>
          <NotificationBanner
            heading="If you have an enquiry about this release contact:"
            title="Contact"
          >
            <p>
              {`${contactTeam}: `}
              <a
                className='class="govuk-notification-banner__link"'
                href={`mailto:${contactEmail}`}
              >
                {contactEmail}
              </a>
            </p>
          </NotificationBanner>
          <NavBar
            className="govuk-!-margin-top-0"
            routes={preReleaseNavRoutes.map(route => ({
              title: route.title,
              to: generatePath<ReleaseRouteParams>(route.path, {
                publicationId,
                releaseId,
              }),
            }))}
            label="Pre-release"
          />

          <Switch>
            {preReleaseRoutes.map(route => (
              <Route key={route.path} {...route} />
            ))}
          </Switch>
        </>
      );
    }

    return null;
  };

  return (
    <Page
      breadcrumbs={
        user && user.permissions.canAccessAnalystPages
          ? [{ name: 'Pre-release access' }]
          : []
      }
      homePath={user?.permissions.canAccessAnalystPages ? '/' : ''}
    >
      <LoadingSpinner loading={isLoading}>{renderPage()}</LoadingSpinner>
    </Page>
  );
};

export default PreReleasePageContainer;
