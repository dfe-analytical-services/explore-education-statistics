import WarningMessage from '@common/components/WarningMessage';
import React from 'react';
import releasePermissionService, {
  ManageAccessPageContributor,
} from '@admin/services/releasePermissionService';
import { Formik } from 'formik';
import { Form, FormFieldCheckboxGroup } from '@common/components/form';
import Button from '@common/components/Button';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { generatePath, useHistory } from 'react-router-dom';
import { publicationManageTeamAccessReleaseRoute } from '@admin/routes/routes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import ButtonText from '@common/components/ButtonText';
import ButtonGroup from '@common/components/ButtonGroup';

interface AddExistingUsersFormValues {
  userIds: string[];
}

export interface Props {
  publicationId: string;
  releaseId: string;
  publicationContributors: ManageAccessPageContributor[];
  releaseContributors: ManageAccessPageContributor[];
}

const PublicationReleaseContributorsForm = ({
  publicationId,
  releaseId,
  publicationContributors,
  releaseContributors,
}: Props) => {
  const history = useHistory();

  const handleSubmit = useFormSubmit<AddExistingUsersFormValues>(
    async values => {
      await releasePermissionService.updateReleaseContributors(
        releaseId,
        values.userIds,
      );
      history.push(
        generatePath<ReleaseRouteParams>(
          publicationManageTeamAccessReleaseRoute.path,
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
      <WarningMessage>
        There are no contributors for this release's publication.
      </WarningMessage>
    );
  }

  const releaseContributorIds = releaseContributors.map(c => c.userId);
  const initialValues: AddExistingUsersFormValues = {
    userIds: publicationContributors
      .map(c => c.userId)
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
              options={publicationContributors.map(c => {
                return {
                  label: `${c.userDisplayName} (${c.userEmail})`,
                  value: c.userId,
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
                    generatePath<ReleaseRouteParams>(
                      publicationManageTeamAccessReleaseRoute.path,
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
