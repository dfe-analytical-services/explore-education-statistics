import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { ReactNode } from 'react';
import KeyStat from '@common/modules/find-statistics/components/KeyStat';
import { useQuery } from '@tanstack/react-query';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';

export interface KeyStatDataBlockProps {
  children?: ReactNode;
  releaseId: string;
  dataBlockParentId: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;
  testId?: string;
}

export default function KeyStatDataBlock({
  children,
  releaseId,
  dataBlockParentId,
  trend,
  guidanceTitle = 'Help',
  guidanceText,
  testId = 'keyStat',
}: KeyStatDataBlockProps) {
  const {
    data: dataBlock,
    isLoading,
    error,
  } = useQuery(tableBuilderQueries.getKeyStat(releaseId, dataBlockParentId));

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
