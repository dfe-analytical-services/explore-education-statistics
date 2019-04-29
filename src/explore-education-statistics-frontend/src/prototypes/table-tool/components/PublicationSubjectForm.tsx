import Button from '@common/components/Button';
import { Form, FormFieldRadioGroup, FormGroup } from '@common/components/form';
import Yup from '@common/lib/validation/yup';
import { InjectedWizardProps } from '@frontend/prototypes/table-tool/components/Wizard';
import { Formik, FormikProps } from 'formik';
import React from 'react';

export interface PublicationSubjectMenuOption {
  value: string;
  label: string;
}

interface FormValues {
  publicationSubject: string;
}

interface Props {
  onSubmit: (values: FormValues) => void;
  options: PublicationSubjectMenuOption[];
}

const PublicationSubjectForm = ({
  onSubmit,
  options,
  goToNextStep,
  goToPreviousStep,
}: Props & InjectedWizardProps) => {
  return (
    <Formik
      onSubmit={values => {
        onSubmit(values);
        goToNextStep();
      }}
      initialValues={{
        publicationSubject: '',
      }}
      validationSchema={Yup.object<FormValues>({
        publicationSubject: Yup.string().required(
          'Choose a publication subject',
        ),
      })}
      render={(form: FormikProps<FormValues>) => {
        return (
          <Form {...form} id="publicationSubjectForm">
            <FormFieldRadioGroup
              name="publicationSubject"
              legend="Choose publication"
              legendHidden
              options={options.map(option => ({
                id: option.value,
                label: option.label,
                value: option.value,
              }))}
              id="publicationSubject"
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
        );
      }}
    />
  );
};

export default PublicationSubjectForm;
