import { Form, FormFieldCheckboxGroup } from '@common/components/form';
import { CheckboxOption } from '@common/components/form/FormCheckboxGroup';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import { FileInfo } from '@common/services/types/file';
import Yup from '@common/validation/yup';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Details from '@common/components/Details';
import { Subject } from '@common/services/tableBuilderService';
import ContentHtml from '@common/components/ContentHtml';
import Tag from '@common/components/Tag';
import React, { useMemo } from 'react';
import { Formik } from 'formik';

interface DownloadFormValues {
  files: string[];
}
export type DownloadFormSubmitHandler = (values: { files: string[] }) => void;

export interface SubjectWithDownloadFiles extends Subject {
  downloadFile: FileInfo;
}

interface Props {
  isLatestRelease: boolean;
  subjects: SubjectWithDownloadFiles[];
  initialValues?: { files: string[] };
  onSubmit: DownloadFormSubmitHandler;
}

const DownloadStep = ({
  isLatestRelease,
  subjects,
  initialValues = { files: [] },
  onSubmit,
  ...stepProps
}: Props & InjectedWizardProps) => {
  const { isActive, currentStep, stepNumber } = stepProps;

  const stepEnabled = currentStep > stepNumber;
  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading stepEnabled={stepEnabled}>
      <span className="dfe-flex dfe-align-items--center">
        Choose files to download{' '}
        {isLatestRelease && (
          <Tag strong className="govuk-!-margin-left-4">
            This is the latest data
          </Tag>
        )}
        {!isLatestRelease && (
          <Tag strong colour="orange" className="govuk-!-margin-left-4">
            This is not the latest data
          </Tag>
        )}
      </span>
    </WizardStepHeading>
  );

  const getTimePeriod = (subject: Subject) => {
    const { from, to } = subject.timePeriods;

    if (from && to) {
      return from === to ? from : `${from} to ${to}`;
    }

    return from || to;
  };

  const checkboxOptions = useMemo<CheckboxOption[]>(
    () =>
      subjects.map(subject => {
        const { content } = subject;
        const geographicLevels = [...subject.geographicLevels]
          .sort()
          .join('; ');
        const timePeriod = getTimePeriod(subject);
        const hasDetails = content || geographicLevels || timePeriod;

        return {
          label: `${subject.name} (${subject.downloadFile.extension}, ${subject.downloadFile.size})`,
          value: subject.downloadFile.id,
          hint: hasDetails ? (
            <Details summary="More details" className="govuk-!-margin-bottom-2">
              <h4>This subject includes the following data:</h4>
              <SummaryList>
                {geographicLevels && (
                  <SummaryListItem term="Geographic levels">
                    {geographicLevels}
                  </SummaryListItem>
                )}

                {timePeriod && (
                  <SummaryListItem term="Time period">
                    {timePeriod}
                  </SummaryListItem>
                )}

                {content && (
                  <SummaryListItem term="Content">
                    <ContentHtml html={content} />
                  </SummaryListItem>
                )}
              </SummaryList>
            </Details>
          ) : null,
        };
      }),
    [subjects],
  );

  return (
    <Formik<DownloadFormValues>
      enableReinitialize
      initialValues={initialValues}
      validateOnBlur={false}
      validationSchema={Yup.object<DownloadFormValues>({
        files: Yup.array().of(Yup.string()).required('Choose a file'),
      })}
      onSubmit={async ({ files }) => {
        await onSubmit({ files });
      }}
    >
      {form => {
        return isActive ? (
          <Form {...form} id="downloadForm" showSubmitError>
            {checkboxOptions.length > 0 ? (
              <>
                <FormFieldCheckboxGroup<DownloadFormValues>
                  name="files"
                  legend={stepHeading}
                  disabled={form.isSubmitting}
                  options={checkboxOptions}
                />

                <WizardStepFormActions
                  submitText="Download selected files"
                  submittingText="Downloading"
                  {...stepProps}
                />
              </>
            ) : (
              <p>No downloads available.</p>
            )}
          </Form>
        ) : (
          <ResetFormOnPreviousStep
            currentStep={currentStep}
            stepNumber={stepNumber}
          />
        );
      }}
    </Formik>
  );
};

export default DownloadStep;
