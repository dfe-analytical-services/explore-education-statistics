import { Form, FormFieldRadioGroup, Formik } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { PublicationSubject } from '@common/modules/full-table/services/tableBuilderService';
import useResetFormOnPreviousStep from '@common/modules/table-tool/components/hooks/useResetFormOnPreviousStep';
import { FormikProps } from 'formik';
import React, { useRef, useState } from 'react';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

interface FormValues {
  subjectId: string;
}

export type PublicationSubjectFormSubmitHandler = (values: {
  subjectId: string;
  subjectName: string;
}) => void;

interface Props {
  onSubmit: PublicationSubjectFormSubmitHandler;
  options: PublicationSubject[];
  initialValues?: { subjectId? : string };
}
const initialiseSubjectName = (
  sid: string,
  options: PublicationSubject[],
): string => (options.find(({ id }) => sid === id) || { label: '' }).label;

const PublicationSubjectForm = (props: Props & InjectedWizardProps) => {
  const {
    isActive,
    onSubmit,
    options,
    goToNextStep,
    currentStep,
    stepNumber,
    initialValues : { subjectId: initialSubjectId = ''  } = {},
  } = props;

  const [subjectName, setSubjectName] = useState(() =>
    initialiseSubjectName(initialSubjectId, options),
  );

  const formikRef = useRef<Formik<FormValues>>(null);
  const formId = 'publicationSubjectForm';

  useResetFormOnPreviousStep(formikRef, currentStep, stepNumber, () => {
    setSubjectName('');
  });

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
      Choose a subject
    </WizardStepHeading>
  );

  const initialValues = {
    subjectId: initialSubjectId,
  };

  React.useEffect(() => {
    if (formikRef.current) {
      formikRef.current.setValues({
        subjectId: `${initialSubjectId}`,
      });
    }
    setSubjectName(initialiseSubjectName(initialSubjectId, options));
    // eslint-disable-next-line
  }, [options, initialSubjectId]);

  return (
    <Formik<FormValues>
      enableReinitialize
      ref={formikRef}
      onSubmit={async ({ subjectId }) => {
        await onSubmit({
          subjectId,
          subjectName,
        });
        goToNextStep();
      }}
      initialValues={initialValues}
      validationSchema={Yup.object<FormValues>({
        subjectId: Yup.string().required('Choose a subject'),
      })}
      render={(form: FormikProps<FormValues>) => {
        return isActive ? (
          <Form {...form} id={formId}>
            <FormFieldRadioGroup<FormValues>
              name="subjectId"
              legend={stepHeading}
              legendSize="l"
              options={options.map(option => ({
                label: option.label,
                value: `${option.id}`,
              }))}
              id={`${formId}-subjectId`}
              onChange={(event, option) => {
                setSubjectName(option.label);
              }}
            />

            <WizardStepFormActions {...props} form={form} formId={formId} />
          </Form>
        ) : (
          <>
            {stepHeading}
            <SummaryList noBorder>
              <SummaryListItem term="Subject">{subjectName}</SummaryListItem>
            </SummaryList>
          </>
        );
      }}
    />
  );
};

export default PublicationSubjectForm;
