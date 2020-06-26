import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useTableQuery from '@common/modules/find-statistics/hooks/useTableQuery';
import { AxiosErrorHandler } from '@common/services/api/Client';
import { DataBlock } from '@common/services/types/blocks';
import formatPretty from '@common/utils/number/formatPretty';
import React, { ReactNode, useMemo } from 'react';
import ReactMarkdown from 'react-markdown';
import styles from './KeyStatTile.module.scss';

export interface KeyStatProps extends Omit<DataBlock, 'type'> {
  children?: ReactNode;
  handleApiErrors?: AxiosErrorHandler;
}

export interface KeyStatConfig {
  indicatorLabel: string;
  value: string;
}

const KeyStatTile = ({ dataBlockRequest, summary, children }: KeyStatProps) => {
  const { value: tableData, isLoading, error } = useTableQuery(
    {
      ...dataBlockRequest,
      includeGeoJson: false,
    },
    {
      expiresIn: 60 * 60 * 24,
    },
  );

  const resultValue = useMemo<string>(() => {
    if (tableData) {
      const [indicator] = tableData.subjectMeta.indicators;

      return formatPretty(
        tableData.results[0].measures[indicator.value],
        indicator.unit,
        indicator.decimalPlaces,
      );
    }

    return '';
  }, [tableData]);

  const indicator = tableData?.subjectMeta?.indicators[0];

  if (error) {
    return null;
  }

  return (
    <div className={styles.keyStatTile}>
      <LoadingSpinner loading={isLoading}>
        {tableData && resultValue && (
          <>
            <div className={styles.keyStat} data-testid="key-stat-tile">
              <h3 className="govuk-heading-s" data-testid="key-stat-tile-title">
                {indicator?.label}
              </h3>

              <p className="govuk-heading-xl" data-testid="key-stat-tile-value">
                {resultValue}
              </p>

              {summary?.dataSummary && (
                <p className="govuk-body-s">{summary.dataSummary}</p>
              )}
            </div>

            {summary?.dataDefinition?.[0] && (
              <Details summary={summary?.dataDefinitionTitle || 'Help'}>
                {summary.dataDefinition.map(data => (
                  <ReactMarkdown key={data}>{data}</ReactMarkdown>
                ))}
              </Details>
            )}

            {children}
          </>
        )}
      </LoadingSpinner>
    </div>
  );
};

export default KeyStatTile;
