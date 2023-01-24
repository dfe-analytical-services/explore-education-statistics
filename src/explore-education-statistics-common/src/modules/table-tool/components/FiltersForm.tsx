import { FormFieldset } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import TableQueryError from '@common/modules/table-tool/components/TableQueryError';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import RHFCheckboxSearchSubGroup from '@common/modules/table-tool/components/rhf-filters-form/RHFCheckboxSearchSubGroup';
import RHFSelectedCount from '@common/modules/table-tool/components/rhf-filters-form/RHFSelectedCount';
import RHFSelectedIndicators from '@common/modules/table-tool/components/rhf-filters-form/RHFSelectedIndicators';
import RHFSelectedFilters from '@common/modules/table-tool/components/rhf-filters-form/RHFSelectedFilters';
import RHFWizardStepFormActions from '@common/modules/table-tool/components/rhf-filters-form/RHFWizardStepFormActions';
import RHFResetFormOnPreviousStep from '@common/modules/table-tool/components/rhf-filters-form/RHFResetFormOnPreviousStep';
import DetailsMenu from '@common/components/DetailsMenu';
import {
  SelectedPublication,
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import {
  getErrorMessage,
  hasErrorMessage,
  isServerValidationError,
} from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import camelCase from 'lodash/camelCase';
import isEqual from 'lodash/isEqual';
import mapValues from 'lodash/mapValues';
import orderBy from 'lodash/orderBy';
import React, { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import ErrorSummary from '@common/components/ErrorSummary';

type FormValues = {
  indicators: string[];
  filters: Dictionary<string[]>;
};

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

/**
 * FiltersForm - filters step on the table tool.
 * Uses React Hook Form instead of Formik as it is more performant when rendering and
 * interacting large numbers of form elements.
 */
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

  const handleSubmitForm = async (values: FormValues) => {
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

  const schema = Yup.object<FormValues>({
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
  });

  const {
    control,
    register,
    getValues,
    setValue,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting, submitCount },
  } = useForm<FormValues>({
    defaultValues: initialFormValues,
    resolver: yupResolver(schema),
  });

  const formValues = getValues();

  const getErrors = () => {
    const errorsArray = [];
    if (errors.indicators) {
      errorsArray.push({
        id: `${formId}-indicators`,
        message: errors.indicators.message ?? '',
      });
    }

    if (errors.filters) {
      const filterErrors = Object.entries(errors.filters).map(
        ([errorName, _]) => ({
          id: `${formId}-filters-${camelCase(errorName)}`,
          message: `Select at least one option from ${subjectMeta.filters[
            errorName
          ].legend.toLowerCase()}`,
        }),
      );
      return [...errorsArray, ...filterErrors];
    }

    return errorsArray;
  };

  if (isActive) {
    return (
      <form id={formId} onSubmit={handleSubmit(handleSubmitForm)}>
        {tableQueryError &&
          submitCount > 0 &&
          isEqual(formValues, previousValues) && (
            <TableQueryError
              id={`${formId}-tableQueryError`}
              errorCode={tableQueryError}
              releaseId={selectedPublication?.selectedRelease.id}
              showDownloadOption={showTableQueryErrorDownload}
              subject={subject}
            />
          )}

        <ErrorSummary
          errors={getErrors()}
          id={`${formId}-summary`}
          focusOnError
        />

        {stepHeading}

        <div className="govuk-grid-row">
          <div className="govuk-grid-column-one-half-from-desktop">
            <RHFCheckboxSearchSubGroup
              error={errors.indicators?.message}
              id={`${formId}-indicators`}
              name="indicators"
              legend={
                <>
                  Indicators
                  <RHFSelectedCount name="indicators" control={control} />
                </>
              }
              register={register}
              control={control}
              options={orderedIndicators.map(group => ({
                legend: group.label,
                options: group.options,
              }))}
              legendSize="m"
              hint="Select at least one indicator below"
              disabled={isSubmitting}
              onToggleSelectAll={nextValues =>
                setValue('indicators', nextValues)
              }
            />

            {orderedFilters.length > 0 && (
              <FormFieldset
                id="filters"
                legend="Categories"
                legendSize="m"
                hint="Select at least one option from all categories"
              >
                {orderedFilters.map(([filterKey, filterGroup]) => {
                  const filterName = `filters.${filterKey}`;
                  const orderedFilterGroupOptions = orderBy(
                    Object.values(filterGroup.options),
                    'order',
                  );
                  const hasError = errors.filters?.[filterKey] !== undefined;
                  return (
                    <DetailsMenu
                      key={filterName}
                      open={orderedFilterGroupOptions.length === 1}
                      jsRequired
                      preventToggle={hasError}
                      summary={filterGroup.legend}
                      summaryAfter={
                        <RHFSelectedCount
                          name={`filters.${filterKey}`}
                          control={control}
                        />
                      }
                    >
                      <RHFCheckboxSearchSubGroup
                        id={`${formId}-filters-${filterKey}`}
                        name={`filters.${filterKey}`}
                        legend={filterGroup.legend}
                        legendHidden
                        register={register}
                        control={control}
                        options={orderedFilterGroupOptions.map(group => ({
                          legend: group.label,
                          options: group.options,
                        }))}
                        hint={filterGroup.hint}
                        disabled={isSubmitting}
                        onToggleSelectAll={nextValues =>
                          setValue(`filters.${filterKey}`, nextValues)
                        }
                        error={
                          errors.filters?.[filterKey]
                            ? `Select at least one option from ${filterGroup.legend.toLowerCase()}`
                            : undefined
                        }
                      />
                    </DetailsMenu>
                  );
                })}
              </FormFieldset>
            )}
          </div>
        </div>

        <RHFWizardStepFormActions
          {...stepProps}
          formId={formId}
          isSubmitting={isSubmitting}
          submitText="Create table"
          submittingText="Creating table"
          onSubmit={() => {
            // Automatically select totalValue for filters that haven't had a selection made
            Object.keys(formValues.filters).forEach(filterName => {
              const { totalValue } = subjectMeta.filters[filterName];
              if (
                (!formValues.filters[filterName] ||
                  formValues.filters[filterName].length === 0) &&
                totalValue
              ) {
                setValue(`filters.${filterName}`, [totalValue]);
              }
            });
          }}
        />
      </form>
    );
  }

  return (
    <WizardStepSummary {...stepProps} goToButtonText="Edit filters">
      {stepHeading}

      <SummaryList noBorder>
        <RHFSelectedIndicators
          indicators={orderedIndicators}
          name="indicators"
          control={control}
        />

        {orderedFilters.map(([filterGroupKey, filterGroup]) => {
          return (
            <RHFSelectedFilters
              filterGroup={filterGroup}
              key={`filters.${filterGroupKey}`}
              name={`filters.${filterGroupKey}`}
              control={control}
            />
          );
        })}
      </SummaryList>
      <RHFResetFormOnPreviousStep {...stepProps} onReset={reset} />
    </WizardStepSummary>
  );
};

export default FiltersForm;
