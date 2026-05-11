import { SelectOption } from '@common/components/form/FormSelect';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { FormRadioGroup, FormSelect } from '@common/components/form';
import { ThemeSummary } from '@common/services/themeService';
import { ReleaseType } from '@common/services/types/releaseType';
import Button from '@common/components/Button';
import ReleaseTypesModal from '@common/modules/release/components/ReleaseTypesModal';
import { SortOption } from '@frontend/components/SortControls';
import ThemesModal from '@frontend/modules/find-statistics/components/ThemesModal';
import { PublicationSortOption } from '@frontend/modules/find-statistics/utils/publicationSortOptions';
import ExpandableFilterGroup from '@frontend/modules/search-data/components/ExpandableFilterGroup';
import styles from '@frontend/modules/search-data/components/SearchDataFilters.module.scss';
import { SearchDataFilter } from '@frontend/modules/search-data/utils/searchDataFilters';
import { DataSetType } from '@frontend/services/dataSetFileService';
import React from 'react';

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
  sortBy: SortOptionType;
  sortOptions: SortOption[];
  themeId?: string;
  themes: ThemeSummary[];
  themeOptions: SelectOption[];
  onChange: FilterChangeHandler;
  onSortChange: (nextSortBy: SortOptionType) => void;
}

export default function Filters({
  dataSetType,
  includeDataFilters,
  latestDataOnly,
  releaseType,
  releaseTypeOptions,
  sortBy,
  sortOptions,
  themeId,
  themes,
  themeOptions,
  onChange,
  onSortChange,
}: Props) {
  return (
    <form className={styles.form} id={formId}>
      <h2 className="govuk-heading-m">Filter and sort</h2>
      <ExpandableFilterGroup id={`${formId}-theme-group`} label="Theme">
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
        <div className={styles.modalButtonContainer}>
          <ThemesModal themes={themes} />
        </div>
      </ExpandableFilterGroup>

      {includeDataFilters && (
        <ExpandableFilterGroup
          id={`${formId}-showLatest-group`}
          label="Show latest or all releases"
        >
          <FormRadioGroup<'true' | 'false'>
            formGroupClass="govuk-!-margin-top-0"
            id={`${formId}-showLatest`}
            legend="Show latest or all releases"
            legendHidden
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
        </ExpandableFilterGroup>
      )}

      <ExpandableFilterGroup
        id={`${formId}-release-type-group`}
        label="Release types"
      >
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
        <div className={styles.modalButtonContainer}>
          <ReleaseTypesModal />
        </div>
      </ExpandableFilterGroup>

      {includeDataFilters && (
        <ExpandableFilterGroup
          id={`${formId}-dataSetType-group`}
          label="API data sets"
        >
          <FormRadioGroup<DataSetType>
            formGroupClass="govuk-!-margin-top-0"
            id={`${formId}-dataSetType`}
            legend="Type of data"
            legendHidden
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
        </ExpandableFilterGroup>
      )}
      <ExpandableFilterGroup id={`${formId}-sortBy-group`} label="Sort by">
        <FormRadioGroup<SortOptionType>
          formGroupClass="govuk-!-margin-top-0"
          id={`${formId}-sortBy`}
          legend="Sort by"
          legendHidden
          name="sortBy"
          options={sortOptions}
          small
          value={sortBy}
          onChange={event => onSortChange(event.target.value as SortOptionType)}
        />
      </ExpandableFilterGroup>

      <Button className="dfe-js-hidden" type="submit">
        Submit
      </Button>
    </form>
  );
}
