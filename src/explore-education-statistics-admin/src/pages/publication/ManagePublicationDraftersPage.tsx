import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  PublicationTeamRouteParams,
  publicationTeamAccessRoute,
} from '@admin/routes/publicationRoutes';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { useHistory } from 'react-router-dom';
import { generatePath } from 'react-router';
import { ObjectSchema } from 'yup';
import publicationRolesService from '@admin/services/user-management/publicationRolesService';
import FormFieldCheckboxGroup from '@common/components/form/FormFieldCheckboxGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { PublicationRole } from '@admin/services/types/PublicationRole';
import publicationRolesQueries from '@admin/queries/user-management/publicationRolesQueries';
import { useQuery } from '@tanstack/react-query';
import WarningMessage from '@common/components/WarningMessage';

interface InviteDrafterFormValues {
  email: string;
}

interface UpdateDraftersFormValues {
  userIds: string[];
}

export const errorMappings = [
  mapFieldErrors<InviteDrafterFormValues>({
    target: 'releaseIds',
    messages: {
      UserAlreadyHasResourceRoleOrMorePowerfulRole:
        'The user has already been invited with these permissions',
    },
  }),
];

const ManagePublicationDraftersPage = () => {
  const history = useHistory();

  const { publicationId } = usePublicationContext();

  const { data: publicationRoles = [], isLoading } = useQuery(
    publicationRolesQueries.listPublicationRoles(publicationId),
  );

  const publicationDrafters = publicationRoles.filter(
    pr => pr.role === PublicationRole.Drafter,
  );

  const existsDrafters = publicationDrafters.length > 0;

  const handleInviteDrafter = async (values: InviteDrafterFormValues) => {
    await publicationRolesService.inviteDrafter({
      publicationId,
      email: values.email,
    });
  };

  const handleUpdateDrafters = async (values: UpdateDraftersFormValues) => {
    await publicationRolesService.updatePublicationDrafters(publicationId, {
      userIds: values.userIds,
    });
  };

  const inviteDrafterInitialValues: InviteDrafterFormValues = {
    email: '',
  };

  const updateDraftersInitialValues: UpdateDraftersFormValues = {
    userIds: publicationDrafters.map(c => c.userId),
  };

  const inviteDrafterValidationSchema = useMemo<
    ObjectSchema<InviteDrafterFormValues>
  >(() => {
    return Yup.object({
      email: Yup.string()
        .required('Enter an email address')
        .email('Enter a valid email address'),
    });
  }, []);

  return (
    <LoadingSpinner loading={isLoading}>
      <h2>Invite a user to edit this publication</h2>
      <FormProvider
        errorMappings={errorMappings}
        initialValues={inviteDrafterInitialValues}
        validationSchema={inviteDrafterValidationSchema}
      >
        {({ formState }) => {
          return (
            <Form id="inviteDrafterForm" onSubmit={handleInviteDrafter}>
              <FormFieldTextInput
                className="govuk-!-width-one-third"
                name="email"
                label="Enter an email address"
              />
              <Button type="submit" disabled={formState.isSubmitting}>
                Invite user
              </Button>
            </Form>
          );
        }}
      </FormProvider>

      <h2>Edit drafters for this publication</h2>
      {existsDrafters ? (
        <FormProvider initialValues={updateDraftersInitialValues}>
          {({ formState }) => (
            <Form id="addExistingUsersForm" onSubmit={handleUpdateDrafters}>
              <FormFieldCheckboxGroup<UpdateDraftersFormValues>
                name="userIds"
                legend="Select drafters for this publication"
                legendSize="m"
                disabled={formState.isSubmitting}
                selectAll
                small
                options={publicationDrafters.map(c => {
                  return {
                    label: `${c.userName} (${c.email})`,
                    value: c.userId,
                  };
                })}
              />
              <Button type="submit" disabled={formState.isSubmitting}>
                Update drafters
              </Button>
            </Form>
          )}
        </FormProvider>
      ) : (
        <WarningMessage>
          There are no drafters for this publication.
        </WarningMessage>
      )}

      <ButtonText
        onClick={() => {
          history.push(
            generatePath<PublicationTeamRouteParams>(
              publicationTeamAccessRoute.path,
              {
                publicationId,
              },
            ),
          );
        }}
      >
        Go back
      </ButtonText>
    </LoadingSpinner>
  );
};

export default ManagePublicationDraftersPage;
