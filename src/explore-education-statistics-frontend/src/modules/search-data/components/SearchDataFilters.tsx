import Button from '@common/components/Button';
import { FormCheckboxGroup, FormRadioGroup } from '@common/components/form';
import { CheckboxOption } from '@common/components/form/FormCheckboxGroup';
import ReleaseTypesModal from '@common/modules/release/components/ReleaseTypesModal';
import { ThemeSummary } from '@common/services/themeService';
import { ReleaseType } from '@common/services/types/releaseType';
import { GeographicLevelCode } from '@common/utils/locationLevelsMap';
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
  geographicLevels?: GeographicLevelCode[];
  geographicLevelOptions: CheckboxOption[];
  includeDataFilters: boolean;
  latestDataOnly?: boolean;
  releaseTypes?: ReleaseType[];
  releaseTypeOptions: CheckboxOption[];
  sortBy: SortOptionType;
  sortOptions: SortOption[];
  themeIds?: string[];
  themes: ThemeSummary[];
  themeOptions: CheckboxOption[];
  onChange: FilterChangeHandler;
  onSortChange: (nextSortBy: SortOptionType) => void;
}

export default function Filters({
  dataSetType,
  geographicLevels,
  geographicLevelOptions,
  includeDataFilters,
  latestDataOnly,
  releaseTypes,
  releaseTypeOptions,
  sortBy,
  sortOptions,
  themeIds,
  themes,
  themeOptions,
  onChange,
  onSortChange,
}: Props) {
  return (
    <form className={styles.form} id={formId}>
      <h2 className="govuk-heading-m">Filter and sort</h2>
      <ExpandableFilterGroup
        id={`${formId}-theme-group`}
        label="Theme"
        open={!!themeIds}
      >
        <FormCheckboxGroup
          id={`${formId}-theme`}
          legend="Filter by Theme"
          legendHidden
          name="themeId"
          options={themeOptions}
          small
          value={themeIds || []}
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
          id={`${formId}-geographicLevel-group`}
          label="Geographic level"
          open={!!geographicLevels}
        >
          <FormCheckboxGroup
            id={`${formId}-geographicLevel`}
            legend="Filter by Geographic Level"
            legendHidden
            name="geographicLevel"
            options={geographicLevelOptions}
            small
            value={geographicLevels || []}
            onChange={e => {
              onChange({
                filterType: 'geographicLevel',
                nextValue: e.target.value,
              });
            }}
          />
        </ExpandableFilterGroup>
      )}

      {includeDataFilters && (
        <ExpandableFilterGroup
          id={`${formId}-showLatest-group`}
          label="Show latest or all releases"
          open={latestDataOnly !== undefined}
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
        open={!!releaseTypes}
      >
        <FormCheckboxGroup
          id={`${formId}-release-type`}
          legend="Filter by Release Type"
          legendHidden
          name="releaseType"
          options={releaseTypeOptions}
          small
          value={releaseTypes || []}
          onChange={e => {
            onChange({ filterType: 'releaseType', nextValue: e.target.value });
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
          open={dataSetType !== undefined}
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

      <ExpandableFilterGroup
        id={`${formId}-sortBy-group`}
        label="Sort by"
        open={sortBy !== undefined}
      >
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
