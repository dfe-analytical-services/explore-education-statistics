import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import ReleaseSummaryForm, {
  EditFormValues,
} from '@admin/pages/release/summary/ReleaseSummaryForm';
import { assembleCreateReleaseRequestFromForm } from '@admin/pages/release/util/releaseSummaryUtil';
import dashboardRoutes from '@admin/routes/dashboard/routes';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import {
  IdTitlePair,
  TimePeriodCoverageGroup,
} from '@admin/services/common/types';
import service from '@admin/services/release/create-release/service';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import submitWithFormikValidation from '@admin/validation/formikSubmitHandler';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import {
  errorCodeAndFieldNameToFieldError,
  errorCodeToFieldError,
} from '@common/components/form/util/serverValidationHandler';
import RelatedInformation from '@common/components/RelatedInformation';
import Yup from '@common/lib/validation/yup';
import {
  emptyDayMonthYear,
  Publication,
} from '@common/services/publicationService';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';
import { ObjectSchemaDefinition } from 'yup';

interface MatchProps {
  publicationId: string;
}

export type FormValues = {
  templateReleaseId: string;
} & EditFormValues;

interface TemplateReleaseModel {
  templateRelease: IdTitlePair | undefined;
  publication: Publication;
}

const CreateReleasePage = ({
  match,
  history,
  handleApiErrors,
}: RouteComponentProps<MatchProps> & ErrorControlProps) => {
  const { publicationId } = match.params;

  const [model, setModel] = useState<TemplateReleaseModel>();

  useEffect(() => {
    Promise.all([
      service.getTemplateRelease(publicationId),
      service.getPublication(publicationId),
    ])
      .then(([templateRelease, publication]) => {
        setModel({
          templateRelease,
          publication,
        });
      })
      .catch(handleApiErrors);
  }, [publicationId, handleApiErrors]);

  const errorCodeMappings = [
    errorCodeToFieldError(
      'SLUG_NOT_UNIQUE',
      'timePeriodCoverageStartYear',
      'Choose a unique combination of time period and start year',
    ),
    errorCodeAndFieldNameToFieldError(
      'PARTIAL_DATE_NOT_VALID',
      'NextReleaseDate',
      'nextReleaseDate',
      'Enter a valid date',
    ),
  ];

  const submitFormHandler = submitWithFormikValidation<FormValues>(
    async values => {
      const createReleaseDetails: CreateReleaseRequest = assembleCreateReleaseRequestFromForm(
        publicationId,
        values,
      );

      const createdRelease = await service.createRelease(createReleaseDetails);
      history.push(summaryRoute.generateLink(publicationId, createdRelease.id));
    },
    handleApiErrors,
    ...errorCodeMappings,
  );

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
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">
            <span className="govuk-caption-xl">
              {model && model.publication.title}
            </span>
            Create new release
          </h1>
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/documentation/create-new-release" target="blank">
                  Creating a new release{' '}
                </Link>
              </li>
              <li>
                <Link to="/documentation/edit-release" target="blank">
                  Editing a release and updating release status{' '}
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>
      <ReleaseSummaryForm<FormValues>
        submitButtonText="Create new release"
        initialValuesSupplier={(
          timePeriodCoverageGroups: TimePeriodCoverageGroup[],
        ): FormValues => ({
          timePeriodCoverageCode:
            timePeriodCoverageGroups[0].timeIdentifiers[0].identifier.value,
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
          templateReleaseId:
            model && model.templateRelease
              ? Yup.string().required('Choose a template')
              : Yup.string(),
        })}
        onSubmitHandler={submitFormHandler}
        onCancelHandler={cancelHandler}
        additionalFields={
          model && (
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
                  label: `Copy existing template (${model.templateRelease &&
                    model.templateRelease.title})`,
                  value: `${model.templateRelease && model.templateRelease.id}`,
                },
              ]}
            />
          )
        }
      />
    </Page>
  );
};

export default withErrorControl(withRouter(CreateReleasePage));
