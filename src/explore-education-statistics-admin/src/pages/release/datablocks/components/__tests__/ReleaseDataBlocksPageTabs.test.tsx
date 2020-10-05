import ReleaseDataBlocksPageTabs from '@admin/pages/release/datablocks/components/ReleaseDataBlocksPageTabs';
import _dataBlockService, {
  ReleaseDataBlock,
} from '@admin/services/dataBlockService';
import _tableBuilderService, {
  SubjectMeta,
  TableDataResponse,
  TimePeriodQuery,
} from '@common/services/tableBuilderService';
import { render, screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/dataBlockService');
jest.mock('@common/services/tableBuilderService');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('ReleaseDataBlocksPageTabs', () => {
  const testSubjectMeta: SubjectMeta = {
    filters: {
      Characteristic: {
        totalValue: '',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        name: 'characteristic',
        options: {
          Gender: {
            label: 'Gender',
            options: [
              {
                label: 'Gender female',
                value: 'gender-female',
              },
            ],
          },
        },
      },
    },
    indicators: {
      AbsenceFields: {
        label: 'Absence fields',
        options: [
          {
            value: 'authorised-absence-sessions',
            label: 'Number of authorised absence sessions',
            unit: '',
            name: 'sess_authorised',
            decimalPlaces: 2,
          },
        ],
      },
    },
    locations: {
      localAuthority: {
        legend: 'Local authority',
        options: [{ value: 'barnet', label: 'Barnet' }],
      },
    },
    timePeriod: {
      legend: 'Time period',
      options: [{ label: '2020/21', code: 'AY', year: 2020 }],
    },
  };

  const testTableData: TableDataResponse = {
    subjectMeta: {
      publicationName: '',
      boundaryLevels: [],
      footnotes: [],
      subjectName: 'Subject 1',
      geoJsonAvailable: false,
      locations: [
        {
          level: 'localAuthority',
          label: 'Barnet',
          value: 'barnet',
        },
      ],
      timePeriodRange: [{ code: 'AY', year: 2020, label: '2020/21' }],
      indicators: [
        {
          value: 'authorised-absence-sessions',
          label: 'Number of authorised absence sessions',
          unit: '',
          name: 'sess_authorised',
          decimalPlaces: 2,
        },
      ],
      filters: {
        Characteristic: {
          totalValue: '',
          hint: 'Filter by pupil characteristic',
          legend: 'Characteristic',
          name: 'characteristic',
          options: {
            Gender: {
              label: 'Gender',
              options: [
                {
                  label: 'Gender female',
                  value: 'gender-female',
                },
              ],
            },
          },
        },
      },
    },
    results: [],
  };

  const testDataBlock: ReleaseDataBlock = {
    name: 'Test data block',
    heading: 'Test title',
    id: 'block-1',
    source: 'Test source',
    highlightName: '',
    query: {
      includeGeoJson: false,
      subjectId: 'subject-1',
      locations: {
        localAuthority: ['barnet'],
      },
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
        columnGroups: [],
        columns: [],
        rowGroups: [],
        rows: [],
      },
    },
    charts: [],
  };

  test('renders uninitialised table tool when no data block is selected', async () => {
    tableBuilderService.getReleaseMeta.mockResolvedValue({
      releaseId: 'release-1',
      subjects: [],
    });

    render(
      <ReleaseDataBlocksPageTabs
        releaseId="release-1"
        onDataBlockSave={noop}
      />,
    );

    await waitFor(() => {
      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(1);
      expect(stepHeadings[0]).toHaveTextContent(
        'Step 1 (current): Choose a subject',
      );

      expect(screen.getAllByRole('listitem')).toHaveLength(1);
    });
  });

  test('does not render table or chart tabs when no data block is selected', async () => {
    tableBuilderService.getReleaseMeta.mockResolvedValue({
      releaseId: 'release-1',
      subjects: [],
    });

    render(
      <ReleaseDataBlocksPageTabs
        releaseId="release-1"
        onDataBlockSave={noop}
      />,
    );

    await waitFor(() => {
      const tabs = screen.getAllByRole('tab');

      expect(tabs).toHaveLength(1);
      expect(tabs[0]).toHaveTextContent('Data source');
    });
  });

  test('renders fully initialised table tool when data block is selected', async () => {
    tableBuilderService.getReleaseMeta.mockResolvedValue({
      releaseId: 'release-1',
      subjects: [],
    });

    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.filterSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.getTableData.mockResolvedValue(testTableData);

    render(
      <ReleaseDataBlocksPageTabs
        releaseId="release-1"
        selectedDataBlock={testDataBlock}
        onDataBlockSave={noop}
      />,
    );

    await waitFor(() => {
      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(5);
      expect(stepHeadings[0]).toHaveTextContent('Step 1: Choose a subject');
      expect(stepHeadings[1]).toHaveTextContent('Step 2: Choose locations');
      expect(stepHeadings[2]).toHaveTextContent('Step 3: Choose time period');
      expect(stepHeadings[3]).toHaveTextContent('Step 4: Choose your filters');
      expect(stepHeadings[4]).toHaveTextContent(
        'Step 5 (current): Update data block',
      );

      expect(screen.getByLabelText('Name')).toHaveValue('Test data block');
      expect(screen.getByLabelText('Table title')).toHaveValue('Test title');
      expect(screen.getByLabelText('Source')).toHaveValue('Test source');
    });
  });

  test('renders table and chart tabs when data block is selected', async () => {
    tableBuilderService.getReleaseMeta.mockResolvedValue({
      releaseId: 'release-1',
      subjects: [],
    });

    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.filterSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.getTableData.mockResolvedValue(testTableData);

    render(
      <ReleaseDataBlocksPageTabs
        releaseId="release-1"
        selectedDataBlock={testDataBlock}
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

  test('renders partially initialised table tool when there are invalid locations in query', async () => {
    tableBuilderService.getReleaseMeta.mockResolvedValue({
      releaseId: 'release-1',
      subjects: [],
    });

    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.filterSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.getTableData.mockResolvedValue(testTableData);

    render(
      <ReleaseDataBlocksPageTabs
        releaseId="release-1"
        selectedDataBlock={{
          ...testDataBlock,
          query: {
            ...testDataBlock.query,
            locations: {
              localAuthority: ['barnet', 'invalid-location'],
            },
          },
        }}
        onDataBlockSave={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          /There is a problem with this data block as some of the selected options are invalid/,
        ),
      ).toBeInTheDocument();

      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(2);
      expect(stepHeadings[0]).toHaveTextContent('Step 1: Choose a subject');
      expect(stepHeadings[1]).toHaveTextContent(
        'Step 2 (current): Choose locations',
      );
    });
  });

  test('renders partially initialised table tool when there are invalid time periods in query', async () => {
    tableBuilderService.getReleaseMeta.mockResolvedValue({
      releaseId: 'release-1',
      subjects: [],
    });

    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.filterSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.getTableData.mockResolvedValue(testTableData);

    render(
      <ReleaseDataBlocksPageTabs
        releaseId="release-1"
        selectedDataBlock={{
          ...testDataBlock,
          query: {
            ...testDataBlock.query,
            timePeriod: {
              ...(testDataBlock.query.timePeriod as TimePeriodQuery),
              endCode: 'AY',
              endYear: 2021,
            },
          },
        }}
        onDataBlockSave={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          /There is a problem with this data block as some of the selected options are invalid/,
        ),
      ).toBeInTheDocument();

      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(3);
      expect(stepHeadings[0]).toHaveTextContent('Step 1: Choose a subject');
      expect(stepHeadings[1]).toHaveTextContent('Step 2: Choose locations');
      expect(stepHeadings[2]).toHaveTextContent(
        'Step 3 (current): Choose time period',
      );
    });
  });

  test('renders partially initialised table tool when there are invalid filters in query', async () => {
    tableBuilderService.getReleaseMeta.mockResolvedValue({
      releaseId: 'release-1',
      subjects: [],
    });

    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.filterSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.getTableData.mockResolvedValue(testTableData);

    render(
      <ReleaseDataBlocksPageTabs
        releaseId="release-1"
        selectedDataBlock={{
          ...testDataBlock,
          query: {
            ...testDataBlock.query,
            filters: ['gender-female', 'invalid-filter'],
          },
        }}
        onDataBlockSave={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          /There is a problem with this data block as some of the selected options are invalid/,
        ),
      ).toBeInTheDocument();

      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(4);
      expect(stepHeadings[0]).toHaveTextContent('Step 1: Choose a subject');
      expect(stepHeadings[1]).toHaveTextContent('Step 2: Choose locations');
      expect(stepHeadings[2]).toHaveTextContent('Step 3: Choose time period');
      expect(stepHeadings[3]).toHaveTextContent(
        'Step 4 (current): Choose your filters',
      );
    });
  });

  test('renders partially initialised table tool when there are invalid indicators in query', async () => {
    tableBuilderService.getReleaseMeta.mockResolvedValue({
      releaseId: 'release-1',
      subjects: [],
    });

    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.filterSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.getTableData.mockResolvedValue(testTableData);

    render(
      <ReleaseDataBlocksPageTabs
        releaseId="release-1"
        selectedDataBlock={{
          ...testDataBlock,
          query: {
            ...testDataBlock.query,
            indicators: ['authorised-absence-sessions', 'invalid-indicator'],
          },
        }}
        onDataBlockSave={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          /There is a problem with this data block as some of the selected options are invalid/,
        ),
      ).toBeInTheDocument();

      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(4);
      expect(stepHeadings[0]).toHaveTextContent('Step 1: Choose a subject');
      expect(stepHeadings[1]).toHaveTextContent('Step 2: Choose locations');
      expect(stepHeadings[2]).toHaveTextContent('Step 3: Choose time period');
      expect(stepHeadings[3]).toHaveTextContent(
        'Step 4 (current): Choose your filters',
      );
    });
  });

  describe('updating data block', () => {
    test('submitting data source form calls correct service to update data block', async () => {
      tableBuilderService.getReleaseMeta.mockResolvedValue({
        releaseId: 'release-1',
        subjects: [],
      });

      tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
      tableBuilderService.filterSubjectMeta.mockResolvedValue(testSubjectMeta);
      tableBuilderService.getTableData.mockResolvedValue(testTableData);

      render(
        <ReleaseDataBlocksPageTabs
          releaseId="release-1"
          selectedDataBlock={testDataBlock}
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
      await userEvent.type(nameInput, 'Updated test data block');

      userEvent.clear(titleInput);
      await userEvent.type(titleInput, 'Updated test title');

      userEvent.clear(sourceInput);
      await userEvent.type(sourceInput, 'Updated test source');

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
