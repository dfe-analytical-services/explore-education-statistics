import _releaseDataFileService, {
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import render from '@common-test/render';
import { Dictionary } from '@common/types';
import React from 'react';
import { screen, waitFor, within } from '@testing-library/dom';
import DataSetUploadTableRow from '@admin/pages/release/data/components/data-uploads/data-set-uploads/DataSetUploadTableRow';

jest.mock('@admin/services/releaseDataFileService');
const releaseDataFileService = jest.mocked(_releaseDataFileService);

describe('DataSetUploadTableRow', () => {
  const rowBaseProps = {
    isReplacement: false,
    permissions: {
      canUpdateRelease: true,
      canOverrideScreenerResult: false,
      canManagePublicApiDataSets: false,
    },
    releaseVersionId: 'release-version-id-1',
    onImportDataSets: jest.fn(),
    onDeleteUpload: jest.fn(),
    onRefreshUploads: jest.fn(),
  };

  const fileUploads: Dictionary<DataSetUpload> = {
    pass: {
      id: 'b13b2247-ae76-41c7-b442-08ddafffd9e4',
      dataSetTitle: 'pass',
      dataFileName: 'one-pass.csv',
      dataFileSize: '696 B',
      metaFileName: 'one-pass.meta.csv',
      metaFileSize: '210 B',
      screeningStatus: 'PendingImport',
      screenerResult: {
        overallResult: 'Passed',
        passed: true,
        testResults: [
          {
            id: 'pass-test-1',
            testFunctionName: 'check_filename_spaces',
            result: 'PASS',
            notes: "'one-pass.csv' does not have spaces in the filename.",
            stage: 'InitialFileValidation',
          },
          {
            id: 'pass-test-2',
            testFunctionName: 'check_filename_spaces',
            result: 'PASS',
            notes: "'one-pass.meta.csv' does not have spaces in the filename.",
            stage: 'InitialFileValidation',
          },
          {
            id: 'pass-test-3',
            testFunctionName: 'check_empty_cols',
            result: 'PASS',
            notes: "'one-pass.csv' does not have any blank columns.",
            stage: 'InitialFileValidation',
          },
        ],
      },
      created: new Date('2025-06-23T13:23:08.6258337'),
      uploadedBy: 'ees-test.bau1@education.gov.uk',
    },
    passAndWarning: {
      id: 'b13b2247-ae76-41c7-b442-08ddafffd9e4',
      dataSetTitle: 'pass',
      dataFileName: 'one-pass.csv',
      dataFileSize: '696 B',
      metaFileName: 'one-pass.meta.csv',
      metaFileSize: '210 B',
      screeningStatus: 'PendingImport',
      screenerResult: {
        overallResult: 'Passed',
        passed: true,
        testResults: [
          {
            id: 'passAndWarning-test-1',
            testFunctionName: 'check_filename_spaces',
            result: 'PASS',
            notes: "'one-pass.csv' does not have spaces in the filename.",
            stage: 'InitialFileValidation',
          },
          {
            id: 'passAndWarning-test-2',
            testFunctionName: 'check_filename_spaces',
            result: 'WARNING',
            notes: "'one-pass.meta.csv' does not have spaces in the filename.",
            stage: 'InitialFileValidation',
          },
          {
            id: 'passAndWarning-test-3',
            testFunctionName: 'check_empty_cols',
            result: 'WARNING',
            notes: "'one-pass.csv' does not have any blank columns.",
            stage: 'InitialFileValidation',
          },
        ],
      },
      created: new Date('2025-06-23T13:23:08.6258337'),
      uploadedBy: 'ees-test.bau1@education.gov.uk',
    },
    fail: {
      id: 'acccf244-1a1b-450b-b441-08ddafffd9e4',
      dataSetTitle: 'fail',
      dataFileName: 'absence-fail.csv',
      dataFileSize: '677 Kb',
      metaFileName: 'absence-fail.meta.csv',
      metaFileSize: '2 Kb',
      screeningStatus: 'FailedScreening',
      screenerResult: {
        overallResult: 'Passed',
        passed: true,
        testResults: [
          {
            id: 'fail-test-1',
            testFunctionName: 'check_filename_spaces',
            result: 'PASS',
            notes: "'absence-fail.csv' does not have spaces in the filename.",
            stage: 'InitialFileValidation',
          },
          {
            id: 'fail-test-2',
            testFunctionName: 'check_filename_spaces',
            result: 'PASS',
            notes:
              "'absence-fail.meta.csv' does not have spaces in the filename.",
            stage: 'InitialFileValidation',
          },
          {
            id: 'fail-test-3',
            testFunctionName: 'check_empty_cols',
            result: 'FAIL',
            notes:
              "The following columns in 'absence-fail.csv' are empty: 'estab', 'laestab', 'urn', 'academy_type', 'academy_open_date', 'all_through'.",
            stage: 'InitialFileValidation',
          },
        ],
      },
      created: new Date('2025-06-23T13:22:09.0753215'),
      uploadedBy: 'ees-test.bau1@education.gov.uk',
    },
    screening: {
      id: 'b13b2247-ae76-41c7-b442-08ddafffd9e4',
      dataSetTitle: 'screening',
      dataFileName: 'one-pass.csv',
      dataFileSize: '696 B',
      metaFileName: 'one-pass.meta.csv',
      metaFileSize: '210 B',
      screeningStatus: 'Screening',
      created: new Date('2025-06-23T13:23:08.6258337'),
      uploadedBy: 'ees-test.bau1@education.gov.uk',
    },
  };

  test('"Passed" screener file', async () => {
    const { user } = render(
      <table>
        <tbody>
          <DataSetUploadTableRow
            {...rowBaseProps}
            dataSetUpload={fileUploads.pass}
          />
        </tbody>
      </table>,
    );

    const cells = await screen.findAllByRole('cell');
    expect(cells[0]).toHaveTextContent('pass');
    expect(cells[1]).toHaveTextContent('696 B');
    expect(cells[2]).toHaveTextContent('Pending import');

    const detailsButton = screen.getByRole('button', {
      name: 'View details for pass',
    });
    const deleteButton = screen.getByRole('button', {
      name: 'Delete files for pass',
    });
    expect(detailsButton).toBeInTheDocument();
    expect(deleteButton).toBeInTheDocument();

    await user.click(deleteButton);
    expect(
      await screen.findByRole('heading', {
        name: 'Confirm deletion of selected data files',
      }),
    ).toBeInTheDocument();

    const cancelButton = await screen.findByRole('button', {
      name: 'Cancel',
    });

    await user.click(cancelButton);
    await user.click(detailsButton);

    expect(
      await screen.findByRole('heading', {
        name: 'Data set details',
      }),
    ).toBeInTheDocument();

    const allTestsTab = screen.getByRole('tab', {
      name: 'All tests',
    });
    const allTestsTabPanel = screen.getByTestId('screener-results-all');
    expect(allTestsTab).toBeInTheDocument();
    expect(allTestsTabPanel).toBeInTheDocument();

    const fileDetailsTab = screen.getByRole('tab', {
      name: 'File details',
    });
    const fileDetailsTabPanel = screen.getByTestId('file-details');
    expect(fileDetailsTab).toBeInTheDocument();
    expect(fileDetailsTabPanel).toBeInTheDocument();

    user.click(allTestsTab);
    expect(within(allTestsTabPanel).getAllByText('Pass').length).toEqual(3);
    user.click(fileDetailsTab);
    expect(
      within(fileDetailsTabPanel).getAllByText('Pending import').length,
    ).toEqual(1);

    user.click(screen.getByRole('button', { name: 'Continue import' }));
    await waitFor(async () =>
      expect(rowBaseProps.onImportDataSets).toHaveBeenCalled(),
    );
  });

  test('"Passed with warnings" screener file', async () => {
    const { user } = render(
      <table>
        <tbody>
          <DataSetUploadTableRow
            {...rowBaseProps}
            dataSetUpload={fileUploads.passAndWarning}
          />
        </tbody>
      </table>,
    );

    const cells = screen.getAllByRole('cell');
    expect(cells[0]).toHaveTextContent('pass');
    expect(cells[1]).toHaveTextContent('696 B');
    expect(cells[2]).toHaveTextContent('Pending import');

    const detailsButton = screen.getByRole('button', {
      name: 'View details for pass',
    });
    const deleteButton = screen.getByRole('button', {
      name: 'Delete files for pass',
    });
    expect(detailsButton).toBeInTheDocument();
    expect(deleteButton).toBeInTheDocument();

    user.click(deleteButton);
    await waitFor(async () => {
      expect(
        screen.getByRole('heading', {
          name: 'Confirm deletion of selected data files',
        }),
      ).toBeInTheDocument();
      user.click(screen.getByRole('button', { name: 'Cancel' }));
    });

    user.click(detailsButton);
    await waitFor(async () => {
      expect(
        screen.getByRole('heading', {
          name: 'Data set details',
        }),
      ).toBeInTheDocument();
    });

    const allTestsTab = screen.getByRole('tab', {
      name: 'All tests',
    });
    const allTestsTabPanel = screen.getByTestId('screener-results-all');
    expect(allTestsTab).toBeInTheDocument();
    expect(allTestsTabPanel).toBeInTheDocument();

    const fileDetailsTab = screen.getByRole('tab', {
      name: 'File details',
    });
    const fileDetailsTabPanel = screen.getByTestId('file-details');
    expect(fileDetailsTab).toBeInTheDocument();
    expect(fileDetailsTabPanel).toBeInTheDocument();

    const warningsTab = screen.getByRole('tab', {
      name: 'Warnings',
    });
    const warningsTabPanel = screen.getByTestId('screener-results-filtered');
    expect(warningsTab).toBeInTheDocument();
    expect(warningsTabPanel).toBeInTheDocument();

    user.click(allTestsTab);
    expect(within(allTestsTabPanel).getAllByText('Pass').length).toEqual(1);
    expect(
      within(
        within(allTestsTabPanel).getByTestId('screener-result-table'),
      ).getAllByText('Warning').length,
    ).toEqual(2);

    user.click(fileDetailsTab);
    expect(
      within(fileDetailsTabPanel).getAllByText('Pending import').length,
    ).toEqual(1);

    const importButton = screen.getByRole('button', {
      name: 'Continue import with warnings',
    });
    expect(importButton).toBeAriaDisabled();

    user.click(warningsTab);
    expect(
      within(warningsTabPanel).getByRole('heading', {
        name: 'Screener test warnings',
      }),
    ).toBeInTheDocument();

    user.click(
      within(warningsTabPanel).getByLabelText('check_filename_spaces'),
    );
    user.click(within(warningsTabPanel).getByLabelText('check_empty_cols'));

    await waitFor(async () => expect(importButton).not.toBeAriaDisabled());
    user.click(importButton);
    await waitFor(async () =>
      expect(rowBaseProps.onImportDataSets).toHaveBeenCalled(),
    );
  });

  test('keeps acknowledged warnings when the details modal is reopened', async () => {
    const { user } = render(
      <table>
        <tbody>
          <DataSetUploadTableRow
            {...rowBaseProps}
            dataSetUpload={fileUploads.passAndWarning}
          />
        </tbody>
      </table>,
    );

    const detailsButton = screen.getByRole('button', {
      name: 'View details for pass',
    });

    await user.click(detailsButton);
    await user.click(screen.getByRole('tab', { name: 'Warnings' }));

    const warningsTabPanel = screen.getByTestId('screener-results-filtered');
    await user.click(
      within(warningsTabPanel).getByLabelText('check_filename_spaces'),
    );

    await waitFor(() =>
      expect(
        screen.getByRole('button', { name: 'Continue import with warnings' }),
      ).toBeAriaDisabled(),
    );

    await user.click(screen.getByRole('button', { name: 'Cancel' }));
    await waitFor(() =>
      expect(
        screen.queryByRole('heading', { name: 'Data set details' }),
      ).not.toBeInTheDocument(),
    );

    await user.click(detailsButton);
    await user.click(screen.getByRole('tab', { name: 'Warnings' }));

    const reopenedPanel = screen.getByTestId('screener-results-filtered');
    expect(
      within(reopenedPanel).getByLabelText('check_filename_spaces'),
    ).toBeChecked();
    expect(
      within(reopenedPanel).getByLabelText('check_empty_cols'),
    ).not.toBeChecked();
    expect(
      screen.getByRole('button', { name: 'Continue import with warnings' }),
    ).toBeAriaDisabled();
    await user.click(within(reopenedPanel).getByLabelText('check_empty_cols'));
    expect(
      screen.getByRole('button', { name: 'Continue import with warnings' }),
    ).not.toBeAriaDisabled();
    await user.click(
      within(reopenedPanel).getByLabelText('check_filename_spaces'),
    );
    expect(
      screen.getByRole('button', { name: 'Continue import with warnings' }),
    ).toBeAriaDisabled();
  });

  test('shows replacement wording when isReplacement is set', async () => {
    const { user } = render(
      <table>
        <tbody>
          <DataSetUploadTableRow
            {...rowBaseProps}
            isReplacement
            dataSetUpload={fileUploads.pass}
          />
        </tbody>
      </table>,
    );

    // The upload below carries no replacingFileId, so this asserts the prop is
    // what drives the wording rather than the shape of the data.
    expect(fileUploads.pass.replacingFileId).toBeUndefined();

    const cancelButton = screen.getByRole('button', {
      name: 'Cancel replacement for pass',
    });
    expect(cancelButton).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Delete files for pass' }),
    ).not.toBeInTheDocument();

    await user.click(cancelButton);

    expect(
      await screen.findByRole('heading', { name: 'Cancel replacement' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        'Are you sure you want to cancel this data replacement? The pending replacement data file will be deleted.',
      ),
    ).toBeInTheDocument();
  });

  test('shows deletion wording when isReplacement is not set', async () => {
    const { user } = render(
      <table>
        <tbody>
          <DataSetUploadTableRow
            {...rowBaseProps}
            dataSetUpload={fileUploads.pass}
          />
        </tbody>
      </table>,
    );

    const deleteButton = screen.getByRole('button', {
      name: 'Delete files for pass',
    });
    expect(deleteButton).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Cancel replacement for pass' }),
    ).not.toBeInTheDocument();

    await user.click(deleteButton);

    expect(
      await screen.findByRole('heading', {
        name: 'Confirm deletion of selected data files',
      }),
    ).toBeInTheDocument();
  });

  test('"Failed screening" screener file', async () => {
    const { user } = render(
      <table>
        <tbody>
          <DataSetUploadTableRow
            {...rowBaseProps}
            dataSetUpload={fileUploads.fail}
          />
        </tbody>
      </table>,
    );

    const cells = screen.getAllByRole('cell');
    expect(cells[0]).toHaveTextContent('fail');
    expect(cells[1]).toHaveTextContent('677 Kb');
    expect(cells[2]).toHaveTextContent('Failed screening');

    const detailsButton = screen.getByRole('button', {
      name: 'View details for fail',
    });
    const deleteButton = screen.getByRole('button', {
      name: 'Delete files for fail',
    });
    expect(detailsButton).toBeInTheDocument();
    expect(deleteButton).toBeInTheDocument();

    user.click(deleteButton);
    await waitFor(async () => {
      expect(
        screen.getByRole('heading', {
          name: 'Confirm deletion of selected data files',
        }),
      ).toBeInTheDocument();
      user.click(screen.getByRole('button', { name: 'Cancel' }));
    });

    user.click(detailsButton);
    await waitFor(async () => {
      expect(
        screen.getByRole('heading', {
          name: 'Data set details',
        }),
      ).toBeInTheDocument();
    });

    const allTestsTab = screen.getByRole('tab', {
      name: 'All tests',
    });
    const allTestsTabPanel = screen.getByTestId('screener-results-all');
    expect(allTestsTab).toBeInTheDocument();
    expect(allTestsTabPanel).toBeInTheDocument();

    const fileDetailsTab = screen.getByRole('tab', {
      name: 'File details',
    });
    const fileDetailsTabPanel = screen.getByTestId('file-details');
    expect(fileDetailsTab).toBeInTheDocument();
    expect(fileDetailsTabPanel).toBeInTheDocument();

    const failuresAndWarningsTab = screen.getByRole('tab', {
      name: 'Failures',
    });
    const failuresAndWarningsPanel = screen.getByTestId(
      'screener-results-filtered',
    );
    expect(failuresAndWarningsTab).toBeInTheDocument();
    expect(failuresAndWarningsPanel).toBeInTheDocument();

    user.click(allTestsTab);
    expect(within(allTestsTabPanel).getAllByText('Pass').length).toEqual(2);
    expect(within(allTestsTabPanel).getAllByText('Fail').length).toEqual(1);
    user.click(fileDetailsTab);
    expect(
      within(fileDetailsTabPanel).getAllByText('Failed screening').length,
    ).toEqual(1);
    expect(
      within(fileDetailsTabPanel).getByText(
        'You will need to delete this file (close this window, and select "Delete files"), fix the failed tests and upload again. If you have any questions, please get in touch with the explore.statistics@education.gov.uk team.',
      ),
    ).toBeInTheDocument();
  });

  test('"Screening" screener file', async () => {
    releaseDataFileService.getDataFileScreeningStatus.mockResolvedValue({
      percentageComplete: 10,
      stage: 'screening',
      status: 'Screening',
      completed: false,
    });

    const { user } = render(
      <table>
        <tbody>
          <DataSetUploadTableRow
            {...rowBaseProps}
            dataSetUpload={fileUploads.screening}
          />
        </tbody>
      </table>,
    );

    await waitFor(() => {
      expect(
        releaseDataFileService.getDataFileScreeningStatus,
      ).toHaveBeenCalled();
    });

    const cells = await screen.findAllByRole('cell');
    expect(cells[0]).toHaveTextContent('screening');
    expect(cells[1]).toHaveTextContent('696 B');
    expect(cells[2]).toHaveTextContent('Screening');
    expect(cells[2]).toHaveTextContent('10%');

    expect(
      screen.queryByRole('button', {
        name: 'Delete files for screening',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Cancel',
      }),
    ).not.toBeInTheDocument();

    const detailsButton = screen.getByRole('button', {
      name: 'View details for screening',
    });
    expect(detailsButton).toBeInTheDocument();

    await user.click(detailsButton);

    expect(
      await screen.findByRole('heading', {
        name: 'Data set details',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('tab', {
        name: 'All tests',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('tab', {
        name: 'File details',
      }),
    ).not.toBeInTheDocument();

    const fileDetailsTabPanel = screen.getByTestId('file-details');
    expect(fileDetailsTabPanel).toBeInTheDocument();

    expect(
      within(fileDetailsTabPanel).getAllByText('Screening').length,
    ).toEqual(1);

    expect(screen.queryByText('Continue import')).not.toBeInTheDocument();

    const cancelButton = await screen.findByRole('button', {
      name: 'Cancel',
    });

    await user.click(cancelButton);

    expect(
      screen.queryByRole('heading', {
        name: 'Data set details',
      }),
    ).not.toBeInTheDocument();
  });

  test('hides the import confirm for a failed screening without override', async () => {
    const { user } = render(
      <table>
        <tbody>
          <DataSetUploadTableRow
            {...rowBaseProps}
            dataSetUpload={fileUploads.fail}
          />
        </tbody>
      </table>,
    );

    await user.click(
      screen.getByRole('button', { name: 'View details for fail' }),
    );

    expect(
      await screen.findByRole('heading', { name: 'Data set details' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: /Continue import/ }),
    ).not.toBeInTheDocument();
  });

  test('offers an override import for a failed screening when permitted', async () => {
    const { user } = render(
      <table>
        <tbody>
          <DataSetUploadTableRow
            {...rowBaseProps}
            permissions={{
              ...rowBaseProps.permissions,
              canOverrideScreenerResult: true,
            }}
            dataSetUpload={fileUploads.fail}
          />
        </tbody>
      </table>,
    );

    await user.click(
      screen.getByRole('button', { name: 'View details for fail' }),
    );

    const importButton = await screen.findByRole('button', {
      name: 'Continue import (override failures)',
    });

    // The override bypasses the warning-acknowledgement gate, which is
    // otherwise what disables this button.
    expect(importButton).not.toBeAriaDisabled();

    await user.click(importButton);

    expect(rowBaseProps.onImportDataSets).toHaveBeenCalledWith([
      fileUploads.fail.id,
    ]);
  });

  test('blocks importing when the release can no longer be updated', async () => {
    const { user } = render(
      <table>
        <tbody>
          <DataSetUploadTableRow
            {...rowBaseProps}
            permissions={{
              ...rowBaseProps.permissions,
              canUpdateRelease: false,
            }}
            dataSetUpload={fileUploads.pass}
          />
        </tbody>
      </table>,
    );

    await user.click(
      screen.getByRole('button', { name: 'View details for pass' }),
    );

    expect(
      await screen.findByRole('heading', { name: 'Data set details' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: /Continue import/ }),
    ).not.toBeInTheDocument();
    // Destructive actions are hidden too.
    expect(
      screen.queryByRole('button', { name: 'Delete files for pass' }),
    ).not.toBeInTheDocument();
  });

  test('refreshes the uploads once screening reaches a terminal status', async () => {
    releaseDataFileService.getDataFileScreeningStatus.mockResolvedValue({
      percentageComplete: 100,
      stage: 'screening',
      status: 'PendingImport',
      completed: true,
    });

    render(
      <table>
        <tbody>
          <DataSetUploadTableRow
            {...rowBaseProps}
            dataSetUpload={fileUploads.screening}
          />
        </tbody>
      </table>,
    );

    expect(await screen.findByText('Pending import')).toBeInTheDocument();

    await waitFor(() =>
      expect(rowBaseProps.onRefreshUploads).toHaveBeenCalled(),
    );
  });

  test('does not poll for screening status when it is already terminal', async () => {
    render(
      <table>
        <tbody>
          <DataSetUploadTableRow
            {...rowBaseProps}
            dataSetUpload={fileUploads.pass}
          />
        </tbody>
      </table>,
    );

    expect(await screen.findByText('Pending import')).toBeInTheDocument();

    expect(
      releaseDataFileService.getDataFileScreeningStatus,
    ).not.toHaveBeenCalled();
    expect(rowBaseProps.onRefreshUploads).not.toHaveBeenCalled();
  });
});
