import { ScreenerResult } from '@admin/services/releaseDataFileService';
import React from 'react';
import Tag from '@common/components/Tag';
import { Dictionary } from '@common/types';
import { FormCheckbox } from '@common/components/form';
import {
  getScreenerTestResultStatusColour,
  getScreenerTestResultStatusLabel,
} from './ImporterStatus';

interface Props {
  screenerResult: ScreenerResult;
  showAll: boolean | undefined;
  onAcknowledgeWarning?: (key: string, value: boolean) => void;
  warningAcknowledgements?: Dictionary<boolean>;
}

export default function ScreenerResultsTable({
  screenerResult,
  showAll,
  onAcknowledgeWarning,
  warningAcknowledgements,
}: Props) {
  const testResults = showAll
    ? screenerResult.testResults
    : screenerResult.testResults.filter(
        testResult =>
          testResult.result === 'FAIL' || testResult.result === 'WARNING',
      );

  const hasFailures = screenerResult.testResults.some(
    testResult => testResult.result === 'FAIL',
  );

  return (
    <table data-testid="screener-result-table">
      <tbody>
        {testResults.map(testResult => (
          <tr data-testid="screener-result-table-row" key={testResult.id}>
            <td>
              <div style={{ display: 'flex', alignItems: 'center' }}>
                {!hasFailures &&
                onAcknowledgeWarning &&
                testResult.result === 'WARNING' ? (
                  <FormCheckbox
                    id={testResult.id}
                    name={testResult.id}
                    label={testResult.testFunctionName}
                    hint={testResult.notes}
                    boldLabel
                    checked={warningAcknowledgements?.[testResult.id]}
                    onChange={event =>
                      onAcknowledgeWarning(testResult.id, event.target.checked)
                    }
                  />
                ) : (
                  <span>
                    <strong>{testResult.testFunctionName}</strong>
                    <br />
                    {testResult.notes}
                  </span>
                )}
              </div>
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
