import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import ReleaseSummaryForm, {
  ReleaseSummaryFormValues,
} from '@admin/pages/release/components/ReleaseSummaryForm';
import appRouteList from '@admin/routes/dashboard/routes';
import { summaryRoute } from '@admin/routes/releaseRoutes';
import publicationService from '@admin/services/publicationService';
import releaseService, {
  CreateReleaseRequest,
} from '@admin/services/releaseService';
import { IdTitlePair } from '@admin/services/types/common';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import {
  errorCodeAndFieldNameToFieldError,
  errorCodeToFieldError,
} from '@common/components/form/util/serverValidationHandler';
import RelatedInformation from '@common/components/RelatedInformation';
import { Publication } from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import { formatISO } from 'date-fns';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

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

interface MatchProps {
  publicationId: string;
}

export type CreateReleaseFormValues = {
  templateReleaseId: string;
} & ReleaseSummaryFormValues;

interface Model {
  templateRelease: IdTitlePair | undefined;
  publication: Publication;
}

const ReleaseCreatePage = ({
  match,
  history,
}: RouteComponentProps<MatchProps>) => {
  const { publicationId } = match.params;

  const [model, setModel] = useState<Model>();

  useEffect(() => {
    Promise.all([
      publicationService.getPublicationReleaseTemplate(publicationId),
      publicationService.getPublication(publicationId),
    ]).then(([templateRelease, publication]) => {
      setModel({
        templateRelease,
        publication: publication as Publication,
      });
    });
  }, [publicationId]);

  const handleSubmit = useFormSubmit<CreateReleaseFormValues>(async values => {
    const release: CreateReleaseRequest = {
      timePeriodCoverage: {
        value: values.timePeriodCoverageCode,
      },
      releaseName: parseInt(values.timePeriodCoverageStartYear, 10),
      publishScheduled: formatISO(values.scheduledPublishDate, {
        representation: 'date',
      }),
      nextReleaseDate: values.nextReleaseDate,
      typeId: values.releaseTypeId,
      publicationId,
      templateReleaseId:
        values.templateReleaseId !== 'new' ? values.templateReleaseId : '',
    };

    const createdRelease = await releaseService.createRelease(release);

    history.push(
      summaryRoute.generateLink({
        publicationId,
        releaseId: createdRelease.id,
      }),
    );
  }, errorCodeMappings);

  const handleCancel = () =>
    history.push(appRouteList.adminDashboard.path as string);

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
      <ReleaseSummaryForm<CreateReleaseFormValues>
        submitText="Create new release"
        initialValues={timePeriodCoverageGroups =>
          ({
            timePeriodCoverageCode:
              timePeriodCoverageGroups[0].timeIdentifiers[0].identifier.value,
            timePeriodCoverageStartYear: '',
            releaseTypeId: '',
            templateReleaseId: '',
          } as CreateReleaseFormValues)
        }
        validationSchema={baseRules =>
          baseRules.shape({
            templateReleaseId:
              model && model.templateRelease
                ? Yup.string().required('Choose a template')
                : Yup.string(),
          })
        }
        onSubmit={handleSubmit}
        onCancel={handleCancel}
        additionalFields={
          model &&
          model.templateRelease && (
            <div className="govuk-!-margin-top-9">
              <FormFieldRadioGroup<CreateReleaseFormValues>
                id="releaseSummaryForm-templateReleaseId"
                legend="Select template"
                name="templateReleaseId"
                options={[
                  {
                    label: 'Create new template',
                    value: 'new',
                  },
                  {
                    label: `Copy existing template (${model.templateRelease.title})`,
                    value: `${model.templateRelease.id}`,
                  },
                ]}
              />
            </div>
          )
        }
      />
    </Page>
  );
};

export default withRouter(ReleaseCreatePage);
