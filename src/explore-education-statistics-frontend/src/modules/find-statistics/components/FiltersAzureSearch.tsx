import { SelectOption } from '@common/components/form/FormSelect';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { FormGroup, FormSelect } from '@common/components/form';
import { ThemeSummary } from '@common/services/themeService';
import { ReleaseType } from '@common/services/types/releaseType';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import ReleaseTypesModal from '@common/modules/release/components/ReleaseTypesModal';
import ThemesModal from '@frontend/modules/find-statistics/components/ThemesModal';
import styles from '@frontend/modules/data-catalogue/components/Filters.module.scss';
import { PublicationFilter } from '@frontend/modules/find-statistics/utils/publicationFilters';
import { SortOption } from '@frontend/components/SortControls';
import React from 'react';
import { PublicationSortOption } from '../utils/publicationSortOptions';

const formId = 'filters-form';

export type FilterChangeHandler = ({
  filterType,
  nextValue,
}: {
  filterType: PublicationFilter;
  nextValue: string;
}) => void;

type SortOptionType = PublicationSortOption;

interface Props {
  releaseType?: ReleaseType;
  releaseTypesWithResultCounts: SelectOption[];
  showResetFiltersButton?: boolean;
  sortBy: SortOptionType;
  sortOptions: SortOption[];
  themeId?: string;
  themes: ThemeSummary[];
  themesWithResultCounts: SelectOption[];
  onChange: FilterChangeHandler;
  onResetFilters?: () => void;
  onSortChange: (nextSortBy: SortOptionType) => void;
}

export default function Filters({
  releaseType,
  releaseTypesWithResultCounts,
  showResetFiltersButton,
  sortBy,
  sortOptions,
  themeId,
  themes,
  themesWithResultCounts,
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
          id={`${formId}-sortBy`}
          label="Sort by"
          name="sortBy"
          options={sortOptions}
          value={sortBy}
          order={[]}
          onChange={event => onSortChange(event.target.value as SortOptionType)}
        />
      </FormGroup>

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
          options={[
            { label: 'All themes', value: 'all' },
            ...themesWithResultCounts,
          ]}
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
            ...releaseTypesWithResultCounts,
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

      {showResetFiltersButton && (
        <ButtonText onClick={onResetFilters}>Reset filters</ButtonText>
      )}

      <Button className="dfe-js-hidden" type="submit">
        Submit
      </Button>
    </form>
  );
}
