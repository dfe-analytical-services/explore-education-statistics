import Page from '@admin/components/Page';
import { TimePeriodCoverageGroup } from '@admin/pages/DummyReferenceData';
import ReleaseSetupForm, {
  EditFormValues,
} from '@admin/pages/release/setup/ReleaseSetupForm';
import { assembleCreateReleaseRequestFromForm } from '@admin/pages/release/util/releaseSetupUtil';
import dashboardRoutes from '@admin/routes/dashboard/routes';
import { setupRoute } from '@admin/routes/edit-release/routes';
import { emptyDayMonthYear } from '@admin/services/common/types';
import service from '@admin/services/release/create-release/service';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import Yup from '@common/lib/validation/yup';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import { ObjectSchemaDefinition } from 'yup';

interface MatchProps {
  publicationId: string;
}

export type FormValues = {
  templateReleaseId: string;
} & EditFormValues;

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
      <ReleaseSetupForm<FormValues>
        submitButtonText="Create new release"
        initialValuesSupplier={(
          timePeriodCoverageGroups: TimePeriodCoverageGroup[],
        ): FormValues => ({
          timePeriodCoverageCode: timePeriodCoverageGroups[0].options[0].id,
          timePeriodCoverageStartYear: '',
          releaseTypeId: '',
          scheduledPublishDate: emptyDayMonthYear(),
          nextReleaseExpectedDate: emptyDayMonthYear(),
          templateReleaseId: '',
        })}
        validationRulesSupplier={(
          baseValidationRules: ObjectSchemaDefinition<EditFormValues>,
        ): ObjectSchemaDefinition<FormValues> => ({
          ...baseValidationRules,
          templateReleaseId: Yup.string().required('Choose a template'),
        })}
        onSubmitHandler={submitHandler}
        onCancelHandler={cancelHandler}
        additionalFields={
          <FormFieldRadioGroup<FormValues>
            id="releaseSetupForm-templateReleaseId"
            legend="Select template"
            name="templateReleaseId"
            options={[
              {
                label: 'Create new template',
                value: `new`,
              },
              {
                label: 'Copy existing template (2017 / 2018)',
                value: `existing`,
              },
            ]}
          />
        }
      />
    </Page>
  );
};

export default CreateReleasePage;
