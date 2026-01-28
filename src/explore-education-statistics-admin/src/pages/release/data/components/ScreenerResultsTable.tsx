import { useAuthContext } from '@admin/contexts/AuthContext';
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
  screenerResult?: ScreenerResult;
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
  const { user } = useAuthContext();
  const isBauUser = user?.permissions.isBauUser;
  const testResults = showAll
    ? screenerResult?.testResults
    : screenerResult?.testResults.filter(
        testResult =>
          testResult.result === 'FAIL' || testResult.result === 'WARNING',
      );

  const hasFailures = screenerResult?.testResults.some(
    testResult => testResult.result === 'FAIL',
  );

  return testResults ? (
    <table
      className="dfe-table--vertical-align-middle"
      data-testid="screener-result-table"
    >
      <thead>
        <tr>
          <th>Test</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        {testResults.map(testResult => (
          <tr data-testid="screener-result-table-row" key={testResult.id}>
            <td className="dfe-word-break--break-word">
              <div>
                {!hasFailures &&
                onAcknowledgeWarning &&
                testResult.result === 'WARNING' ? (
                  <FormCheckbox
                    id={testResult.id}
                    name={testResult.id}
                    label={testResult.testFunctionName}
                    hint={testResult.notes}
                    boldLabel
                    checked={!!warningAcknowledgements?.[testResult.id]}
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
            <td>
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
  ) : (
    <>
      {isBauUser ? (
        <p>
          The data file failed to transfer to the automatic screener service and
          screening has not been run. Make sure to validate the files using the
          external screener and, once checked, bypass this automated screening
          step to continue to import this data set.
        </p>
      ) : (
        <p>
          We've hit an issue with running the automatic screening on this file.
          Please make sure you have fully validated the file in the external
          screener and then e-mail{' '}
          <a href="mailto:explore.statistics@education.gov.uk">
            explore.statistics@education.gov.uk
          </a>{' '}
          with the publication and data set names for help in bypassing this
          issue.
        </p>
      )}
    </>
  );
}
