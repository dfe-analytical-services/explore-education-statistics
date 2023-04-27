import {
  testSubjectMeta,
  testTableData,
} from '@admin/pages/release/datablocks/__data__/tableToolServiceData';
import DataBlockPageTabs from '@admin/pages/release/datablocks/components/DataBlockPageTabs';
import _dataBlockService, {
  ReleaseDataBlock,
} from '@admin/services/dataBlockService';
import _tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@admin/services/dataBlockService');
jest.mock('@common/services/tableBuilderService');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('DataBlockPageTabs', () => {
  const testDataBlock: ReleaseDataBlock = {
    name: 'Test data block',
    heading: 'Test title',
    id: 'block-1',
    source: 'Test source',
    highlightName: '',
    highlightDescription: '',
    query: {
      includeGeoJson: false,
      subjectId: 'subject-1',
      locationIds: ['barnet'],
      timePeriod: {
        startYear: 2020,
        startCode: 'AY',
        endYear: 2020,
        endCode: 'AY',
      },
      filters: ['gender-female'],
      indicators: ['authorised-absence-sessions'],
    },
    table: {
      indicators: [],
      tableHeaders: {
        columnGroups: [[{ type: 'TimePeriod', value: '2020_AY' }]],
        columns: [{ type: 'Filter', value: 'gender-female' }],
        rowGroups: [
          [{ type: 'Location', value: 'barnet', level: 'localAuthority' }],
        ],
        rows: [{ type: 'Indicator', value: 'authorised-absence-sessions' }],
      },
    },
    charts: [],
  };

  const testSubjects: Subject[] = [
    {
      id: 'subject-1',
      name: 'Test subject',
      content: '<p>Test content</p>',
      timePeriods: {
        from: '2018',
        to: '2020',
      },
      geographicLevels: ['National'],
      file: {
        id: 'file-1',
        name: 'Test subject',
        fileName: 'file-1.csv',
        extension: 'csv',
        size: '10 Mb',
        type: 'Data',
      },
    },
  ];

  test('renders uninitialised table tool when no data block is selected', async () => {
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);

    render(<DataBlockPageTabs releaseId="release-1" onDataBlockSave={noop} />);

    await waitFor(() => {
      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(1);
      expect(stepHeadings[0]).toHaveTextContent(
        'Step 1 (current) Choose a subject',
      );

      expect(screen.getByTestId('wizardStep-1')).toBeVisible();
      expect(screen.getByTestId('wizardStep-2')).not.toBeVisible();

      const radios = screen.getAllByRole('radio');
      expect(radios).toHaveLength(1);
      expect(radios[0]).toEqual(
        within(screen.getByTestId('wizardStep-1')).getByLabelText(
          'Test subject',
        ),
      );
    });
  });

  test('renders the data source form without other tabs when no data block is selected', async () => {
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);

    render(<DataBlockPageTabs releaseId="release-1" onDataBlockSave={noop} />);

    await waitFor(() => {
      expect(screen.getByText('Data source')).toBeInTheDocument();
    });
    expect(screen.queryByRole('tab')).not.toBeInTheDocument();
    expect(screen.queryByRole('tabpanel')).not.toBeInTheDocument();
    expect(screen.queryByText('Table')).not.toBeInTheDocument();
    expect(screen.queryByText('Chart')).not.toBeInTheDocument();
  });

  test('renders fully initialised table tool when data block is selected', async () => {
    tableBuilderService.listReleaseSubjects.mockResolvedValue([]);
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.getTableData.mockResolvedValue(testTableData);

    render(
      <DataBlockPageTabs
        releaseId="release-1"
        dataBlock={testDataBlock}
        onDataBlockSave={noop}
      />,
    );

    await waitFor(() => {
      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(5);
      expect(stepHeadings[0]).toHaveTextContent('Step 1 Choose a subject');
      expect(stepHeadings[1]).toHaveTextContent('Step 2 Choose locations');
      expect(stepHeadings[2]).toHaveTextContent('Step 3 Choose time period');
      expect(stepHeadings[3]).toHaveTextContent('Step 4 Choose your filters');
      expect(stepHeadings[4]).toHaveTextContent(
        'Step 5 (current) Update data block',
      );

      expect(screen.getByLabelText('Name')).toHaveValue('Test data block');
      expect(screen.getByLabelText('Table title')).toHaveValue('Test title');
      expect(screen.getByLabelText('Source')).toHaveValue('Test source');
    });
  });

  test('renders table and chart tabs when data block is selected', async () => {
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.getTableData.mockResolvedValue(testTableData);

    render(
      <DataBlockPageTabs
        releaseId="release-1"
        dataBlock={testDataBlock}
        onDataBlockSave={noop}
      />,
    );

    await waitFor(() => {
      const tabs = screen.getAllByRole('tab');

      expect(tabs).toHaveLength(3);
      expect(tabs[0]).toHaveTextContent('Data source');
      expect(tabs[1]).toHaveTextContent('Table');
      expect(tabs[2]).toHaveTextContent('Chart');
    });
  });

  test('renders partially initialised table tool when there are no table results', async () => {
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.getTableData.mockResolvedValue({
      ...testTableData,
      results: [],
    });

    render(
      <DataBlockPageTabs
        releaseId="release-1"
        dataBlock={testDataBlock}
        onDataBlockSave={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          /There is a problem with this data block as we could not render a table/,
        ),
      ).toBeInTheDocument();

      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(2);
      expect(stepHeadings[0]).toHaveTextContent('Step 1 Choose a subject');
      expect(stepHeadings[1]).toHaveTextContent(
        'Step 2 (current) Choose locations',
      );
    });
  });

  test('renders partially initialised table tool when table header configuration would be invalid', async () => {
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.getTableData.mockResolvedValue(testTableData);

    render(
      <DataBlockPageTabs
        releaseId="release-1"
        dataBlock={{
          ...testDataBlock,
          table: {
            indicators: [],
            tableHeaders: {
              ...testDataBlock.table.tableHeaders,
              rows: [
                { type: 'Indicator', value: 'authorised-absence-sessions' },
                { type: 'Indicator', value: 'not-a-valid-indicator' },
              ],
            },
          },
        }}
        onDataBlockSave={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          /There is a problem with this data block as we could not render a table/,
        ),
      ).toBeInTheDocument();

      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(2);
      expect(stepHeadings[0]).toHaveTextContent('Step 1 Choose a subject');
      expect(stepHeadings[1]).toHaveTextContent(
        'Step 2 (current) Choose locations',
      );
    });
  });

  describe('updating data block', () => {
    test('submitting data source form calls correct service to update data block', async () => {
      tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);
      tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
      tableBuilderService.getTableData.mockResolvedValue(testTableData);

      render(
        <DataBlockPageTabs
          releaseId="release-1"
          dataBlock={testDataBlock}
          onDataBlockSave={noop}
        />,
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Name')).toHaveValue('Test data block');
        expect(screen.getByLabelText('Table title')).toHaveValue('Test title');
        expect(screen.getByLabelText('Source')).toHaveValue('Test source');
      });

      const nameInput = screen.getByLabelText('Name');
      const titleInput = screen.getByLabelText('Table title');
      const sourceInput = screen.getByLabelText('Source');

      userEvent.clear(nameInput);
      userEvent.type(nameInput, 'Updated test data block');

      userEvent.clear(titleInput);
      userEvent.type(titleInput, 'Updated test title');

      userEvent.clear(sourceInput);
      userEvent.type(sourceInput, 'Updated test source');

      expect(dataBlockService.updateDataBlock).not.toBeCalled();

      userEvent.click(screen.getByRole('button', { name: 'Save data block' }));

      await waitFor(() => {
        expect(dataBlockService.updateDataBlock).toBeCalledTimes(1);
        expect(dataBlockService.updateDataBlock).toBeCalledWith('block-1', {
          ...testDataBlock,
          name: 'Updated test data block',
          heading: 'Updated test title',
          source: 'Updated test source',
        } as ReleaseDataBlock);
      });
    });
  });
});
