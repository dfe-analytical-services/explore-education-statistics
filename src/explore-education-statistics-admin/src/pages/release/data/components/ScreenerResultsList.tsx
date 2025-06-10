import { ScreenerResultDetails } from '@admin/services/releaseDataFileService';
import React from 'react';

interface Props {
  screenerResult: ScreenerResultDetails;
  showAll: boolean | undefined;
}

export default function ScreenerResultsList({
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
    <table>
      <tbody>
        {results.map(result => (
          <tr key={results.indexOf(result)}>
            <td>
              <strong>{result.testFunctionName}</strong>
              <br />
              {result.notes}
            </td>
            <td style={{ verticalAlign: 'middle' }}>{result.testResult}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
