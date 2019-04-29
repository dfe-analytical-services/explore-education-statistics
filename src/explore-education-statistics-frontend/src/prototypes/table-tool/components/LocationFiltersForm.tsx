import Button from '@common/components/Button';
import { Form, FormGroup } from '@common/components/form';
import FormFieldCheckboxMenu from '@frontend/prototypes/table-tool/components/FormFieldCheckboxMenu';
import { MetaSpecification } from '@frontend/prototypes/table-tool/components/meta/initialSpec';
import { InjectedWizardProps } from '@frontend/prototypes/table-tool/components/Wizard';
import { Formik, FormikProps } from 'formik';
import React from 'react';

interface FormValues {
  [level: string]: string[];
}

interface Props {
  specification: MetaSpecification;
  onSubmit: (values: FormValues) => void;
}

const LocationFiltersForm = ({
  specification,
  onSubmit,
  goToNextStep,
  goToPreviousStep,
}: Props & InjectedWizardProps) => {
  const { locations } = specification;

  return (
    <Formik<FormValues>
      onSubmit={values => {
        onSubmit(values);
        goToNextStep();
      }}
      initialValues={Object.keys(locations).reduce((acc, level) => {
        return {
          ...acc,
          [level]: [],
        };
      }, {})}
      render={(form: FormikProps<FormValues>) => {
        return (
          <Form {...form} id="locationFiltersForm">
            <FormGroup>
              {Object.entries(locations).map(([levelKey, level]) => {
                return (
                  <FormFieldCheckboxMenu
                    summary={level.legend}
                    name={levelKey}
                    key={levelKey}
                    options={level.options}
                    id={`locationFiltersForm-${levelKey}`}
                    legend="Choose options"
                    legendHidden
                  />
                );
              })}
            </FormGroup>

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

export default LocationFiltersForm;
