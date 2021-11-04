import CollapsibleList from '@common/components/CollapsibleList';
import {
  Form,
  FormFieldCheckboxSearchSubGroups,
  FormFieldset,
  FormGroup,
} from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useToggle from '@common/hooks/useToggle';
import FormCheckboxSelectedCount from '@common/modules/table-tool/components/FormCheckboxSelectedCount';
import FormFieldCheckboxGroupsMenu from '@common/modules/table-tool/components/FormFieldCheckboxGroupsMenu';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import TableSizeError from '@common/modules/table-tool/components/TableSizeError';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepEditButton from '@common/modules/table-tool/components/WizardStepEditButton';
import {
  SelectedPublication,
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import createErrorHelper from '@common/validation/createErrorHelper';
import { isServerValidationError } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import isEqual from 'lodash/isEqual';
import mapValues from 'lodash/mapValues';
import React, { useMemo, useState } from 'react';

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
  selectedPublication?: SelectedPublication;
  subject: Subject;
  subjectMeta: SubjectMeta;
  showTableSizeErrorDownload?: boolean;
  onTableSizeError?: (publicationTitle: string, subjectName: string) => void;
  onSubmit: FilterFormSubmitHandler;
}

const formId = 'filtersForm';

const FiltersForm = (props: Props & InjectedWizardProps) => {
  const {
    onSubmit,
    selectedPublication,
    subject,
    subjectMeta,
    goToNextStep,
    currentStep,
    stepNumber,
    initialValues,
    isActive,
    showTableSizeErrorDownload = true,
    onTableSizeError,
  } = props;

  const [hasTableSizeError, toggleTableSizeError] = useToggle(false);
  const [previousValues, setPreviousValues] = useState<FormValues>();

  const initialFormValues = useMemo(() => {
    // Automatically select indicator when one indicator group with one option
    const indicatorValues = Object.values(subjectMeta.indicators);
    const indicators =
      indicatorValues.length === 1 && indicatorValues[0].options.length === 1
        ? [indicatorValues[0].options[0].value]
        : initialValues?.indicators ?? [];

    const filters = mapValues(subjectMeta.filters, filter => {
      const filterGroups = Object.values(filter.options);

      // Automatically select when only one group in filter, with only one option in it.
      if (filterGroups.length === 1 && filterGroups[0].options.length === 1) {
        return [filterGroups[0].options[0].value];
      }

      if (initialValues?.filters) {
        const filterValues = filterGroups
          .flatMap(group => group.options)
          .map(option => option.value);

        return filterValues.filter(filterValue =>
          initialValues.filters.includes(filterValue),
        );
      }

      return [];
    });

    return {
      filters,
      indicators,
    };
  }, [initialValues, subjectMeta]);

  const stepEnabled = currentStep > stepNumber;
  const stepHeading = (
    <WizardStepHeading {...props} stepEnabled={stepEnabled}>
      Choose your filters
    </WizardStepHeading>
  );

  const handleSubmit = async (values: FormValues) => {
    setPreviousValues(values);
    try {
      await onSubmit(values);
      toggleTableSizeError.off();
      goToNextStep();
    } catch (error) {
      if (isServerValidationError(error) && error.response?.data) {
        const errors = Object.values(error.response?.data.errors);
        if (errors.flat().includes('QUERY_EXCEEDS_MAX_ALLOWABLE_TABLE_SIZE')) {
          if (onTableSizeError) {
            onTableSizeError(
              selectedPublication?.title || '',
              subject?.name || '',
            );
          }
          toggleTableSizeError.on();
        }
      }
    }
  };

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
      onSubmit={handleSubmit}
    >
      {form => {
        const { getError } = createErrorHelper(form);

        if (isActive) {
          return (
            <Form {...form} id={formId} showSubmitError>
              {hasTableSizeError &&
                form.submitCount > 0 &&
                isEqual(form.values, previousValues) && (
                  <TableSizeError
                    id={`${formId}-tableSizeError`}
                    releaseId={selectedPublication?.selectedRelease.id}
                    subject={subject}
                    showDownloadOption={showTableSizeErrorDownload}
                  />
                )}

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
                            const filterGroupOptions = Object.values(
                              filterGroup.options,
                            );

                            return (
                              <FormFieldCheckboxGroupsMenu
                                key={filterKey}
                                name={filterName}
                                legend={filterGroup.legend}
                                hint={filterGroup.hint}
                                disabled={form.isSubmitting}
                                order={[]}
                                options={filterGroupOptions.map(group => ({
                                  legend: group.label,
                                  options: group.options,
                                }))}
                                open={filterGroupOptions.length === 1}
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
          );
        }

        return (
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              {stepHeading}
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
            </div>
            <div className="govuk-grid-column-one-third dfe-align--right">
              {stepEnabled && (
                <WizardStepEditButton {...props} editTitle="Edit filters" />
              )}
              <ResetFormOnPreviousStep
                currentStep={currentStep}
                stepNumber={stepNumber}
              />
            </div>
          </div>
        );
      }}
    </Formik>
  );
};

export default FiltersForm;
