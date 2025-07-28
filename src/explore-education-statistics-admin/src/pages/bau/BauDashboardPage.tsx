import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import { useAuthContext } from '@admin/contexts/AuthContext';
import React from 'react';

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
              <Link to="/administration/imports">
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
              <Link to="/administration/boundary-data">
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
              <Link to="/administration/glossary">Manage glossary</Link>
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
              <Link to="/administration/feedback">View feedback</Link>
            </h3>
            <p className="govuk-caption-m govuk-!-margin-top-1">
              View feedback submitted by consumers of the service.
            </p>
          </div>
        )}

        {user?.permissions.isBauUser && (
          <div className="govuk-grid-column-one-third">
            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <Link to="/education-in-numbers">
                Manage Education in Numbers
              </Link>
            </h3>
            <p className="govuk-caption-m govuk-!-margin-top-1">
              View, create and edit Education in Numbers pages.
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
                <Link to="/administration/users">Manage users</Link>
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-1">
                Assign users permissions and roles within the service.
              </p>
            </div>
            <div className="govuk-grid-column-one-third">
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                <Link to="/administration/users/invites">Invite new users</Link>
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-1">
                Invite new users to the service and assign their roles and
                permissions.
              </p>
            </div>
            <div className="govuk-grid-column-one-third">
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                <Link to="/administration/users/pre-release">
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
