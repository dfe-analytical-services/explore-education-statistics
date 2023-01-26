import LoadingSpinner from '@common/components/LoadingSpinner';
import useKeyStatQuery from '@common/modules/find-statistics/hooks/useKeyStatQuery';
import React, { ReactNode } from 'react';
import KeyStat from '@common/modules/find-statistics/components/KeyStat';

export interface KeyStatDataBlockProps {
  children?: ReactNode;
  releaseId: string;
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

  const title = dataBlockResults?.title;
  const statistic = dataBlockResults?.value;

  return (
    <LoadingSpinner loading={isLoading}>
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
    </LoadingSpinner>
  );
};

export default KeyStatDataBlock;
