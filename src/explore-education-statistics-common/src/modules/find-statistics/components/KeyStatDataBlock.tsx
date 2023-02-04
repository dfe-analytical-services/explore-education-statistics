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

export default function KeyStatDataBlock({
  children,
  releaseId,
  dataBlockId,
  trend,
  guidanceTitle = 'Help',
  guidanceText,
  testId = 'keyStat',
}: KeyStatDataBlockProps) {
  const { value: dataBlock, isLoading, error } = useKeyStatQuery(
    releaseId,
    dataBlockId,
  );

  const title = dataBlock?.title;
  const statistic = dataBlock?.value;

  if (error || !title || !statistic) {
    return null;
  }

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
}
