import DataFileReplacementPlan from '@admin/pages/release/data/components/DataFileReplacementPlan';
import _dataBlockService from '@admin/services/dataBlockService';
import _dataReplacementService, {
  DataReplacementPlan,
} from '@admin/services/dataReplacementService';
import _footnoteService from '@admin/services/footnoteService';
import _releaseDataFileService from '@admin/services/releaseDataFileService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/dataBlockService');
jest.mock('@admin/services/dataReplacementService');
jest.mock('@admin/services/footnoteService');
jest.mock('@admin/services/releaseDataFileService');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;
const dataReplacementService = _dataReplacementService as jest.Mocked<
  typeof _dataReplacementService
>;
const footnoteService = _footnoteService as jest.Mocked<
  typeof _footnoteService
>;
const releaseDataFileService = _releaseDataFileService as jest.Mocked<
  typeof _releaseDataFileService
>;

describe('DataReplacementPlan', () => {
  const testReplacementPlan: DataReplacementPlan = {
    dataBlocks: [
      {
        id: 'block-1',
        name: 'Data block 1',
        filters: {
          'filter-1': {
            id: 'filter-1',
            label: 'Filter 1',
            valid: false,
            groups: {
              'group-1': {
                id: 'group-1',
                label: 'Group 1',
                valid: false,
                filters: [
                  {
                    id: 'item-1',
                    label: 'Item 1',
                    valid: false,
                  },
                  {
                    id: 'item-2',
                    label: 'Item 2',
                    target: 'target-item-2',
                    valid: true,
                  },
                ],
              },
              'group-2': {
                id: 'group-2',
                label: 'Group 2',
                valid: true,
                filters: [
                  {
                    id: 'item-3',
                    label: 'Item 3',
                    target: 'target-item-3',
                    valid: true,
                  },
                ],
              },
            },
          },
          'filter-2': {
            id: 'filter-2',
            label: 'Filter 2',
            target: 'target-filter-2',
            valid: false,
            groups: {
              'group-3': {
                id: 'group-3',
                label: 'Group 3',
                target: 'target-group-3',
                valid: true,
                filters: [
                  {
                    id: 'item-4',
                    label: 'Item 4',
                    target: 'target-item-4',
                    valid: true,
                  },
                ],
              },
              'group-4': {
                id: 'group-4',
                label: 'Group 4',
                valid: false,
                filters: [
                  {
                    id: 'item-5',
                    label: 'Item 5',
                    valid: false,
                  },
                  {
                    id: 'item-6',
                    label: 'Item 6',
                    valid: false,
                  },
                ],
              },
            },
          },
        },
        indicatorGroups: {
          'indicator-group-1': {
            id: 'indicator-group-1',
            label: 'Indicator group 1',
            valid: true,
            indicators: [
              {
                id: 'indicator-1',
                label: 'Indicator 1',
                name: 'indicator_1',
                target: 'target-indicator-1',
                valid: true,
              },
            ],
          },
          'indicator-group-2': {
            id: 'indicator-group-2',
            label: 'Indicator group 2',
            valid: false,
            indicators: [
              {
                id: 'indicator-2',
                label: 'Indicator 2',
                name: 'indicator_2',
                target: 'target-indicator-2',
                valid: true,
              },
              {
                id: 'indicator-3',
                label: 'Indicator 3',
                name: 'indicator_3',
                valid: false,
              },
            ],
          },
          'indicator-group-3': {
            id: 'indicator-group-3',
            label: 'Indicator group 3',
            valid: false,
            indicators: [
              {
                id: 'indicator-4',
                label: 'Indicator 4',
                name: 'indicator_4',
                target: 'target-indicator-4',
                valid: false,
              },
            ],
          },
        },
        locations: {
          Country: {
            label: 'Country',
            valid: true,
            observationalUnits: [
              {
                code: 'england',
                label: 'England',
                target: 'england',
                valid: true,
              },
            ],
          },
          LocalAuthority: {
            label: 'Local Authority',
            valid: false,
            observationalUnits: [
              {
                code: 'barnsley',
                label: 'Barnsley',
                target: 'barnsley',
                valid: true,
              },
              {
                code: 'birmingham',
                label: 'Birmingham',
                valid: false,
              },
            ],
          },
        },
        timePeriods: {
          start: { code: 'CY', year: 2016, label: '2016', valid: false },
          end: { code: 'CY', year: 2018, label: '2018', valid: true },
          valid: false,
        },
        valid: false,
      },
      {
        id: 'block-2',
        name: 'Data block 2',
        filters: {},
        indicatorGroups: {},
        locations: {},
        valid: true,
      },
    ],
    footnotes: [
      {
        id: 'footnote-1',
        content: 'Footnote 1',
        valid: false,
        filters: [
          {
            id: 'filter-3',
            label: 'Filter 3',
            valid: true,
          },
          {
            id: 'filter-4',
            label: 'Filter 4',
            valid: false,
          },
        ],
        filterGroups: [
          {
            id: 'group-1',
            label: 'Group 1',
            valid: true,
            filterId: 'filter-1',
            filterLabel: 'Filter 1',
          },
          {
            id: 'group-4',
            label: 'Group 4',
            valid: false,
            filterId: 'filter-2',
            filterLabel: 'Filter 2',
          },
        ],
        filterItems: [
          {
            id: 'item-1',
            label: 'Item 1',
            valid: false,
            filterId: 'filter-1',
            filterLabel: 'Filter 1',
            filterGroupId: 'group-2',
            filterGroupLabel: 'Group 2',
          },
          {
            id: 'item-2',
            label: 'Item 2',
            valid: true,
            filterId: 'filter-1',
            filterLabel: 'Filter 1',
            filterGroupId: 'group-3',
            filterGroupLabel: 'Group 3',
          },
          {
            id: 'item-3',
            label: 'Item 3',
            valid: false,
            filterId: 'filter-1',
            filterLabel: 'Filter 1',
            filterGroupId: 'group-2',
            filterGroupLabel: 'Group 2',
          },
          {
            id: 'item-4',
            label: 'Item 4',
            valid: false,
            filterId: 'filter-2',
            filterLabel: 'Filter 2',
            filterGroupId: 'group-5',
            filterGroupLabel: 'Group 5',
          },
        ],
        indicatorGroups: {
          'indicator-group-1': {
            id: 'indicator-group-1',
            label: 'Indicator group 1',
            valid: true,
            indicators: [
              {
                id: 'indicator-1',
                label: 'Indicator 1',
                name: 'indicator_1',
                target: 'target-indicator-1',
                valid: true,
              },
            ],
          },
          'indicator-group-2': {
            id: 'indicator-group-2',
            label: 'Indicator group 2',
            valid: false,
            indicators: [
              {
                id: 'indicator-3',
                label: 'Indicator 3',
                name: 'indicator_3',
                valid: false,
              },
            ],
          },
        },
      },
      {
        id: 'footnote-2',
        content: 'Footnote 2',
        valid: true,
        filters: [],
        filterGroups: [],
        filterItems: [],
        indicatorGroups: {},
      },
    ],
    originalSubjectId: 'subject-1',
    replacementSubjectId: 'subject-2',
    valid: false,
  };

  const testValidReplacementPlan: DataReplacementPlan = {
    dataBlocks: [
      {
        id: 'block-1',
        name: 'Data block 1',
        indicatorGroups: {},
        locations: {},
        filters: {},
        valid: true,
      },
      {
        id: 'block-2',
        name: 'Data block 2',
        indicatorGroups: {},
        locations: {},
        filters: {},
        valid: true,
      },
    ],
    footnotes: [
      {
        id: 'footnote-1',
        content: 'Footnote 1',
        indicatorGroups: {},
        filterItems: [],
        filterGroups: [],
        filters: [],
        valid: true,
      },
      {
        id: 'footnote-2',
        content: 'Footnote 2',
        indicatorGroups: {},
        filterItems: [],
        filterGroups: [],
        filters: [],
        valid: true,
      },
    ],
    originalSubjectId: 'subject-1',
    replacementSubjectId: 'subject-2',
    valid: true,
  };

  test('renders correctly with invalid plan', async () => {
    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testReplacementPlan,
    );

    render(
      <MemoryRouter>
        <DataFileReplacementPlan
          publicationId="publication-1"
          releaseId="release-1"
          fileId="file-1"
          replacementFileId="file-2"
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Data blocks: ERROR')).toBeInTheDocument();
      expect(screen.getByText('Footnotes: ERROR')).toBeInTheDocument();

      expect(
        screen.getByText(
          /One or more data blocks will be invalidated by this data replacement/,
        ),
      ).toBeInTheDocument();
      expect(
        screen.getByText(
          /One or more footnotes will be invalidated by this data replacement/,
        ),
      ).toBeInTheDocument();

      const details = screen.getAllByRole('group');

      expect(details).toHaveLength(4);

      const dataBlock1 = within(details[0]);

      expect(
        dataBlock1.getByRole('button', { name: /Data block 1/ }),
      ).toHaveTextContent('ERROR');

      expect(dataBlock1.getByTestId('Country')).toHaveTextContent('England');
      expect(dataBlock1.getByTestId('Country')).not.toHaveTextContent(
        'England not present',
      );

      expect(dataBlock1.getByTestId('Local Authority')).toHaveTextContent(
        'Barnsley',
      );
      expect(dataBlock1.getByTestId('Local Authority')).not.toHaveTextContent(
        'Barnsley not present',
      );

      expect(dataBlock1.getByTestId('Local Authority')).toHaveTextContent(
        'Birmingham not present',
      );
      expect(dataBlock1.getByTestId('Start date')).toHaveTextContent(
        '2016 not present',
      );

      expect(dataBlock1.getByTestId('End date')).toHaveTextContent('2018');
      expect(dataBlock1.getByTestId('End date')).not.toHaveTextContent(
        '2018 not present',
      );

      expect(dataBlock1.getByTestId('Filter 1')).toHaveTextContent('Group 1');
      expect(dataBlock1.getByTestId('Filter 1')).toHaveTextContent('Group 2');

      expect(dataBlock1.getByTestId('Group 1')).toHaveTextContent(
        'Item 1 not present',
      );
      expect(dataBlock1.getByTestId('Group 1')).toHaveTextContent('Item 2');
      expect(dataBlock1.getByTestId('Group 1')).not.toHaveTextContent(
        'Item 2 not present',
      );

      expect(dataBlock1.getByTestId('Group 2')).toHaveTextContent('Item 3');
      expect(dataBlock1.getByTestId('Group 2')).not.toHaveTextContent(
        'Item 3 not present',
      );

      expect(dataBlock1.getByTestId('Filter 2')).toHaveTextContent('Group 3');
      expect(dataBlock1.getByTestId('Filter 2')).toHaveTextContent('Group 4');

      expect(dataBlock1.getByTestId('Group 3')).toHaveTextContent('Item 4');
      expect(dataBlock1.getByTestId('Group 3')).not.toHaveTextContent(
        'Item 4 not present',
      );

      expect(dataBlock1.getByTestId('Group 4')).toHaveTextContent(
        'Item 5 not present',
      );
      expect(dataBlock1.getByTestId('Group 4')).toHaveTextContent(
        'Item 6 not present',
      );

      expect(dataBlock1.getByTestId('Indicator group 1')).toHaveTextContent(
        'Indicator 1',
      );
      expect(dataBlock1.getByTestId('Indicator group 1')).not.toHaveTextContent(
        'Indicator 1 not present',
      );

      expect(dataBlock1.getByTestId('Indicator group 2')).toHaveTextContent(
        'Indicator 2',
      );
      expect(dataBlock1.getByTestId('Indicator group 2')).not.toHaveTextContent(
        'Indicator 2 not present',
      );
      expect(dataBlock1.getByTestId('Indicator group 2')).toHaveTextContent(
        'Indicator 3 not present',
      );

      expect(dataBlock1.getByTestId('Indicator group 3')).toHaveTextContent(
        'Indicator 4 not present',
      );

      expect(
        dataBlock1.getByRole('link', { name: 'Edit data block', hidden: true }),
      ).toHaveAttribute(
        'href',
        '/publication/publication-1/release/release-1/datablocks/block-1',
      );
      expect(
        dataBlock1.getByRole('button', {
          name: 'Delete data block',
          hidden: true,
        }),
      ).toBeInTheDocument();

      const dataBlock2 = within(details[1]);

      expect(
        dataBlock2.getByRole('button', { name: /Data block 2/ }),
      ).toHaveTextContent('OK');

      expect(
        dataBlock2.getByText(
          'This data block has no conflicts with the replacement data.',
        ),
      ).toBeInTheDocument();

      const footnote1 = within(details[2]);

      expect(
        footnote1.getByRole('button', { name: /Footnote 1/ }),
      ).toHaveTextContent('ERROR');

      expect(footnote1.getByTestId('Filter 1')).toHaveTextContent('Group 2');
      expect(footnote1.getByTestId('Filter 1')).toHaveTextContent('Group 3');
      expect(footnote1.getByTestId('Filter 1')).toHaveTextContent('Group 1');

      expect(footnote1.getByTestId('Group 2')).toHaveTextContent(
        'Item 1 not present',
      );
      expect(footnote1.getByTestId('Group 2')).toHaveTextContent(
        'Item 3 not present',
      );

      expect(footnote1.getByTestId('Group 3')).toHaveTextContent('Item 2');
      expect(footnote1.getByTestId('Group 3')).not.toHaveTextContent(
        'Item 2 not present',
      );

      expect(footnote1.getByTestId('Group 1')).toHaveTextContent(
        '(All selected)',
      );

      expect(footnote1.getByTestId('Filter 2')).toHaveTextContent('Group 5');
      expect(footnote1.getByTestId('Filter 2')).toHaveTextContent('Group 4');

      expect(footnote1.getByTestId('Group 5')).toHaveTextContent(
        'Item 4 not present',
      );
      expect(footnote1.getByTestId('Group 4')).toHaveTextContent(
        '(All selected)',
      );

      expect(footnote1.getByTestId('Filter 3')).toHaveTextContent(
        '(All selected)',
      );
      expect(footnote1.getByTestId('Filter 4')).toHaveTextContent(
        '(All selected)',
      );

      expect(footnote1.getByTestId('Indicator group 1')).toHaveTextContent(
        'Indicator 1',
      );
      expect(footnote1.getByTestId('Indicator group 1')).not.toHaveTextContent(
        'Indicator 1 not present',
      );
      expect(footnote1.getByTestId('Indicator group 2')).toHaveTextContent(
        'Indicator 3 not present',
      );

      const footnote2 = within(details[3]);

      expect(
        footnote2.getByRole('button', { name: /Footnote 2/ }),
      ).toHaveTextContent('OK');

      expect(
        footnote2.getByText(
          'This footnote has no conflicts with the replacement data.',
        ),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Confirm data replacement' }),
      ).not.toBeInTheDocument();
    });
  });

  test('renders correctly with valid plan', async () => {
    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testValidReplacementPlan,
    );

    render(
      <MemoryRouter>
        <DataFileReplacementPlan
          publicationId="publication-1"
          releaseId="release-1"
          fileId="file-1"
          replacementFileId="file-2"
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Data blocks: OK')).toBeInTheDocument();
      expect(screen.getByText('Footnotes: OK')).toBeInTheDocument();

      expect(
        screen.getByText(
          /All data blocks will still be valid after this data replacement/,
        ),
      ).toBeInTheDocument();
      expect(
        screen.getByText(
          /All footnotes will still be valid after this data replacement/,
        ),
      ).toBeInTheDocument();
    });

    const details = screen.getAllByRole('group');

    expect(details).toHaveLength(4);

    const dataBlock1 = within(details[0]);

    expect(
      dataBlock1.getByRole('button', { name: /Data block 1/ }),
    ).toHaveTextContent('OK');
    expect(
      dataBlock1.getByText(
        'This data block has no conflicts with the replacement data.',
      ),
    ).toBeInTheDocument();

    const dataBlock2 = within(details[1]);

    expect(
      dataBlock2.getByRole('button', { name: /Data block 2/ }),
    ).toHaveTextContent('OK');
    expect(
      dataBlock2.getByText(
        'This data block has no conflicts with the replacement data.',
      ),
    ).toBeInTheDocument();

    const footnote1 = within(details[2]);

    expect(
      footnote1.getByRole('button', { name: /Footnote 1/ }),
    ).toHaveTextContent('OK');
    expect(
      footnote1.getByText(
        'This footnote has no conflicts with the replacement data.',
      ),
    ).toBeInTheDocument();

    const footnote2 = within(details[3]);

    expect(
      footnote2.getByRole('button', { name: /Footnote 2/ }),
    ).toHaveTextContent('OK');
    expect(
      footnote2.getByText(
        'This footnote has no conflicts with the replacement data.',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Confirm data replacement' }),
    ).toBeInTheDocument();
  });

  test('renders error message if there is an error loading replacement plan', async () => {
    dataReplacementService.getReplacementPlan.mockRejectedValue(
      new Error('Something went wrong'),
    );

    render(
      <DataFileReplacementPlan
        publicationId="publication-1"
        releaseId="release-1"
        fileId="file-1"
        replacementFileId="file-2"
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          'There was a problem loading the data replacement information.',
        ),
      ).toBeInTheDocument();
    });
  });

  test("renders correct 'Edit data block' link", async () => {
    dataReplacementService.getReplacementPlan.mockResolvedValue({
      ...testReplacementPlan,
      footnotes: [],
    });

    render(
      <MemoryRouter>
        <DataFileReplacementPlan
          publicationId="publication-1"
          releaseId="release-1"
          fileId="file-1"
          replacementFileId="file-2"
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: /Data block 1/ }),
      ).toBeInTheDocument();
    });

    userEvent.click(screen.getByRole('button', { name: /Data block 1/ }));
    expect(
      screen.getByRole('link', { name: 'Edit data block' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/datablocks/block-1',
    );
  });

  test("clicking 'Delete data block' button renders confirmation modal", async () => {
    dataReplacementService.getReplacementPlan.mockResolvedValue({
      ...testReplacementPlan,
      footnotes: [],
    });

    render(
      <MemoryRouter>
        <DataFileReplacementPlan
          publicationId="publication-1"
          releaseId="release-1"
          fileId="file-1"
          replacementFileId="file-2"
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: /Data block 1/ }),
      ).toBeInTheDocument();
    });

    userEvent.click(screen.getByRole('button', { name: /Data block 1/ }));
    userEvent.click(screen.getByRole('button', { name: 'Delete data block' }));

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      expect(modal.getByRole('heading')).toHaveTextContent('Delete data block');
      expect(
        modal.getByText(/Are you sure you want to delete/),
      ).toHaveTextContent("Are you sure you want to delete 'Data block 1'");
    });
  });

  test('deleting data block hides modal and removes data block from list', async () => {
    dataBlockService.deleteDataBlock.mockResolvedValue(undefined);
    dataReplacementService.getReplacementPlan.mockResolvedValue({
      ...testReplacementPlan,
      footnotes: [],
    });

    render(
      <MemoryRouter>
        <DataFileReplacementPlan
          publicationId="publication-1"
          releaseId="release-1"
          fileId="file-1"
          replacementFileId="file-2"
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: /Data block 1/ }),
      ).toBeInTheDocument();
    });

    userEvent.click(screen.getByRole('button', { name: /Data block 1/ }));

    userEvent.click(screen.getByRole('button', { name: 'Delete data block' }));

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    expect(dataBlockService.deleteDataBlock).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

    expect(dataBlockService.deleteDataBlock).toHaveBeenCalledTimes(1);

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

      const details = screen.getAllByRole('group');

      expect(details).toHaveLength(1);

      const dataBlock = within(details[0]);

      expect(
        dataBlock.getByRole('button', { name: /Data block 2/ }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: /Data block 1/ }),
      ).not.toBeInTheDocument();
    });
  });

  test("renders correct 'Edit footnote' link", async () => {
    dataReplacementService.getReplacementPlan.mockResolvedValue({
      ...testReplacementPlan,
      dataBlocks: [],
    });

    render(
      <MemoryRouter>
        <DataFileReplacementPlan
          publicationId="publication-1"
          releaseId="release-1"
          fileId="file-1"
          replacementFileId="file-2"
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: /Footnote 1/ }),
      ).toBeInTheDocument();
    });

    userEvent.click(screen.getByRole('button', { name: /Footnote 1/ }));
    expect(screen.getByRole('link', { name: 'Edit footnote' })).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/footnotes/footnote-1',
    );
  });

  test("clicking 'Delete footnote' button renders confirmation modal", async () => {
    dataReplacementService.getReplacementPlan.mockResolvedValue({
      ...testReplacementPlan,
      dataBlocks: [],
    });

    render(
      <MemoryRouter>
        <DataFileReplacementPlan
          publicationId="publication-1"
          releaseId="release-1"
          fileId="file-1"
          replacementFileId="file-2"
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: /Footnote 1/ }),
      ).toBeInTheDocument();
    });

    userEvent.click(screen.getByRole('button', { name: /Footnote 1/ }));
    userEvent.click(screen.getByRole('button', { name: 'Delete footnote' }));

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      expect(modal.getByRole('heading')).toHaveTextContent('Delete footnote');
      expect(
        modal.getByText(
          'Are you sure you want to delete the following footnote?',
        ),
      ).toBeInTheDocument();
      expect(modal.getByText('Footnote 1')).toBeInTheDocument();
    });
  });

  test('deleting footnote hides modal and removes footnote from list', async () => {
    footnoteService.deleteFootnote.mockResolvedValue(undefined);
    dataReplacementService.getReplacementPlan.mockResolvedValue({
      ...testReplacementPlan,
      dataBlocks: [],
    });

    render(
      <MemoryRouter>
        <DataFileReplacementPlan
          publicationId="publication-1"
          releaseId="release-1"
          fileId="file-1"
          replacementFileId="file-2"
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: /Footnote 1/ }),
      ).toBeInTheDocument();
    });

    userEvent.click(screen.getByRole('button', { name: /Footnote 1/ }));
    userEvent.click(screen.getByRole('button', { name: 'Delete footnote' }));

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    expect(footnoteService.deleteFootnote).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

    expect(footnoteService.deleteFootnote).toHaveBeenCalledTimes(1);

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

      const details = screen.getAllByRole('group');

      expect(details).toHaveLength(1);

      const footnote = within(details[0]);

      expect(
        footnote.getByRole('button', { name: /Footnote 2/ }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: /Footnote 1/ }),
      ).not.toBeInTheDocument();
    });
  });

  test('calls service to replace data when confirming replacement', async () => {
    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testValidReplacementPlan,
    );

    render(
      <MemoryRouter>
        <DataFileReplacementPlan
          publicationId="publication-1"
          releaseId="release-1"
          fileId="file-1"
          replacementFileId="file-2"
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Confirm data replacement' }),
      ).toBeInTheDocument();
    });

    expect(dataReplacementService.replaceData).not.toHaveBeenCalled();

    userEvent.click(
      screen.getByRole('button', { name: 'Confirm data replacement' }),
    );

    expect(dataReplacementService.replaceData).toHaveBeenCalledWith(
      'file-1',
      'file-2',
    );
  });

  test('renders confirmation modal when cancelling replacement', async () => {
    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testReplacementPlan,
    );

    render(
      <MemoryRouter>
        <DataFileReplacementPlan
          publicationId="publication-1"
          releaseId="release-1"
          fileId="file-1"
          replacementFileId="file-2"
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Cancel data replacement' }),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Cancel data replacement' }),
    );

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      expect(modal.getByRole('heading')).toHaveTextContent(
        'Cancel data replacement',
      );
      expect(
        modal.getByText(
          /Are you sure you want to cancel this data replacement/,
        ),
      ).toBeInTheDocument();
    });
  });

  test('confirming replacement cancellation hides modal and calls service to delete replacement file', async () => {
    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testReplacementPlan,
    );

    render(
      <MemoryRouter>
        <DataFileReplacementPlan
          publicationId="publication-1"
          releaseId="release-1"
          fileId="file-1"
          replacementFileId="file-2"
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Cancel data replacement' }),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Cancel data replacement' }),
    );

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    expect(releaseDataFileService.deleteDataFiles).not.toHaveBeenCalled();

    userEvent.click(
      within(screen.getByRole('dialog')).getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

      expect(releaseDataFileService.deleteDataFiles).toHaveBeenCalledWith(
        'release-1',
        'file-2',
      );
    });
  });
});
