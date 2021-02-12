import Details from '@common/components/Details';
import { Form, FormFieldRadioGroup } from '@common/components/form';
import SanitizeHtml from '@common/components/SanitizeHtml';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import { Subject } from '@common/services/tableBuilderService';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useMemo } from 'react';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

export interface SubjectFormValues {
  subjectId: string;
}

export type SubjectFormSubmitHandler = (values: { subjectId: string }) => void;

const formId = 'publicationSubjectForm';

interface Props {
  initialValues?: { subjectId: string };
  onSubmit: SubjectFormSubmitHandler;
  options: Subject[];
}

const SubjectForm = (props: Props & InjectedWizardProps) => {
  const {
    isActive,
    onSubmit,
    options,
    goToNextStep,
    currentStep,
    stepNumber,
    initialValues = {
      subjectId: '',
    },
  } = props;

  const getSubjectName = (subjectId: string): string => {
    const matching = options.find(({ id }) => subjectId === id);
    return matching?.name ?? '';
  };

  const getTimePeriod = (subject: Subject) => {
    const { from, to } = subject.timePeriods;

    if (from && to) {
      return from === to ? from : `${from} to ${to}`;
    }

    return from || to;
  };

  const radioOptions = useMemo(
    () =>
      options.map(option => {
        const { content } = option;
        const geographicLevels = [...option.geographicLevels].sort().join('; ');

        const timePeriod = getTimePeriod(option);

        const hasDetails = content || geographicLevels || timePeriod;

        return {
          label: option.name,
          value: `${option.id}`,
          hint: hasDetails ? (
            <Details
              summary="More details"
              className="govuk-!-margin-bottom-2 govuk-!-margin-top-2"
            >
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
                    <SanitizeHtml dirtyHtml={content} />
                  </SummaryListItem>
                )}
              </SummaryList>
            </Details>
          ) : null,
        };
      }),
    [options],
  );

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
      Choose a subject
    </WizardStepHeading>
  );

  return (
    <Formik<SubjectFormValues>
      enableReinitialize
      initialValues={initialValues}
      validateOnBlur={false}
      validationSchema={Yup.object<SubjectFormValues>({
        subjectId: Yup.string().required('Choose a subject'),
      })}
      onSubmit={async ({ subjectId }) => {
        await onSubmit({
          subjectId,
        });
        goToNextStep();
      }}
    >
      {form => {
        return isActive ? (
          <Form {...form} id={formId} showSubmitError>
            <FormFieldRadioGroup<SubjectFormValues>
              name="subjectId"
              legend={stepHeading}
              legendSize="l"
              id={`${formId}-subjectId`}
              disabled={form.isSubmitting}
              options={radioOptions}
            />

            {radioOptions.length > 0 ? (
              <WizardStepFormActions {...props} formId={formId} />
            ) : (
              <p>No subjects available.</p>
            )}
          </Form>
        ) : (
          <>
            {stepHeading}

            <ResetFormOnPreviousStep
              currentStep={currentStep}
              stepNumber={stepNumber}
            />

            <SummaryList noBorder>
              <SummaryListItem term="Subject">
                {getSubjectName(form.values.subjectId) || 'None'}
              </SummaryListItem>
            </SummaryList>
          </>
        );
      }}
    </Formik>
  );
};

export default SubjectForm;
