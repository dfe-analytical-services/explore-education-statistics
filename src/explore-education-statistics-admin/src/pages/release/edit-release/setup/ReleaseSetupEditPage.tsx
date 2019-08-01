import ReleaseSetupForm, {
  FormValues,
} from '@admin/pages/release/setup/ReleaseSetupForm';
import { assembleUpdateReleaseSetupRequestFromForm } from '@admin/pages/release/setup/util/releaseSetupUtil';
import { setupRoute } from '@admin/routes/edit-release/routes';
import service from '@admin/services/release/edit-release/setup/service';
import { ReleaseSetupDetails } from '@admin/services/release/types';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import ReleasePageTemplate from '../components/ReleasePageTemplate';

interface MatchProps {
  releaseId: string;
}

const ReleaseSetupEditPage = ({
  match,
  history,
}: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [releaseSetupDetails, setReleaseSetupDetails] = useState<
    ReleaseSetupDetails
  >();

  useEffect(() => {
    service.getReleaseSetupDetails(releaseId).then(release => {
      setReleaseSetupDetails(release);
    });
  }, [releaseId]);

  const submitHandler = (values: FormValues) => {
    const updatedReleaseDetails = assembleUpdateReleaseSetupRequestFromForm(
      releaseId,
      values,
    );

    service
      .updateReleaseSetupDetails(updatedReleaseDetails)
      .then(_ => history.push(setupRoute.generateLink(releaseId)));
  };

  const cancelHandler = () => history.push(setupRoute.generateLink(releaseId));

  return (
    <>
      {releaseSetupDetails && (
        <ReleasePageTemplate
          releaseId={releaseId}
          publicationTitle={releaseSetupDetails.publicationTitle}
        >
          <h2 className="govuk-heading-m">Edit release setup</h2>

          <ReleaseSetupForm
            releaseSetupDetails={releaseSetupDetails}
            submitButtonText="Update release status"
            onSubmitHandler={submitHandler}
            onCancelHandler={cancelHandler}
          />
        </ReleasePageTemplate>
      )}
    </>
  );
};

export default ReleaseSetupEditPage;
