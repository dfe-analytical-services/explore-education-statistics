import {
  Form,
  FormFieldCheckboxSearchSubGroups,
  FormFieldset,
  FormGroup,
  Formik,
} from '@common/components/form';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import Yup from '@common/lib/validation/yup';
import {
  PublicationSubjectMeta,
  FilterOption,
} from '@common/modules/full-table/services/tableBuilderService';
import useResetFormOnPreviousStep from '@common/modules/table-tool/components/hooks/useResetFormOnPreviousStep';
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

interface InitialValuesType {
  indicators: string[];
  filters: string[];
}

export type FilterFormSubmitHandler = (values: FormValues) => void;

interface Props {
  subjectMeta: PublicationSubjectMeta;
  onSubmit: FilterFormSubmitHandler;
  initialValues?: InitialValuesType;
}

export const buildInitialFormValue = (
  meta: PublicationSubjectMeta,
  initial?: InitialValuesType,
) => {
  return {
    filters: mapValues(meta.filters, filter => {
      if (initial && initial.filters) {
        const allFilterOptions = ([] as FilterOption[]).concat(
          ...Object.values(filter.options).map(({ options }) => options),
        );

        const allFilterValues = allFilterOptions.map(({ value }) => value);

        return allFilterValues.filter(value => initial.filters.includes(value));
      }

      if (typeof filter.options.Default !== 'undefined') {
        // Automatically select filter option when there is only one
        return filter.options.Default.options.length === 1
          ? [filter.options.Default.options[0].value]
          : [];
      }
      return [];
    }),
    indicators: (initial && initial.indicators) || [],
  };
};

const FiltersForm = (props: Props & InjectedWizardProps) => {
  const {
    onSubmit,
    subjectMeta,
    goToNextStep,
    currentStep,
    stepNumber,
    initialValues,
    isActive,
  } = props;

  const ref = useRef<HTMLDivElement>(null);

  const formikRef = useRef<Formik<FormValues>>(null);
  const formId = 'filtersForm';

  useResetFormOnPreviousStep(formikRef, currentStep, stepNumber);

  const stepHeading = (
    <WizardStepHeading {...props}>Choose your filters</WizardStepHeading>
  );

  const [initialFormValue, setInitialFormValue] = React.useState(() =>
    buildInitialFormValue(subjectMeta, initialValues),
  );

  React.useEffect(() => {
    if (formikRef.current) {
      const newFormValue = buildInitialFormValue(subjectMeta, initialValues);
      setInitialFormValue(newFormValue);
      formikRef.current.setValues(newFormValue);
    }
  }, [initialValues, subjectMeta, subjectMeta.filters]);

  return (
    <Formik<FormValues>
      enableReinitialize
      ref={formikRef}
      initialValues={initialFormValue}
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
      onSubmit={async submittedValues => {
        await onSubmit(submittedValues);
        goToNextStep();
      }}
      render={(form: FormikProps<FormValues>) => {
        const { getError } = createErrorHelper(form);

        return isActive ? (
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
                      legendSize="m"
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
        ) : (
          <>
            {stepHeading}
            {
              // <SummaryList noBorder>
              //   <SummaryListItem term="Indicators" shouldCollapse>
              //     {form.values.indicators.map(indicator => (
              //       <div key={indicator}>
              //         {subjectMeta.indicators[indicator]}
              //       </div>
              //     ))}
              //   </SummaryListItem>
              //   {Object.entries(form.values.filters).map(
              //     ([filterGroupId, filterItemIds]) => (
              //       <SummaryListItem term={filterGroupId} shouldCollapse>
              //         {filterItemIds.map(filterItemId => (
              //           <div key={filterItemId}>{filterItemId}</div>
              //         ))}
              //       </SummaryListItem>
              //     ),
              //   )}
              // </SummaryList>
            }
          </>
        );
      }}
    />
  );
};

export default FiltersForm;
