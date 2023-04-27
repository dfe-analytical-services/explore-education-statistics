import {
  PublicationTeamRouteParams,
  publicationTeamAccessRoute,
} from '@admin/routes/publicationRoutes';
import { Publication } from '@admin/services/publicationService';
import { ReleaseSummary } from '@admin/services/releaseService';
import userService from '@admin/services/userService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import {
  FormFieldCheckboxGroup,
  FormFieldTextInput,
} from '@common/components/form';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Form, Formik } from 'formik';
import { useHistory } from 'react-router-dom';
import { generatePath } from 'react-router';
import React from 'react';

interface InviteContributorFormValues {
  email: string;
  releaseIds: string[];
}

export const errorMappings = [
  mapFieldErrors<InviteContributorFormValues>({
    target: 'releaseIds',
    messages: {
      NotAllReleasesBelongToPublication:
        "Some of the releases don't belong to the publication",
      UserAlreadyHasReleaseRoleInvites:
        'The user has already been invited with these permissions',
      UserAlreadyHasReleaseRoles: 'The user already has these permissions',
    },
  }),
];

interface Props {
  publication: Publication;
  releases: ReleaseSummary[];
  releaseId: string;
}

const PublicationInviteNewUsersForm = ({
  publication,
  releases,
  releaseId,
}: Props) => {
  const history = useHistory();

  const handleSubmit = useFormSubmit<InviteContributorFormValues>(
    async values => {
      await userService.inviteContributor(
        values.email,
        publication.id,
        values.releaseIds,
      );
      history.push(
        generatePath<PublicationTeamRouteParams>(
          publicationTeamAccessRoute.path,
          {
            publicationId: publication.id,
            releaseId,
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
      <h2>Invite a user to edit this publication</h2>
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
              <FormFieldTextInput
                className="govuk-!-width-one-third"
                name="email"
                label="Enter an email address"
              />
              <FormFieldCheckboxGroup<InviteContributorFormValues>
                name="releaseIds"
                legend="Select which releases you wish the user to have access"
                legendSize="m"
                disabled={form.isSubmitting}
                selectAll
                small
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
                <ButtonText
                  onClick={() => {
                    history.push(
                      generatePath<PublicationTeamRouteParams>(
                        publicationTeamAccessRoute.path,
                        {
                          publicationId: publication.id,
                          releaseId,
                        },
                      ),
                    );
                  }}
                >
                  Cancel
                </ButtonText>
              </ButtonGroup>
            </Form>
          );
        }}
      </Formik>
    </>
  );
};

export default PublicationInviteNewUsersForm;
