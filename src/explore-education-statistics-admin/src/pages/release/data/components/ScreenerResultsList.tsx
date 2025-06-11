import { ScreenerResult } from '@admin/services/releaseDataFileService';
import React from 'react';

interface Props {
  screenerResult: ScreenerResult;
  showAll: boolean | undefined;
}

export default function ScreenerResultsList({
  screenerResult,
  showAll,
}: Props) {
  const testResults = showAll
    ? screenerResult.testResults
    : screenerResult.testResults.filter(
        testResult => testResult.result === 1 || testResult.result === 2, // FAIL || WARNING
      );

  return (
    <table>
      <tbody>
        {testResults.map(testResult => (
          <tr key={testResults.indexOf(testResult)}>
            <td>
              <strong>{testResult.testFunctionName}</strong>
              <br />
              {testResult.notes}
            </td>
            <td style={{ verticalAlign: 'middle' }}>{testResult.result}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
