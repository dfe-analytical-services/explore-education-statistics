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
import {
  Subject,
  SubjectMeta,
  SubjectMetaFilter,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import {
  getErrorCode,
  hasErrorMessage,
  isServerValidationError,
} from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import sortAlphabeticalTotalsFirst from '@common/utils/sortAlphabeticalTotalsFirst';
import camelCase from 'lodash/camelCase';
import mapValues from 'lodash/mapValues';
import orderBy from 'lodash/orderBy';
import sortBy from 'lodash/sortBy';
import React, { useCallback, useMemo, useState } from 'react';
import { ObjectSchema } from 'yup';
import FilterHierarchy, { FilterHierarchyType } from './FilterHierarchy';

export interface FiltersFormValues {
  indicators: string[];
  filters: Dictionary<string[]>;
  filterHierarchies: Dictionary<string[]>;
}

export type FilterFormSubmitHandler = (values: FiltersFormValues) => void;

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
  hideFilterHierarchies: boolean;
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
  hideFilterHierarchies = true, // hierarchies feature flag
  ...stepProps
}: Props) {
  const { goToNextStep, isActive } = stepProps;

  const [tableQueryError, setTableQueryError] = useState<TableQueryErrorCode>();
  const [previousValues, setPreviousValues] = useState<FiltersFormValues>();
  const [openFilterGroups, setOpenFilterGroups] = useState<string[]>([]);

  const hierarchiedFilterIds = useMemo(() => {
    return (
      subjectMeta.filterHierarchies?.flatMap(hierarchy => {
        return [hierarchy.rootFilterId, ...hierarchy.childFilterIds];
      }) ?? []
    );
  }, [subjectMeta]);

  const filterGroups = useMemo(() => {
    if (hideFilterHierarchies) return Object.entries(subjectMeta.filters);

    return orderBy(
      Object.entries(subjectMeta.filters).filter(
        ([_, filter]) =>
          // filter out filters that are hierarchied
          !hierarchiedFilterIds.includes(filter.id),
      ),
      ([_, value]) => value.order,
    );
  }, [subjectMeta, hierarchiedFilterIds, hideFilterHierarchies]);

  const hierarchiedFilters: [string, SubjectMetaFilter][] = useMemo(() => {
    if (hideFilterHierarchies) return [];

    return orderBy(
      Object.entries(subjectMeta.filters).filter(([_, filter]) =>
        hierarchiedFilterIds.includes(filter.id),
      ),
      ([_, value]) => value.order,
    ).map(([_, filter]) => [filter.id, filter]);
  }, [subjectMeta, hierarchiedFilterIds, hideFilterHierarchies]);

  const labelsDictionary = useMemo(() => {
    const labelsMap: Dictionary<{
      label: string;
      parentLegend?: string;
      parentLabel?: string;
    }> = {};

    if (!subjectMeta.filterHierarchies?.length) {
      return labelsMap;
    }

    hierarchiedFilters.forEach(([_, filter]) => {
      labelsMap[filter.id] = { label: filter.legend };
      Object.values(filter.options).forEach(filterOptions => {
        labelsMap[filterOptions.id] = {
          parentLegend: filter.legend,
          label: filterOptions.label,
        };
        filterOptions.options.forEach(filterGroupOption => {
          labelsMap[filterGroupOption.value] = {
            parentLegend: filter.legend,
            parentLabel: filterOptions.label,
            label: filterGroupOption.label,
          };
        });
      });
    });

    return labelsMap;
  }, [hierarchiedFilters, subjectMeta.filterHierarchies]);

  const getLabelsAndSortOptions = useCallback(
    (options: string[]) => {
      return options
        .map(optionId => [optionId, labelsDictionary[optionId].label])
        .sort((a, b) => sortAlphabeticalTotalsFirst(a[1], b[1]));
    },
    [labelsDictionary],
  );

  const [filterHierarchies, filterHierarchiesValuesMap] = useMemo(() => {
    if (hideFilterHierarchies) {
      return [[], {}];
    }

    const optionRelatedValuesDictionary: { [key: string]: string[] } = {};

    const hierarchies: FilterHierarchyType[] =
      subjectMeta.filterHierarchies?.map(filterHierarchy => {
        const tiersTotal = filterHierarchy.childFilterIds.length + 1;
        const tiers = sortBy(filterHierarchy.tiers, 'level');
        const optionTotalDictionary: { [key: string]: string } = {};

        const bottomTierOptionId = Object.values(
          tiers[tiers.length - 1].hierarchy,
        )?.find(([firstOption]) => !!firstOption)?.[0] as string;
        // The bottom tier filter group legend is used as the hierarchy legend
        const legend =
          labelsDictionary[bottomTierOptionId].parentLegend ?? 'missing legend';

        interface TierOption {
          label: string;
          value: string;
          parentValues: string[];
          options?: TierOption[];
        }

        function generateOptionsTreeRecursively({
          currentTier = 0,
          parentOptionId,
          parentValues,
        }: {
          parentOptionId: string;
          parentValues: string[];
          currentTier?: number;
        }): TierOption[] {
          const isBottomTier = currentTier === tiersTotal - 1;
          if (isBottomTier) {
            return [];
          }

          const tierOptionsIds =
            tiers[currentTier]?.hierarchy[parentOptionId] ?? [];

          return getLabelsAndSortOptions(tierOptionsIds).map(
            ([optionId, optionLabel]) => {
              if (optionLabel.toLocaleLowerCase() === 'total') {
                // Add "total"s to hashmap
                optionTotalDictionary[parentOptionId] = optionId;
              }

              const appendedParentValues = [...parentValues, optionId];

              return {
                value: optionId,
                label: optionLabel,
                parentValues: appendedParentValues,
                options: generateOptionsTreeRecursively({
                  currentTier: currentTier + 1,
                  parentOptionId: optionId,
                  parentValues: appendedParentValues,
                }),
              };
            },
          );
        }

        const tierOptions: TierOption[] = getLabelsAndSortOptions(
          filterHierarchy.rootOptionIds,
        ).map(([value, label]) => {
          return {
            parentValues: [],
            value,
            label,
            options: generateOptionsTreeRecursively({
              parentOptionId: value,
              parentValues: [value],
            }),
          };
        });

        function getOptionChildTotalIdsRecursively(
          optionIds: string[],
        ): string[] {
          // Any options that aren't bottom tier options select child total(s) when selected
          const nextItem = optionTotalDictionary[optionIds.at(-1) ?? ''];
          return nextItem
            ? getOptionChildTotalIdsRecursively([...optionIds, nextItem])
            : optionIds;
        }

        function generateOptionToRelatedValuesDictionaryRecursively(
          options: TierOption[],
        ) {
          options.forEach(option => {
            if (option.options) {
              generateOptionToRelatedValuesDictionaryRecursively(
                option.options,
              );
            }
            optionRelatedValuesDictionary[option.value] =
              getOptionChildTotalIdsRecursively([
                ...option.parentValues,
                option.value,
              ]);
          });
        }
        generateOptionToRelatedValuesDictionaryRecursively(tierOptions);

        return {
          tiersTotal,
          legend,
          id: camelCase(labelsDictionary[filterHierarchy.rootFilterId].label),
          options: tierOptions,
        };
      }) ?? [];
    return [hierarchies, optionRelatedValuesDictionary];
  }, [
    subjectMeta,
    labelsDictionary,
    getLabelsAndSortOptions,
    hideFilterHierarchies,
  ]);

  const initialFormValues = useMemo(() => {
    // Automatically select indicator when one indicator group with one option
    const indicatorValues = Object.values(subjectMeta.indicators);
    const indicators =
      indicatorValues.length === 1 && indicatorValues[0].options.length === 1
        ? [indicatorValues[0].options[0].value]
        : initialValues?.indicators ?? [];

    const filters = mapValues(Object.fromEntries(filterGroups), filter => {
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

    return {
      filters,
      indicators,
    };
  }, [initialValues, subjectMeta, filterGroups]);

  const stepHeading = (
    <WizardStepHeading {...stepProps}>{stepTitle}</WizardStepHeading>
  );

  const handleSubmit = async (values: FiltersFormValues) => {
    setPreviousValues({ ...values });

    try {
      setTableQueryError(undefined);

      const relatedSelectedFilterHierarchyOptions = mapValues(
        values.filterHierarchies,
        selectedOptions => {
          return selectedOptions
            .map(option => filterHierarchiesValuesMap[option])
            .flat();
        },
      );

      await goToNextStep(async () => {
        await onSubmit({
          ...values,
          filterHierarchies: relatedSelectedFilterHierarchyOptions,
        });
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
  };

  const allFilterGroupKeys = useMemo(
    () => [
      ...filterGroups.map(([key]) => key),
      ...filterHierarchies.map(({ id }) => id),
    ],
    [filterGroups, filterHierarchies],
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
        mapValues(Object.fromEntries(filterGroups), filter => {
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
          filterHierarchies.map(filter => {
            const label = filter.legend.toLowerCase();

            return [
              filter.id,
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
  }, [filterGroups, filterHierarchies]);

  const filtersIncludeTotal = Object.values(subjectMeta.filters).some(
    filter => filter.autoSelectFilterItemId,
  );

  return (
    <FormProvider
      enableReinitialize
      initialValues={initialFormValues}
      validationSchema={validationSchema}
    >
      {({ formState, getValues, reset, setValue, watch }) => {
        const { getError } = createErrorHelper({
          errors: formState.errors,
          touchedFields: formState.touchedFields,
        });
        const values = watch();

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

              <div className="govuk-grid-row">
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

                  {filterGroups.length + hierarchiedFilterIds.length > 0 && (
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
                          {filterGroups.length > 1 && (
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
                        const hierarchyName = `filterHierarchies.${filterHierarchy.id}`;
                        return (
                          <FilterHierarchy
                            {...filterHierarchy}
                            disabled={formState.isSubmitting}
                            key={hierarchyName}
                            name={hierarchyName}
                          />
                        );
                      })}
                      <div id="filterGroups">
                        {filterGroups.map(([filterKey, filterGroup]) => {
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
        // const values = getValues();

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
              {filterGroups
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
}
