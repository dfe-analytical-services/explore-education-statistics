import Button from '@common/components/Button';
import { Form, FormGroup } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import Yup from '@common/lib/validation/yup';
import { Formik, FormikProps } from 'formik';
import camelCase from 'lodash/camelCase';
import mapValues from 'lodash/mapValues';
import React, { Component, createRef } from 'react';
import FormFieldCheckboxGroupsMenu from './FormFieldCheckboxGroupsMenu';
import FormFieldCheckboxMenu from './FormFieldCheckboxMenu';
import { MetaSpecification } from './meta/initialSpec';
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
  specification: MetaSpecification;
  onSubmit: FilterFormSubmitHandler;
}

interface State {
  submitError: string;
}

class FiltersForm extends Component<Props & InjectedWizardProps, State> {
  public state: State = {
    submitError: '',
  };

  private ref = createRef<HTMLDivElement>();

  public render() {
    const {
      isActive,
      onSubmit,
      specification,
      goToNextStep,
      goToPreviousStep,
    } = this.props;

    const { submitError } = this.state;

    const stepHeading = (
      <WizardStepHeading {...this.props}>Choose your filters</WizardStepHeading>
    );

    return (
      <Formik<FormValues>
        initialValues={{
          filters: mapValues(specification.filters, () => []),
          indicators: [],
        }}
        validationSchema={Yup.object<FormValues>({
          filters: Yup.object(
            mapValues(specification.filters, () =>
              Yup.array()
                .of(Yup.string())
                .required('Select at least one option'),
            ),
          ),
          indicators: Yup.array()
            .of(Yup.string())
            .required('Select at least one option'),
        })}
        onSubmit={async (values, actions) => {
          try {
            await onSubmit(values);

            goToNextStep();
          } catch (error) {
            this.setState({
              submitError: 'Could not submit filters. Please try again later.',
            });
          }

          actions.setSubmitting(false);
        }}
        render={(form: FormikProps<FormValues>) => {
          const { getError } = createErrorHelper(form);

          return isActive ? (
            <div ref={this.ref}>
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
                  {Object.entries(specification.filters).map(
                    ([filterKey, filterSpec]) => {
                      const filterName = `filters.${filterKey}`;

                      return (
                        <div key={filterKey}>
                          {Object.keys(filterSpec.options).length === 1 ? (
                            <FormFieldCheckboxMenu<FormValues>
                              id={`filtersForm-${camelCase(filterName)}`}
                              name={filterName}
                              legend={filterSpec.legend}
                              hint={filterSpec.hint}
                              error={getError(filterName)}
                              selectAll
                              options={filterSpec.options.default.options.map(
                                option => ({
                                  id: `${filterKey}-${option.value}`,
                                  label: option.label,
                                  value: option.value,
                                }),
                              )}
                            />
                          ) : (
                            <FormFieldCheckboxGroupsMenu<FormValues>
                              name={filterName}
                              id={`filtersForm-${camelCase(filterName)}`}
                              legend={filterSpec.legend}
                              hint={filterSpec.hint}
                              error={getError(filterName)}
                              selectAll
                              options={Object.entries(filterSpec.options).map(
                                ([_, group]) => {
                                  return {
                                    legend: group.label,
                                    options: group.options,
                                  };
                                },
                              )}
                            />
                          )}
                        </div>
                      );
                    },
                  )}

                  <FormFieldCheckboxGroupsMenu
                    name="indicators"
                    id="filtersForm-indicators"
                    legend="Indicators"
                    hint="Filter by at least one statistical indicator from the publication"
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
                          this.ref.current
                        ) {
                          this.ref.current.scrollIntoView({
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
          ) : (
            <>
              {stepHeading}
              <SummaryList noBorder>
                <SummaryListItem term="Publication">Test</SummaryListItem>
              </SummaryList>
            </>
          );
        }}
      />
    );
  }
}

export default FiltersForm;
