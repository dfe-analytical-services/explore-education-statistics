import DataFileUploadForm from '@admin/pages/release/data/components/DataFileUploadForm';
import _releaseDataFileService from '@admin/services/releaseDataFileService';
import { screen, waitFor } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import render from '@common-test/render';

jest.mock('@admin/services/releaseDataFileService');

const releaseDataFileService = _releaseDataFileService as jest.Mocked<
  typeof _releaseDataFileService
>;

describe('DataFileUploadForm', () => {
  test('shows validation message when no data file selected', async () => {
    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={noop}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a data file', {
          selector: '#dataFileUploadForm-dataFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when data file is not empty', async () => {
    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={noop}
      />,
    );

    const file = new File([], 'test.csv', {
      type: 'text/csv',
    });

    await user.upload(screen.getByLabelText('Upload data file'), file);
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a data file that is not empty', {
          selector: '#dataFileUploadForm-dataFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when no meta data file selected', async () => {
    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={noop}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a metadata file', {
          selector: '#dataFileUploadForm-metadataFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when metadata file is not empty', async () => {
    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={noop}
      />,
    );

    const file = new File([], 'test.csv', {
      type: 'text/csv',
    });

    await user.upload(screen.getByLabelText('Upload metadata file'), file);
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a metadata file that is not empty', {
          selector: '#dataFileUploadForm-metadataFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when no ZIP file selected', async () => {
    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={noop}
      />,
    );

    await user.click(screen.getByLabelText('ZIP file'));
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a zip file', {
          selector: '#dataFileUploadForm-zipFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when ZIP file is empty', async () => {
    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={noop}
      />,
    );

    const file = new File([], 'test.zip', {
      type: 'application/zip',
    });

    await user.click(screen.getByLabelText('ZIP file'));
    await user.upload(screen.getByLabelText('Upload ZIP file'), file);
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a ZIP file that is not empty', {
          selector: '#dataFileUploadForm-zipFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation message when no bulk ZIP file selected', async () => {
    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={noop}
      />,
    );

    await user.click(screen.getByLabelText('Bulk ZIP upload'));
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a zip file', {
          selector: '#dataFileUploadForm-bulkZipFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('form submits and calls the correct endpoint csv files are used', async () => {
    const onSubmit = jest.fn();
    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={onSubmit}
      />,
    );

    const file = new File(['hello, world!'], 'test.csv', {
      type: 'application/csv',
    });
    const metaFile = new File(['hello, world!'], 'test.meta.csv', {
      type: 'application/csv',
    });

    await user.type(screen.getByLabelText('Title'), 'Test title');
    await user.click(screen.getByLabelText('CSV files'));
    await user.upload(screen.getByLabelText('Upload data file'), file);
    await user.upload(screen.getByLabelText('Upload metadata file'), metaFile);

    expect(releaseDataFileService.uploadDataSetFilePair).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    expect(releaseDataFileService.uploadDataSetFilePair).toHaveBeenCalledTimes(
      1,
    );

    expect(releaseDataFileService.uploadDataSetFilePair).toHaveBeenCalledWith(
      'release-version-id',
      {
        title: 'Test title',
        dataFile: file,
        metadataFile: metaFile,
      },
    );

    expect(onSubmit).toHaveBeenCalledTimes(1);
  });

  test('form submits and calls the correct endpoint when a zip file is used', async () => {
    const onSubmit = jest.fn();
    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={onSubmit}
      />,
    );

    const file = new File(['hello, world!'], 'test.zip', {
      type: 'application/zip',
    });

    await user.type(screen.getByLabelText('Title'), 'Test title');
    await user.click(screen.getByLabelText('ZIP file'));
    await user.upload(screen.getByLabelText('Upload ZIP file'), file);

    expect(
      releaseDataFileService.uploadZippedDataSetFilePair,
    ).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    expect(
      releaseDataFileService.uploadZippedDataSetFilePair,
    ).toHaveBeenCalledTimes(1);

    expect(
      releaseDataFileService.uploadZippedDataSetFilePair,
    ).toHaveBeenCalledWith('release-version-id', {
      title: 'Test title',
      zipFile: file,
    });

    expect(onSubmit).toHaveBeenCalledTimes(1);
  });

  test('form submits and calls the correct endpoint when a bulk zip file is used', async () => {
    const onSubmit = jest.fn();
    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={onSubmit}
      />,
    );

    const file = new File(['hello, world!'], 'test.zip', {
      type: 'application/zip',
    });

    await user.click(screen.getByLabelText('Bulk ZIP upload'));
    await user.upload(screen.getByLabelText('Upload bulk ZIP file'), file);

    expect(
      releaseDataFileService.uploadBulkZipDataSetFile,
    ).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    expect(
      releaseDataFileService.uploadBulkZipDataSetFile,
    ).toHaveBeenCalledTimes(1);

    expect(
      releaseDataFileService.uploadBulkZipDataSetFile,
    ).toHaveBeenCalledWith('release-version-id', file);

    expect(onSubmit).toHaveBeenCalledTimes(1);
  });

  test('shows validation message when bulk ZIP file is empty', async () => {
    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={noop}
      />,
    );

    const file = new File([], 'test.zip', {
      type: 'application/zip',
    });

    await user.click(screen.getByLabelText('Bulk ZIP upload'));
    await user.upload(screen.getByLabelText('Upload bulk ZIP file'), file);
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Choose a ZIP file that is not empty', {
          selector: '#dataFileUploadForm-bulkZipFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('cannot submit with invalid values when trying to upload CSV files', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={handleSubmit}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();

      expect(
        screen.getByText('Choose a data file', {
          selector: '#dataFileUploadForm-dataFile-error',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByText('Choose a metadata file', {
          selector: '#dataFileUploadForm-metadataFile-error',
        }),
      ).toBeInTheDocument();
      expect(
        screen.queryByText('Choose a zip file', {
          selector: '#dataFileUploadForm-metadataFile-error',
        }),
      ).not.toBeInTheDocument();
    });
  });

  test('cannot submit with invalid values when trying to upload ZIP file', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByLabelText('ZIP file'));
    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();

      expect(
        screen.queryByText('Choose a data file', {
          selector: '#dataFileUploadForm-dataFile-error',
        }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByText('Choose a metadata file', {
          selector: '#dataFileUploadForm-metadataFile-error',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.getByText('Choose a zip file', {
          selector: '#dataFileUploadForm-zipFile-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows the title field when not a data replacement', () => {
    render(
      <DataFileUploadForm
        releaseVersionId="release-version-id"
        onSubmit={noop}
      />,
    );
    expect(screen.getByLabelText('Title')).toBeInTheDocument();
  });

  test('does not show title field for data replacement', () => {
    render(
      <DataFileUploadForm
        isDataReplacement
        releaseVersionId="release-version-id"
        dataFileTitle="Test title"
        onSubmit={noop}
      />,
    );
    expect(screen.queryByLabelText('Title')).not.toBeInTheDocument();
  });

  test('submits data replacement correctly', async () => {
    const onSubmit = jest.fn();
    const { user } = render(
      <DataFileUploadForm
        isDataReplacement
        releaseVersionId="release-version-id"
        dataFileTitle="Test title"
        onSubmit={onSubmit}
      />,
    );

    const file = new File(['hello, world!'], 'test.zip', {
      type: 'application/zip',
    });

    await user.click(screen.getByLabelText('ZIP file'));
    await user.upload(screen.getByLabelText('Upload ZIP file'), file);

    expect(
      releaseDataFileService.uploadZippedDataSetFilePair,
    ).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', {
        name: 'Upload data files',
      }),
    );

    expect(
      releaseDataFileService.uploadZippedDataSetFilePair,
    ).toHaveBeenCalledTimes(1);

    expect(
      releaseDataFileService.uploadZippedDataSetFilePair,
    ).toHaveBeenCalledWith('release-version-id', {
      title: 'Test title',
      zipFile: file,
    });

    expect(onSubmit).toHaveBeenCalledTimes(1);
  });

  test('shows a warning if uploading a data replacement when using the main form', async () => {
    const { user } = render(
      <DataFileUploadForm
        dataSetFileTitles={['Test title']}
        releaseVersionId="release-version-id"
        onSubmit={noop}
      />,
    );
    await user.type(screen.getByLabelText('Title'), 'Test title');
    await user.tab();
    expect(
      screen.getByText(
        /Using this title will trigger a data replacement on the matching file/,
      ),
    ).toBeInTheDocument();
  });
});
