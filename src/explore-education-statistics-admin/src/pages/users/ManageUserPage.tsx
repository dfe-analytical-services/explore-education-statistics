import Link from '@admin/components/Link';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import {
  FormFieldset,
  FormFieldSelect,
  FormFieldCheckboxGroup,
  FormFieldTextInput,
  FormGroup,
  Formik,
} from '@common/components/form';
import Form from '@common/components/form/Form';
import Page from '@admin/components/Page';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import userService from '@admin/services/users/service';
import { UserStatus } from '@admin/services/users/types';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import Yup from '@common/validation/yup';

import React, { useEffect, useState } from 'react';
import { Route, RouteComponentProps, Switch } from 'react-router';
import { IdLabelPair } from 'src/services/common/types';

const errorCodeMappings = [
  errorCodeToFieldError(
    'USER_ALREADY_EXISTS',
    'userEmail',
    'User already exists',
  ),
];

interface Model {
  user: UserStatus;
}

interface FormValues {
  userEmail: string;
}

const ManageUserPage = ({
  match,
  history,
}: RouteComponentProps<{ userId: string }>) => {
  const { userId } = match.params;
  const [model, setModel] = useState<Model>();
  const formId = userId;

  useEffect(() => {
    userService.getUser(userId).then(user => {
      setModel({
        user,
      });
    });
  }, []);

  const cancelHandler = () => history.push('/administration/users');

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    // const submission: UserInvite = {
    //   email: values.userEmail,
    // };

    // await userService.inviteUser(submission);

    history.push(`/administration/users`);
  }, errorCodeMappings);

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

      <Formik<FormValues>
        enableReinitialize
        initialValues={{
          userEmail: '',
        }}
        onSubmit={handleSubmit}
        render={_ => {
          return (
            <Form id={formId}>
              <FormFieldset
                id={`${formId}-details`}
                legend="Detals"
                legendSize="m"
              >
                <dl className="govuk-summary-list">
                  <div className="govuk-summary-list__row">
                    <dt className="govuk-summary-list__key">Name</dt>
                    <dd className="govuk-summary-list__value">
                      {model?.user.name}
                    </dd>
                    <dd className="govuk-summary-list__actions">
                      {/* <a className="govuk-link" href="#">
                        Change
                        <span className="govuk-visually-hidden"> name</span>
                      </a> */}
                    </dd>
                  </div>
                  <div className="govuk-summary-list__row">
                    <dt className="govuk-summary-list__key">Email</dt>
                    <dd className="govuk-summary-list__value">
                      {model?.user.email}
                    </dd>
                    <dd className="govuk-summary-list__actions">
                      {/* <a className="govuk-link" href="#">
                        Change
                        <span className="govuk-visually-hidden"> email</span>
                      </a> */}
                    </dd>
                  </div>
                  <div className="govuk-summary-list__row">
                    <dt className="govuk-summary-list__key">Phone</dt>
                    <dd className="govuk-summary-list__value">-</dd>
                    <dd className="govuk-summary-list__actions">
                      {/* <a className="govuk-link" href="#">
                        Change
                        <span className="govuk-visually-hidden"> phone</span>
                      </a> */}
                    </dd>
                  </div>
                </dl>
              </FormFieldset>
              <FormFieldset
                id={`${formId}-role`}
                legend="Role"
                legendSize="m"
                hint="The users role within the service."
              >
                <FormFieldSelect
                  id={`${formId}-role`}
                  label="Role"
                  name="userRole"
                  // options={model.roles.map(role => ({
                  //   label: role.name,
                  //   value: role.id,
                  // }))}
                />
              </FormFieldset>

              <FormFieldCheckboxGroup
                id={`${formId}-releaseaccess`}
                legend="Release access"
                legendSize="m"
                hint="The releases a user has access to (BAU Users have access to all releases)."
                name="releaseAccess"
                options={[]}
              />

              <Button type="submit" className="govuk-!-margin-top-6">
                Save
              </Button>
              <div className="govuk-!-margin-top-6">
                <ButtonText onClick={cancelHandler}>Cancel</ButtonText>
              </div>
            </Form>
          );
        }}
      />
    </Page>
  );
};

export default ManageUserPage;
