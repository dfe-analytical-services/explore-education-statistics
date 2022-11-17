import CollapsibleList from '@common/components/CollapsibleList';
import {
  Form,
  FormFieldCheckboxSearchSubGroups,
  FormFieldset,
  FormGroup,
} from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import FormCheckboxSelectedCount from '@common/modules/table-tool/components/FormCheckboxSelectedCount';
import FormFieldCheckboxGroupsMenu from '@common/modules/table-tool/components/FormFieldCheckboxGroupsMenu';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import TableQueryError from '@common/modules/table-tool/components/TableQueryError';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import {
  SelectedPublication,
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import createErrorHelper from '@common/validation/createErrorHelper';
import {
  getErrorMessage,
  hasErrorMessage,
  isServerValidationError,
} from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import isEqual from 'lodash/isEqual';
import mapValues from 'lodash/mapValues';
import orderBy from 'lodash/orderBy';
import React, { useMemo, useState } from 'react';

export interface FormValues {
  indicators: string[];
  filters: Dictionary<string[]>;
}

export type FilterFormSubmitHandler = (values: FormValues) => void;

const formId = 'filtersForm';

const TableQueryErrorCodes = [
  'QueryExceedsMaxAllowableTableSize',
  'RequestCancelled',
] as const;

export type TableQueryErrorCode = typeof TableQueryErrorCodes[number];

interface Props extends InjectedWizardProps {
  initialValues?: {
    indicators: string[];
    filters: string[];
  };
  selectedPublication?: SelectedPublication;
  showTableQueryErrorDownload?: boolean;
  subject: Subject;
  subjectMeta: SubjectMeta;
  onSubmit: FilterFormSubmitHandler;
  onTableQueryError?: (
    errorCode: TableQueryErrorCode,
    publicationTitle: string,
    subjectName: string,
  ) => void;
}

const FiltersForm = ({
  initialValues,
  selectedPublication,
  subject,
  subjectMeta,
  showTableQueryErrorDownload = true,
  onSubmit,
  onTableQueryError,
  ...stepProps
}: Props) => {
  const { goToNextStep, isActive } = stepProps;

  const [tableQueryError, setTableQueryError] = useState<TableQueryErrorCode>();
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

  const stepHeading = (
    <WizardStepHeading {...stepProps}>Choose your filters</WizardStepHeading>
  );

  const handleSubmit = async (values: FormValues) => {
    setPreviousValues(values);

    try {
      setTableQueryError(undefined);

      await goToNextStep(async () => {
        await onSubmit(values);
      });
    } catch (error) {
      if (
        !isServerValidationError<TableQueryErrorCode>(error) ||
        !hasErrorMessage(error, TableQueryErrorCodes)
      ) {
        throw error;
      }

      const errorCode = getErrorMessage(error);

      if (onTableQueryError) {
        if (errorCode) {
          onTableQueryError(
            errorCode,
            selectedPublication?.title || '',
            subject?.name || '',
          );
        }
      }

      setTableQueryError(errorCode);
    }
  };

  const orderedFilters = orderBy(
    Object.entries(subjectMeta.filters),
    ([_, value]) => value.order,
  );

  const orderedIndicators = orderBy(
    Object.values(subjectMeta.indicators),
    'order',
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
      onSubmit={handleSubmit}
    >
      {form => {
        const { getError } = createErrorHelper(form);

        if (isActive) {
          return (
            <Form {...form} id={formId} showSubmitError>
              {tableQueryError &&
                form.submitCount > 0 &&
                isEqual(form.values, previousValues) && (
                  <TableQueryError
                    id={`${formId}-tableQueryError`}
                    errorCode={tableQueryError}
                    releaseId={selectedPublication?.selectedRelease.id}
                    showDownloadOption={showTableQueryErrorDownload}
                    subject={subject}
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
                      options={orderedIndicators.map(group => ({
                        legend: group.label,
                        options: group.options,
                      }))}
                    />

                    {orderedFilters.length > 0 && (
                      <FormFieldset
                        id="filters"
                        legend="Categories"
                        legendSize="m"
                        hint="Select at least one option from all categories"
                        error={getError('filters')}
                      >
                        {orderedFilters.map(([filterKey, filterGroup]) => {
                          const filterName = `filters.${filterKey}`;
                          const orderedFilterGroupOptions = orderBy(
                            Object.values(filterGroup.options),
                            'order',
                          );

                          return (
                            <FormFieldCheckboxGroupsMenu
                              key={filterKey}
                              name={filterName}
                              legend={filterGroup.legend}
                              hint={filterGroup.hint}
                              disabled={form.isSubmitting}
                              order={[]}
                              options={orderedFilterGroupOptions.map(group => ({
                                legend: group.label,
                                options: group.options,
                              }))}
                              open={orderedFilterGroupOptions.length === 1}
                            />
                          );
                        })}
                      </FormFieldset>
                    )}
                  </div>
                </div>
              </FormGroup>

              <WizardStepFormActions
                {...stepProps}
                submitText="Create table"
                submittingText="Creating table"
                onSubmit={() => {
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
          <WizardStepSummary {...stepProps} goToButtonText="Edit filters">
            {stepHeading}

            <SummaryList noBorder>
              <SummaryListItem term="Indicators">
                <CollapsibleList
                  id="indicatorsList"
                  itemName="indicator"
                  itemNamePlural="indicators"
                >
                  {orderedIndicators
                    .flatMap(group => group.options)
                    .filter(indicator =>
                      form.values.indicators.includes(indicator.value),
                    )
                    .map(indicator => (
                      <li key={indicator.value}>{indicator.label}</li>
                    ))}
                </CollapsibleList>
              </SummaryListItem>

              {orderedFilters
                .filter(([groupKey]) => !!form.values.filters[groupKey])
                .map(([filterGroupKey, filterGroup]) => (
                  <SummaryListItem
                    key={filterGroupKey}
                    term={filterGroup.legend}
                  >
                    <CollapsibleList
                      id={`filtersList-${filterGroupKey}`}
                      itemName="filter"
                      itemNamePlural="filters"
                    >
                      {orderBy(Object.values(filterGroup.options), 'order')
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

            <ResetFormOnPreviousStep {...stepProps} />
          </WizardStepSummary>
        );
      }}
    </Formik>
  );
};

export default FiltersForm;
