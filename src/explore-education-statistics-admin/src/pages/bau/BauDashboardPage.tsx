import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import { useAuthContext } from '@admin/contexts/AuthContext';
import React from 'react';
import {
  administrationBoundaryDataRoute,
  administrationFeedbackRoute,
  administrationGlossaryRoute,
  administrationImportsRoute,
  administrationInvitedUsersRoute,
  administrationPreReleaseUsersRoute,
  administrationServiceAnnouncementRoute,
  administrationUsersRoute,
} from '@admin/routes/administrationRoutes';
import { educationInNumbersListRoute } from '@admin/routes/routes';

const BauDashboardPage = () => {
  const { user } = useAuthContext();

  return (
    <Page
      title="Platform administration"
      wide
      breadcrumbs={[{ name: 'Platform administration' }]}
    >
      <h2 className="govuk-heading-m govuk-!-margin-top-9">Content and data</h2>

      <div className="govuk-grid-row govuk-!-margin-bottom-6">
        {user?.permissions.canAccessAllImports && (
          <div className="govuk-grid-column-one-third">
            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <Link to={administrationImportsRoute.fullPath}>
                Manage incomplete imports
              </Link>
            </h3>
            <p className="govuk-caption-m govuk-!-margin-top-1">
              View a list of all current incomplete imports.
            </p>
          </div>
        )}

        {user?.permissions.isBauUser && (
          <div className="govuk-grid-column-one-third">
            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <Link to={administrationBoundaryDataRoute.fullPath}>
                Manage boundary data
              </Link>
            </h3>
            <p className="govuk-caption-m govuk-!-margin-top-1">
              View boundary level data, and upload new boundary files.
            </p>
          </div>
        )}

        {user?.permissions.isBauUser && (
          <div className="govuk-grid-column-one-third">
            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <Link to={administrationGlossaryRoute.fullPath}>
                Manage glossary
              </Link>
            </h3>
            <p className="govuk-caption-m govuk-!-margin-top-1">
              Clear glossary cache.
            </p>
          </div>
        )}
      </div>

      <div className="govuk-grid-row govuk-!-margin-bottom-9">
        {user?.permissions.isBauUser && (
          <div className="govuk-grid-column-one-third">
            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <Link to={administrationFeedbackRoute.fullPath}>
                View feedback
              </Link>
            </h3>
            <p className="govuk-caption-m govuk-!-margin-top-1">
              View feedback submitted by consumers of the service.
            </p>
          </div>
        )}

        {user?.permissions.isBauUser && (
          <div className="govuk-grid-column-one-third">
            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <Link to={educationInNumbersListRoute.fullPath}>
                Manage Education in Numbers
              </Link>
            </h3>
            <p className="govuk-caption-m govuk-!-margin-top-1">
              View, create and edit Education in Numbers pages.
            </p>
          </div>
        )}

        {user?.permissions.isBauUser && (
          <div className="govuk-grid-column-one-third">
            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <Link to={administrationServiceAnnouncementRoute.fullPath}>
                Service announcement
              </Link>
            </h3>
            <p className="govuk-caption-m govuk-!-margin-top-1">
              Send a real-time service announcement to all connected users.
            </p>
          </div>
        )}
      </div>

      <hr className="govuk-!-margin-top-9" />

      <h2 className="govuk-heading-m govuk-!-margin-top-9">
        Access to the service
      </h2>
      <div className="govuk-grid-row govuk-!-margin-bottom-9">
        {user?.permissions.isBauUser && (
          <>
            <div className="govuk-grid-column-one-third">
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                <Link to={administrationUsersRoute.fullPath}>Manage users</Link>
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-1">
                Assign users permissions and roles within the service.
              </p>
            </div>
            <div className="govuk-grid-column-one-third">
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                <Link to={administrationInvitedUsersRoute.fullPath}>
                  Invite new users
                </Link>
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-1">
                Invite new users to the service and assign their roles and
                permissions.
              </p>
            </div>
            <div className="govuk-grid-column-one-third">
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                <Link to={administrationPreReleaseUsersRoute.fullPath}>
                  Pre-release users
                </Link>
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-1">
                Manage pre-release users and their access to the service.
              </p>
            </div>
          </>
        )}
      </div>
    </Page>
  );
};

export default BauDashboardPage;
