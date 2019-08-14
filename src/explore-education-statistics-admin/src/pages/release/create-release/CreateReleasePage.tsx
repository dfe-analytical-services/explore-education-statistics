import Page from '@admin/components/Page';
import ReleaseSummaryForm, {
  EditFormValues,
} from '@admin/pages/release/summary/ReleaseSummaryForm';
import { assembleCreateReleaseRequestFromForm } from '@admin/pages/release/util/releaseSummaryUtil';
import dashboardRoutes from '@admin/routes/dashboard/routes';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import {emptyDayMonthYear, IdTitlePair, TimePeriodCoverageGroup} from '@admin/services/common/types';
import service from '@admin/services/release/create-release/service';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import Yup from '@common/lib/validation/yup';
import React, { useEffect, useState } from 'react';
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

  const [templateRelease, setTemplateRelease] = useState<IdTitlePair>();

  useEffect(() => {
    service.getTemplateRelease(publicationId).then(setTemplateRelease);
  }, []);

  const submitHandler = (values: FormValues) => {
    const createReleaseDetails: CreateReleaseRequest = assembleCreateReleaseRequestFromForm(
      publicationId,
      values,
    );

    service
      .createRelease(createReleaseDetails)
      .then(createdRelease =>
        history.push(
          summaryRoute.generateLink(publicationId, createdRelease.id),
        ),
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
      <ReleaseSummaryForm<FormValues>
        submitButtonText="Create new release"
        initialValuesSupplier={(
          timePeriodCoverageGroups: TimePeriodCoverageGroup[],
        ): FormValues => ({
          timePeriodCoverageCode: timePeriodCoverageGroups[0].timeIdentifiers[0].identifier.value,
          timePeriodCoverageStartYear: '',
          releaseTypeId: '',
          scheduledPublishDate: emptyDayMonthYear(),
          nextReleaseDate: emptyDayMonthYear(),
          templateReleaseId: '',
        })}
        validationRulesSupplier={(
          baseValidationRules: ObjectSchemaDefinition<EditFormValues>,
        ): ObjectSchemaDefinition<FormValues> => ({
          ...baseValidationRules,
          templateReleaseId: templateRelease
            ? Yup.string().required('Choose a template')
            : Yup.string(),
        })}
        onSubmitHandler={submitHandler}
        onCancelHandler={cancelHandler}
        additionalFields={
          templateRelease && (
            <FormFieldRadioGroup<FormValues>
              id="releaseSummaryForm-templateReleaseId"
              legend="Select template"
              name="templateReleaseId"
              options={[
                {
                  label: 'Create new template',
                  value: 'new',
                },
                {
                  label: `Copy existing template (${templateRelease.title})`,
                  value: `${templateRelease.id}`,
                },
              ]}
            />
          )
        }
      />
    </Page>
  );
};

export default CreateReleasePage;
