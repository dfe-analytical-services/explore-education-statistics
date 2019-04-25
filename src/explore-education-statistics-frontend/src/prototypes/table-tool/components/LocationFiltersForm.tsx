import Button from '@common/components/Button';
import { FormGroup } from '@common/components/form';
import FormFieldCheckboxMenu from '@frontend/prototypes/table-tool/components/FormFieldCheckboxMenu';
import { MetaSpecification } from '@frontend/prototypes/table-tool/components/meta/initialSpec';
import { Form, Formik } from 'formik';
import React from 'react';

interface FormValues {
  [level: string]: string[];
}

interface Props {
  specification: MetaSpecification;
  onSubmit: (values: FormValues) => void;
}

const LocationFiltersForm = ({ specification, onSubmit }: Props) => {
  const { locations } = specification;

  return (
    <Formik<FormValues>
      onSubmit={onSubmit}
      initialValues={Object.keys(locations).reduce((acc, level) => {
        return {
          ...acc,
          [level]: [],
        };
      }, {})}
      render={() => {
        return (
          <Form>
            <FormGroup>
              {Object.entries(locations).map(([levelKey, level]) => {
                return (
                  <FormFieldCheckboxMenu
                    summary={level.legend}
                    name={levelKey}
                    key={levelKey}
                    options={level.options}
                    id={`filter-location-${levelKey}`}
                    legend="Choose options"
                    legendHidden
                  />
                );
              })}
            </FormGroup>

            <FormGroup>
              <Button type="submit">Submit</Button>
            </FormGroup>
          </Form>
        );
      }}
    />
  );
};

export default LocationFiltersForm;
