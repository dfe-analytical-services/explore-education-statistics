import Page from '@admin/components/Page';
import userService, {
  ResourceRoles,
  Role,
  UserInvite,
} from '@admin/services/userService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset } from '@common/components/form';
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

interface FormValues {
  userEmail: string;
  selectedRoleId: string;
  selectedUserReleaseRoles: {
    releaseId: string;
    releaseTitle?: string;
    releaseRole: string;
  }[];

  selectedReleaseId: string;
  selectedReleaseRole: string;
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
    const userReleaseRoles = values.selectedUserReleaseRoles.map(
      userReleaseRole => {
        return {
          releaseId: userReleaseRole.releaseId,
          releaseRole: userReleaseRole.releaseRole,
        };
      },
    );
    const submission: UserInvite = {
      email: values.userEmail,
      roleId: values.selectedRoleId,
      userReleaseRoles,
    };

    await userService.inviteUser(submission);

    history.push(`/administration/users/invites`);
  }, errorMappings);

  return (
    <LoadingSpinner loading={!model || isLoading}>
      <Page
        wide
        breadcrumbs={[
          { name: 'Platform administration', link: '/administration' },
          { name: 'Invites', link: '/administration/users/invites' },
          { name: 'Invite user' },
        ]}
        title="Invite user"
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <h1 className="govuk-heading-xl">
              <span className="govuk-caption-xl">
                Manage access to the service
              </span>
              Invite a new user
            </h1>
          </div>
          {/* EES-2464
         <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/documentation/" target="blank">
                  Inviting new users{' '}
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div> */}
        </div>
        {!isLoading && (
          <Formik<FormValues>
            enableReinitialize
            initialValues={{
              userEmail: '',
              selectedRoleId:
                orderBy(model?.roles, role => role.name)?.[0]?.id ?? '',
              selectedUserReleaseRoles: [],

              selectedReleaseId: '',
              selectedReleaseRole: '',
            }}
            validationSchema={Yup.object<FormValues>({
              userEmail: Yup.string()
                .required('Provide the users email')
                .email('Provide a valid email address'),
              selectedRoleId: Yup.string().required('Choose role for the user'),
              selectedUserReleaseRoles: Yup.array(),

              selectedReleaseId: Yup.string(),
              selectedReleaseRole: Yup.string(),
            })}
            onSubmit={handleSubmit}
          >
            {form => {
              return (
                <Form id={formId}>
                  <FormFieldset
                    id="email-fieldset"
                    legend="Provide the email address for the user"
                    legendSize="m"
                    hint="The invited user must have a @education.gov.uk email address"
                  >
                    <FormFieldTextInput<FormValues>
                      label="User email"
                      name="userEmail"
                    />
                  </FormFieldset>

                  <FormFieldset
                    id="role-fieldset"
                    legend="Role"
                    legendSize="m"
                    hint="The user's role within the service."
                  >
                    <FormFieldSelect<FormValues>
                      label="Role"
                      name="selectedRoleId"
                      placeholder="Choose role"
                      options={model?.roles?.map(role => ({
                        label: role.name,
                        value: role.id,
                      }))}
                    />
                  </FormFieldset>

                  <FormFieldset
                    id="release-role-fieldset"
                    legend="Release roles"
                    legendSize="m"
                    hint="The user's release roles within the service."
                  >
                    <FormFieldSelect
                      label="Release"
                      name="selectedReleaseId"
                      placeholder="Choose release"
                      options={model?.releases?.map(release => ({
                        label: release.title,
                        value: release.id,
                      }))}
                    />
                    <FormFieldSelect
                      label="Release role"
                      name="selectedReleaseRole"
                      placeholder="Choose release role"
                      options={model?.resourceRoles?.Release?.map(role => ({
                        label: role,
                        value: role,
                      }))}
                    />
                    <Button
                      type="button"
                      className="govuk-!-margin-top-6"
                      onClick={() => {
                        if (
                          !form.values.selectedUserReleaseRoles.some(
                            role =>
                              role.releaseId ===
                                form.values.selectedReleaseId &&
                              role.releaseRole ===
                                form.values.selectedReleaseRole,
                          )
                        ) {
                          const newUserReleaseRole = {
                            releaseId: form.values.selectedReleaseId,
                            releaseTitle: model?.releases.filter(
                              r => r.id === form.values.selectedReleaseId,
                            )[0].title,
                            releaseRole: form.values.selectedReleaseRole,
                          };
                          form.setFieldValue('selectedUserReleaseRoles', [
                            ...form.values.selectedUserReleaseRoles,
                            newUserReleaseRole,
                          ]);
                        }
                      }}
                    >
                      Add release role
                    </Button>

                    {form.values.selectedUserReleaseRoles.length === 0 ? (
                      <p>No user release roles added</p>
                    ) : (
                      <table>
                        <thead>
                          <tr>
                            <th scope="col">Release Title</th>
                            <th scope="col">Release Role</th>
                            <th scope="col">Options</th>
                          </tr>
                        </thead>
                        <tbody>
                          {form.values.selectedUserReleaseRoles.map(
                            selectedUserReleaseRole => (
                              <tr
                                key={`${selectedUserReleaseRole.releaseId}${selectedUserReleaseRole.releaseRole}`}
                              >
                                <td>{selectedUserReleaseRole.releaseTitle}</td>
                                <td>{selectedUserReleaseRole.releaseRole}</td>
                                <td>
                                  <Button
                                    onClick={() => {
                                      form.setFieldValue(
                                        'selectedUserReleaseRoles',
                                        form.values.selectedUserReleaseRoles.filter(
                                          userReleaseRole =>
                                            userReleaseRole.releaseId !==
                                              selectedUserReleaseRole.releaseId ||
                                            userReleaseRole.releaseRole !==
                                              selectedUserReleaseRole.releaseRole,
                                        ),
                                      );
                                    }}
                                  >
                                    Remove
                                  </Button>
                                </td>
                              </tr>
                            ),
                          )}
                        </tbody>
                      </table>
                    )}
                  </FormFieldset>

                  <Button type="submit" className="govuk-!-margin-top-6">
                    Send invite
                  </Button>
                  <div className="govuk-!-margin-top-6">
                    <ButtonText onClick={cancelHandler}>Cancel</ButtonText>
                  </div>
                </Form>
              );
            }}
          </Formik>
        )}
      </Page>
    </LoadingSpinner>
  );
};

export default UserInvitePage;
