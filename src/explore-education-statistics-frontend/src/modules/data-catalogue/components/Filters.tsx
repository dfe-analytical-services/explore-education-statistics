import {
  PublicationTreeSummary,
  ReleaseSummary,
  Theme,
} from '@common/services/publicationService';
import Button from '@common/components/Button';
import { FormFieldset, FormGroup, FormSelect } from '@common/components/form';
import { DataSetFilter } from '@frontend/services/dataSetService';
import styles from '@frontend/modules/data-catalogue/components/Filters.module.scss';
import React from 'react';

interface Props {
  latestOnly?: string;
  publicationId?: string;
  publications?: PublicationTreeSummary[];
  releaseId?: string;
  releases?: ReleaseSummary[];
  themeId?: string;
  themes: Theme[];
  onChange: ({
    filterType,
    nextValue,
  }: {
    filterType: DataSetFilter;
    nextValue: string;
  }) => void;
}

export default function Filters({
  latestOnly = 'true',
  publications = [],
  publicationId,
  releaseId,
  releases = [],
  themeId,
  themes,
  onChange,
}: Props) {
  const latestValue = latestOnly === 'true' ? 'latest' : 'all';
  return (
    <form className={styles.form} id="filters-form">
      <FormFieldset
        id="filters"
        legend="Filter data sets"
        legendSize="m"
        className="govuk-fieldset "
      >
        <FormGroup>
          <FormSelect
            className="govuk-!-width-full"
            id="theme"
            label="Theme"
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

        <FormGroup>
          <FormSelect
            className="govuk-!-width-full"
            disabled={!themeId}
            id="publication"
            label="Publication"
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
            id="release"
            label="Releases"
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
      </FormFieldset>

      <Button className="dfe-js-hidden" type="submit">
        Submit
      </Button>
    </form>
  );
}
