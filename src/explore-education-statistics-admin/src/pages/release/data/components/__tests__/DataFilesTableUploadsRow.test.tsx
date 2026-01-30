import { DataSetUpload } from '@admin/services/releaseDataFileService';
import render from '@common-test/render';
import { Dictionary } from '@common/types';
import React from 'react';
import { screen, waitFor, within } from '@testing-library/dom';
import DataFilesTableUploadRow from '../DataFilesTableUploadsRow';

describe('DataFilesTableUploadsRow', () => {
  const rowBaseProps = {
    canUpdateRelease: true,
    releaseVersionId: 'release-version-id-1',
    onConfirmDelete: jest.fn(),
    onConfirmImport: jest.fn(),
  };

  const fileUploads: Dictionary<DataSetUpload> = {
    pass: {
      id: 'b13b2247-ae76-41c7-b442-08ddafffd9e4',
      dataSetTitle: 'pass',
      dataFileName: 'one-pass.csv',
      dataFileSize: '696 B',
      metaFileName: 'one-pass.meta.csv',
      metaFileSize: '210 B',
      status: 'PENDING_IMPORT',
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
      status: 'PENDING_IMPORT',
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
      status: 'FAILED_SCREENING',
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
  };

  test('"passed" screener file', async () => {
    const { user } = render(
      <table>
        <tbody>
          <DataFilesTableUploadRow
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
      expect(rowBaseProps.onConfirmImport).toHaveBeenCalled(),
    );
  });

  test('"passed with warnings" screener file', async () => {
    const { user } = render(
      <table>
        <tbody>
          <DataFilesTableUploadRow
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
      expect(rowBaseProps.onConfirmImport).toHaveBeenCalled(),
    );
  });

  test('"failed" screener file', async () => {
    const { user } = render(
      <table>
        <tbody>
          <DataFilesTableUploadRow
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
});
