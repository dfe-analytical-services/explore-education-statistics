import DataSetUploadSummaryList from '@admin/pages/release/data/components/data-uploads/data-set-uploads/DataSetUploadSummaryList';
import { DataSetUpload } from '@admin/services/releaseDataFileService';
import render from '@common-test/render';
import { screen } from '@testing-library/react';
import React from 'react';

describe('DataSetUploadSummaryList', () => {
  const testDataSetUpload: DataSetUpload = {
    id: 'data-set-upload-1',
    dataSetTitle: 'Test data set',
    dataFileName: 'test-data.csv',
    dataFileSize: '805 Mb',
    metaFileName: 'test-data.meta.csv',
    metaFileSize: '2 Kb',
    screeningStatus: 'PendingImport',
    created: new Date('2026-07-02T12:51:00'),
    uploadedBy: 'test@education.gov.uk',
    publicApiCompatible: true,
  };

  test('renders the data set upload details', () => {
    render(
      <DataSetUploadSummaryList
        releaseVersionId="release-version-1"
        dataSetUpload={testDataSetUpload}
      />,
    );

    expect(screen.getByTestId('Title')).toHaveTextContent('Test data set');
    expect(screen.getByTestId('Data file')).toHaveTextContent('test-data.csv');
    expect(screen.getByTestId('Meta file')).toHaveTextContent(
      'test-data.meta.csv',
    );
    expect(screen.getByTestId('Size')).toHaveTextContent('805 Mb');
    expect(screen.getByTestId('Status')).toHaveTextContent('Pending import');
    expect(screen.getByTestId('Uploaded by')).toHaveTextContent(
      'test@education.gov.uk',
    );
    expect(screen.getByTestId('Date uploaded')).toHaveTextContent(
      '2 July 2026, 12:51',
    );
    expect(screen.getByTestId('API compatible')).toHaveTextContent('Yes');
  });

  test('renders the Screening tag for API compatible whilst screening', () => {
    render(
      <DataSetUploadSummaryList
        releaseVersionId="release-version-1"
        dataSetUpload={{
          ...testDataSetUpload,
          screeningStatus: 'Screening',
          publicApiCompatible: false,
        }}
      />,
    );

    expect(screen.getByTestId('Status')).toHaveTextContent('Screening');
    expect(screen.getByTestId('API compatible')).toHaveTextContent('Screening');
    expect(screen.getByTestId('API compatible')).not.toHaveTextContent('No');
  });

  test.each<[boolean | undefined, string]>([
    [true, 'Yes'],
    [false, 'No'],
    [undefined, 'Not available'],
  ])(
    'renders API compatible as %s once screening has finished',
    (publicApiCompatible, expected) => {
      render(
        <DataSetUploadSummaryList
          releaseVersionId="release-version-1"
          dataSetUpload={{
            ...testDataSetUpload,
            screeningStatus: 'PendingReview',
            publicApiCompatible,
          }}
        />,
      );

      expect(screen.getByTestId('Status')).toHaveTextContent('Pending review');
      expect(screen.getByTestId('API compatible')).toHaveTextContent(expected);
    },
  );
});
