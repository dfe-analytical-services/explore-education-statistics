import Page from '@admin/components/Page';
import userService, { UserInvite } from '@admin/services/userService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import { ErrorControlState } from '@common/contexts/ErrorControlContext';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import publicationQueries from '@admin/queries/publicationQueries';
import userQueries from '@admin/queries/userQueries';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ButtonGroup from '@common/components/ButtonGroup';
import InviteUserReleaseRoleForm from '@admin/pages/users/components/InviteUserReleaseRoleForm';
import InviteUserPublicationRoleForm from '@admin/pages/users/components/InviteUserPublicationRoleForm';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import { ObjectSchema } from 'yup';
import { useQuery } from '@tanstack/react-query';
import React, { useMemo } from 'react';
import { RouteComponentProps } from 'react-router';
import orderBy from 'lodash/orderBy';
import {
  PublicationRole,
  publicationRoles,
} from '@admin/services/types/PublicationRole';

export interface InviteUserReleaseRole {
  releaseId: string;
  releaseTitle?: string;
  releaseRole: string;
}

export interface InviteUserPublicationRole {
  publicationId: string;
  publicationTitle?: string;
  publicationRole: PublicationRole;
}

export interface UserInviteFormValues {
  userEmail: string;
  roleId: string;
  userReleaseRoles?: InviteUserReleaseRole[];
  userPublicationRoles?: InviteUserPublicationRole[];
  publicationId?: string;
  publicationRole?: PublicationRole;
  releaseId?: string;
  releaseRole?: string;
}

const errorMappings = [
  mapFieldErrors<UserInviteFormValues>({
    target: 'userEmail',
    messages: {
      UserAlreadyExists: 'User already exists',
    },
  }),
];

export default function UserInvitePage({
  history,
}: RouteComponentProps & ErrorControlState) {
  const formId = 'inviteUserForm';

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

  const isLoading =
    isLoadingRoles ||
    isLoadingResourceRoles ||
    isLoadingReleases ||
    isLoadingPublications;

  const cancelHandler = () => history.push('/administration/users/invites');

  const handleSubmit = async (values: UserInviteFormValues) => {
    const userReleaseRoles =
      values.userReleaseRoles?.map(userReleaseRole => {
        return {
          releaseId: userReleaseRole.releaseId,
          releaseRole: userReleaseRole.releaseRole,
        };
      }) ?? [];
    const userPublicationRoles =
      values.userPublicationRoles?.map(userPublicationRole => {
        return {
          publicationId: userPublicationRole.publicationId,
          publicationRole: userPublicationRole.publicationRole,
        };
      }) ?? [];
    const submission: UserInvite = {
      email: values.userEmail,
      roleId: values.roleId,
      userReleaseRoles,
      userPublicationRoles,
    };

    await userService.inviteUser(submission);

    history.push(`/administration/users/invites`);
  };

  const validationSchema = useMemo<ObjectSchema<UserInviteFormValues>>(() => {
    return Yup.object({
      userEmail: Yup.string()
        .required('Provide the users email')
        .email('Provide a valid email address'),
      roleId: Yup.string().required('Choose role for the user'),
      userReleaseRoles: Yup.array().of(
        Yup.object({
          releaseId: Yup.string().required(
            'Choose release to give the user access to',
          ),
          releaseTitle: Yup.string(),
          releaseRole: Yup.string().required(
            'Choose release role for the user',
          ),
        }),
      ),
      userPublicationRoles: Yup.array(
        Yup.object({
          publicationId: Yup.string().required(),
          publicationTitle: Yup.string(),
          publicationRole: Yup.mixed<PublicationRole>()
            .oneOf(publicationRoles)
            .required(),
        }),
      ),
      publicationId: Yup.string(),
      publicationRole: Yup.mixed<PublicationRole>(),
      releaseId: Yup.string(),
      releaseRole: Yup.string(),
    });
  }, []);

  return (
    <LoadingSpinner loading={isLoading}>
      <Page
        title="Invite user"
        caption="Manage access to this service"
        wide
        breadcrumbs={[
          { name: 'Platform administration', link: '/administration' },
          { name: 'Invites', link: '/administration/users/invites' },
          { name: 'Invite user' },
        ]}
      >
        <FormProvider
          errorMappings={errorMappings}
          initialValues={{
            userEmail: '',
            roleId: orderBy(roles, role => role.name)?.[0]?.id ?? '',
            userReleaseRoles: [],
            userPublicationRoles: [],
          }}
          validationSchema={validationSchema}
        >
          <Form id={formId} onSubmit={handleSubmit}>
            <FormFieldTextInput<UserInviteFormValues>
              label="User email"
              name="userEmail"
              width={20}
              hint="The invited user must be on the DfE AAD. Contact explore.statistics@education.gov.uk if unsure."
            />

            <FormFieldSelect<UserInviteFormValues>
              label="Role"
              name="roleId"
              hint="The user's role within the service."
              placeholder="Choose role"
              options={roles?.map(role => ({
                label: role.name,
                value: role.id,
              }))}
            />

            <InviteUserReleaseRoleForm
              releases={releases}
              releaseRoles={resourceRoles?.Release}
            />

            <InviteUserPublicationRoleForm
              publications={publications}
              publicationRoles={resourceRoles?.Publication}
            />

            <ButtonGroup className="govuk-!-margin-top-6">
              <Button type="submit">Send invite</Button>
              <ButtonText onClick={cancelHandler}>Cancel</ButtonText>
            </ButtonGroup>
          </Form>
        </FormProvider>
      </Page>
    </LoadingSpinner>
  );
}
