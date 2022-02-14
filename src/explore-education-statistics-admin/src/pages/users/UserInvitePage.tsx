import Page from '@admin/components/Page';
import userService, { Role, UserInvite } from '@admin/services/userService';
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
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';

interface FormValues {
  userEmail: string;
  selectedRoleId: string;
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
}

const UserInvitePage = ({
  history,
}: RouteComponentProps & ErrorControlState) => {
  const [model, setModel] = useState<InviteUserModel>();
  const formId = 'inviteUserForm';

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
  }, errorMappings);

  return (
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
      {model && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            userEmail: '',
            selectedRoleId:
              orderBy(model.roles, role => role.name)?.[0]?.id ?? '',
          }}
          validationSchema={Yup.object<FormValues>({
            userEmail: Yup.string()
              .required('Provide the users email')
              .email('Provide a valid email address'),
            selectedRoleId: Yup.string().required('Choose role for the user'),
          })}
          onSubmit={handleSubmit}
        >
          {() => {
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
                  hint="The users' role within the service."
                >
                  <FormFieldSelect<FormValues>
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
        </Formik>
      )}
    </Page>
  );
};

export default UserInvitePage;
