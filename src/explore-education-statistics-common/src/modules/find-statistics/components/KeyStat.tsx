import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import useKeyStatQuery from '@common/modules/find-statistics/hooks/useKeyStatQuery';
import { Summary } from '@common/services/types/blocks';
import React, { ReactNode } from 'react';
import ReactMarkdown from 'react-markdown';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';

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
  releaseId: string;
  dataBlockId: string;
  summary?: Summary;
  testId?: string;
}

const KeyStat = ({
  children,
  summary,
  releaseId,
  dataBlockId,
  testId = 'keyStat',
}: KeyStatProps) => {
  const { value: keyStat, isLoading, error } = useKeyStatQuery(
    releaseId,
    dataBlockId,
  );

  if (error) {
    return null;
  }

  const dataDefinitionTitle = summary?.dataDefinitionTitle[0] || 'Help';

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
              {summary?.dataSummary[0] && (
                <p className="govuk-body-s" data-testid={`${testId}-summary`}>
                  {summary.dataSummary[0]}
                </p>
              )}
            </KeyStatTile>

            {summary?.dataDefinition[0] && (
              <Details
                summary={dataDefinitionTitle}
                className={styles.definition}
                hiddenText={
                  dataDefinitionTitle === 'Help'
                    ? `for ${keyStat.title}`
                    : undefined
                }
              >
                <div data-testid={`${testId}-definition`}>
                  {summary.dataDefinition.map(data => (
                    <ReactMarkdown key={data}>{data}</ReactMarkdown>
                  ))}
                </div>
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
