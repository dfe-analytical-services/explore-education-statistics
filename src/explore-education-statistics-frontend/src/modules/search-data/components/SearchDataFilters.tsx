import { SelectOption } from '@common/components/form/FormSelect';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { FormGroup, FormRadioGroup, FormSelect } from '@common/components/form';
import { ThemeSummary } from '@common/services/themeService';
import { ReleaseType } from '@common/services/types/releaseType';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import ReleaseTypesModal from '@common/modules/release/components/ReleaseTypesModal';
import ThemesModal from '@frontend/modules/find-statistics/components/ThemesModal';
import styles from '@frontend/modules/data-catalogue/components/Filters.module.scss';
import { SortOption } from '@frontend/components/SortControls';
import React from 'react';
import { PublicationSortOption } from '@frontend/modules/find-statistics/utils/publicationSortOptions';
import { SearchDataFilter } from '@frontend/modules/search-data/utils/searchDataFilters';
import { DataSetType } from '@frontend/services/dataSetFileService';

const formId = 'filters-form';

export type FilterChangeHandler = ({
  filterType,
  nextValue,
}: {
  filterType: SearchDataFilter;
  nextValue: string;
}) => void;

type SortOptionType = PublicationSortOption;

interface Props {
  dataSetType?: DataSetType;
  includeDataFilters: boolean;
  latestDataOnly?: boolean;
  releaseType?: ReleaseType;
  releaseTypeOptions: SelectOption[];
  showResetFiltersButton?: boolean;
  sortBy: SortOptionType;
  sortOptions: SortOption[];
  themeId?: string;
  themes: ThemeSummary[];
  themeOptions: SelectOption[];
  onChange: FilterChangeHandler;
  onResetFilters?: () => void;
  onSortChange: (nextSortBy: SortOptionType) => void;
}

export default function Filters({
  dataSetType,
  includeDataFilters,
  latestDataOnly,
  releaseType,
  releaseTypeOptions,
  showResetFiltersButton,
  sortBy,
  sortOptions,
  themeId,
  themes,
  themeOptions,
  onChange,
  onResetFilters,
  onSortChange,
}: Props) {
  return (
    <form className={styles.form} id={formId}>
      <h2 className="govuk-heading-m">Sort and filter publications</h2>
      <FormGroup>
        <FormSelect
          className="govuk-!-width-full govuk-!-margin-bottom-1"
          id={`${formId}-theme`}
          label={
            <>
              <VisuallyHidden>Filter by </VisuallyHidden>Theme
            </>
          }
          name="themeId"
          options={[{ label: 'All themes', value: 'all' }, ...themeOptions]}
          value={themeId ?? 'all'}
          order={[]}
          onChange={e => {
            onChange({ filterType: 'themeId', nextValue: e.target.value });
          }}
        />
        <ThemesModal themes={themes} />
      </FormGroup>

      <FormGroup>
        <FormSelect
          className="govuk-!-width-full govuk-!-margin-bottom-1"
          id={`${formId}-release-type`}
          label={
            <>
              <VisuallyHidden>Filter by </VisuallyHidden>Release type
            </>
          }
          name="releaseType"
          options={[
            { label: 'All release types', value: 'all' },
            ...releaseTypeOptions,
          ]}
          value={releaseType ?? 'all'}
          order={[]}
          onChange={e => {
            onChange({
              filterType: 'releaseType',
              nextValue: e.target.value,
            });
          }}
        />
        <ReleaseTypesModal />
      </FormGroup>

      {includeDataFilters && (
        <>
          <FormRadioGroup<DataSetType>
            formGroupClass="dfe-border-top govuk-!-padding-top-4 govuk-!-margin-top-2"
            id={`${formId}-dataSetType`}
            legend="Type of data"
            legendSize="s"
            name="dataSetType"
            options={[
              { label: 'All data', value: 'all' },
              {
                label: 'API data sets only',
                value: 'api',
              },
            ]}
            small
            value={dataSetType}
            onChange={e => {
              onChange({
                filterType: 'dataSetType',
                nextValue: e.target.value,
              });
            }}
          />
          <FormGroup>
            <FormRadioGroup<'true' | 'false'>
              formGroupClass="dfe-border-top govuk-!-padding-top-4 govuk-!-margin-top-2"
              id={`${formId}-showLatest`}
              legend="Show latest or all releases"
              legendSize="s"
              name="latestDataOnly"
              options={[
                { label: 'Show latest releases only', value: 'true' },
                {
                  label: 'Show all releases',
                  value: 'false',
                },
              ]}
              small
              order={[]}
              value={latestDataOnly ? 'true' : 'false'}
              onChange={e => {
                onChange({
                  filterType: 'latestDataOnly',
                  nextValue: e.target.value,
                });
              }}
            />
          </FormGroup>
        </>
      )}

      <FormGroup>
        <FormSelect
          className="govuk-!-width-full govuk-!-margin-bottom-1"
          id={`${formId}-sortBy`}
          label="Sort by"
          name="sortBy"
          options={sortOptions}
          value={sortBy}
          order={[]}
          onChange={event => onSortChange(event.target.value as SortOptionType)}
        />
      </FormGroup>

      {showResetFiltersButton && (
        <ButtonText onClick={onResetFilters}>Reset filters</ButtonText>
      )}

      <Button className="dfe-js-hidden" type="submit">
        Submit
      </Button>
    </form>
  );
}
