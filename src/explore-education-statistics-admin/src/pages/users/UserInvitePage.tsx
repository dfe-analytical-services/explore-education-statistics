import React from 'react';
import Page from '@admin/components/Page';
import RelatedInformation from '@common/components/RelatedInformation';
import Link from '@admin/components/Link';
import userService from '@admin/services/users/service';
import { UserInvite } from '@admin/services/users/types';
import { RouteComponentProps } from 'react-router';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import Button from '@common/components/Button';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import Form from '@common/components/form/Form';
import { FormFieldset, Formik } from '@common/components/form';
import { FormikProps } from 'formik';
import submitWithFormikValidation from '@admin/validation/formikSubmitHandler';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import Yup from '@common/lib/validation/yup';

interface FormValues {
  userEmail: string;
}

const UserInvitePage = ({
  history,
  handleApiErrors,
}: RouteComponentProps & ErrorControlProps) => {
  const formId = 'inviteUserForm';

  const errorCodeMappings = [
    errorCodeToFieldError(
      'USER_ALREADY_EXISTS',
      'userEmail',
      'User already exists',
    ),
  ];
  const cancelHandler = () => history.push('/administration/users');

  const submitFormHandler = submitWithFormikValidation<FormValues>(
    async values => {
      const submission: UserInvite = {
        email: values.userEmail,
      };

      const invite = await userService.inviteUser(submission);

      history.push(`/administration/users`);
    },
    handleApiErrors,
    ...errorCodeMappings,
  );

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
            onSubmit={submitFormHandler}
            render={(form: FormikProps<FormValues>) => {
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
                    <Link to="#" onClick={cancelHandler}>
                      Cancel
                    </Link>
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

export default withErrorControl(UserInvitePage);
