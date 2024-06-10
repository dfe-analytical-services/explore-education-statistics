import {
  PublicationTreeSummary,
  ReleaseSummary,
  Theme,
} from '@common/services/publicationService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormGroup, FormRadioGroup, FormSelect } from '@common/components/form';
import VisuallyHidden from '@common/components/VisuallyHidden';
import ThemesModal from '@frontend/modules/find-statistics/components/ThemesModal';
import {
  DataSetFileFilter,
  DataSetType,
} from '@frontend/services/dataSetFileService';
import styles from '@frontend/modules/data-catalogue/components/Filters.module.scss';
import React from 'react';
import classNames from 'classnames';

const formId = 'filters-form';

interface Props {
  dataSetType?: DataSetType;
  latestOnly?: string;
  publicationId?: string;
  publications?: PublicationTreeSummary[];
  releaseId?: string;
  releases?: ReleaseSummary[];
  showClearFiltersButton?: boolean;
  showTypeFilter?: boolean;
  themeId?: string;
  themes: Theme[];
  onChange: ({
    filterType,
    nextValue,
  }: {
    filterType: DataSetFileFilter;
    nextValue: string;
  }) => void;
  onClearFilters?: () => void;
}

export default function Filters({
  dataSetType = 'all',
  latestOnly = 'true',
  publications = [],
  publicationId,
  releaseId,
  releases = [],
  showClearFiltersButton,
  showTypeFilter,
  themeId,
  themes,
  onChange,
  onClearFilters,
}: Props) {
  const latestValue = latestOnly === 'true' ? 'latest' : 'all';
  return (
    <form className={styles.form} id={formId}>
      <FormGroup className="govuk-!-margin-bottom-1">
        <h2 className="govuk-heading-m">Filter data sets</h2>
        <FormSelect
          className="govuk-!-width-full"
          id={`${formId}-theme`}
          inlineHint
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
      </FormGroup>
      <ThemesModal themes={themes} />

      <FormGroup className="govuk-!-margin-top-4">
        <FormSelect
          className="govuk-!-width-full"
          disabled={!themeId}
          id={`${formId}-publication`}
          label={
            <>
              <VisuallyHidden>Filter by </VisuallyHidden>Publication
            </>
          }
          name="publicationId"
          options={[
            { label: 'All publications', value: 'all' },
            ...publications.map(publication => ({
              label: publication.title,
              value: publication.id,
            })),
          ]}
          value={publicationId ?? 'all'}
          order={[]}
          onChange={e => {
            onChange({
              filterType: 'publicationId',
              nextValue: e.target.value,
            });
          }}
        />
      </FormGroup>

      <FormGroup>
        <FormSelect
          className="govuk-!-width-full"
          id={`${formId}-release`}
          label={
            <>
              <VisuallyHidden>Filter by </VisuallyHidden>Releases
            </>
          }
          name="releaseId"
          options={[
            ...(!publicationId
              ? [{ label: 'Latest releases', value: 'latest' }]
              : []),
            { label: 'All releases', value: 'all' },
            ...(publicationId
              ? releases.map(release => ({
                  label: release.title,
                  value: release.id,
                }))
              : []),
          ]}
          value={releaseId ?? latestValue}
          order={[]}
          onChange={e => {
            onChange({ filterType: 'releaseId', nextValue: e.target.value });
          }}
        />
      </FormGroup>

      {showClearFiltersButton && (
        <ButtonText
          className={classNames({ 'govuk-!-margin-top-4': !showTypeFilter })}
          onClick={onClearFilters}
        >
          Reset filters
        </ButtonText>
      )}
      {showTypeFilter && (
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
      )}

      <Button className="dfe-js-hidden" type="submit">
        Submit
      </Button>
    </form>
  );
}
