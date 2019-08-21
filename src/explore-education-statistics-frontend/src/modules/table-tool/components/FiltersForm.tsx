import {
  Form,
  FormFieldCheckboxSearchSubGroups,
  FormFieldset,
  FormGroup,
  Formik,
} from '@common/components/form';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import Yup from '@common/lib/validation/yup';
import { PublicationSubjectMeta } from '@frontend/services/tableBuilderService';
import useResetFormOnPreviousStep from '@frontend/modules/table-tool/components/hooks/useResetFormOnPreviousStep';
import { FormikProps } from 'formik';
import camelCase from 'lodash/camelCase';
import mapValues from 'lodash/mapValues';
import React, { useRef } from 'react';
import FormFieldCheckboxGroupsMenu from './FormFieldCheckboxGroupsMenu';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

export interface FormValues {
  indicators: string[];
  filters: {
    [key: string]: string[];
  };
}

export type FilterFormSubmitHandler = (values: FormValues) => void;

interface Props {
  subjectMeta: PublicationSubjectMeta;
  onSubmit: FilterFormSubmitHandler;
}

const FiltersForm = (props: Props & InjectedWizardProps) => {
  const {
    onSubmit,
    subjectMeta,
    goToNextStep,
    currentStep,
    stepNumber,
  } = props;

  const ref = useRef<HTMLDivElement>(null);

  const formikRef = useRef<Formik<FormValues>>(null);
  const formId = 'filtersForm';

  useResetFormOnPreviousStep(formikRef, currentStep, stepNumber);

  const stepHeading = (
    <WizardStepHeading {...props}>Choose your filters</WizardStepHeading>
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      ref={formikRef}
      initialValues={{
        filters: mapValues(subjectMeta.filters, filter => {
          if (typeof filter.options.Default !== 'undefined') {
            // Automatically select filter option when there is only one
            return filter.options.Default.options.length === 1
              ? [filter.options.Default.options[0].value]
              : [];
          }
          return [];
        }),
        indicators: [],
      }}
      validationSchema={Yup.object<FormValues>({
        indicators: Yup.array()
          .of(Yup.string())
          .required('Select at least one indicator'),
        filters: Yup.object(
          mapValues(subjectMeta.filters, filter =>
            Yup.array()
              .of(Yup.string())
              .min(
                1,
                `Select at least one option under ${filter.legend.toLowerCase()}`,
              ),
          ),
        ),
      })}
      onSubmit={async values => {
        await onSubmit(values);
        goToNextStep();
      }}
      render={(form: FormikProps<FormValues>) => {
        const { getError } = createErrorHelper(form);

        return (
          <div ref={ref}>
            <Form {...form} id={formId}>
              {stepHeading}

              <FormGroup>
                <div className="govuk-grid-row">
                  <div className="govuk-grid-column-one-half-from-desktop">
                    <FormFieldCheckboxSearchSubGroups
                      name="indicators"
                      id={`${formId}-indicators`}
                      legend="Indicators"
                      legendSize="s"
                      hint="Select at least one indicator"
                      error={getError('indicators')}
                      selectAll
                      options={Object.entries(subjectMeta.indicators).map(
                        ([_, group]) => {
                          return {
                            legend: group.label,
                            options: group.options,
                          };
                        },
                      )}
                    />

                    <FormFieldset
                      id={`${formId}-filters`}
                      legend="Categories"
                      legendSize="s"
                      hint="Select at least one option from all categories"
                      error={getError('filters')}
                    >
                      {Object.entries(subjectMeta.filters).map(
                        ([filterKey, filterGroup]) => {
                          const filterName = `filters.${filterKey}`;

                          return (
                            <FormFieldCheckboxGroupsMenu<FormValues>
                              key={filterKey}
                              name={filterName}
                              id={`${formId}-${camelCase(filterName)}`}
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
                </div>
              </FormGroup>

              <WizardStepFormActions
                {...props}
                form={form}
                formId={formId}
                onSubmitClick={() => {
                  // Automatically select totalValue for filters that haven't had a selection made
                  Object.keys(form.values.filters).forEach(filterName => {
                    if (
                      form.values.filters[filterName].length === 0 &&
                      subjectMeta.filters[filterName].totalValue
                    ) {
                      form.setFieldValue(`filters.${filterName}`, [
                        subjectMeta.filters[filterName].totalValue,
                      ]);
                    }
                  });
                }}
                submitText="Create table"
                submittingText="Creating table"
              />
            </Form>
          </div>
        );
      }}
    />
  );
};

export default FiltersForm;
