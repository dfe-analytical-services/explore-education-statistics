import {
  PublicationTeamRouteParams,
  publicationTeamAccessRoute,
} from '@admin/routes/publicationRoutes';
import {
  BasicPublicationDetails,
  MyPublication,
} from '@admin/services/publicationService';
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
import React from 'react';
import { useHistory } from 'react-router-dom';
import { generatePath } from 'react-router';

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
  hideCancelButton?: boolean; // EES-3217 remove when pages live
  publication: BasicPublicationDetails | MyPublication; // EES-3217 can be just one type when this goes live
  releases: ReleaseSummary[];
  releaseId: string;
  returnRoute?: string; // EES-3217 remove when pages live
}

const PublicationInviteNewUsersForm = ({
  hideCancelButton = false,
  publication,
  releases,
  releaseId,
  returnRoute,
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
        returnRoute ??
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
                {!hideCancelButton && (
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
                )}
              </ButtonGroup>
            </Form>
          );
        }}
      </Formik>
    </>
  );
};

export default PublicationInviteNewUsersForm;
