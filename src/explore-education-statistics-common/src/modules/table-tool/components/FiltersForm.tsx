import CollapsibleList from '@common/components/CollapsibleList';
import Details from '@common/components/Details';
import {
  Form,
  FormFieldCheckboxSearchSubGroups,
  FormFieldset,
  FormGroup,
} from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import FormCheckboxSelectedCount from '@common/modules/table-tool/components/FormCheckboxSelectedCount';
import { SubjectMeta } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import createErrorHelper from '@common/validation/createErrorHelper';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import mapValues from 'lodash/mapValues';
import React, { useMemo } from 'react';
import FormFieldCheckboxGroupsMenu from './FormFieldCheckboxGroupsMenu';
import ResetFormOnPreviousStep from './ResetFormOnPreviousStep';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

export interface FormValues {
  indicators: string[];
  filters: Dictionary<string[]>;
}

export type FilterFormSubmitHandler = (values: FormValues) => void;

interface Props {
  initialValues?: {
    indicators: string[];
    filters: string[];
  };
  subjectMeta: SubjectMeta;
  onSubmit: FilterFormSubmitHandler;
}

const formId = 'filtersForm';

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

  const initialFormValues = useMemo(() => {
    return {
      filters: mapValues(subjectMeta.filters, filter => {
        if (initialValues?.filters) {
          const filterValues = Object.values(filter.options)
            .flatMap(group => group.options)
            .map(option => option.value);

          return filterValues.filter(filterValue =>
            initialValues.filters.includes(filterValue),
          );
        }

        if (filter.options.Default) {
          // Automatically select filter option when there is only one
          return filter.options.Default.options.length === 1
            ? [filter.options.Default.options[0].value]
            : [];
        }

        return [];
      }),
      indicators: initialValues?.indicators ?? [],
    };
  }, [initialValues, subjectMeta]);

  const stepHeading = (
    <WizardStepHeading {...props}>Choose your filters</WizardStepHeading>
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={initialFormValues}
      validateOnBlur={false}
      validationSchema={Yup.object<FormValues>({
        indicators: Yup.array()
          .of(Yup.string())
          .required('Select at least one option from indicators'),
        filters: Yup.object(
          mapValues(subjectMeta.filters, filter =>
            Yup.array()
              .of(Yup.string())
              .min(
                1,
                `Select at least one option from ${filter.legend.toLowerCase()}`,
              ),
          ),
        ),
      })}
      onSubmit={async submittedValues => {
        await onSubmit(submittedValues);
        goToNextStep();
      }}
    >
      {form => {
        const { getError } = createErrorHelper(form);

        if (isActive) {
          return (
            <>
              <Form {...form} id={formId} showSubmitError>
                {stepHeading}

                <FormGroup>
                  <div className="govuk-grid-row">
                    <div className="govuk-grid-column-one-half-from-desktop">
                      <FormFieldCheckboxSearchSubGroups
                        name="indicators"
                        legend={
                          <>
                            Indicators
                            <FormCheckboxSelectedCount name="indicators" />
                          </>
                        }
                        legendSize="m"
                        hint="Select at least one indicator below"
                        disabled={form.isSubmitting}
                        order={[]}
                        options={Object.values(subjectMeta.indicators).map(
                          group => ({
                            legend: group.label,
                            options: group.options,
                          }),
                        )}
                      />

                      {Object.entries(subjectMeta.filters).length > 0 && (
                        <FormFieldset
                          id="filters"
                          legend="Categories"
                          legendSize="m"
                          hint="Select at least one option from all categories"
                          error={getError('filters')}
                        >
                          {Object.entries(subjectMeta.filters).map(
                            ([filterKey, filterGroup]) => {
                              const filterName = `filters.${filterKey}`;

                              return (
                                <FormFieldCheckboxGroupsMenu
                                  key={filterKey}
                                  name={filterName}
                                  legend={filterGroup.legend}
                                  hint={filterGroup.hint}
                                  disabled={form.isSubmitting}
                                  options={Object.values(
                                    filterGroup.options,
                                  ).map(group => ({
                                    legend: group.label,
                                    options: group.options,
                                  }))}
                                />
                              );
                            },
                          )}
                        </FormFieldset>
                      )}
                    </div>
                  </div>
                </FormGroup>

                <WizardStepFormActions
                  {...props}
                  submitText="Create table"
                  submittingText="Creating table"
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
                />
              </Form>
              <h2 className="govuk-heading-m govuk-!-margin-top-9">
                View or edit previous step
              </h2>
            </>
          );
        }

        return (
          <>
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-one-quarter">
                {stepHeading}

                <ResetFormOnPreviousStep
                  currentStep={currentStep}
                  stepNumber={stepNumber}
                />
              </div>
              <div className="govuk-grid-column-three-quarters">
                <Details
                  summary="View details"
                  className="govuk-!-margin-bottom-2"
                >
                  <SummaryList noBorder>
                    <SummaryListItem term="Indicators">
                      <CollapsibleList>
                        {Object.values(subjectMeta.indicators)
                          .flatMap(group => group.options)
                          .filter(indicator =>
                            form.values.indicators.includes(indicator.value),
                          )
                          .map(indicator => (
                            <li key={indicator.value}>{indicator.label}</li>
                          ))}
                      </CollapsibleList>
                    </SummaryListItem>

                    {Object.entries(subjectMeta.filters)
                      .filter(([groupKey]) => !!form.values.filters[groupKey])
                      .map(([filterGroupKey, filterGroup]) => (
                        <SummaryListItem
                          key={filterGroupKey}
                          term={filterGroup.legend}
                        >
                          <CollapsibleList>
                            {Object.values(filterGroup.options)
                              .flatMap(group => group.options)
                              .filter(option =>
                                form.values.filters[filterGroupKey].includes(
                                  option.value,
                                ),
                              )
                              .map(option => (
                                <li key={option.value}>{option.label}</li>
                              ))}
                          </CollapsibleList>
                        </SummaryListItem>
                      ))}
                  </SummaryList>
                </Details>
              </div>
            </div>
          </>
        );
      }}
    </Formik>
  );
};

export default FiltersForm;
