import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { AxiosErrorHandler } from '@common/services/api/Client';
import DataBlockService, {
  DataBlock,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import formatPretty from '@common/utils/number/formatPretty';
import React, { ReactNode, useEffect, useState, useMemo } from 'react';
import ReactMarkdown from 'react-markdown';
import { useErrorControl } from '@common/contexts/ErrorControlContext';
import styles from './SummaryRenderer.module.scss';

export interface KeyStatProps extends Omit<DataBlock, 'type'> {
  children?: ReactNode;
  handleApiErrors?: AxiosErrorHandler;
}

export interface KeyStatConfig {
  indicatorLabel: string;
  value: string;
}

const KeyStatTile = ({ dataBlockRequest, summary, children }: KeyStatProps) => {
  const { withoutErrorHandling } = useErrorControl();
  const [dataBlockResponse, setDataBlockResponse] = useState<
    DataBlockResponse | undefined
  >();

  useEffect(() => {
    withoutErrorHandling(async () => {
      setDataBlockResponse(
        await DataBlockService.getDataBlockForSubject({
          ...dataBlockRequest,
          includeGeoJson: false,
        }),
      );
    });
  }, [dataBlockRequest, withoutErrorHandling]);

  const config: KeyStatConfig = useMemo(() => {
    if (dataBlockResponse) {
      const [indicatorKey, theIndicator] = Object.entries(
        dataBlockResponse.metaData.indicators,
      )[0];
      return {
        indicatorLabel: theIndicator.label,
        value: `${formatPretty(
          dataBlockResponse.result[0].measures[indicatorKey],
        )}${theIndicator.unit}`,
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
