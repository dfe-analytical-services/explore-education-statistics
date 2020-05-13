import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import userService from '@admin/services/users/service';
import { UserInvite, Role } from '@admin/services/users/types';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset, Formik } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import RelatedInformation from '@common/components/RelatedInformation';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { ErrorControlState } from '@common/contexts/ErrorControlContext';
import Yup from '@common/validation/yup';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';

const errorCodeMappings = [
  errorCodeToFieldError(
    'USER_ALREADY_EXISTS',
    'userEmail',
    'User already exists',
  ),
];

interface FormValues {
  userEmail: string;
  selectedRoleId: string;
}

interface InviteUserModel {
  roles: Role[];
}

const UserInvitePage = ({
  history,
}: RouteComponentProps & ErrorControlState) => {
  const [model, setModel] = useState<InviteUserModel>();
  const formId = 'inviteUserForm';

  const { value: releaseRoles } = useAsyncRetry(() =>
    userService.getReleaseRoles(),
  );

  useEffect(() => {
    userService.getRoles().then(roles => {
      setModel({
        roles,
      });
    });
  }, []);

  const cancelHandler = () => history.push('/administration/users/invites');

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    const submission: UserInvite = {
      email: values.userEmail,
      roleId: values.selectedRoleId,
    };

    await userService.inviteUser(submission);

    history.push(`/administration/users/invites`);
  }, errorCodeMappings);

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Invites', link: '/administration/users/invites' },
        { name: 'Invite user' },
      ]}
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
        </div>
      </div>
      {model && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            userEmail: '',
            selectedRoleId:
              orderBy(model.roles, role => role.name)?.[0]?.id ?? '',
          }}
          validationSchema={Yup.object({
            userEmail: Yup.string()
              .required('Provide the users email')
              .email('Provide a valid email address'),
            selectedRoleId: Yup.string().required('Choose role for the user'),
          })}
          onSubmit={handleSubmit}
          render={_ => {
            return (
              <Form id={formId}>
                <FormFieldset
                  id={`${formId}-userEmailFieldset`}
                  legend="Provide the email address for the user"
                  legendSize="m"
                  hint="The invited user must have a @education.gov.uk email address"
                >
                  <FormFieldTextInput
                    id={`${formId}-userEmail`}
                    label="User email"
                    name="userEmail"
                  />
                </FormFieldset>

                <FormFieldset
                  id={`${formId}-selectedRoleIdFieldset`}
                  legend="Role"
                  legendSize="m"
                  hint="The users role within the service."
                >
                  <FormFieldSelect
                    id={`${formId}-selectedRoleId`}
                    label="Role"
                    name="selectedRoleId"
                    placeholder="Choose role"
                    options={model?.roles.map(role => ({
                      label: role.name,
                      value: role.id,
                    }))}
                  />
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
        />
      )}
    </Page>
  );
};

export default UserInvitePage;
