import {
  PublicationTeamRouteParams,
  publicationTeamAccessRoute,
} from '@admin/routes/publicationRoutes';
import { Publication } from '@admin/services/publicationService';
import { ReleaseVersionSummary } from '@admin/services/releaseVersionService';
import userService from '@admin/services/userService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldCheckboxGroup from '@common/components/form/FormFieldCheckboxGroup';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { useHistory } from 'react-router-dom';
import { generatePath } from 'react-router';
import { ObjectSchema } from 'yup';

interface FormValues {
  email: string;
  releaseIds: string[];
}

export const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'releaseIds',
    messages: {
      NotAllReleasesBelongToPublication:
        "Some of the releases don't belong to the publication",
      UserAlreadyHasReleaseRoleInvites:
        'The user has already been invited with these permissions',
    },
  }),
];

interface Props {
  publication: Publication;
  releases: ReleaseVersionSummary[];
  releaseVersionId: string;
}

const PublicationInviteNewUsersForm = ({
  publication,
  releases,
  releaseVersionId,
}: Props) => {
  const history = useHistory();

  const handleSubmit = async (values: FormValues) => {
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
          releaseVersionId,
        },
      ),
    );
  };

  const initialValues: FormValues = {
    email: '',
    releaseIds: releases.map(r => r.id),
  };

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      email: Yup.string()
        .required('Enter an email address')
        .email('Enter a valid email address'),
      releaseIds: Yup.array()
        .of(Yup.string().defined())
        .min(1, 'Select at least one release')
        .required('Select at least one release'),
    });
  }, []);

  return (
    <>
      <h2>Invite a user to edit this publication</h2>
      <FormProvider
        errorMappings={errorMappings}
        initialValues={initialValues}
        validationSchema={validationSchema}
      >
        {({ formState }) => {
          return (
            <Form id="inviteContributorForm" onSubmit={handleSubmit}>
              <FormFieldTextInput
                className="govuk-!-width-one-third"
                name="email"
                label="Enter an email address"
              />
              <FormFieldCheckboxGroup<FormValues>
                name="releaseIds"
                legend="Select which releases you wish the user to have access"
                legendSize="m"
                disabled={formState.isSubmitting}
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
                <Button type="submit" disabled={formState.isSubmitting}>
                  Invite user
                </Button>
                <ButtonText
                  onClick={() => {
                    history.push(
                      generatePath<PublicationTeamRouteParams>(
                        publicationTeamAccessRoute.path,
                        {
                          publicationId: publication.id,
                          releaseVersionId,
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
      </FormProvider>
    </>
  );
};

export default PublicationInviteNewUsersForm;
