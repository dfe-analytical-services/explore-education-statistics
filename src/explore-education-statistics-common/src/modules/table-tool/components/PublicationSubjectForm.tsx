import { Form, FormFieldRadioGroup, Formik } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useResetFormOnPreviousStep from '@common/modules/table-tool/components/hooks/useResetFormOnPreviousStep';
import { PublicationSubject } from '@common/modules/table-tool/services/tableBuilderService';
import Yup from '@common/validation/yup';
import { FormikProps } from 'formik';
import React, { useRef } from 'react';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

interface FormValues {
  subjectId: string;
}

export type PublicationSubjectFormSubmitHandler = (values: {
  subjectId: string;
}) => void;

const formId = 'publicationSubjectForm';

interface Props {
  initialValues?: { subjectId: string };
  onSubmit: PublicationSubjectFormSubmitHandler;
  options: PublicationSubject[];
}

const PublicationSubjectForm = (props: Props & InjectedWizardProps) => {
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

  const formikRef = useRef<Formik<FormValues>>(null);

  useResetFormOnPreviousStep(formikRef, currentStep, stepNumber);

  const getSubjectName = (subjectId: string): string => {
    const matching = options.find(({ id }) => subjectId === id);
    return matching?.label ?? '';
  };

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
      Choose a subject
    </WizardStepHeading>
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      ref={formikRef}
      initialValues={initialValues}
      validateOnBlur={false}
      validateOnChange={false}
      validationSchema={Yup.object<FormValues>({
        subjectId: Yup.string().required('Choose a subject'),
      })}
      onSubmit={async ({ subjectId }) => {
        await onSubmit({
          subjectId,
        });
        goToNextStep();
      }}
      render={(form: FormikProps<FormValues>) => {
        return isActive ? (
          <Form {...form} id={formId} showSubmitError>
            <FormFieldRadioGroup<FormValues>
              name="subjectId"
              legend={stepHeading}
              legendSize="l"
              options={options.map(option => ({
                label: option.label,
                value: `${option.id}`,
              }))}
              id={`${formId}-subjectId`}
              disabled={form.isSubmitting}
            />

            {options.length > 0 ? (
              <WizardStepFormActions {...props} form={form} formId={formId} />
            ) : (
              <p>No subjects available for this release.</p>
            )}
          </Form>
        ) : (
          <>
            {stepHeading}
            <SummaryList noBorder>
              <SummaryListItem term="Subject">
                {getSubjectName(form.values.subjectId)}
              </SummaryListItem>
            </SummaryList>
          </>
        );
      }}
    />
  );
};

export default PublicationSubjectForm;
