import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { AxiosErrorHandler } from '@common/services/api/Client';
import { DataBlock } from '@common/services/types/blocks';
import formatPretty from '@common/utils/number/formatPretty';
import React, { ReactNode, useEffect, useMemo, useState } from 'react';
import ReactMarkdown from 'react-markdown';
import styles from './KeyStatTile.module.scss';

export interface KeyStatProps extends Omit<DataBlock, 'type'> {
  releaseUuid: string;
  children?: ReactNode;
  handleApiErrors?: AxiosErrorHandler;
}

export interface KeyStatConfig {
  indicatorLabel: string;
  value: string;
}

const KeyStatTile = ({
  releaseUuid,
  dataBlockRequest,
  summary,
  children,
}: KeyStatProps) => {
  const [dataBlockResponse, setDataBlockResponse] = useState<
    TableDataResponse | undefined
  >();

  useEffect(() => {
    tableBuilderService
      .getTableData(releaseUuid, {
        ...dataBlockRequest,
        includeGeoJson: false,
      })
      .then(setDataBlockResponse);
  }, [dataBlockRequest]);

  const config: KeyStatConfig = useMemo(() => {
    if (dataBlockResponse) {
      const [indicator] = dataBlockResponse.subjectMeta.indicators;

      return {
        indicatorLabel: indicator.label,
        value: `${formatPretty(
          dataBlockResponse.results[0].measures[indicator.value],
          indicator.unit,
          indicator.decimalPlaces,
        )}`,
      };
    }
    return {
      indicatorLabel: '',
      value: '',
    };
  }, [dataBlockResponse]);

  return (
    <div className={styles.keyStatTile}>
      {dataBlockResponse && config ? (
        <>
          <div className={styles.keyStat} data-testid="key-stat-tile">
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
          {summary && summary?.dataDefinition?.[0] !== '' && (
            <Details
              summary={(summary && summary.dataDefinitionTitle) || 'Help'}
            >
              {summary.dataDefinition.map(data => (
                <ReactMarkdown key={data}>{data}</ReactMarkdown>
              ))}
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
