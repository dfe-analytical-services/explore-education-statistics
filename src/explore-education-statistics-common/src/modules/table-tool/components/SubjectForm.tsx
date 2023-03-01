import Details from '@common/components/Details';
import { Form, FormFieldRadioGroup } from '@common/components/form';
import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import ContentHtml from '@common/components/ContentHtml';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import { Subject } from '@common/services/tableBuilderService';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { createElement, ReactNode, useMemo } from 'react';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';

export interface SubjectFormValues {
  subjectId: string;
}

export type SubjectFormSubmitHandler = (values: { subjectId: string }) => void;

const formId = 'publicationSubjectForm';

interface Props extends InjectedWizardProps {
  hasFeaturedTables?: boolean;
  legend?: ReactNode;
  legendSize?: FormFieldsetProps['legendSize'];
  legendHint?: string;
  initialValues?: { subjectId: string };
  options: Subject[];
  onSubmit: SubjectFormSubmitHandler;
}

const SubjectForm = ({
  hasFeaturedTables = false,
  legend,
  legendSize = 'l',
  legendHint,
  initialValues = {
    subjectId: '',
  },
  options,
  onSubmit,
  ...stepProps
}: Props) => {
  const { goToNextStep, isActive, currentStep, stepNumber } = stepProps;

  const getTimePeriod = (subject: Subject) => {
    const { from, to } = subject.timePeriods;

    if (from && to) {
      return from === to ? from : `${from} to ${to}`;
    }

    return from || to;
  };

  const radioOptions = useMemo<RadioOption[]>(
    () =>
      options.map(option => {
        const { content } = option;
        const geographicLevels = [...option.geographicLevels].sort().join('; ');

        const timePeriod = getTimePeriod(option);

        const hasDetails = content || geographicLevels || timePeriod;

        return {
          label: option.name,
          value: option.id,
          hint: hasDetails && (
            <Details
              summary="More details"
              className="govuk-!-margin-bottom-2"
              hiddenText={`for ${option.name}`}
            >
              {createElement(
                hasFeaturedTables ? 'h4' : 'h3',
                hasFeaturedTables
                  ? null
                  : {
                      className: 'govuk-heading-s',
                    },
                'This subject includes the following data:',
              )}

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
          ),
        };
      }),
    [hasFeaturedTables, options],
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
        await goToNextStep(async () => {
          await onSubmit({ subjectId });
        });
      }}
    >
      {form => {
        return isActive ? (
          <Form {...form} id={formId} showSubmitError>
            <FormFieldRadioGroup<SubjectFormValues>
              name="subjectId"
              order={[]}
              legend={legend}
              legendSize={legendSize}
              hint={legendHint}
              disabled={form.isSubmitting}
              options={radioOptions}
            />

            {radioOptions.length > 0 ? (
              <WizardStepFormActions
                {...stepProps}
                isSubmitting={form.isSubmitting}
              />
            ) : (
              <p>No subjects available.</p>
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

export default SubjectForm;
