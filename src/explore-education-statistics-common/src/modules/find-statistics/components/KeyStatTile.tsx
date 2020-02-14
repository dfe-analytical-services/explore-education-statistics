import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import formatPretty from '@common/lib/utils/number/formatPretty';
import DataBlockService, {
  DataBlock,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { AxiosResponse } from 'axios';
import React, { ReactNode, useEffect, useState } from 'react';
import styles from './SummaryRenderer.module.scss';

export interface KeyStatProps extends Omit<DataBlock, 'type'> {
  dataBlockResponse?: DataBlockResponse;
  children?: ReactNode;
  handleApiErrors?: (error: AxiosResponse) => void;
}

export interface KeyStatConfig {
  indicatorLabel: string;
  value: string;
}

const KeyStatTile = ({
  dataBlockRequest,
  summary,
  dataBlockResponse: response,
  children,
  handleApiErrors,
}: KeyStatProps) => {
  const [dataBlockResponse, setDataBlockResponse] = useState<
    DataBlockResponse | undefined
  >(response);

  const [config, setConfig] = useState<KeyStatConfig | undefined>();

  useEffect(() => {
    if (!dataBlockResponse) {
      DataBlockService.getDataBlockForSubject({
        ...dataBlockRequest,
        includeGeoJson: false,
      })
        .then(setDataBlockResponse)
        .catch(handleApiErrors);
    } else {
      const [indicatorKey, theIndicator] = Object.entries(
        dataBlockResponse.metaData.indicators,
      )[0];
      setConfig({
        indicatorLabel: theIndicator.label,
        value: `${formatPretty(
          dataBlockResponse.result[0].measures[indicatorKey],
        )}${theIndicator.unit}`,
      });
    }
  }, [dataBlockResponse, dataBlockRequest, handleApiErrors]);

  return (
    <div className={styles.keyStatTile}>
      {dataBlockResponse && config ? (
        <>
          <div className={styles.keyStat}>
            <h3 className="govuk-heading-s" data-testid="key-stat-tile-title">
              {config.indicatorLabel}
            </h3>
            <p className="govuk-heading-xl" data-testid="key-stat-tile-value">
              {config.value}
            </p>
            {summary && summary.dataSummary && (
              <p className="govuk-body-s">{summary.dataSummary}</p>
            )}
          </div>
          {summary && summary.dataDefinition && (
            <Details
              summary={
                (summary && summary.dataDefinitionTitle) ||
                `Define '${config.indicatorLabel}'`
              }
            >
              <div>{summary.dataDefinition}</div>
            </Details>
          )}
          {children}
        </>
      ) : (
        <LoadingSpinner />
      )}
    </div>
  );
};

export default KeyStatTile;
