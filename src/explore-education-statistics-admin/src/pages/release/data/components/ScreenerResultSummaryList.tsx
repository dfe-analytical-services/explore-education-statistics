import { ScreenerResultDetails } from '@admin/services/releaseDataFileService';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React from 'react';

interface Props {
  screenerResult: ScreenerResultDetails;
  showAll: boolean | undefined;
}

export default function ScreenerResultSummaryList({
  screenerResult,
  showAll,
}: Props) {
  const results = showAll
    ? screenerResult.testResults
    : screenerResult.testResults.filter(
        result =>
          result.testResult === 'FAIL' || result.testResult === 'WARNING',
      );

  return (
    <SummaryList>
      {results.map(result => (
        <SummaryListItem
          key={result.testFunctionName}
          term={`${result.testFunctionName}\n${result.notes}`}
        >
          {result.testResult}
        </SummaryListItem>
      ))}
    </SummaryList>
  );
}
