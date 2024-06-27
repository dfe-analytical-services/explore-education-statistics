import VisuallyHidden from '@common/components/VisuallyHidden';
import { FormGroup, FormSelect } from '@common/components/form';
import { ThemeSummary } from '@common/services/themeService';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import ReleaseTypesModal from '@common/modules/release/components/ReleaseTypesModal';
import ThemesModal from '@frontend/modules/find-statistics/components/ThemesModal';
import styles from '@frontend/modules/data-catalogue/components/Filters.module.scss';
import { PublicationFilter } from '@frontend/modules/find-statistics/utils/publicationFilters';
import React from 'react';

const formId = 'filters-form';

export type FilterChangeHandler = ({
  filterType,
  nextValue,
}: {
  filterType: PublicationFilter;
  nextValue: string;
}) => void;

interface Props {
  releaseType?: ReleaseType;
  showResetFiltersButton?: boolean;
  themeId?: string;
  themes: ThemeSummary[];
  onChange: FilterChangeHandler;
  onResetFilters?: () => void;
}

export default function Filters({
  releaseType,
  showResetFiltersButton,
  themeId,
  themes,
  onChange,
  onResetFilters,
}: Props) {
  return (
    <form className={styles.form} id={formId}>
      <h2 className="govuk-heading-m">Filter publications</h2>
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
            ...themes.map(theme => ({
              label: theme.title,
              value: theme.id,
            })),
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
            ...Object.keys(releaseTypes).map(type => ({
              label: releaseTypes[type as ReleaseType],
              value: type,
            })),
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
