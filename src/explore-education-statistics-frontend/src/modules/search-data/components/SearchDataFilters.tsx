import Button from '@common/components/Button';
import { FormCheckboxGroup, FormRadioGroup } from '@common/components/form';
import { CheckboxOption } from '@common/components/form/FormCheckboxGroup';
import ReleaseTypesModal from '@common/modules/release/components/ReleaseTypesModal';
import { Theme } from '@common/services/publicationService';
import { ThemeSummary } from '@common/services/themeService';
import { ReleaseType } from '@common/services/types/releaseType';
import { GeographicLevelCode } from '@common/utils/locationLevelsMap';
import { SortOption } from '@frontend/components/SortControls';
import ThemesModal from '@frontend/modules/find-statistics/components/ThemesModal';
import { PublicationSortOption } from '@frontend/modules/find-statistics/utils/publicationSortOptions';
import ExpandableFilterGroup from '@frontend/modules/search-data/components/ExpandableFilterGroup';
import styles from '@frontend/modules/search-data/components/SearchDataFilters.module.scss';
import ThemesAndReleasesFilterGroup from '@frontend/modules/search-data/components/ThemesAndReleasesFilterGroup';
import { SearchDataFilter } from '@frontend/modules/search-data/utils/searchDataFilters';
import { DataSetType } from '@frontend/services/dataSetFileService';
import ApiDataSetsGuidanceModal from '@frontend/modules/search-data/components/ApiDataSetsGuidanceModal';
import GeographicLevelsGuidanceModal from '@frontend/modules/search-data/components/GeographicLevelsGuidanceModal';
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
  publicationIds?: string[];
  publicationTree: Theme[];
  releaseTypes?: ReleaseType[];
  releaseTypeOptions: CheckboxOption[];
  sortBy: SortOptionType;
  sortOptions: SortOption[];
  themeIds?: string[];
  themes: ThemeSummary[];
  themeOptions: CheckboxOption[];
  onChange: FilterChangeHandler;
  onChangeBatch: (updates: Record<string, string[]>) => void;
  onSortChange: (nextSortBy: SortOptionType) => void;
}

export default function Filters({
  dataSetType,
  geographicLevels,
  geographicLevelOptions,
  includeDataFilters,
  latestDataOnly,
  publicationIds,
  publicationTree,
  releaseTypes,
  releaseTypeOptions,
  sortBy,
  sortOptions,
  themeIds,
  themes,
  themeOptions,
  onChange,
  onChangeBatch,
  onSortChange,
}: Props) {
  return (
    <form className={styles.form} id={formId}>
      <h2 className="govuk-heading-m">Filter and sort</h2>
      <ExpandableFilterGroup id={`${formId}-theme-group`} label="Theme">
        {includeDataFilters ? (
          <ThemesAndReleasesFilterGroup
            publicationTree={publicationTree}
            themeIds={themeIds}
            publicationIds={publicationIds}
            onChangeBatch={(nextThemes, nextPubs) => {
              onChangeBatch({
                themeId: nextThemes,
                publicationId: nextPubs,
              });
            }}
          />
        ) : (
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
        )}
        <div className={styles.modalButtonContainer}>
          <ThemesModal themes={themes} />
        </div>
      </ExpandableFilterGroup>

      {includeDataFilters && (
        <ExpandableFilterGroup
          id={`${formId}-geographicLevel-group`}
          label="Geographic level"
          labelSub="(includes schools and providers)"
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
          <div className={styles.modalButtonContainer}>
            <GeographicLevelsGuidanceModal />
          </div>
        </ExpandableFilterGroup>
      )}

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
        <FormCheckboxGroup
          id={`${formId}-release-type`}
          legend="Filter by Release type"
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
          <div className={styles.modalButtonContainer}>
            <ApiDataSetsGuidanceModal />
          </div>
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
