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
import { DataSetType } from '@frontend/services/dataSetFileService';
import styles from '@frontend/modules/data-catalogue/components/Filters.module.scss';
import { DataSetFileFilter } from '@frontend/modules/data-catalogue/utils/dataSetFileFilters';
import React from 'react';
import classNames from 'classnames';
import locationLevelsMap, {
  GeographicLevelCode,
} from '@common/utils/locationLevelsMap';
import typedKeys from '@common/utils/object/typedKeys';

const formId = 'filters-form';

interface Props {
  dataSetType?: DataSetType;
  latestOnly?: string;
  publicationId?: string;
  publications?: PublicationTreeSummary[];
  releaseVersionId?: string;
  releases?: ReleaseSummary[];
  geographicLevel?: GeographicLevelCode;
  showResetFiltersButton?: boolean;
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
  onResetFilters?: () => void;
}

export default function Filters({
  dataSetType = 'all',
  latestOnly = 'true',
  publications = [],
  publicationId,
  releaseVersionId,
  releases = [],
  geographicLevel,
  showResetFiltersButton,
  showTypeFilter,
  themeId,
  themes,
  onChange,
  onResetFilters,
}: Props) {
  const latestValue = latestOnly === 'true' ? 'latest' : 'all';
  return (
    <form className={styles.form} id={formId}>
      <h2 className="govuk-heading-m">Filter data sets</h2>
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
          name="releaseVersionId"
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
          value={releaseVersionId ?? latestValue}
          order={[]}
          onChange={e => {
            onChange({
              filterType: 'releaseVersionId',
              nextValue: e.target.value,
            });
          }}
        />
      </FormGroup>

      <FormGroup>
        <FormSelect
          className="govuk-!-width-full"
          id={`${formId}-geographic-level`}
          label={
            <>
              <VisuallyHidden>Filter by </VisuallyHidden>Geographic level
            </>
          }
          name="geographicLevel"
          options={[
            { label: 'All', value: 'all' },
            ...typedKeys(locationLevelsMap).map(key => {
              return {
                label: locationLevelsMap[key].filterLabel,
                value: locationLevelsMap[key].code,
              };
            }),
          ]}
          value={geographicLevel ?? 'all'}
          order={[]}
          onChange={e => {
            onChange({
              filterType: 'geographicLevel',
              nextValue: e.target.value,
            });
          }}
        />
      </FormGroup>

      {showResetFiltersButton && (
        <ButtonText
          className={classNames({ 'govuk-!-margin-top-4': !showTypeFilter })}
          onClick={onResetFilters}
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
