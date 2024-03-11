import ButtonText from '@common/components/ButtonText';
import DataSetPageSection from '@frontend/modules/data-catalogue/components/DataSetPageSection';
import styles from '@frontend/modules/data-catalogue/components/DataSetPreview.module.scss';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetPage';
import React from 'react';

// TODO EES-4856 replace with real data
const tableHeaders = [
  'time_period',
  'time_identifier',
  'geographic_level',
  'country_code',
  'country_name',
  'group',
  'standard',
  'age',
  'apprenticeship_level',
];

const tableRows = [
  [
    '201819',
    'Academic year',
    'National',
    'E9200001',
    'England',
    'Ethnicity group',
    'Framework',
    '16-18',
    'advanced',
  ],
  [
    '201819',
    'Academic year',
    'National',
    'E9200001',
    'England',
    'Ethnicity group',
    'Framework',
    '16-18',
    'advanced',
  ],
  [
    '201819',
    'Academic year',
    'National',
    'E9200001',
    'England',
    'Ethnicity group',
    'Framework',
    '16-18',
    'advanced',
  ],
  [
    '201819',
    'Academic year',
    'National',
    'E9200001',
    'England',
    'Ethnicity group',
    'Framework',
    '16-18',
    'advanced',
  ],
  [
    '201819',
    'Academic year',
    'National',
    'E9200001',
    'England',
    'Ethnicity group',
    'Framework',
    '16-18',
    'advanced',
  ],
  [
    '201819',
    'Academic year',
    'National',
    'E9200001',
    'England',
    'Ethnicity group',
    'Framework',
    '16-18',
    'advanced',
  ],
  [
    '201819',
    'Academic year',
    'National',
    'E9200001',
    'England',
    'Ethnicity group',
    'Framework',
    '16-18',
    'advanced',
  ],
  [
    '201819',
    'Academic year',
    'National',
    'E9200001',
    'England',
    'Ethnicity group',
    'Framework',
    '16-18',
    'advanced',
  ],
  [
    '201819',
    'Academic year',
    'National',
    'E9200001',
    'England',
    'Ethnicity group',
    'Framework',
    '16-18',
    'advanced',
  ],
];

const tableId = 'preview-table';

interface Props {
  fullScreen: boolean;
  showAll: boolean;
  onToggleFullScreen: () => void;
  onToggleShowAll: () => void;
}

export default function DataSetPreview({
  fullScreen,
  showAll,
  onToggleFullScreen,
  onToggleShowAll,
}: Props) {
  const displayRows = showAll ? tableRows : tableRows.slice(0, 4);

  return (
    <DataSetPageSection heading={pageSections.preview} id="preview">
      {/* eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex */}
      <div className={styles.container} tabIndex={0}>
        <table id={tableId}>
          <caption className="govuk-!-font-weight-regular govuk-!-margin-bottom-3">
            Table showing first 5 rows from underlying data
          </caption>
          <thead>
            {tableHeaders.map(header => (
              <th key={header}>{header}</th>
            ))}
          </thead>
          <tbody>
            {displayRows.map((row, rowIndex) => (
              <tr key={`row-${rowIndex.toString()}`}>
                {row.map((cell, cellIndex) => (
                  <td key={`cell-${cellIndex.toString()}`}>{cell}</td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <div className="govuk-!-margin-bottom-6">
        <ButtonText
          ariaControls={tableId}
          ariaExpanded={!showAll}
          className="govuk-!-margin-right-3"
          onClick={onToggleShowAll}
        >
          {showAll ? 'Show only 5 rows' : 'Show 5 more rows'}
        </ButtonText>
        <ButtonText onClick={onToggleFullScreen}>
          {fullScreen ? 'Close full screen table' : 'Show full screen table'}
        </ButtonText>
      </div>
    </DataSetPageSection>
  );
}
