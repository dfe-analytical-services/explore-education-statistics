import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import CollapsibleList from '@common/components/CollapsibleList';
import { FormFieldset } from '@common/components/form';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormCheckboxSelectedCount from '@common/components/form/rhf/RHFFormCheckboxSelectedCount';
import RHFFormFieldCheckboxSearchSubGroups from '@common/components/form/rhf/RHFFormFieldCheckboxSearchSubGroups';
import RHFFormFieldCheckboxGroupsMenu from '@common/components/form/rhf/RHFFormFieldCheckboxGroupsMenu';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
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
import createRHFErrorHelper from '@common/components/form/rhf/validation/createRHFErrorHelper';
import {
  getErrorMessage,
  hasErrorMessage,
  isServerValidationError,
} from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
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
  const [openFilterGroups, setOpenFilterGroups] = useState<string[]>([]);

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

  const handleSubmitForm = async (values: FormValues) => {
    setPreviousValues({ ...values });

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

  const allFilterKeys = orderedFilters.map(([filterKey]) => filterKey);

  const allFiltersOpen = openFilterGroups.length === allFilterKeys.length;

  const orderedIndicators = orderBy(
    Object.values(subjectMeta.indicators),
    'order',
  );

  const validationSchema = useMemo(
    () =>
      Yup.object<FormValues>({
        indicators: Yup.array()
          .of(Yup.string())
          .required('Select at least one option from indicators'),
        filters: Yup.object(
          mapValues(subjectMeta.filters, filter =>
            Yup.array()
              .typeError(
                `Select at least one option from ${filter.legend.toLowerCase()}`,
              )
              .of(Yup.string())
              .min(
                1,
                `Select at least one option from ${filter.legend.toLowerCase()}`,
              ),
          ),
        ),
      }),
    [subjectMeta.filters],
  );

  return (
    <FormProvider
      enableReinitialize
      initialValues={initialFormValues}
      validationSchema={validationSchema}
    >
      {({ formState, getValues, reset, setValue }) => {
        const { getError } = createRHFErrorHelper({
          errors: formState.errors,
          touchedFields: formState.touchedFields,
        });
        if (isActive) {
          return (
            <RHFForm id={formId} showSubmitError onSubmit={handleSubmitForm}>
              {tableQueryError && formState.submitCount > 0 && (
                <TableQueryError
                  id={`${formId}-tableQueryError`}
                  errorCode={tableQueryError}
                  releaseId={selectedPublication?.selectedRelease.id}
                  showDownloadOption={showTableQueryErrorDownload}
                  subject={subject}
                  previousValues={previousValues}
                />
              )}

              {stepHeading}

              <div className="govuk-grid-row">
                <div className="govuk-grid-column-one-half-from-desktop">
                  <RHFFormFieldCheckboxSearchSubGroups
                    disabled={formState.isSubmitting}
                    hint="Select at least one indicator below"
                    legend={
                      <>
                        Indicators
                        <RHFFormCheckboxSelectedCount name="indicators" />
                      </>
                    }
                    legendSize="m"
                    name="indicators"
                    options={orderedIndicators.map(group => ({
                      legend: group.label,
                      options: group.options,
                    }))}
                    order={[]}
                  />
                  {orderedFilters.length > 0 && (
                    <FormFieldset
                      error={getError('filters')}
                      hint={
                        <div className="dfe-flex dfe-justify-content--space-between dfe-flex-wrap">
                          <span className="govuk-!-margin-bottom-2">
                            Select at least one option from all categories
                          </span>
                          {orderedFilters.length > 1 && (
                            <ButtonText
                              ariaExpanded={allFiltersOpen}
                              ariaControls="filterGroups"
                              className="govuk-!-margin-bottom-2"
                              onClick={() => {
                                setOpenFilterGroups(
                                  allFiltersOpen ? [] : allFilterKeys,
                                );
                              }}
                            >
                              {allFiltersOpen ? 'Collapse all' : 'Expand all'}
                              <VisuallyHidden> categories</VisuallyHidden>
                            </ButtonText>
                          )}
                        </div>
                      }
                      id="filters"
                      legend="Categories"
                      legendSize="m"
                    >
                      <div id="filterGroups">
                        {orderedFilters.map(([filterKey, filterGroup]) => {
                          const filterName = `filters.${filterKey}`;
                          const orderedFilterGroupOptions = orderBy(
                            Object.values(filterGroup.options),
                            'order',
                          );
                          return (
                            <RHFFormFieldCheckboxGroupsMenu
                              disabled={formState.isSubmitting}
                              hint={filterGroup.hint}
                              id={`${formId}-${filterName}`}
                              key={filterKey}
                              legend={filterGroup.legend}
                              name={filterName}
                              open={openFilterGroups.includes(filterKey)}
                              options={orderedFilterGroupOptions.map(group => ({
                                legend: group.label,
                                options: group.options,
                              }))}
                              order={[]}
                              onToggle={isOpen => {
                                setOpenFilterGroups(groups =>
                                  isOpen
                                    ? [...groups, filterKey]
                                    : groups.filter(
                                        group => group !== filterKey,
                                      ),
                                );
                              }}
                            />
                          );
                        })}
                      </div>
                    </FormFieldset>
                  )}
                </div>
              </div>

              <WizardStepFormActions
                {...stepProps}
                isSubmitting={formState.isSubmitting}
                submitText="Create table"
                submittingText="Creating table"
                onSubmit={() => {
                  const filterValues = getValues('filters');
                  // Automatically select totalValue for filters that haven't had a selection made
                  Object.keys(filterValues).forEach(filterName => {
                    if (
                      (!filterValues[filterName] ||
                        filterValues[filterName].length === 0) &&
                      subjectMeta.filters[filterName].totalValue
                    ) {
                      setValue(`filters.${filterName}`, [
                        subjectMeta.filters[filterName].totalValue as string,
                      ]);
                    }
                  });
                }}
              />
            </RHFForm>
          );
        }

        const values = getValues();

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
                      values.indicators.includes(indicator.value),
                    )
                    .map(indicator => (
                      <li key={indicator.value}>{indicator.label}</li>
                    ))}
                </CollapsibleList>
              </SummaryListItem>

              {orderedFilters
                .filter(([groupKey]) => !!values.filters[groupKey])
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
                          values.filters[filterGroupKey].includes(option.value),
                        )
                        .map(option => (
                          <li key={option.value}>{option.label}</li>
                        ))}
                    </CollapsibleList>
                  </SummaryListItem>
                ))}
            </SummaryList>

            <ResetFormOnPreviousStep {...stepProps} onReset={reset} />
          </WizardStepSummary>
        );
      }}
    </FormProvider>
  );
};

export default FiltersForm;
