import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import useTableQuery, {
  TableQueryOptions,
} from '@common/modules/find-statistics/hooks/useTableQuery';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';
import { Summary } from '@common/services/types/blocks';
import formatPretty from '@common/utils/number/formatPretty';
import React, { FC, ReactNode, useMemo } from 'react';
import ReactMarkdown from 'react-markdown';
import styles from './KeyStat.module.scss';

interface KeyStatContainerProps {
  children: ReactNode;
  tag?: keyof JSX.IntrinsicElements;
}

export const KeyStatContainer = ({
  children,
  tag: ElementTag = 'div',
}: KeyStatContainerProps) => {
  return <ElementTag className={styles.container}>{children}</ElementTag>;
};

interface KeyStatColumnProps {
  children: ReactNode;
  testId?: string;
}

export const KeyStatColumn = ({ children, testId }: KeyStatColumnProps) => {
  return (
    <div className={styles.column} data-testid={testId}>
      {children}
    </div>
  );
};

export interface KeyStatProps {
  children?: ReactNode;
  query: ReleaseTableDataQuery;
  queryOptions?: TableQueryOptions;
  summary?: Summary;
  renderDataSummary?: ReactNode;
}

const KeyStat = ({
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
    <LoadingSpinner loading={isLoading}>
      {indicator && tableData && resultValue && (
        <>
          <KeyStatTile title={indicator.label} value={resultValue}>
            {renderDataSummary ||
              (summary?.dataSummary && (
                <p className="govuk-body-s">{summary.dataSummary}</p>
              ))}
          </KeyStatTile>

          {summary?.dataDefinition?.[0] && (
            <Details
              summary={summary?.dataDefinitionTitle || 'Help'}
              className={styles.definition}
            >
              {summary.dataDefinition.map(data => (
                <ReactMarkdown key={data}>{data}</ReactMarkdown>
              ))}
            </Details>
          )}

          {children}
        </>
      )}
    </LoadingSpinner>
  );
};

export default KeyStat;
