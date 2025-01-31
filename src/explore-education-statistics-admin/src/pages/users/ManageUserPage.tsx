import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import userQueries from '@admin/queries/userQueries';
import publicationQueries from '@admin/queries/publicationQueries';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RoleForm from '@admin/pages/users/components/RoleForm';
import ReleaseAccessForm from '@admin/pages/users/components/ReleaseAccessForm';
import PublicationAccessForm from '@admin/pages/users/components/PublicationAccessForm';
import { useQuery } from '@tanstack/react-query';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';

export default function ManageUserPage({
  match,
}: RouteComponentProps<{ userId: string }>) {
  const { userId } = match.params;

  const {
    data: user,
    isLoading: isLoadingUser,
    refetch,
  } = useQuery(userQueries.get(userId));

  const { data: roles, isLoading: isLoadingRoles } = useQuery(
    userQueries.getRoles,
  );
  const { data: resourceRoles, isLoading: isLoadingResourceRoles } = useQuery(
    userQueries.getResourceRoles,
  );
  const { data: releases, isLoading: isLoadingReleases } = useQuery(
    userQueries.getReleases,
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

            <LoadingSpinner
              loading={isLoadingReleases || isLoadingResourceRoles}
            >
              <ReleaseAccessForm
                releases={releases}
                releaseRoles={resourceRoles?.Release}
                user={user}
                onUpdate={refetch}
              />
            </LoadingSpinner>

            <LoadingSpinner
              loading={isLoadingPublications || isLoadingResourceRoles}
            >
              <PublicationAccessForm
                publications={publications}
                publicationRoles={resourceRoles?.Publication}
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
