import ReleaseDataGuidancePageContent from '@common/modules/release/components/ReleaseDataGuidancePageContent';
import { DataSetDataGuidance } from '@common/services/releaseDataGuidanceService';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('ReleaseDataGuidancePageContent', () => {
  const testDataSetDataGuidance: DataSetDataGuidance[] = [
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
  ];

  test('renders published date if present', () => {
    render(
      <ReleaseDataGuidancePageContent
        published="2020-10-22T12:00:00"
        dataGuidance="Test data guidance content"
        dataSets={[]}
      />,
    );

    expect(screen.getByTestId('published-date')).toHaveTextContent(
      'Published 22 October 2020',
    );
  });

  test('does not render published date if not present', () => {
    render(
      <ReleaseDataGuidancePageContent
        dataGuidance="Test data guidance content"
        dataSets={[]}
      />,
    );

    expect(screen.queryByTestId('published-date')).not.toBeInTheDocument();
  });

  test('renders data guidance content as HTML', () => {
    const dataGuidance = `
      <h2>Description</h2>
      <p>Test data guidance content</p>`;

    render(
      <ReleaseDataGuidancePageContent
        dataGuidance={dataGuidance}
        dataSets={[]}
      />,
    );

    expect(
      screen.getByText('Description', { selector: 'h2' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Test data guidance content', { selector: 'p' }),
    ).toBeInTheDocument();
  });

  test('does not render empty data guidance content', () => {
    render(<ReleaseDataGuidancePageContent dataGuidance="" dataSets={[]} />);

    expect(
      screen.queryByTestId('dataGuidance-content'),
    ).not.toBeInTheDocument();
  });

  test('renders guidance for data sets', async () => {
    render(
      <ReleaseDataGuidancePageContent
        dataGuidance="Test data guidance content"
        dataSets={testDataSetDataGuidance}
      />,
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
      within(dataSet1.getByTestId('Content')).getByText(
        'Test data set 1 content',
        { selector: 'p' },
      ),
    ).toBeInTheDocument();

    await userEvent.click(
      dataSet1.getByRole('button', { name: /Variable names and descriptions/ }),
    );

    const section1VariableRows = within(
      dataSet1.getByTestId('Variables'),
    ).getAllByRole('row');

    const dataSet1VariableRow1Cells = within(
      section1VariableRows[1],
    ).getAllByRole('cell');

    expect(dataSet1VariableRow1Cells[0]).toHaveTextContent('filter_1');
    expect(dataSet1VariableRow1Cells[1]).toHaveTextContent('Filter 1');

    const dataSet1VariableRow2Cells = within(
      section1VariableRows[2],
    ).getAllByRole('cell');

    expect(dataSet1VariableRow2Cells[0]).toHaveTextContent('indicator_1');
    expect(dataSet1VariableRow2Cells[1]).toHaveTextContent('Indicator 1');

    await userEvent.click(dataSet1.getByRole('button', { name: /Footnotes/ }));

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
      within(dataSet2.getByTestId('Content')).getByText(
        'Test data set 2 content',
        { selector: 'p' },
      ),
    ).toBeInTheDocument();

    await userEvent.click(
      dataSet2.getByRole('button', { name: /Variable names and descriptions/ }),
    );

    const dataSet2VariableRows = within(
      dataSet2.getByTestId('Variables'),
    ).getAllByRole('row');

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

    await userEvent.click(dataSet2.getByRole('button', { name: /Footnotes/ }));

    const dataSet2Footnotes = within(
      dataSet2.getByTestId('Footnotes'),
    ).getAllByRole('listitem');

    expect(dataSet2Footnotes).toHaveLength(1);
    expect(dataSet2Footnotes[0]).toHaveTextContent('Footnote 3');
  });

  test('renders single time period when `from` and `to` are the same', () => {
    render(
      <ReleaseDataGuidancePageContent
        dataGuidance="Test data guidance content"
        dataSets={[
          {
            ...testDataSetDataGuidance[0],
            timePeriods: {
              from: '2020',
              to: '2020',
            },
          },
        ]}
      />,
    );

    const dataSets = screen.getAllByTestId('accordionSection');

    const dataSet1 = within(dataSets[0]);

    expect(dataSet1.queryByTestId('Time period')).toHaveTextContent('2020');
    expect(dataSet1.queryByTestId('Time period')).not.toHaveTextContent(
      '2020 to 2020',
    );
  });

  test('does not render empty geographic levels', () => {
    render(
      <ReleaseDataGuidancePageContent
        dataGuidance="Test data guidance content"
        dataSets={[
          {
            ...testDataSetDataGuidance[0],
            geographicLevels: [],
          },
        ]}
      />,
    );

    const dataSets = screen.getAllByTestId('accordionSection');

    expect(
      within(dataSets[0]).queryByTestId('Geographic levels'),
    ).not.toBeInTheDocument();
  });

  test('does not render empty time periods', () => {
    render(
      <ReleaseDataGuidancePageContent
        dataGuidance="Test data guidance content"
        dataSets={[
          {
            ...testDataSetDataGuidance[0],
            timePeriods: {
              from: '',
              to: '',
            },
          },
        ]}
      />,
    );

    const dataSets = screen.getAllByTestId('accordionSection');

    expect(
      within(dataSets[0]).queryByTestId('Time periods'),
    ).not.toBeInTheDocument();
  });

  test('does not render empty variables section', () => {
    render(
      <ReleaseDataGuidancePageContent
        dataGuidance="Test data guidance content"
        dataSets={[
          {
            ...testDataSetDataGuidance[0],
            variables: [],
          },
        ]}
      />,
    );

    const dataSets = screen.getAllByTestId('accordionSection');

    expect(
      within(dataSets[0]).queryByRole('button', {
        name: 'Variable names and descriptions',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(dataSets[0]).queryByTestId('Variables'),
    ).not.toBeInTheDocument();
  });

  test('does not render empty footnotes section', () => {
    render(
      <ReleaseDataGuidancePageContent
        dataGuidance="Test data guidance content"
        dataSets={[
          {
            ...testDataSetDataGuidance[0],
            footnotes: [],
          },
        ]}
      />,
    );

    const dataSets = screen.getAllByTestId('accordionSection');

    expect(
      within(dataSets[0]).queryByRole('button', { name: /Footnotes/ }),
    ).not.toBeInTheDocument();
    expect(
      within(dataSets[0]).queryByTestId('Footnotes'),
    ).not.toBeInTheDocument();
  });

  test('does not render empty file content', () => {
    render(
      <ReleaseDataGuidancePageContent
        dataGuidance="Test data guidance content"
        dataSets={[
          {
            ...testDataSetDataGuidance[0],
            content: '',
          },
        ]}
      />,
    );

    const dataSets = screen.getAllByTestId('accordionSection');

    expect(
      within(dataSets[0]).queryByTestId('Content'),
    ).not.toBeInTheDocument();
  });

  test('renders no data set guidance when empty', () => {
    render(
      <ReleaseDataGuidancePageContent
        dataGuidance="Test data guidance content"
        dataSets={[]}
      />,
    );

    expect(screen.queryAllByTestId('accordionSection')).toHaveLength(0);
  });
});
