import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useTableQuery, {
  TableQueryOptions,
} from '@common/modules/find-statistics/hooks/useTableQuery';
import { TableDataQuery } from '@common/services/tableBuilderService';
import { Summary } from '@common/services/types/blocks';
import formatPretty from '@common/utils/number/formatPretty';
import React, { ReactNode, useMemo } from 'react';
import ReactMarkdown from 'react-markdown';
import styles from './KeyStatTile.module.scss';

export interface KeyStatProps {
  children?: ReactNode;
  query: TableDataQuery;
  queryOptions?: TableQueryOptions;
  summary?: Summary;
  renderDataSummary?: ReactNode;
}

const KeyStatTile = ({
  children,
  query,
  queryOptions,
  summary,
  renderDataSummary,
}: KeyStatProps) => {
  const { value: tableData, isLoading, error } = useTableQuery(
    {
      ...query,
      includeGeoJson: false,
    },
    queryOptions,
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

              {renderDataSummary ||
                (summary?.dataSummary && (
                  <p className="govuk-body-s">{summary.dataSummary}</p>
                ))}
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
