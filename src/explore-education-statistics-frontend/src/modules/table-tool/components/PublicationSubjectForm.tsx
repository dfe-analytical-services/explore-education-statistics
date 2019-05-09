import Button from '@common/components/Button';
import { Form, FormFieldRadioGroup, FormGroup } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { PublicationSubject } from '@common/services/tableBuilderService';
import { InjectedWizardProps } from '@frontend/modules/table-tool/components/Wizard';
import WizardStepHeading from '@frontend/modules/table-tool/components/WizardStepHeading';
import { Formik, FormikProps } from 'formik';
import React, { useState } from 'react';

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
}

const PublicationSubjectForm = (props: Props & InjectedWizardProps) => {
  const { isActive, onSubmit, options, goToNextStep, goToPreviousStep } = props;
  const [subjectName, setSubjectName] = useState('');

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
      Choose a subject
    </WizardStepHeading>
  );

  return (
    <Formik
      onSubmit={({ subjectId }) => {
        onSubmit({
          subjectId,
          subjectName,
        });
        goToNextStep();
      }}
      initialValues={{
        subjectId: '',
      }}
      validationSchema={Yup.object<FormValues>({
        subjectId: Yup.string().required('Choose a publication subject'),
      })}
      render={(form: FormikProps<FormValues>) => {
        return isActive ? (
          <Form {...form} id="publicationSubjectForm">
            <FormFieldRadioGroup<FormValues>
              name="subjectId"
              legend={stepHeading}
              options={options.map(option => ({
                label: option.label,
                value: `${option.id}`,
              }))}
              id="publicationSubjectForm-subjectId"
              onChange={(event, option) => {
                setSubjectName(option.label);
              }}
            />

            <FormGroup>
              <Button type="submit">Next step</Button>

              <Button
                type="button"
                variant="secondary"
                onClick={goToPreviousStep}
              >
                Previous step
              </Button>
            </FormGroup>
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
