import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import useKeyStatQuery from '@common/modules/find-statistics/hooks/useKeyStatQuery';
import { TableQueryOptions } from '@common/modules/find-statistics/hooks/useTableQuery';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';
import { Summary } from '@common/services/types/blocks';
import React, { ReactNode } from 'react';
import ReactMarkdown from 'react-markdown';
import styles from './KeyStat.module.scss';

interface KeyStatContainerProps {
  children: ReactNode;
}

export const KeyStatContainer = ({ children }: KeyStatContainerProps) => {
  return <div className={styles.container}>{children}</div>;
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
  testId?: string;
  renderDataSummary?: ReactNode;
}

const KeyStat = ({
  children,
  query,
  queryOptions,
  summary,
  testId = 'keyStat',
}: KeyStatProps) => {
  const { value: keyStat, isLoading, error } = useKeyStatQuery(
    {
      ...query,
      includeGeoJson: false,
    },
    queryOptions,
  );

  if (error) {
    return null;
  }

  return (
    <KeyStatColumn testId={testId}>
      <LoadingSpinner loading={isLoading}>
        {keyStat && (
          <>
            <KeyStatTile
              title={keyStat.title}
              value={keyStat.value}
              testId={testId}
            >
              {summary?.dataSummary && (
                <p className="govuk-body-s" data-testid={`${testId}-summary`}>
                  {summary.dataSummary}
                </p>
              )}
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
    </KeyStatColumn>
  );
};

export default KeyStat;
