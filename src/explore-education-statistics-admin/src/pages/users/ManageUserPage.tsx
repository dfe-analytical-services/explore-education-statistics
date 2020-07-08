import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import userService, {
  User,
  UserReleaseRoleSubmission,
  UserUpdate,
} from '@admin/services/userService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldSelect, FormFieldset } from '@common/components/form';
import Form from '@common/components/form/Form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import orderBy from 'lodash/orderBy';
import React, { useCallback, useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';

interface UpdateRoleFormValues {
  selectedRoleId: string;
}

const updateRoleFormErrorMappings = [
  mapFieldErrors<UpdateRoleFormValues>({
    target: 'selectedRoleId',
    messages: {
      ROLE_DOES_NOT_EXIST: 'Role does not exist',
    },
  }),
];

interface AddReleaseRoleFormValues {
  selectedReleaseId: string;
  selectedReleaseRoleId: string;
}

const addReleaseFormErrorMappings = [
  mapFieldErrors<AddReleaseRoleFormValues>({
    target: 'selectedReleaseRoleId',
    messages: {
      USER_ALREADY_HAS_RELEASE_ROLE: 'The user already has this release role',
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
  const formId = userId;

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
  const { value: releaseRoles } = useAsyncRetry(() =>
    userService.getReleaseRoles(),
  );

  const handleSubmit = useFormSubmit<UpdateRoleFormValues>(async values => {
    const submission: UserUpdate = {
      id: userId,
      roleId: values.selectedRoleId,
    };

    await userService.updateUser(submission);
    getUser();
  }, updateRoleFormErrorMappings);

  const addReleaseRole = useFormSubmit<AddReleaseRoleFormValues>(
    async values => {
      const submission: UserReleaseRoleSubmission = {
        releaseId: values.selectedReleaseId,
        releaseRole: values.selectedReleaseRoleId,
      };

      await userService.addUserReleaseRole(userId, submission);

      getUser();
    },
    addReleaseFormErrorMappings,
  );

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
    >
      <h1 className="govuk-heading-xl">
        <span className="govuk-caption-xl">Manage user</span>
        {model?.user.name}
      </h1>

      <LoadingSpinner loading={isLoading} text="Loading user details">
        {model && (
          <>
            <Formik<UpdateRoleFormValues>
              enableReinitialize
              initialValues={{
                selectedRoleId: model.user.role ?? '',
              }}
              validationSchema={Yup.object<UpdateRoleFormValues>({
                selectedRoleId: Yup.string().required(
                  'Choose role for the user',
                ),
              })}
              onSubmit={handleSubmit}
            >
              {() => {
                return (
                  <Form id={formId}>
                    <FormFieldset
                      id={`${formId}-details`}
                      legend="Details"
                      legendSize="m"
                    >
                      <SummaryList>
                        <SummaryListItem term="Name">
                          {model.user.name}
                        </SummaryListItem>
                        <SummaryListItem term="Email">
                          <a href={`mailto:${model.user.email}`}>
                            {model.user.email}
                          </a>
                        </SummaryListItem>
                        <SummaryListItem term="Phone">-</SummaryListItem>
                      </SummaryList>
                    </FormFieldset>
                    <FormFieldset
                      id={`${formId}-role`}
                      legend="Role"
                      legendSize="m"
                      hint="The users role within the service."
                    >
                      <div className="govuk-grid-row">
                        <div className="govuk-grid-column-one-quarter">
                          <FormFieldSelect<UpdateRoleFormValues>
                            id={`${formId}-selectedRoleId`}
                            label="Role"
                            name="selectedRoleId"
                            options={roles?.map(role => ({
                              label: role.name,
                              value: role.id,
                            }))}
                          />
                        </div>
                        <div className="govuk-grid-column-one-quarter">
                          <Button
                            type="submit"
                            className="govuk-!-margin-top-6"
                          >
                            Update role
                          </Button>
                        </div>
                      </div>
                    </FormFieldset>
                  </Form>
                );
              }}
            </Formik>
            <Formik<AddReleaseRoleFormValues>
              initialValues={{
                selectedReleaseId:
                  orderBy(releases, release => release)?.[0]?.id ?? '',
                selectedReleaseRoleId:
                  orderBy(releaseRoles, releaseRole => releaseRole)?.[0]
                    ?.value ?? '',
              }}
              enableReinitialize
              onSubmit={addReleaseRole}
            >
              {() => {
                return (
                  <Form id={`${formId}-releaseRole`}>
                    <FormFieldset
                      id={`${formId}-role`}
                      legend="Release access"
                      legendSize="m"
                      hint="The releases a user can access within the service."
                    >
                      <div className="govuk-grid-row">
                        <div className="govuk-grid-column-one-half">
                          <FormFieldSelect<AddReleaseRoleFormValues>
                            id={`${formId}-selectedReleaseId`}
                            label="Release"
                            name="selectedReleaseId"
                            options={releases?.map(release => ({
                              label: release.title,
                              value: release.id,
                            }))}
                          />
                        </div>

                        <div className="govuk-grid-column-one-quarter">
                          <FormFieldSelect<AddReleaseRoleFormValues>
                            id={`${formId}-selectedReleaseRoleId`}
                            label="Release role"
                            name="selectedReleaseRoleId"
                            options={releaseRoles?.map(releaseRole => ({
                              label: releaseRole.name,
                              value: releaseRole.value,
                            }))}
                          />
                        </div>
                        <div className="govuk-grid-column-one-quarter">
                          {model?.user && (
                            <Button
                              type="submit"
                              className="govuk-!-margin-top-6"
                            >
                              Add release access
                            </Button>
                          )}
                        </div>
                      </div>
                    </FormFieldset>
                    <table className="govuk-table">
                      <thead className="govuk-table__head">
                        <tr className="govuk-table__row">
                          <th scope="col" className="govuk-table__header">
                            Publication
                          </th>
                          <th scope="col" className="govuk-table__header">
                            Release
                          </th>
                          <th scope="col" className="govuk-table__header">
                            Role
                          </th>
                          <th scope="col" className="govuk-table__header">
                            Actions
                          </th>
                        </tr>
                      </thead>

                      <tbody className="govuk-table__body">
                        {model.user.userReleaseRoles.map(userReleaseRole => (
                          <tr
                            className="govuk-table__row"
                            key={userReleaseRole.id}
                          >
                            <td className="govuk-table__cell">
                              {userReleaseRole.publication.title}
                            </td>
                            <td className="govuk-table__cell">
                              {userReleaseRole.release.title}
                            </td>
                            <td className="govuk-table__cell">
                              {userReleaseRole.releaseRole.name}
                            </td>
                            <td className="govuk-table__cell">
                              <ButtonText
                                onClick={() => {
                                  userService
                                    .removeUserReleaseRole(
                                      userId,
                                      userReleaseRole,
                                    )
                                    .then(() => getUser());
                                }}
                              >
                                Remove
                              </ButtonText>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </Form>
                );
              }}
            </Formik>
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
