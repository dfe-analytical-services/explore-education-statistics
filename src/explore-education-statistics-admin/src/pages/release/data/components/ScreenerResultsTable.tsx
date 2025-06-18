import { ScreenerResult } from '@admin/services/releaseDataFileService';
import React from 'react';
import Tag from '@common/components/Tag';
import {
  getScreenerTestResultStatusColour,
  getScreenerTestResultStatusLabel,
} from './ImporterStatus';

interface Props {
  screenerResult: ScreenerResult;
  showAll: boolean | undefined;
}

export default function ScreenerResultsTable({
  screenerResult,
  showAll,
}: Props) {
  const testResults = showAll
    ? screenerResult.testResults
    : screenerResult.testResults.filter(
        testResult =>
          testResult.result === 'FAIL' || testResult.result === 'WARNING',
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
            <td style={{ verticalAlign: 'middle' }}>
              <Tag
                colour={getScreenerTestResultStatusColour(testResult.result)}
              >
                {getScreenerTestResultStatusLabel(testResult.result)}
              </Tag>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
