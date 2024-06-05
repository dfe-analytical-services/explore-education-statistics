import ButtonText from '@common/components/ButtonText';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import styles from '@frontend/modules/data-catalogue/components/DataSetFilePreview.module.scss';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import { DataSetCsvPreview } from '@frontend/services/dataSetFileService';
import React from 'react';

const sectionId = 'dataSetPreview';
const tableId = 'preview-table';

interface Props {
  dataCsvPreview: DataSetCsvPreview;
  fullScreen?: boolean;
  onToggleFullScreen: () => void;
}

export default function DataSetFilePreview({
  dataCsvPreview,
  fullScreen,
  onToggleFullScreen,
}: Props) {
  const { headers, rows } = dataCsvPreview;

  return (
    <DataSetFilePageSection heading={pageSections[sectionId]} id={sectionId}>
      {/* eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex */}
      <div className={styles.container} tabIndex={0}>
        <table id={tableId}>
          <caption className="govuk-!-font-weight-regular govuk-!-margin-bottom-3">
            Table showing first 5 rows, from underlying data
          </caption>
          <thead>
            <tr>
              {headers.map(header => (
                <th key={header}>{header}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.map((row, rowIndex) => (
              <tr key={`row-${rowIndex.toString()}`}>
                {row.map((cell, cellIndex) => (
                  <td key={`cell-${cellIndex.toString()}`}>{cell}</td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <ButtonText onClick={onToggleFullScreen}>
        {fullScreen ? 'Close full screen table' : 'Show full screen table'}
      </ButtonText>
    </DataSetFilePageSection>
  );
}
