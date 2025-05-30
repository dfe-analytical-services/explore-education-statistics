import ButtonText from '@common/components/ButtonText';
import CollapsibleList from '@common/components/CollapsibleList';
import { FormFieldset } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import FormFieldCheckboxGroupsMenu from '@common/components/form/FormFieldCheckboxGroupsMenu';
import FormFieldCheckboxSearchSubGroups from '@common/components/form/FormFieldCheckboxSearchSubGroups';
import FormProvider from '@common/components/form/FormProvider';
import createErrorHelper from '@common/components/form/validation/createErrorHelper';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import VisuallyHidden from '@common/components/VisuallyHidden';
import styles from '@common/modules/table-tool/components/FiltersForm.module.scss';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import TableQueryError from '@common/modules/table-tool/components/TableQueryError';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import { SelectedPublication } from '@common/modules/table-tool/types/selectedPublication';
import { Subject, SubjectMeta } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import {
  getErrorCode,
  hasErrorMessage,
  isServerValidationError,
} from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import mapValues from 'lodash/mapValues';
import orderBy from 'lodash/orderBy';
import React, { useCallback, useMemo, useState } from 'react';
import { ObjectSchema } from 'yup';
import FilterHierarchy from './FilterHierarchy';
import getFilterHierarchyLabelsMap from './utils/getFilterHierarchyLabelsMap';
import augmentFilterHierarchySelections from './utils/augmentFilterHierarchySelections';

export interface FiltersFormValues {
  indicators: string[];
  filters: Dictionary<string[]>;
  filterHierarchies: Dictionary<string[]>;
}

export type FilterFormSubmitHandler = (
  values: Omit<FiltersFormValues, 'filterHierachies'>,
) => void;

const TableQueryErrorCodes = [
  'QueryExceedsMaxAllowableTableSize',
  'RequestCancelled',
] as const;

export type TableQueryErrorCode = (typeof TableQueryErrorCodes)[number];

interface Props extends InjectedWizardProps {
  initialValues?: {
    indicators: string[];
    filters: string[];
  };
  selectedPublication?: SelectedPublication;
  showTableQueryErrorDownload?: boolean;
  stepTitle: string;
  subject: Subject;
  subjectMeta: SubjectMeta;
  onSubmit: FilterFormSubmitHandler;
  onTableQueryError?: (
    errorCode: TableQueryErrorCode,
    publicationTitle: string,
    subjectName: string,
  ) => void;
}

export default function FiltersForm({
  initialValues,
  selectedPublication,
  stepTitle,
  subject,
  subjectMeta,
  showTableQueryErrorDownload = true,
  onSubmit,
  onTableQueryError,
  ...stepProps
}: Props) {
  const { goToNextStep, isActive } = stepProps;

  const [tableQueryError, setTableQueryError] = useState<TableQueryErrorCode>();
  const [previousValues, setPreviousValues] = useState<FiltersFormValues>();
  const [
    augmentedPreviousHierarchyValues,
    setAugmentedPreviousHierarchyValues,
  ] = useState<Dictionary<Dictionary<string[]>>>();

  const [openFilterGroups, setOpenFilterGroups] = useState<string[]>([]);

  const filterHierarchies = useMemo(() => {
    return subjectMeta.filterHierarchies ?? [];
  }, [subjectMeta]);

  const hierarchiedFilterIds = useMemo(() => {
    return (
      filterHierarchies.flatMap(hierarchy => {
        return [
          ...hierarchy.map(({ filterId }) => filterId),
          hierarchy.at(-1)?.childFilterId,
        ];
      }) ?? []
    );
  }, [filterHierarchies]);

  const standardFilters = useMemo(() => {
    return orderBy(
      Object.entries(subjectMeta.filters).filter(
        ([_, filter]) => !hierarchiedFilterIds.includes(filter.id),
      ),
      ([_, value]) => value.order,
    );
  }, [subjectMeta, hierarchiedFilterIds]);

  const hierarchiedFilters = useMemo(() => {
    return Object.entries(subjectMeta.filters).filter(([_, filter]) =>
      hierarchiedFilterIds.includes(filter.id),
    );
  }, [subjectMeta, hierarchiedFilterIds]);

  const optionLabelsMap = useMemo(
    () =>
      getFilterHierarchyLabelsMap(
        hierarchiedFilters.map(([_, filter]) => filter),
      ),
    [hierarchiedFilters],
  );

  const initialFormValues: FiltersFormValues = useMemo(() => {
    // Automatically select indicator when one indicator group with one option
    const indicatorValues = Object.values(subjectMeta.indicators);
    const indicators =
      indicatorValues.length === 1 && indicatorValues[0].options.length === 1
        ? [indicatorValues[0].options[0].value]
        : initialValues?.indicators ?? [];

    const filters = mapValues(Object.fromEntries(standardFilters), filter => {
      const filterGroupOptions = Object.values(filter.options);

      // Automatically select when only one group in filter, with only one option in it.
      if (
        filterGroupOptions.length === 1 &&
        filterGroupOptions[0].options.length === 1
      ) {
        return [filterGroupOptions[0].options[0].value];
      }

      if (initialValues?.filters) {
        const filterValues = filterGroupOptions
          .flatMap(group => group.options)
          .map(option => option.value);

        return filterValues.filter(filterValue =>
          initialValues.filters.includes(filterValue),
        );
      }

      return [];
    });

    const initialFilterHierarchyValues = Object.fromEntries(
      // Set up intitial value for each hierachy,
      // include initialValue filters if they're in the hierarchy
      filterHierarchies.map(tiers => {
        const hierachyKey = tiers.at(-1)!.childFilterId;

        const hierarchyOptionValues = tiers
          .flatMap(tier => Object.values(tier.hierarchy))
          .flat()
          .concat(Object.keys(tiers[0].hierarchy));

        return [
          hierachyKey,
          initialValues?.filters.filter(optionId =>
            hierarchyOptionValues.includes(optionId),
          ) ?? [],
        ];
      }),
    );

    return {
      filters,
      indicators,
      filterHierarchies: initialFilterHierarchyValues,
    };
  }, [initialValues, subjectMeta, standardFilters, filterHierarchies]);

  const stepHeading = (
    <WizardStepHeading {...stepProps}>{stepTitle}</WizardStepHeading>
  );

  const handleSubmit = useCallback(
    async (values: FiltersFormValues) => {
      setPreviousValues({ ...values });
      setAugmentedPreviousHierarchyValues(
        augmentFilterHierarchySelections(
          values.filterHierarchies,
          filterHierarchies,
          optionLabelsMap,
        ),
      );

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

        const errorCode = getErrorCode<TableQueryErrorCode>(error);

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
    },
    [
      filterHierarchies,
      goToNextStep,
      onSubmit,
      onTableQueryError,
      optionLabelsMap,
      selectedPublication?.title,
      subject?.name,
    ],
  );

  const allFilterGroupKeys = useMemo(
    () => [
      ...standardFilters.map(([key]) => key),
      ...(subjectMeta.filterHierarchies ?? []).map(
        tiers => tiers.at(-1)?.childFilterId ?? '',
      ),
    ],
    [standardFilters, subjectMeta],
  );

  const allFiltersOpen = openFilterGroups.length === allFilterGroupKeys.length;

  const orderedIndicators = orderBy(
    Object.values(subjectMeta.indicators),
    'order',
  );

  const validationSchema = useMemo<ObjectSchema<FiltersFormValues>>(() => {
    return Yup.object({
      indicators: Yup.array()
        .required('Select at least one option from indicators')
        .of(Yup.string().defined())
        .min(1, 'Select at least one option from indicators'),
      filters: Yup.object(
        mapValues(Object.fromEntries(standardFilters), filter => {
          const label = filter.legend.toLowerCase();

          return Yup.array()
            .required(`Select at least one option from ${label}`)
            .typeError(`Select at least one option from ${label}`)
            .of(Yup.string().defined())
            .min(1, `Select at least one option from ${label}`);
        }),
      ),
      filterHierarchies: Yup.object(
        Object.fromEntries(
          filterHierarchies.map(tiers => {
            const hierarchyFilterId = tiers.at(-1)?.childFilterId ?? '';

            const label =
              optionLabelsMap[hierarchyFilterId]?.toLocaleLowerCase();

            return [
              hierarchyFilterId,
              Yup.array()
                .required(`Select at least one option from ${label}`)
                .typeError(`Select at least one option from ${label}`)
                .of(Yup.string().defined())
                .min(1, `Select at least one option from ${label}`),
            ];
          }),
        ),
      ),
    });
  }, [standardFilters, filterHierarchies, optionLabelsMap]);

  const filtersIncludeTotal = Object.values(subjectMeta.filters).some(
    filter => filter.autoSelectFilterItemId,
  );

  return (
    <FormProvider
      enableReinitialize
      initialValues={initialFormValues}
      validationSchema={validationSchema}
    >
      {({ formState, getValues, reset, setValue }) => {
        const { getError } = createErrorHelper({
          errors: formState.errors,
          touchedFields: formState.touchedFields,
        });

        if (isActive) {
          return (
            <Form id="filtersForm" onSubmit={handleSubmit}>
              {tableQueryError && formState.submitCount > 0 && (
                <TableQueryError
                  errorCode={tableQueryError}
                  releaseVersionId={selectedPublication?.selectedRelease.id}
                  showDownloadOption={showTableQueryErrorDownload}
                  subject={subject}
                  previousValues={previousValues}
                />
              )}
              {stepHeading}
              <div>
                <div className="govuk-grid-column govuk-!-margin-bottom-6">
                  <FormFieldCheckboxSearchSubGroups
                    disabled={formState.isSubmitting}
                    groupLabel="Indicators"
                    hint="Select at least one indicator below"
                    legend={
                      <>
                        Indicators
                        <FormCheckboxSelectedCount name="indicators" />
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

                  {standardFilters.length + hierarchiedFilterIds.length > 0 && (
                    <FormFieldset
                      error={
                        getError('filters') ?? getError('filterHierarchies')
                      }
                      hint={
                        <div className="dfe-flex dfe-justify-content--space-between dfe-flex-wrap dfe-align-items-start">
                          <span
                            className={`govuk-!-margin-bottom-2 ${styles.hintText}`}
                          >
                            {`Select at least one option from all categories.
                            ${
                              filtersIncludeTotal
                                ? ` If no options are selected from a category then
                                a default option (often 'Total') may be selected automatically
                                when creating a table. Where present, the
                                'Total' option is usually an aggregate of all
                                other options within a category.`
                                : ''
                            }`}
                          </span>
                          {standardFilters.length > 1 && (
                            <ButtonText
                              ariaExpanded={allFiltersOpen}
                              ariaControls="filterGroups"
                              className="govuk-!-margin-bottom-2"
                              onClick={() => {
                                setOpenFilterGroups(
                                  allFiltersOpen ? [] : allFilterGroupKeys,
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
                      {filterHierarchies.map(filterHierarchy => {
                        if (filterHierarchy.length === 0) return null;

                        const filterName =
                          filterHierarchy.at(-1)?.childFilterId ?? '';
                        const hierarchyName = `filterHierarchies.${filterName}`;

                        return (
                          <FilterHierarchy
                            filterHierarchy={filterHierarchy}
                            optionLabelsMap={optionLabelsMap}
                            disabled={formState.isSubmitting}
                            key={hierarchyName}
                            name={hierarchyName}
                            open={openFilterGroups.includes(filterName)}
                            isActive={isActive}
                            onToggle={isOpen => {
                              setOpenFilterGroups(groups =>
                                isOpen
                                  ? [...groups, filterName]
                                  : groups.filter(
                                      group => group !== filterName,
                                    ),
                              );
                            }}
                          />
                        );
                      })}
                      <div id="filterGroups">
                        {standardFilters.map(([filterKey, filterGroup]) => {
                          const filterName = `filters.${filterKey}`;
                          const orderedFilterGroupOptions = orderBy(
                            Object.values(filterGroup.options),
                            'order',
                          );

                          return (
                            <FormFieldCheckboxGroupsMenu
                              disabled={formState.isSubmitting}
                              groupLabel={filterGroup.legend}
                              hint={filterGroup.hint}
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
                  // Automatically select autoSelectFilterItemId for filters that haven't had a selection made
                  Object.keys(filterValues).forEach(filterName => {
                    if (
                      (!filterValues[filterName] ||
                        filterValues[filterName].length === 0) &&
                      subjectMeta.filters[filterName].autoSelectFilterItemId
                    ) {
                      setValue(`filters.${filterName}`, [
                        subjectMeta.filters[filterName]
                          .autoSelectFilterItemId as string,
                      ]);
                    }
                  });
                }}
              />
            </Form>
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
              {standardFilters
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
              {augmentedPreviousHierarchyValues &&
                Object.keys(values.filterHierarchies).map(filterGroupKey => (
                  <SummaryListItem
                    key={filterGroupKey}
                    term={optionLabelsMap[filterGroupKey]}
                  >
                    <CollapsibleList
                      id={`filtersList-${filterGroupKey}`}
                      itemName="filter"
                      itemNamePlural="filters"
                      listClassName={styles.augmentedFilters}
                    >
                      {Object.entries(
                        augmentedPreviousHierarchyValues[filterGroupKey],
                      )?.map(([optionId, relatedOptions]) => (
                        <li key={optionId}>
                          {relatedOptions.map((relatedOptionId, index) => {
                            const isLast = index === relatedOptions.length - 1;
                            const isNextTotal = !isLast
                              ? optionLabelsMap[
                                  relatedOptions[index + 1]
                                ].toLowerCase() === 'total'
                              : false;

                            const isThisTotal =
                              optionLabelsMap[relatedOptionId].toLowerCase() ===
                              'total';
                            if (isThisTotal && index !== 0) {
                              return null;
                            }

                            const relatedOptionLabel =
                              relatedOptionId === optionId ? (
                                <strong>
                                  {optionLabelsMap[relatedOptionId]}{' '}
                                  {!isThisTotal && isNextTotal && ` (total)`}
                                </strong>
                              ) : (
                                optionLabelsMap[relatedOptionId]
                              );
                            return (
                              <span
                                key={`augmentedOption-${optionId}-${relatedOptionId}`}
                              >
                                {relatedOptionLabel}
                                {!isLast && !isNextTotal && (
                                  <span
                                    aria-hidden
                                    className={styles.separator}
                                  />
                                )}
                              </span>
                            );
                          })}
                        </li>
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
}
