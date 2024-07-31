import {
  publicationTeamAccessRoute,
  PublicationTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import releasePermissionService, {
  UserReleaseRole,
} from '@admin/services/releasePermissionService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import WarningMessage from '@common/components/WarningMessage';
import Form from '@common/components/form/Form';
import FormProvider from '@common/components/form/FormProvider';
import FormFieldCheckboxGroup from '@common/components/form/FormFieldCheckboxGroup';
import React from 'react';
import { generatePath, useHistory } from 'react-router-dom';

interface FormValues {
  userIds: string[];
}

interface Props {
  publicationContributors: UserReleaseRole[];
  publicationId: string;
  releaseContributors: UserReleaseRole[];
  releaseVersionId: string;
}

const PublicationReleaseContributorsForm = ({
  publicationContributors,
  publicationId,
  releaseContributors,
  releaseVersionId,
}: Props) => {
  const history = useHistory();

  const handleSubmit = async (values: FormValues) => {
    await releasePermissionService.updateReleaseContributors(
      releaseVersionId,
      values.userIds,
    );
    history.push(
      generatePath<PublicationTeamRouteParams>(
        publicationTeamAccessRoute.path,
        {
          publicationId,
          releaseVersionId,
        },
      ),
    );
  };

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
                  releaseVersionId,
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
  const initialValues: FormValues = {
    userIds: publicationContributors
      .map(c => c.userId)
      .filter(id => releaseContributorIds.includes(id)),
  };

  return (
    <FormProvider initialValues={initialValues}>
      {({ formState }) => {
        return (
          <Form id="addExistingUsersForm" onSubmit={handleSubmit}>
            <FormFieldCheckboxGroup<FormValues>
              name="userIds"
              legend="Select contributors for this release"
              legendSize="m"
              disabled={formState.isSubmitting}
              selectAll
              small
              options={publicationContributors.map(c => {
                return {
                  label: `${c.userDisplayName} (${c.userEmail})`,
                  value: c.userId,
                };
              })}
            />
            <ButtonGroup>
              <Button type="submit" disabled={formState.isSubmitting}>
                Update contributors
              </Button>
              <ButtonText
                onClick={() => {
                  history.push(
                    generatePath<PublicationTeamRouteParams>(
                      publicationTeamAccessRoute.path,
                      {
                        publicationId,
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
  );
};

export default PublicationReleaseContributorsForm;
