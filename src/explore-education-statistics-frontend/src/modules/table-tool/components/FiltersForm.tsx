import Button from '@common/components/Button';
import { Form, FormFieldset, FormGroup } from '@common/components/form';
import FormFieldCheckboxSearchSubGroups from '@common/components/form/FormFieldCheckboxSearchSubGroups';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import Yup from '@common/lib/validation/yup';
import { PublicationSubjectMeta } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types/util';
import { Formik, FormikProps } from 'formik';
import camelCase from 'lodash/camelCase';
import mapValues from 'lodash/mapValues';
import React, { useRef, useState } from 'react';
import FormFieldCheckboxGroupsMenu from './FormFieldCheckboxGroupsMenu';
import { InjectedWizardProps } from './Wizard';
import WizardStepHeading from './WizardStepHeading';

export interface FormValues {
  indicators: string[];
  filters: {
    [key: string]: string[];
  };
}

export type FilterFormSubmitHandler = (values: FormValues) => void;

interface Props {
  specification: PublicationSubjectMeta;
  onSubmit: FilterFormSubmitHandler;
}

const FiltersForm = (props: Props & InjectedWizardProps) => {
  const { onSubmit, specification, goToNextStep, goToPreviousStep } = props;

  const [submitError, setSubmitError] = useState('');

  const ref = useRef<HTMLDivElement>(null);

  const stepHeading = (
    <WizardStepHeading {...props}>Choose your filters</WizardStepHeading>
  );

  return (
    <Formik<FormValues>
      initialValues={{
        filters: mapValues(specification.filters, () => []),
        indicators: [],
      }}
      validationSchema={Yup.object<FormValues>({
        filters: Yup.mixed().test(
          'required',
          'Must select options from only two categories',
          (value: Dictionary<string[]>) =>
            Object.values(value)
              .map(group => group.length > 0)
              .filter(Boolean).length === 2,
        ),
        indicators: Yup.array()
          .of(Yup.string())
          .required('Must select at least one indicator'),
      })}
      onSubmit={async (values, actions) => {
        try {
          await onSubmit(values);

          goToNextStep();
        } catch (error) {
          setSubmitError('Could not submit filters. Please try again later.');
        }

        actions.setSubmitting(false);
      }}
      render={(form: FormikProps<FormValues>) => {
        const { getError } = createErrorHelper(form);

        return (
          <div ref={ref}>
            <Form
              {...form}
              id="filtersForm"
              otherErrors={
                submitError !== ''
                  ? [
                      {
                        id: 'filtersForm-submit',
                        message: submitError,
                      },
                    ]
                  : []
              }
            >
              {stepHeading}

              <FormGroup>
                <div className="govuk-grid-row">
                  <div className="govuk-grid-column-one-half-from-desktop">
                    <FormFieldset
                      id="filtersForm-filters"
                      legend="Categories"
                      legendSize="s"
                      hint="Select options from two categories"
                      error={getError('filters')}
                    >
                      {Object.entries(specification.filters).map(
                        ([filterKey, filterGroup]) => {
                          const filterName = `filters.${filterKey}`;

                          return (
                            <FormFieldCheckboxGroupsMenu<FormValues>
                              key={filterKey}
                              name={filterName}
                              id={`filtersForm-${camelCase(filterName)}`}
                              legend={filterGroup.legend}
                              hint={filterGroup.hint}
                              error={getError(filterName)}
                              selectAll
                              options={Object.entries(filterGroup.options).map(
                                ([_, group]) => {
                                  return {
                                    legend: group.label,
                                    options: group.options,
                                  };
                                },
                              )}
                            />
                          );
                        },
                      )}
                    </FormFieldset>
                  </div>
                  <div className="govuk-grid-column-one-half-from-desktop">
                    <FormFieldCheckboxSearchSubGroups
                      name="indicators"
                      id="filtersForm-indicators"
                      legend="Indicators"
                      legendSize="s"
                      hint="Select at least one indicator"
                      error={getError('indicators')}
                      options={Object.entries(specification.indicators).map(
                        ([_, group]) => {
                          return {
                            legend: group.label,
                            options: group.options,
                          };
                        },
                      )}
                    />
                  </div>
                </div>
              </FormGroup>

              <FormGroup>
                <Button
                  disabled={form.isSubmitting}
                  id="filtersForm-submit"
                  onClick={event => {
                    event.preventDefault();

                    // Manually validate/submit so we can scroll
                    // back to the top of the form if there are errors
                    form.validateForm().then(validationErrors => {
                      form.submitForm();

                      if (
                        Object.keys(validationErrors).length > 0 &&
                        ref.current
                      ) {
                        ref.current.scrollIntoView({
                          behavior: 'smooth',
                          block: 'start',
                        });
                      }
                    });
                  }}
                  type="submit"
                >
                  {form.isSubmitting && form.isValid
                    ? 'Creating table...'
                    : 'Create table'}
                </Button>

                <Button
                  type="button"
                  variant="secondary"
                  onClick={() => {
                    goToPreviousStep();
                  }}
                >
                  Previous step
                </Button>
              </FormGroup>
            </Form>
          </div>
        );
      }}
    />
  );
};

export default FiltersForm;
