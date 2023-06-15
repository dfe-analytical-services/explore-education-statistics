import {
  Form,
  FormFieldCheckboxGroup,
  FormFieldset,
} from '@common/components/form';
import { CheckboxOption } from '@common/components/form/FormCheckboxGroup';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import { ReleaseSummary } from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useFormSubmit from '@common/hooks/useFormSubmit';
import Details from '@common/components/Details';
import { Subject } from '@common/services/tableBuilderService';
import ContentHtml from '@common/components/ContentHtml';
import Tag from '@common/components/Tag';
import useMounted from '@common/hooks/useMounted';
import React, { useMemo } from 'react';
import { Formik } from 'formik';

interface DownloadFormValues {
  files: string[];
}
export type DownloadFormSubmitHandler = (values: { files: string[] }) => void;

interface Props {
  release?: ReleaseSummary;
  subjects: Subject[];
  initialValues?: { files: string[] };
  onSubmit: DownloadFormSubmitHandler;
  hideLatestDataTag?: boolean;
}

const DownloadStep = ({
  release,
  subjects,
  initialValues = { files: [] },
  onSubmit,
  hideLatestDataTag,
  ...stepProps
}: Props & InjectedWizardProps) => {
  const { isActive, currentStep, stepNumber } = stepProps;
  const { isMounted } = useMounted();

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
      <span
        className="dfe-flex dfe-align-items--center"
        data-testid="choose-files"
      >
        Choose files to download{' '}
        {release && release.latestRelease && !hideLatestDataTag ? (
          <Tag
            strong
            className="govuk-!-margin-left-4"
            data-testid="latest-data-tag"
          >
            This is the latest data
          </Tag>
        ) : (
          <Tag
            strong
            colour="orange"
            className="govuk-!-margin-left-4"
            data-testid="not-latest-data-tag"
          >
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
          label: `${subject.name} (${subject.file.extension}, ${subject.file.size})`,
          value: subject.file.id,
          hint: hasDetails ? (
            <Details
              summary="More details"
              className="govuk-!-margin-bottom-2"
              hiddenText={`for ${subject.name}`}
            >
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

  const handleSubmit = useFormSubmit<DownloadFormValues>(onSubmit);

  return (
    <Formik<DownloadFormValues>
      enableReinitialize
      initialValues={initialValues}
      validateOnBlur={false}
      // validationSchema={Yup.object<DownloadFormValues>({
      //   files: Yup.array().required('Choose a file'),
      // })}

      validationSchema={Yup.object<DownloadFormValues>({
        files: Yup.array()
          .of(Yup.string())
          .min(1, 'Choose a file')
          .required('Choose a file'),
      })}
      onSubmit={handleSubmit}
    >
      {form => {
        // isMounted check required as Formik context can be undefined
        // if the step is active on page load.
        return isActive && isMounted ? (
          <Form id="downloadForm" showSubmitError>
            <FormFieldset id="downloadFiles" legend={stepHeading}>
              {checkboxOptions.length > 0 && (
                <FormFieldCheckboxGroup<DownloadFormValues>
                  name="files"
                  order={[]}
                  legend="Choose files from the list below"
                  legendHidden
                  disabled={form.isSubmitting}
                  selectAll
                  options={checkboxOptions}
                />
              )}
            </FormFieldset>

            {checkboxOptions.length > 0 ? (
              <WizardStepFormActions
                {...stepProps}
                isSubmitting={form.isSubmitting}
                submitText="Download selected files"
                submittingText="Downloading"
              />
            ) : (
              <p>No downloads available.</p>
            )}
          </Form>
        ) : (
          <ResetFormOnPreviousStep
            currentStep={currentStep}
            stepNumber={stepNumber}
            onReset={form.resetForm}
          />
        );
      }}
    </Formik>
  );
};

export default DownloadStep;
