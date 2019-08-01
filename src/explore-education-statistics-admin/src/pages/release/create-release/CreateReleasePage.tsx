import Page from '@admin/components/Page';
import ReleaseSetupForm, {
  FormValues,
} from '@admin/pages/release/setup/ReleaseSetupForm';
import { assembleCreateReleaseRequestFromForm } from '@admin/pages/release/setup/util/releaseSetupUtil';
import { setupRoute } from '@admin/routes/edit-release/routes';
import dashboardRoutes from '@admin/routes/dashboard/routes';
import service from '@admin/services/release/create-release/service';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import React from 'react';
import { RouteComponentProps } from 'react-router';

interface MatchProps {
  publicationId: string;
}

const CreateReleasePage = ({
  match,
  history,
}: RouteComponentProps<MatchProps>) => {
  const { publicationId } = match.params;

  const submitHandler = (values: FormValues) => {
    const createReleaseDetails: CreateReleaseRequest = assembleCreateReleaseRequestFromForm(
      publicationId,
      values,
    );

    service
      .createRelease(createReleaseDetails)
      .then(createdRelease =>
        history.push(setupRoute.generateLink(createdRelease.id)),
      );
  };

  const cancelHandler = () => history.push(dashboardRoutes.adminDashboard);

  return (
    <Page
      wide
      breadcrumbs={[
        {
          name: 'Create new release',
        },
      ]}
    >
      <ReleaseSetupForm
        submitButtonText="Create new release"
        onSubmitHandler={submitHandler}
        onCancelHandler={cancelHandler}
      />
    </Page>
  );
};

export default CreateReleasePage;
