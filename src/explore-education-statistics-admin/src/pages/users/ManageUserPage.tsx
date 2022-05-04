import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import userService, {
  User,
  UserPublicationRole,
  UserPublicationRoleSubmission,
  UserReleaseRole,
  UserReleaseRoleSubmission,
  UserUpdate,
} from '@admin/services/userService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { mapFieldErrors } from '@common/validation/serverValidations';
import React, { useCallback, useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import RoleForm, {
  UpdateRoleFormValues,
} from '@admin/pages/users/components/RoleForm';
import ReleaseAccessForm, {
  AddReleaseRoleFormValues,
} from '@admin/pages/users/components/ReleaseAccessForm';
import PublicationAccessForm, {
  AddPublicationRoleFormValues,
} from '@admin/pages/users/components/PublicationAccessForm';
import publicationService from '@admin/services/publicationService';

const updateRoleFormErrorMappings = [
  mapFieldErrors<UpdateRoleFormValues>({
    target: 'selectedRoleId',
    messages: {
      RoleDoesNotExist: 'Role does not exist',
    },
  }),
];

const addReleaseFormErrorMappings = [
  mapFieldErrors<AddReleaseRoleFormValues>({
    target: 'selectedReleaseRole',
    messages: {
      UserAlreadyHasResourceRole: 'The user already has this release role',
    },
  }),
];

const addPublicationFormErrorMappings = [
  mapFieldErrors<AddPublicationRoleFormValues>({
    target: 'selectedPublicationRole',
    messages: {
      UserAlreadyHasResourceRole: 'The user already has this publication role',
    },
  }),
];

interface Model {
  user: User;
}

const ManageUserPage = ({ match }: RouteComponentProps<{ userId: string }>) => {
  const [model, setModel] = useState<Model>();
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [, setErrorStatus] = useState<number>();

  const { userId } = match.params;

  const getUser = useCallback(() => {
    setIsLoading(true);
    userService
      .getUser(userId)
      .then(u => {
        setModel({
          user: u,
        });
      })
      .catch(error => {
        setErrorStatus(error.response.status);
      })
      .then(() => setIsLoading(false));
  }, [userId]);

  const { value: roles } = useAsyncRetry(() => userService.getRoles());
  const { value: releases } = useAsyncRetry(() => userService.getReleases());
  const { value: publications } = useAsyncRetry(() =>
    publicationService.getPublicationSummaries(),
  );
  const { value: resourceRoles } = useAsyncRetry(() =>
    userService.getResourceRoles(),
  );

  const handleUpdateRoleFormSubmit = useFormSubmit<UpdateRoleFormValues>(
    async values => {
      const submission: UserUpdate = {
        roleId: values.selectedRoleId,
      };

      await userService.updateUser(userId, submission);
      getUser();
    },
    updateRoleFormErrorMappings,
  );

  const handleAddReleaseRole = useFormSubmit<AddReleaseRoleFormValues>(
    async values => {
      const submission: UserReleaseRoleSubmission = {
        releaseId: values.selectedReleaseId,
        releaseRole: values.selectedReleaseRole,
      };

      await userService.addUserReleaseRole(userId, submission);

      getUser();
    },
    addReleaseFormErrorMappings,
  );

  const handleRemoveReleaseAccess = (userReleaseRole: UserReleaseRole) => {
    userService.removeUserReleaseRole(userReleaseRole.id).then(() => getUser());
  };

  const handleAddPublicationRole = useFormSubmit<AddPublicationRoleFormValues>(
    async values => {
      const submission: UserPublicationRoleSubmission = {
        publicationId: values.selectedPublicationId,
        publicationRole: values.selectedPublicationRole,
      };

      await userService.addUserPublicationRole(userId, submission);

      getUser();
    },
    addPublicationFormErrorMappings,
  );

  const handleRemovePublicationAccess = (
    userPublicationRole: UserPublicationRole,
  ) => {
    userService
      .removeUserPublicationRole(userId, userPublicationRole)
      .then(() => getUser());
  };

  useEffect(() => {
    getUser();
  }, [getUser]);

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Users', link: '/administration/users' },
        { name: 'Manage user' },
      ]}
      title="Manage user"
    >
      <h1 className="govuk-heading-xl">
        <span className="govuk-caption-xl">Manage user</span>
        {model?.user.name}
      </h1>

      <LoadingSpinner loading={isLoading} text="Loading user details">
        {model && (
          <>
            <RoleForm
              roles={roles}
              user={model.user}
              onSubmit={handleUpdateRoleFormSubmit}
            />

            <ReleaseAccessForm
              releases={releases}
              releaseRoles={resourceRoles?.Release}
              user={model.user}
              onRemove={handleRemoveReleaseAccess}
              onSubmit={handleAddReleaseRole}
            />

            <PublicationAccessForm
              publications={publications}
              publicationRoles={resourceRoles?.Publication}
              user={model.user}
              onRemove={handleRemovePublicationAccess}
              onSubmit={handleAddPublicationRole}
            />
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
};

export default ManageUserPage;
