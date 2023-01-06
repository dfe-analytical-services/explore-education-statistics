import LoadingSpinner from '@common/components/LoadingSpinner';
import useKeyStatQuery from '@common/modules/find-statistics/hooks/useKeyStatQuery';
import React, { ReactNode } from 'react';
import KeyStat from '@common/modules/find-statistics/components/KeyStat';

export interface KeyStatDataBlockProps {
  children?: ReactNode;
  releaseId: string;

  // NOTE: Cannot accept KeyStatistic as a prop because KeyStatSelectForm keystat preview
  dataBlockId: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;

  testId?: string;
}

const KeyStatDataBlock = ({
  children,
  releaseId,
  dataBlockId,
  trend,
  guidanceTitle = 'Help',
  guidanceText,
  testId = 'keyStat',
}: KeyStatDataBlockProps) => {
  const { value: dataBlockResults, isLoading, error } = useKeyStatQuery(
    releaseId,
    dataBlockId,
  );

  if (error) {
    return null;
  }

  if (isLoading) {
    return <LoadingSpinner />;
  }

  const title = dataBlockResults?.title;
  const statistic = dataBlockResults?.value;

  if (title && statistic) {
    return (
      <KeyStat
        title={title}
        statistic={statistic}
        trend={trend}
        guidanceTitle={guidanceTitle}
        guidanceText={guidanceText}
        testId={testId}
      >
        {children}
      </KeyStat>
    );
  }
  return null;
};

export default KeyStatDataBlock;
