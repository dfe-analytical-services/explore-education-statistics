import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import ReleaseSummaryForm, {
  ReleaseSummaryFormValues,
} from '@admin/pages/release/components/ReleaseSummaryForm';
import appRouteList from '@admin/routes/dashboard/routes';
import { summaryRoute } from '@admin/routes/releaseRoutes';
import publicationService, {
  BasicPublicationDetails,
} from '@admin/services/publicationService';
import releaseService from '@admin/services/releaseService';
import { IdTitlePair } from '@admin/services/types/common';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import RelatedInformation from '@common/components/RelatedInformation';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

export interface FormValues extends ReleaseSummaryFormValues {
  templateReleaseId: string;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'timePeriodCoverageStartYear',
    messages: {
      SLUG_NOT_UNIQUE:
        'Choose a unique combination of time period and start year',
    },
  }),
];

interface MatchProps {
  publicationId: string;
}

interface Model {
  templateRelease: IdTitlePair;
  publication: BasicPublicationDetails;
}

const ReleaseCreatePage = ({
  match,
  history,
}: RouteComponentProps<MatchProps>) => {
  const { publicationId } = match.params;

  const { value: model } = useAsyncRetry<Model>(async () => {
    const [templateRelease, publication] = await Promise.all([
      publicationService.getPublicationReleaseTemplate(publicationId),
      publicationService.getPublication(publicationId),
    ]);

    return {
      templateRelease,
      publication,
    } as Model;
  }, [publicationId]);

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    const createdRelease = await releaseService.createRelease({
      timePeriodCoverage: {
        value: values.timePeriodCoverageCode,
      },
      releaseName: values.timePeriodCoverageStartYear,
      typeId: values.releaseTypeId,
      publicationId,
      templateReleaseId:
        values.templateReleaseId !== 'new' ? values.templateReleaseId : '',
    });

    history.push(
      summaryRoute.generateLink({
        publicationId,
        releaseId: createdRelease.id,
      }),
    );
  }, errorMappings);

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
          <PageTitle
            title="Create new release"
            caption={model?.publication.title}
          />
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
        submitText="Create new release"
        initialValues={timePeriodCoverageGroups =>
          ({
            timePeriodCoverageCode:
              timePeriodCoverageGroups[0]?.timeIdentifiers[0]?.identifier
                .value ?? '',
            timePeriodCoverageStartYear: '',
            releaseTypeId: '',
            templateReleaseId: '',
          } as FormValues)
        }
        validationSchema={baseRules =>
          baseRules.shape({
            templateReleaseId: model?.templateRelease
              ? Yup.string().required('Choose a template')
              : Yup.string(),
          })
        }
        onSubmit={handleSubmit}
        onCancel={handleCancel}
        additionalFields={
          model?.templateRelease && (
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
                  label: `Copy existing template (${model.templateRelease.title})`,
                  value: `${model.templateRelease.id}`,
                },
              ]}
            />
          )
        }
      />
    </Page>
  );
};

export default withRouter(ReleaseCreatePage);
