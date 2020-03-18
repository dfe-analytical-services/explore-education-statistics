import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import userService from '@admin/services/users/service';
import { UserInvite } from '@admin/services/users/types';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset, Formik } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import RelatedInformation from '@common/components/RelatedInformation';
import { ErrorControlState } from '@common/contexts/ErrorControlContext';
import Yup from '@common/validation/validation/yup';
import React from 'react';
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
}

const UserInvitePage = ({
  history,
}: RouteComponentProps & ErrorControlState) => {
  const formId = 'inviteUserForm';

  const cancelHandler = () => history.push('/administration/users');

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    const submission: UserInvite = {
      email: values.userEmail,
    };

    await userService.inviteUser(submission);

    history.push(`/administration/users`);
  }, errorCodeMappings);

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Users', link: '/administration/users' },
        { name: 'Invite' },
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
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <Formik<FormValues>
            enableReinitialize
            initialValues={{
              userEmail: '',
            }}
            validationSchema={Yup.object({
              userEmail: Yup.string()
                .required('Provide the users email')
                .email('Provide a valid email address'),
            })}
            onSubmit={handleSubmit}
            render={_ => {
              return (
                <Form id={formId}>
                  <FormFieldset
                    id={`${formId}-selectedContactIdFieldset`}
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
        </div>
      </div>
    </Page>
  );
};

export default UserInvitePage;
