import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { ReactNode } from 'react';
import KeyStat from '@common/modules/find-statistics/components/KeyStat';
import { useQuery } from '@tanstack/react-query';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';

export interface KeyStatDataBlockProps {
  children?: ReactNode;
  releaseVersionId: string;
  dataBlockParentId: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;
  isRedesignStyle?: boolean;
  testId?: string;
}

export default function KeyStatDataBlock({
  children,
  releaseVersionId,
  dataBlockParentId,
  trend,
  guidanceTitle = 'Help',
  guidanceText,
  isRedesignStyle = false,
  testId = 'keyStat',
}: KeyStatDataBlockProps) {
  const {
    data: dataBlock,
    isLoading,
    error,
  } = useQuery(
    tableBuilderQueries.getKeyStat(releaseVersionId, dataBlockParentId),
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
        isRedesignStyle={isRedesignStyle}
        testId={testId}
      >
        {children}
      </KeyStat>
    </LoadingSpinner>
  );
}
