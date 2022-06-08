import Page from '@admin/components/Page';
import userService, {
  ResourceRoles,
  Role,
  UserInvite,
} from '@admin/services/userService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import Form from '@common/components/form/Form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { ErrorControlState } from '@common/contexts/ErrorControlContext';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import orderBy from 'lodash/orderBy';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { IdTitlePair } from '@admin/services/types/common';
import ButtonGroup from '@common/components/ButtonGroup';
import InviteUserReleaseRoleForm from '@admin/pages/users/components/InviteUserReleaseRoleForm';

export interface InviteUserReleaseRole {
  releaseId: string;
  releaseTitle?: string;
  releaseRole: string;
}

interface FormValues {
  userEmail: string;
  roleId: string;
  userReleaseRoles: InviteUserReleaseRole[];
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'userEmail',
    messages: {
      UserAlreadyExists: 'User already exists',
    },
  }),
];

interface InviteUserModel {
  roles: Role[];
  resourceRoles: ResourceRoles;
  releases: IdTitlePair[];
}

const UserInvitePage = ({
  history,
}: RouteComponentProps & ErrorControlState) => {
  const formId = 'inviteUserForm';

  const { value: model, isLoading } = useAsyncHandledRetry<
    InviteUserModel
  >(async () => {
    const [roles, resourceRoles, releases] = await Promise.all([
      userService.getRoles(),
      userService.getResourceRoles(),
      userService.getReleases(),
    ]);
    return { roles, resourceRoles, releases };
  }, []);

  const cancelHandler = () => history.push('/administration/users/invites');

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    const userReleaseRoles = values.userReleaseRoles.map(userReleaseRole => {
      return {
        releaseId: userReleaseRole.releaseId,
        releaseRole: userReleaseRole.releaseRole,
      };
    });
    const submission: UserInvite = {
      email: values.userEmail,
      roleId: values.roleId,
      userReleaseRoles,
    };

    await userService.inviteUser(submission);

    history.push(`/administration/users/invites`);
  }, errorMappings);

  return (
    <LoadingSpinner loading={!model || isLoading}>
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
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            userEmail: '',
            roleId: orderBy(model?.roles, role => role.name)?.[0]?.id ?? '',
            userReleaseRoles: [],
          }}
          validationSchema={Yup.object<FormValues>({
            userEmail: Yup.string()
              .required('Provide the users email')
              .email('Provide a valid email address'),
            roleId: Yup.string().required('Choose role for the user'),
            userReleaseRoles: Yup.array(),
          })}
          onSubmit={handleSubmit}
        >
          {form => {
            return (
              <Form id={formId}>
                <FormFieldTextInput<FormValues>
                  label="User email"
                  name="userEmail"
                  width={20}
                  hint="The invited user must be on the DfE AAD. Contact explore.statistics@education.gov.uk if unsure."
                />

                <FormFieldSelect<FormValues>
                  label="Role"
                  name="roleId"
                  hint="The user's role within the service."
                  placeholder="Choose role"
                  options={model?.roles?.map(role => ({
                    label: role.name,
                    value: role.id,
                  }))}
                />

                <InviteUserReleaseRoleForm
                  releases={model?.releases}
                  releaseRoles={model?.resourceRoles.Release}
                  userReleaseRoles={form.values.userReleaseRoles}
                  onAddUserReleaseRole={(
                    newUserReleaseRole: InviteUserReleaseRole,
                  ) => {
                    form.setFieldValue('userReleaseRoles', [
                      ...form.values.userReleaseRoles,
                      newUserReleaseRole,
                    ]);
                  }}
                  onRemoveUserReleaseRole={(
                    userReleaseRoleToRemove: InviteUserReleaseRole,
                  ) => {
                    form.setFieldValue(
                      'userReleaseRoles',
                      form.values.userReleaseRoles.filter(
                        userReleaseRole =>
                          userReleaseRoleToRemove !== userReleaseRole,
                      ),
                    );
                  }}
                />

                <ButtonGroup className="govuk-!-margin-top-6">
                  <Button type="submit">Send invite</Button>
                  <ButtonText onClick={cancelHandler}>Cancel</ButtonText>
                </ButtonGroup>
              </Form>
            );
          }}
        </Formik>
      </Page>
    </LoadingSpinner>
  );
};

export default UserInvitePage;
