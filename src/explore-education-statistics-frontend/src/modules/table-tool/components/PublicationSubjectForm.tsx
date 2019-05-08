import Button from '@common/components/Button';
import { Form, FormFieldRadioGroup, FormGroup } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { InjectedWizardProps } from '@frontend/modules/table-tool/components/Wizard';
import WizardStepHeading from '@frontend/modules/table-tool/components/WizardStepHeading';
import { Formik, FormikProps } from 'formik';
import React, { useState } from 'react';

export interface PublicationSubjectMenuOption {
  value: string;
  label: string;
}

interface FormValues {
  publicationSubjectId: string;
}

interface Props {
  onSubmit: (values: {
    publicationSubjectId: string;
    publicationSubjectName: string;
  }) => void;
  options: PublicationSubjectMenuOption[];
}

const PublicationSubjectForm = (props: Props & InjectedWizardProps) => {
  const { isActive, onSubmit, options, goToNextStep, goToPreviousStep } = props;
  const [publicationSubjectName, setPublicationSubjectName] = useState('');

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
      Choose a subject
    </WizardStepHeading>
  );

  return (
    <Formik
      onSubmit={({ publicationSubjectId }) => {
        onSubmit({
          publicationSubjectId,
          publicationSubjectName,
        });
        goToNextStep();
      }}
      initialValues={{
        publicationSubjectId: '',
      }}
      validationSchema={Yup.object<FormValues>({
        publicationSubjectId: Yup.string().required(
          'Choose a publication subject',
        ),
      })}
      render={(form: FormikProps<FormValues>) => {
        return isActive ? (
          <Form {...form} id="publicationSubjectForm">
            <FormFieldRadioGroup
              name="publicationSubjectId"
              legend={stepHeading}
              legendSize="l"
              options={options.map(option => ({
                id: option.value,
                label: option.label,
                value: option.value,
              }))}
              id="publicationSubjectForm-publicationSubjectId"
              onChange={(event, option) => {
                setPublicationSubjectName(option.label);
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
              <SummaryListItem term="Subject">
                {publicationSubjectName}
              </SummaryListItem>
            </SummaryList>
          </>
        );
      }}
    />
  );
};

export default PublicationSubjectForm;
