import React from 'react';
import { BasicPublicationDetails } from 'src/services/publicationService';
import { ReleaseSummary } from '@admin/services/releaseService';
import Yup from '@common/validation/yup';
import { Form, Formik } from 'formik';
import {
  FormFieldCheckboxGroup,
  FormFieldTextInput,
} from '@common/components/form';
import useFormSubmit from '@common/hooks/useFormSubmit';
import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import userService from '@admin/services/userService';
import { mapFieldErrors } from '@common/validation/serverValidations';
import { useHistory } from 'react-router-dom';
import { generatePath } from 'react-router';
import {
  publicationManageTeamAccessRoute,
  PublicationRouteParams,
} from '@admin/routes/routes';

interface InviteContributorFormValues {
  email: string;
  releaseIds: string[];
}

export const errorMappings = [
  mapFieldErrors<InviteContributorFormValues>({
    target: 'releaseIds',
    messages: {
      NOT_ALL_RELEASES_BELONG_TO_PUBLICATION:
        "Some of the releases don't belong to the publication",
      USER_ALREADY_HAS_RELEASE_ROLE_INVITES:
        'The user has already been invited with these permissions',
      USER_ALREADY_HAS_RELEASE_ROLES: 'The user already has these permissions',
    },
  }),
];

export interface Props {
  publication: BasicPublicationDetails;
  releases: ReleaseSummary[];
}

const PublicationInviteNewUsersTab = ({ publication, releases }: Props) => {
  const history = useHistory();
  const handleSubmit = useFormSubmit<InviteContributorFormValues>(
    async values => {
      await userService.inviteContributor(
        values.email,
        publication.id,
        values.releaseIds,
      );
      history.push(
        generatePath<PublicationRouteParams>(
          publicationManageTeamAccessRoute.path,
          {
            publicationId: publication.id,
          },
        ),
      );
    },
    errorMappings,
  );

  const initialValues: InviteContributorFormValues = {
    email: '',
    releaseIds: releases.map(r => r.id),
  };
  return (
    <>
      <h2>Invite a user to edit {publication.title}</h2>
      <Formik<InviteContributorFormValues>
        initialValues={initialValues}
        validationSchema={Yup.object({
          email: Yup.string()
            .required('Enter an email address')
            .email('Enter a valid email address'),
          releaseIds: Yup.array().min(1, 'Select at least one release'),
        })}
        onSubmit={handleSubmit}
      >
        {form => {
          return (
            <Form id="inviteContributorForm">
              <FormFieldTextInput name="email" label="Enter an email address" />
              <FormFieldCheckboxGroup<InviteContributorFormValues>
                name="releaseIds"
                legend="Select which releases you wish the user to have access"
                legendSize="m"
                disabled={form.isSubmitting}
                selectAll
                order={[]}
                options={releases.map(release => {
                  return {
                    label: release.title,
                    value: release.id,
                  };
                })}
              />
              <ButtonGroup>
                <Button type="submit" disabled={form.isSubmitting}>
                  Invite user
                </Button>
              </ButtonGroup>
            </Form>
          );
        }}
      </Formik>
    </>
  );
};

export default PublicationInviteNewUsersTab;
