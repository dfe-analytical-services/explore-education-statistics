import ReleaseDataGuidanceSection from '@admin/pages/release/data/components/ReleaseDataGuidanceSection';
import _releaseDataGuidanceService, {
  ReleaseDataGuidance,
} from '@admin/services/releaseDataGuidanceService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';

jest.mock('@admin/services/releaseDataGuidanceService');

const releaseDataGuidanceService = _releaseDataGuidanceService as jest.Mocked<
  typeof _releaseDataGuidanceService
>;

describe('ReleaseDataGuidanceSection', () => {
  const testDataGuidance: ReleaseDataGuidance = {
    id: 'release-1',
    content: '<p>Test main content</p>',
    dataSets: [
      {
        fileId: 'file-1',
        name: 'Data set 1',
        filename: 'data-1.csv',
        content: '<p>Test data set 1 content</p>',
        geographicLevels: ['Local authority', 'National'],
        timePeriods: {
          from: '2018',
          to: '2019',
        },
        variables: [
          { value: 'filter_1', label: 'Filter 1' },
          { value: 'indicator_1', label: 'Indicator 1' },
        ],
        footnotes: [
          {
            id: 'footnote-1',
            label: 'Footnote 1',
          },
          {
            id: 'footnote-2',
            label: 'Footnote 2',
          },
        ],
      },
      {
        fileId: 'file-2',
        name: 'Data set 2',
        filename: 'data-2.csv',
        content: '<p>Test data set 2 content</p>',
        geographicLevels: ['Regional', 'Ward'],
        timePeriods: {
          from: '2020',
          to: '2021',
        },
        variables: [
          { value: 'filter_2', label: 'Filter 2' },
          { value: 'indicator_2', label: 'Indicator 2' },
        ],
        footnotes: [
          {
            id: 'footnote-3',
            label: 'Footnote 3',
          },
        ],
      },
    ],
  };

  describe('can update release', () => {
    test('renders correct edit buttons', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue(
        testDataGuidance,
      );

      render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByRole('button', { name: 'Save guidance' }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', { name: 'Preview guidance' }),
        ).toBeInTheDocument();
      });
    });

    test('renders empty guidance correctly', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue({
        ...testDataGuidance,
        content: '',
      });

      render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Main guidance content'),
        ).toBeInTheDocument();
      });

      const mainGuidanceContent = screen.getByLabelText(
        'Main guidance content',
      ) as HTMLTextAreaElement;

      const mainGuidanceContentValue = mainGuidanceContent.value;

      expect(mainGuidanceContentValue).toContain('<h3>Description</h3>');
      expect(mainGuidanceContentValue).toContain('<h3>Coverage</h3>');
      expect(mainGuidanceContentValue).toContain(
        '<h3>File formats and conventions</h3>',
      );
    });

    test('renders correct message when there are no data sets', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue({
        ...testDataGuidance,
        dataSets: [],
      });

      render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByText(
            'Before you can change the public data guidance, you must upload at least one data file.',
          ),
        ).toBeInTheDocument();
      });

      expect(
        screen.queryByLabelText('Main guidance content'),
      ).not.toBeInTheDocument();
      expect(screen.queryByText('Data files')).not.toBeInTheDocument();
      expect(screen.queryAllByRole('accordionSection')).toHaveLength(0);
    });

    test('renders existing guidance with data sets', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue(
        testDataGuidance,
      );

      const { user } = render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Main guidance content'),
        ).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Main guidance content')).toHaveValue(
        '<p>Test main content</p>',
      );

      const dataSets = screen.getAllByTestId('accordionSection');

      // Data set 1

      const dataSet1 = within(dataSets[0]);

      expect(dataSet1.getByTestId('Filename')).toHaveTextContent('data-1.csv');
      expect(dataSet1.getByTestId('Geographic levels')).toHaveTextContent(
        'Local authority; National',
      );
      expect(dataSet1.getByTestId('Time period')).toHaveTextContent(
        '2018 to 2019',
      );

      expect(dataSet1.getByLabelText('File guidance content')).toHaveValue(
        '<p>Test data set 1 content</p>',
      );

      await user.click(
        dataSet1.getByRole('button', {
          name: /Variable names and descriptions/,
        }),
      );

      const dataSet1VariableRows = dataSet1.getAllByRole('row');

      const dataSet1VariableRow1Cells = within(
        dataSet1VariableRows[1],
      ).getAllByRole('cell');

      expect(dataSet1VariableRow1Cells[0]).toHaveTextContent('filter_1');
      expect(dataSet1VariableRow1Cells[1]).toHaveTextContent('Filter 1');

      const dataSet1VariableRow2Cells = within(
        dataSet1VariableRows[2],
      ).getAllByRole('cell');

      expect(dataSet1VariableRow2Cells[0]).toHaveTextContent('indicator_1');
      expect(dataSet1VariableRow2Cells[1]).toHaveTextContent('Indicator 1');

      await user.click(dataSet1.getByRole('button', { name: /Footnotes/ }));

      const dataSet1Footnotes = within(
        dataSet1.getByTestId('Footnotes'),
      ).getAllByRole('listitem');

      expect(dataSet1Footnotes).toHaveLength(2);
      expect(dataSet1Footnotes[0]).toHaveTextContent('Footnote 1');
      expect(dataSet1Footnotes[1]).toHaveTextContent('Footnote 2');

      // Data set 2

      const dataSet2 = within(dataSets[1]);

      expect(dataSet2.getByTestId('Filename')).toHaveTextContent('data-2.csv');
      expect(dataSet2.getByTestId('Geographic levels')).toHaveTextContent(
        'Regional; Ward',
      );
      expect(dataSet2.getByTestId('Time period')).toHaveTextContent(
        '2020 to 2021',
      );

      expect(dataSet2.getByLabelText('File guidance content')).toHaveValue(
        '<p>Test data set 2 content</p>',
      );

      await user.click(
        dataSet2.getByRole('button', {
          name: /Variable names and descriptions/,
        }),
      );

      const dataSet2VariableRows = dataSet2.getAllByRole('row');

      const dataSet2VariableRow1Cells = within(
        dataSet2VariableRows[1],
      ).getAllByRole('cell');

      expect(dataSet2VariableRow1Cells[0]).toHaveTextContent('filter_2');
      expect(dataSet2VariableRow1Cells[1]).toHaveTextContent('Filter 2');

      const dataSet2VariableRow2Cells = within(
        dataSet2VariableRows[2],
      ).getAllByRole('cell');

      expect(dataSet2VariableRow2Cells[0]).toHaveTextContent('indicator_2');
      expect(dataSet2VariableRow2Cells[1]).toHaveTextContent('Indicator 2');

      await user.click(dataSet2.getByRole('button', { name: /Footnotes/ }));

      const dataSet2Footnotes = within(
        dataSet2.getByTestId('Footnotes'),
      ).getAllByRole('listitem');

      expect(dataSet2Footnotes).toHaveLength(1);
      expect(dataSet2Footnotes[0]).toHaveTextContent('Footnote 3');
    });

    test('renders correctly with data sets in preview mode', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue(
        testDataGuidance,
      );

      const { user } = render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByRole('button', { name: 'Preview guidance' }),
        ).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Preview guidance' }),
      );

      expect(
        screen.queryByLabelText('Main guidance content'),
      ).not.toBeInTheDocument();

      expect(
        screen.getByTestId('mainGuidanceContent').innerHTML,
      ).toMatchInlineSnapshot(
        `
        <p>
          Test main content
        </p>
      `,
      );

      const dataSets = screen.getAllByTestId('accordionSection');

      // Data set 1

      const dataSet1 = within(dataSets[0]);

      expect(dataSet1.getByTestId('Filename')).toHaveTextContent('data-1.csv');
      expect(dataSet1.getByTestId('Geographic levels')).toHaveTextContent(
        'Local authority; National',
      );
      expect(dataSet1.getByTestId('Time period')).toHaveTextContent(
        '2018 to 2019',
      );

      expect(
        dataSet1.queryByLabelText('File guidance content'),
      ).not.toBeInTheDocument();

      expect(
        dataSet1.getByTestId('fileGuidanceContent').innerHTML,
      ).toMatchInlineSnapshot(
        `
        <p>
          Test data set 1 content
        </p>
      `,
      );

      await user.click(
        dataSet1.getByRole('button', {
          name: /Variable names and descriptions/,
        }),
      );

      const dataSet1VariableRows = dataSet1.getAllByRole('row');

      const dataSet1VariableRow1Cells = within(
        dataSet1VariableRows[1],
      ).getAllByRole('cell');

      expect(dataSet1VariableRow1Cells[0]).toHaveTextContent('filter_1');
      expect(dataSet1VariableRow1Cells[1]).toHaveTextContent('Filter 1');

      const dataSet1VariableRow2Cells = within(
        dataSet1VariableRows[2],
      ).getAllByRole('cell');

      expect(dataSet1VariableRow2Cells[0]).toHaveTextContent('indicator_1');
      expect(dataSet1VariableRow2Cells[1]).toHaveTextContent('Indicator 1');

      await user.click(dataSet1.getByRole('button', { name: /Footnotes/ }));

      const dataSet1Footnotes = within(
        dataSet1.getByTestId('Footnotes'),
      ).getAllByRole('listitem');

      expect(dataSet1Footnotes).toHaveLength(2);
      expect(dataSet1Footnotes[0]).toHaveTextContent('Footnote 1');
      expect(dataSet1Footnotes[1]).toHaveTextContent('Footnote 2');

      // Data set 2

      const dataSet2 = within(dataSets[1]);

      expect(dataSet2.getByTestId('Filename')).toHaveTextContent('data-2.csv');
      expect(dataSet2.getByTestId('Geographic levels')).toHaveTextContent(
        'Regional; Ward',
      );
      expect(dataSet2.getByTestId('Time period')).toHaveTextContent(
        '2020 to 2021',
      );

      expect(
        dataSet2.queryByLabelText('File guidance content'),
      ).not.toBeInTheDocument();

      expect(
        dataSet2.getByTestId('fileGuidanceContent').innerHTML,
      ).toMatchInlineSnapshot(
        `
        <p>
          Test data set 2 content
        </p>
      `,
      );

      await user.click(
        dataSet2.getByRole('button', {
          name: /Variable names and descriptions/,
        }),
      );

      const dataSet2VariableRows = dataSet2.getAllByRole('row');

      const dataSet2VariableRow1Cells = within(
        dataSet2VariableRows[1],
      ).getAllByRole('cell');

      expect(dataSet2VariableRow1Cells[0]).toHaveTextContent('filter_2');
      expect(dataSet2VariableRow1Cells[1]).toHaveTextContent('Filter 2');

      const dataSet2VariableRow2Cells = within(
        dataSet2VariableRows[2],
      ).getAllByRole('cell');

      expect(dataSet2VariableRow2Cells[0]).toHaveTextContent('indicator_2');
      expect(dataSet2VariableRow2Cells[1]).toHaveTextContent('Indicator 2');

      await user.click(dataSet2.getByRole('button', { name: /Footnotes/ }));

      const dataSet2Footnotes = within(
        dataSet2.getByTestId('Footnotes'),
      ).getAllByRole('listitem');

      expect(dataSet2Footnotes).toHaveLength(1);
      expect(dataSet2Footnotes[0]).toHaveTextContent('Footnote 3');
    });

    test('shows validation message when main guidance content is empty', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue(
        testDataGuidance,
      );

      const { user } = render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Main guidance content'),
        ).toBeInTheDocument();
      });

      const mainGuidanceContent = screen.getByLabelText(
        'Main guidance content',
      );

      expect(
        screen.queryByRole('link', { name: 'Enter main guidance content' }),
      ).not.toBeInTheDocument();

      await user.clear(mainGuidanceContent);
      await user.click(screen.getByRole('button', { name: 'Save guidance' }));

      await waitFor(() => {
        expect(
          screen.getByRole('link', { name: 'Enter main guidance content' }),
        ).toHaveAttribute('href', '#dataGuidanceForm-content');

        expect(mainGuidanceContent).toHaveAttribute(
          'id',
          'dataGuidanceForm-content',
        );
      });
    });

    test('shows validation message when file guidance content is empty', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue(
        testDataGuidance,
      );

      const { user } = render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease
        />,
      );

      await waitFor(() => {
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(2);
      });

      const dataSets = screen.getAllByTestId('accordionSection');
      const dataSet1 = within(dataSets[0]);

      const fileGuidanceContent = dataSet1.getByLabelText(
        'File guidance content',
      );

      expect(
        screen.queryByRole('link', {
          name: 'Enter file guidance content for Data set 1',
        }),
      ).not.toBeInTheDocument();

      await user.clear(fileGuidanceContent);
      await user.click(screen.getByRole('button', { name: 'Save guidance' }));

      await waitFor(() => {
        expect(
          screen.getByRole('link', {
            name: 'Enter file guidance content for Data set 1',
          }),
        ).toHaveAttribute('href', '#dataGuidanceForm-dataSets-0-content');

        expect(fileGuidanceContent).toHaveAttribute(
          'id',
          'dataGuidanceForm-dataSets-0-content',
        );
      });
    });

    test('cannot submit with invalid main guidance content', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue(
        testDataGuidance,
      );

      const { user } = render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Main guidance content'),
        ).toBeInTheDocument();
      });

      await user.clear(screen.getByLabelText('Main guidance content'));

      expect(
        releaseDataGuidanceService.updateDataGuidance,
      ).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Save guidance' }));

      await waitFor(() => {
        expect(
          screen.getByRole('link', {
            name: 'Enter main guidance content',
          }),
        ).toHaveAttribute('href', '#dataGuidanceForm-content');

        expect(
          releaseDataGuidanceService.updateDataGuidance,
        ).not.toHaveBeenCalled();
      });
    });

    test('cannot submit with invalid data set content', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue(
        testDataGuidance,
      );

      const { user } = render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease
        />,
      );

      await waitFor(() => {
        expect(screen.getAllByTestId('accordionSection')).toHaveLength(2);
      });

      const dataSets = screen.getAllByTestId('accordionSection');

      await user.clear(
        within(dataSets[0]).getByLabelText('File guidance content'),
      );

      expect(
        releaseDataGuidanceService.updateDataGuidance,
      ).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Save guidance' }));

      await waitFor(() => {
        expect(
          screen.getByRole('link', {
            name: 'Enter file guidance content for Data set 1',
          }),
        ).toHaveAttribute('href', '#dataGuidanceForm-dataSets-0-content');

        expect(
          releaseDataGuidanceService.updateDataGuidance,
        ).not.toHaveBeenCalled();
      });
    });

    test('can successfully submit with updated values', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue(
        testDataGuidance,
      );

      const { user } = render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Main guidance content'),
        ).toBeInTheDocument();
      });

      await user.clear(screen.getByLabelText('Main guidance content'));
      await user.type(
        screen.getByLabelText('Main guidance content'),
        '<p>Updated main guidance content</p>',
      );

      const dataSets = screen.getAllByTestId('accordionSection');

      const dataSet1 = within(dataSets[0]);
      const dataSet2 = within(dataSets[1]);

      await user.clear(dataSet1.getByLabelText('File guidance content'));
      await user.type(
        dataSet1.getByLabelText('File guidance content'),
        '<p>Updated data set 1 guidance content</p>',
      );

      await user.clear(dataSet2.getByLabelText('File guidance content'));
      await user.type(
        dataSet2.getByLabelText('File guidance content'),
        '<p>Updated data set 2 guidance content</p>',
      );

      // Not the right return value, but we'll just
      // return something to make sure things don't break.
      releaseDataGuidanceService.updateDataGuidance.mockResolvedValue(
        testDataGuidance,
      );

      expect(
        releaseDataGuidanceService.updateDataGuidance,
      ).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Save guidance' }));

      await waitFor(() => {
        expect(
          releaseDataGuidanceService.updateDataGuidance,
        ).toHaveBeenCalledTimes(1);
        expect(
          releaseDataGuidanceService.updateDataGuidance,
        ).toHaveBeenCalledWith('release-1', {
          content: '<p>Updated main guidance content</p>',
          dataSets: [
            {
              fileId: 'file-1',
              content: '<p>Updated data set 1 guidance content</p>',
            },
            {
              fileId: 'file-2',
              content: '<p>Updated data set 2 guidance content</p>',
            },
          ],
        });
      });
    });
  });

  describe('cannot update release', () => {
    test('renders with warning message', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue(
        testDataGuidance,
      );

      render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease={false}
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByText(
            'This release has been approved, and can no longer be updated.',
          ),
        ).toBeInTheDocument();
      });
    });

    test('does not render any edit buttons', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue(
        testDataGuidance,
      );

      render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease={false}
        />,
      );

      await waitFor(() => {
        expect(
          screen.queryByRole('button', {
            name: /(Save|Edit|Preview) guidance/,
          }),
        ).not.toBeInTheDocument();
      });
    });

    test('renders empty guidance correctly', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue({
        ...testDataGuidance,
        content: '',
      });

      render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease={false}
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByText('No guidance content was saved.'),
        ).toBeInTheDocument();
      });
    });

    test('renders message when there are no data sets', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue({
        ...testDataGuidance,
        dataSets: [],
      });

      render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease={false}
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByText(
            'The public data guidance has not been created as no data files were uploaded.',
          ),
        ).toBeInTheDocument();
      });

      expect(
        screen.queryByTestId('mainGuidanceContent'),
      ).not.toBeInTheDocument();
      expect(screen.queryByText('Data files')).not.toBeInTheDocument();
      expect(screen.queryAllByRole('accordionSection')).toHaveLength(0);
    });

    test('renders existing guidance with data sets', async () => {
      releaseDataGuidanceService.getDataGuidance.mockResolvedValue(
        testDataGuidance,
      );

      const { user } = render(
        <ReleaseDataGuidanceSection
          releaseVersionId="release-1"
          canUpdateRelease={false}
        />,
      );

      await waitFor(() => {
        expect(screen.getByTestId('mainGuidanceContent')).toBeInTheDocument();
      });

      expect(screen.getByTestId('mainGuidanceContent').innerHTML)
        .toMatchInlineSnapshot(`
              <p>
                Test main content
              </p>
          `);

      const dataSets = screen.getAllByTestId('accordionSection');

      // Data set 1

      const dataSet1 = within(dataSets[0]);

      expect(dataSet1.getByTestId('Filename')).toHaveTextContent('data-1.csv');
      expect(dataSet1.getByTestId('Geographic levels')).toHaveTextContent(
        'Local authority; National',
      );
      expect(dataSet1.getByTestId('Time period')).toHaveTextContent(
        '2018 to 2019',
      );

      expect(dataSet1.getByTestId('fileGuidanceContent').innerHTML)
        .toMatchInlineSnapshot(`
              <p>
                Test data set 1 content
              </p>
          `);

      await user.click(
        dataSet1.getByRole('button', {
          name: /Variable names and descriptions/,
        }),
      );

      const dataSet1VariableRows = dataSet1.getAllByRole('row');

      const dataSet1VariableRow1Cells = within(
        dataSet1VariableRows[1],
      ).getAllByRole('cell');

      expect(dataSet1VariableRow1Cells[0]).toHaveTextContent('filter_1');
      expect(dataSet1VariableRow1Cells[1]).toHaveTextContent('Filter 1');

      const dataSet1VariableRow2Cells = within(
        dataSet1VariableRows[2],
      ).getAllByRole('cell');

      expect(dataSet1VariableRow2Cells[0]).toHaveTextContent('indicator_1');
      expect(dataSet1VariableRow2Cells[1]).toHaveTextContent('Indicator 1');

      await user.click(dataSet1.getByRole('button', { name: /Footnotes/ }));

      const dataSet1Footnotes = within(
        dataSet1.getByTestId('Footnotes'),
      ).getAllByRole('listitem');

      expect(dataSet1Footnotes).toHaveLength(2);
      expect(dataSet1Footnotes[0]).toHaveTextContent('Footnote 1');
      expect(dataSet1Footnotes[1]).toHaveTextContent('Footnote 2');

      // Data set 2

      const dataSet2 = within(dataSets[1]);

      expect(dataSet2.getByTestId('Filename')).toHaveTextContent('data-2.csv');
      expect(dataSet2.getByTestId('Geographic levels')).toHaveTextContent(
        'Regional; Ward',
      );
      expect(dataSet2.getByTestId('Time period')).toHaveTextContent(
        '2020 to 2021',
      );

      expect(dataSet2.getByTestId('fileGuidanceContent').innerHTML)
        .toMatchInlineSnapshot(`
              <p>
                Test data set 2 content
              </p>
          `);

      await user.click(
        dataSet2.getByRole('button', {
          name: /Variable names and descriptions/,
        }),
      );

      const dataSet2VariableRows = dataSet2.getAllByRole('row');

      const dataSet2VariableRow1Cells = within(
        dataSet2VariableRows[1],
      ).getAllByRole('cell');

      expect(dataSet2VariableRow1Cells[0]).toHaveTextContent('filter_2');
      expect(dataSet2VariableRow1Cells[1]).toHaveTextContent('Filter 2');

      const dataSet2VariableRow2Cells = within(
        dataSet2VariableRows[2],
      ).getAllByRole('cell');

      expect(dataSet2VariableRow2Cells[0]).toHaveTextContent('indicator_2');
      expect(dataSet2VariableRow2Cells[1]).toHaveTextContent('Indicator 2');

      await user.click(dataSet2.getByRole('button', { name: /Footnotes/ }));

      const dataSet2Footnotes = within(
        dataSet2.getByTestId('Footnotes'),
      ).getAllByRole('listitem');

      expect(dataSet2Footnotes).toHaveLength(1);
      expect(dataSet2Footnotes[0]).toHaveTextContent('Footnote 3');
    });
  });
});
