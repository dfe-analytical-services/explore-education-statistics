import { SelectedRelease } from '@common/modules/table-tool/types/selectedPublication';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldRadioGroup from '@common/components/form/rhf/RHFFormFieldRadioGroup';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import DataSetStepContent from '@common/modules/table-tool/components/DataSetStepContent';
import { Subject, FeaturedTable } from '@common/services/tableBuilderService';
import Yup from '@common/validation/yup';
import Details from '@common/components/Details';
import DataSetDetailsList from '@common/modules/table-tool/components/DataSetDetailsList';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { ReactNode } from 'react';
import orderBy from 'lodash/orderBy';

const formId = 'publicationDataStepForm';

export type DataSetFormSubmitHandler = (values: { subjectId: string }) => void;

export interface DataSetFormValues {
  subjectId: string;
}

interface Props extends InjectedWizardProps {
  featuredTables?: FeaturedTable[];
  loadingFastTrack?: boolean;
  release?: SelectedRelease;
  subjects: Subject[];
  subjectId?: string;
  renderFeaturedTableLink?: (featuredTable: FeaturedTable) => ReactNode;
  onSubmit: DataSetFormSubmitHandler;
}

export default function DataSetStep({
  featuredTables = [],
  loadingFastTrack = false,
  release,
  subjects,
  subjectId = '',
  renderFeaturedTableLink,
  onSubmit,
  ...stepProps
}: Props) {
  const { goToNextStep, isActive } = stepProps;

  const stepHeading = (
    <WizardStepHeading
      {...stepProps}
      fieldsetHeading={!renderFeaturedTableLink}
    >
      {featuredTables.length > 0
        ? 'Select a data set or featured table'
        : 'Select a data set'}
    </WizardStepHeading>
  );

  const renderLegend = () => {
    if (renderFeaturedTableLink) {
      return featuredTables?.length
        ? 'View all featured tables or select a data set'
        : 'Select a data set';
    }

    return stepHeading;
  };

  const options = subjects.map(subject => ({
    label: subject.name,
    value: subject.id,
    hint: !renderFeaturedTableLink && (
      <Details
        summary="More details"
        className="govuk-!-margin-bottom-2"
        hiddenText={`for ${subject.name}`}
      >
        <h3 className="govuk-heading-s">
          This subject includes the following data:
        </h3>
        <DataSetDetailsList subject={subject} />
      </Details>
    ),
  }));

  if (!subjects.length) {
    return (
      <>
        {stepHeading}
        <p>No data sets available.</p>
      </>
    );
  }

  if (isActive) {
    return (
      <FormProvider
        enableReinitialize
        initialValues={{
          subjectId,
        }}
        validationSchema={Yup.object<DataSetFormValues>({
          subjectId: Yup.string().ensure().required('Choose a data set'),
        })}
      >
        {({ formState }) => {
          return (
            <RHFForm
              id={formId}
              onSubmit={async ({ subjectId: id }) => {
                await goToNextStep(async () => {
                  await onSubmit({ subjectId: id });
                });
              }}
            >
              {renderFeaturedTableLink ? stepHeading : undefined}

              <LoadingSpinner loading={loadingFastTrack}>
                <div className="govuk-grid-row">
                  <div
                    className={
                      !renderFeaturedTableLink
                        ? 'govuk-grid-column-full'
                        : 'govuk-grid-column-one-third govuk-grid-column-one-quarter-from-desktop'
                    }
                  >
                    <RHFFormFieldRadioGroup
                      className="govuk-!-margin-bottom-2"
                      disabled={formState.isSubmitting}
                      legend={renderLegend()}
                      legendHidden={!!renderFeaturedTableLink}
                      name="subjectId"
                      order={[]}
                      options={
                        renderFeaturedTableLink && featuredTables.length
                          ? [
                              {
                                divider: 'or select a data set',
                                label: 'View all featured tables',
                                labelClassName: 'govuk-!-font-weight-bold',
                                value: 'all-featured',
                              },
                              ...options,
                            ]
                          : options
                      }
                      small={!!renderFeaturedTableLink}
                    />

                    {!renderFeaturedTableLink && (
                      <WizardStepFormActions
                        {...stepProps}
                        isSubmitting={formState.isSubmitting}
                      />
                    )}
                  </div>

                  {renderFeaturedTableLink && (
                    <div className="govuk-grid-column-two-thirds govuk-grid-column-three-quarters-from-desktop">
                      <DataSetStepContent
                        {...stepProps}
                        featuredTables={orderBy(featuredTables, 'order')}
                        isSubmitting={formState.isSubmitting}
                        release={release}
                        renderFeaturedTableLink={renderFeaturedTableLink}
                        subjects={subjects}
                      />
                    </div>
                  )}
                </div>
              </LoadingSpinner>
            </RHFForm>
          );
        }}
      </FormProvider>
    );
  }

  const subjectName =
    subjects.find(({ id }) => subjectId === id)?.name ?? 'None';

  return (
    <WizardStepSummary {...stepProps} goToButtonText="Change data set">
      {stepHeading}

      <SummaryList noBorder>
        <SummaryListItem term="Data set">{subjectName}</SummaryListItem>
      </SummaryList>
    </WizardStepSummary>
  );
}
