import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import formatPretty from '@common/lib/utils/number/formatPretty';
import DataBlockService, {
  DataBlock,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { useEffect, useState } from 'react';
import styles from './SummaryRenderer.module.scss';

export interface KeyStatProps extends DataBlock {
  dataBlockResponse?: DataBlockResponse;
}

const KeyStatTile = ({
  dataBlockRequest,
  summary,
  dataBlockResponse: response,
}: KeyStatProps) => {
  const [dataBlockResponse, setDataBlockResponse] = useState<
    DataBlockResponse | undefined
  >(response);
  const [config, setConfig] = useState<
    { indicatorLabel: string; value: string } | undefined
  >();

  useEffect(() => {
    if (!dataBlockResponse) {
      DataBlockService.getDataBlockForSubject(dataBlockRequest).then(
        setDataBlockResponse,
      );
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
  }, [dataBlockResponse]);

  return dataBlockResponse && config ? (
    <div className={styles.keyStatTile}>
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
    </div>
  ) : (
    <LoadingSpinner />
  );
};

export default KeyStatTile;
