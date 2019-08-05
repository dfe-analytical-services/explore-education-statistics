import { TimePeriodCoverageGroup } from '@admin/pages/DummyReferenceData';
import ReleaseSetupForm, {
  BaseFormValues,
} from '@admin/pages/release/setup/ReleaseSetupForm';
import { assembleUpdateReleaseSetupRequestFromForm } from '@admin/pages/release/util/releaseSetupUtil';
import { setupRoute } from '@admin/routes/edit-release/routes';
import { dayMonthYearValuesToInputs } from '@admin/services/common/types';
import service from '@admin/services/release/edit-release/setup/service';
import { ReleaseSetupDetails } from '@admin/services/release/types';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { ObjectSchemaDefinition } from 'yup';
import ReleasePageTemplate from '../components/ReleasePageTemplate';

interface MatchProps {
  releaseId: string;
}

export interface FormValues extends BaseFormValues {
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
            submitButtonText="Update release status"
            validationRulesSupplier={(
              baseValidationRules: ObjectSchemaDefinition<BaseFormValues>,
            ) => baseValidationRules as ObjectSchemaDefinition<FormValues>}
            initialValuesSupplier={(
              _: TimePeriodCoverageGroup[],
            ): FormValues => ({
              releaseId,
              timePeriodCoverageCode:
                releaseSetupDetails.timePeriodCoverageCode,
              timePeriodCoverageStartYear: releaseSetupDetails.timePeriodCoverageStartYear.toString(),
              releaseTypeId: releaseSetupDetails.releaseType.id,
              scheduledPublishDate: dayMonthYearValuesToInputs(
                releaseSetupDetails.scheduledPublishDate,
              ),
              nextReleaseExpectedDate: dayMonthYearValuesToInputs(
                releaseSetupDetails.nextReleaseExpectedDate,
              ),
            })}
            onSubmitHandler={submitHandler}
            onCancelHandler={cancelHandler}
          />
        </ReleasePageTemplate>
      )}
    </>
  );
};

export default ReleaseSetupEditPage;
