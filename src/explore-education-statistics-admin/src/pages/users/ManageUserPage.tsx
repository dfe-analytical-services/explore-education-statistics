import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import usersQueries from '@admin/queries/user-management/usersQueries';
import publicationQueries from '@admin/queries/publicationQueries';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RoleForm from '@admin/pages/users/components/RoleForm';
import PreReleaseAccessForm from '@admin/pages/users/components/PreReleaseAccessForm';
import PublicationAccessForm from '@admin/pages/users/components/PublicationAccessForm';
import { useQuery } from '@tanstack/react-query';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import releaseQueries from '@admin/queries/releaseQueries';
import globalRolesQueries from '@admin/queries/user-management/globalRolesQueries';

export default function ManageUserPage({
  match,
}: RouteComponentProps<{ userId: string }>) {
  const { userId } = match.params;

  const {
    data: user,
    isLoading: isLoadingUser,
    refetch,
  } = useQuery(usersQueries.getUser(userId));

  const { data: roles, isLoading: isLoadingRoles } = useQuery(
    globalRolesQueries.getRoles,
  );
  const { data: releases, isLoading: isLoadingReleases } = useQuery(
    releaseQueries.getReleases,
  );
  const { data: publications, isLoading: isLoadingPublications } = useQuery(
    publicationQueries.getPublicationSummaries,
  );

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Users', link: '/administration/users' },
        { name: 'Manage user' },
      ]}
      caption="Manage user"
      title={user && user.name}
    >
      <LoadingSpinner loading={isLoadingUser} text="Loading user details">
        {user && (
          <>
            <h2 className="govuk-heading-m">Details</h2>
            <SummaryList>
              <SummaryListItem term="Name">{user.name}</SummaryListItem>
              <SummaryListItem term="Email">
                <a href={`mailto:${user.email}`}>{user.email}</a>
              </SummaryListItem>
            </SummaryList>

            <LoadingSpinner loading={isLoadingRoles}>
              <RoleForm roles={roles} user={user} onUpdate={refetch} />
            </LoadingSpinner>

            <LoadingSpinner loading={isLoadingReleases}>
              <PreReleaseAccessForm
                releases={releases}
                user={user}
                onUpdate={refetch}
              />
            </LoadingSpinner>

            <LoadingSpinner loading={isLoadingPublications}>
              <PublicationAccessForm
                publications={publications}
                user={user}
                onUpdate={refetch}
              />
            </LoadingSpinner>
          </>
        )}
      </LoadingSpinner>
      <div className="govuk-!-margin-top-6">
        <Link to="/administration/users" className="govuk-back-link">
          Back
        </Link>
      </div>
    </Page>
  );
}
