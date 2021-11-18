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
import {
  publicationManageTeamAccessRoute,
  PublicationRouteParams,
} from '@admin/routes/routes';

interface AddExistingUsersFormValues {
  users: string[];
}

export interface Props {
  publicationId: string;
  releaseId: string;
  releaseTitle: string;
  contributors: ManageAccessPageContributor[];
}

const PublicationAddExistingUsersTab = ({
  publicationId,
  releaseId,
  releaseTitle,
  contributors,
}: Props) => {
  const history = useHistory();

  const handleSubmit = useFormSubmit<AddExistingUsersFormValues>(
    async values => {
      await releasePermissionService.updateReleaseContributors(
        releaseId,
        values.users,
      );
      history.push(
        generatePath<PublicationRouteParams>(
          publicationManageTeamAccessRoute.path,
          {
            publicationId,
          },
        ),
      );
    },
    [],
  );

  if (!contributors || !contributors.length) {
    return (
      <WarningMessage>
        There are no contributors for this release's publication.
      </WarningMessage>
    );
  }

  const initialValues: AddExistingUsersFormValues = {
    users: contributors
      .filter((c: ManageAccessPageContributor) => c.releaseRoleId !== undefined)
      .map((c: ManageAccessPageContributor) => c.userId),
  };

  const checkboxOptions = contributors.map(
    (contributor: ManageAccessPageContributor) => {
      return {
        label: `${contributor.userFullName} (${contributor.userEmail})`,
        value: contributor.userId,
      };
    },
  );

  return (
    <>
      <h2>Add existing publication users to release ({releaseTitle})</h2>

      <Formik<AddExistingUsersFormValues>
        initialValues={initialValues}
        onSubmit={handleSubmit}
      >
        {form => {
          return (
            <Form id="addExistingUsersForm" showSubmitError>
              <FormFieldCheckboxGroup<AddExistingUsersFormValues>
                name="users"
                legend="Select users you wish to have permission to edit the release"
                legendHidden
                disabled={form.isSubmitting}
                selectAll
                options={checkboxOptions}
              />
              <Button type="submit" disabled={form.isSubmitting}>
                Update permissions
              </Button>
            </Form>
          );
        }}
      </Formik>
    </>
  );
};

export default PublicationAddExistingUsersTab;
