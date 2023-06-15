import {
  publicationTeamAccessRoute,
  PublicationTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import releasePermissionService, {
  UserReleaseRole,
} from '@admin/services/releasePermissionService';
import { Form, FormFieldCheckboxGroup } from '@common/components/form';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import WarningMessage from '@common/components/WarningMessage';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { Formik } from 'formik';
import { generatePath, useHistory } from 'react-router-dom';
import React from 'react';

interface AddExistingUsersFormValues {
  userIds: string[];
}

interface Props {
  publicationContributors: UserReleaseRole[];
  publicationId: string;
  releaseContributors: UserReleaseRole[];
  releaseId: string;
}

const PublicationReleaseContributorsForm = ({
  publicationContributors,
  publicationId,
  releaseContributors,
  releaseId,
}: Props) => {
  const history = useHistory();

  const handleSubmit = useFormSubmit<AddExistingUsersFormValues>(
    async values => {
      await releasePermissionService.updateReleaseContributors(
        releaseId,
        values.userIds,
      );
      history.push(
        generatePath<PublicationTeamRouteParams>(
          publicationTeamAccessRoute.path,
          {
            publicationId,
            releaseId,
          },
        ),
      );
    },
    [],
  );

  if (!publicationContributors || !publicationContributors.length) {
    return (
      <>
        <WarningMessage>
          There are no contributors for this release's publication.
        </WarningMessage>
        <ButtonText
          onClick={() => {
            history.push(
              generatePath<PublicationTeamRouteParams>(
                publicationTeamAccessRoute.path,
                {
                  publicationId,
                  releaseId,
                },
              ),
            );
          }}
        >
          Go back
        </ButtonText>
      </>
    );
  }

  const releaseContributorIds = releaseContributors.map(c => c.userId);
  const initialValues: AddExistingUsersFormValues = {
    userIds: publicationContributors
      .map(contributor => contributor.userId)
      .filter(id => releaseContributorIds.includes(id)),
  };

  return (
    <Formik<AddExistingUsersFormValues>
      initialValues={initialValues}
      onSubmit={handleSubmit}
    >
      {form => {
        return (
          <Form id="addExistingUsersForm" showSubmitError>
            <FormFieldCheckboxGroup<AddExistingUsersFormValues>
              name="userIds"
              legend="Select contributors for this release"
              legendSize="m"
              disabled={form.isSubmitting}
              selectAll
              small
              options={publicationContributors.map(contributor => {
                return {
                  label: `${contributor.userDisplayName} (${contributor.userEmail})`,
                  value: contributor.userId,
                };
              })}
            />
            <ButtonGroup>
              <Button type="submit" disabled={form.isSubmitting}>
                Update contributors
              </Button>
              <ButtonText
                onClick={() => {
                  history.push(
                    generatePath<PublicationTeamRouteParams>(
                      publicationTeamAccessRoute.path,
                      {
                        publicationId,
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
  );
};

export default PublicationReleaseContributorsForm;
