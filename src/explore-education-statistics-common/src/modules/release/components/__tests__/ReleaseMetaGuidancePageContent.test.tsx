import ReleaseMetaGuidancePageContent from '@common/modules/release/components/ReleaseMetaGuidancePageContent';
import { SubjectMetaGuidance } from '@common/services/releaseMetaGuidanceService';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('ReleaseMetaGuidancePageContent', () => {
  const testSubjectMetaGuidance: SubjectMetaGuidance[] = [
    {
      id: 'subject-1',
      name: 'Subject 1',
      filename: 'subject-1.csv',
      content: '<p>Test subject 1 content</p>',
      geographicLevels: ['Local Authority', 'National'],
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
      id: 'subject-2',
      name: 'Subject 2',
      filename: 'subject-2.csv',
      content: '<p>Test subject 2 content</p>',
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
      <ReleaseMetaGuidancePageContent
        published="2020-10-22T12:00:00"
        metaGuidance="Test meta guidance content"
        subjects={[]}
      />,
    );

    expect(screen.getByTestId('published-date')).toHaveTextContent(
      'Published 22 October 2020',
    );
  });

  test('does not render published date if not present', () => {
    render(
      <ReleaseMetaGuidancePageContent
        metaGuidance="Test meta guidance content"
        subjects={[]}
      />,
    );

    expect(screen.queryByTestId('published-date')).not.toBeInTheDocument();
  });

  test('renders meta guidance content as HTML', () => {
    const metaGuidance = `
      <h2>Description</h2>
      <p>Test meta guidance content</p>`;

    render(
      <ReleaseMetaGuidancePageContent
        metaGuidance={metaGuidance}
        subjects={[]}
      />,
    );

    expect(
      screen.getByText('Description', { selector: 'h2' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Test meta guidance content', { selector: 'p' }),
    ).toBeInTheDocument();
  });

  test('does not render empty meta guidance content', () => {
    render(<ReleaseMetaGuidancePageContent metaGuidance="" subjects={[]} />);

    expect(
      screen.queryByTestId('metaGuidance-content'),
    ).not.toBeInTheDocument();
  });

  test('renders guidance for subjects', async () => {
    render(
      <ReleaseMetaGuidancePageContent
        metaGuidance="Test meta guidance content"
        subjects={testSubjectMetaGuidance}
      />,
    );

    const subjects = screen.getAllByTestId('accordionSection');

    // Subject 1

    const subject1 = within(subjects[0]);

    expect(subject1.getByTestId('Filename')).toHaveTextContent('subject-1.csv');
    expect(subject1.getByTestId('Geographic levels')).toHaveTextContent(
      'Local Authority; National',
    );
    expect(subject1.getByTestId('Time period')).toHaveTextContent(
      '2018 to 2019',
    );
    expect(
      within(
        subject1.getByTestId('Content'),
      ).getByText('Test subject 1 content', { selector: 'p' }),
    ).toBeInTheDocument();

    userEvent.click(
      subject1.getByRole('button', { name: 'Variable names and descriptions' }),
    );

    const section1VariableRows = within(
      subject1.getByTestId('Variables'),
    ).getAllByRole('row');

    const subject1VariableRow1Cells = within(
      section1VariableRows[1],
    ).getAllByRole('cell');

    expect(subject1VariableRow1Cells[0]).toHaveTextContent('filter_1');
    expect(subject1VariableRow1Cells[1]).toHaveTextContent('Filter 1');

    const subject1VariableRow2Cells = within(
      section1VariableRows[2],
    ).getAllByRole('cell');

    expect(subject1VariableRow2Cells[0]).toHaveTextContent('indicator_1');
    expect(subject1VariableRow2Cells[1]).toHaveTextContent('Indicator 1');

    userEvent.click(subject1.getByRole('button', { name: 'Footnotes' }));

    const subject1Footnotes = within(
      subject1.getByTestId('Footnotes'),
    ).getAllByRole('listitem');

    expect(subject1Footnotes).toHaveLength(2);
    expect(subject1Footnotes[0]).toHaveTextContent('Footnote 1');
    expect(subject1Footnotes[1]).toHaveTextContent('Footnote 2');

    // Subject 2

    const subject2 = within(subjects[1]);

    expect(subject2.getByTestId('Filename')).toHaveTextContent('subject-2.csv');
    expect(subject2.getByTestId('Geographic levels')).toHaveTextContent(
      'Regional; Ward',
    );
    expect(subject2.getByTestId('Time period')).toHaveTextContent(
      '2020 to 2021',
    );
    expect(
      within(
        subject2.getByTestId('Content'),
      ).getByText('Test subject 2 content', { selector: 'p' }),
    ).toBeInTheDocument();

    userEvent.click(
      subject2.getByRole('button', { name: 'Variable names and descriptions' }),
    );

    const subject2VariableRows = within(
      subject2.getByTestId('Variables'),
    ).getAllByRole('row');

    const subject2VariableRow1Cells = within(
      subject2VariableRows[1],
    ).getAllByRole('cell');

    expect(subject2VariableRow1Cells[0]).toHaveTextContent('filter_2');
    expect(subject2VariableRow1Cells[1]).toHaveTextContent('Filter 2');

    const subject2VariableRow2Cells = within(
      subject2VariableRows[2],
    ).getAllByRole('cell');

    expect(subject2VariableRow2Cells[0]).toHaveTextContent('indicator_2');
    expect(subject2VariableRow2Cells[1]).toHaveTextContent('Indicator 2');

    userEvent.click(subject2.getByRole('button', { name: 'Footnotes' }));

    const subject2Footnotes = within(
      subject2.getByTestId('Footnotes'),
    ).getAllByRole('listitem');

    expect(subject2Footnotes).toHaveLength(1);
    expect(subject2Footnotes[0]).toHaveTextContent('Footnote 3');
  });

  test('renders single time period when `from` and `to` are the same', () => {
    render(
      <ReleaseMetaGuidancePageContent
        metaGuidance="Test meta guidance content"
        subjects={[
          {
            ...testSubjectMetaGuidance[0],
            timePeriods: {
              from: '2020',
              to: '2020',
            },
          },
        ]}
      />,
    );

    const subjects = screen.getAllByTestId('accordionSection');

    const subject1 = within(subjects[0]);

    expect(subject1.queryByTestId('Time period')).toHaveTextContent('2020');
    expect(subject1.queryByTestId('Time period')).not.toHaveTextContent(
      '2020 to 2020',
    );
  });

  test('does not render empty geographic levels', () => {
    render(
      <ReleaseMetaGuidancePageContent
        metaGuidance="Test meta guidance content"
        subjects={[
          {
            ...testSubjectMetaGuidance[0],
            geographicLevels: [],
          },
        ]}
      />,
    );

    const subjects = screen.getAllByTestId('accordionSection');

    expect(
      within(subjects[0]).queryByTestId('Geographic levels'),
    ).not.toBeInTheDocument();
  });

  test('does not render empty time periods', () => {
    render(
      <ReleaseMetaGuidancePageContent
        metaGuidance="Test meta guidance content"
        subjects={[
          {
            ...testSubjectMetaGuidance[0],
            timePeriods: {
              from: '',
              to: '',
            },
          },
        ]}
      />,
    );

    const subjects = screen.getAllByTestId('accordionSection');

    expect(
      within(subjects[0]).queryByTestId('Time periods'),
    ).not.toBeInTheDocument();
  });

  test('does not render empty variables section', () => {
    render(
      <ReleaseMetaGuidancePageContent
        metaGuidance="Test meta guidance content"
        subjects={[
          {
            ...testSubjectMetaGuidance[0],
            variables: [],
          },
        ]}
      />,
    );

    const subjects = screen.getAllByTestId('accordionSection');

    expect(
      within(subjects[0]).queryByRole('button', {
        name: 'Variable names and descriptions',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(subjects[0]).queryByTestId('Variables'),
    ).not.toBeInTheDocument();
  });

  test('does not render empty footnotes section', () => {
    render(
      <ReleaseMetaGuidancePageContent
        metaGuidance="Test meta guidance content"
        subjects={[
          {
            ...testSubjectMetaGuidance[0],
            footnotes: [],
          },
        ]}
      />,
    );

    const subjects = screen.getAllByTestId('accordionSection');

    expect(
      within(subjects[0]).queryByRole('button', { name: 'Footnotes' }),
    ).not.toBeInTheDocument();
    expect(
      within(subjects[0]).queryByTestId('Footnotes'),
    ).not.toBeInTheDocument();
  });

  test('does not render empty file content', () => {
    render(
      <ReleaseMetaGuidancePageContent
        metaGuidance="Test meta guidance content"
        subjects={[
          {
            ...testSubjectMetaGuidance[0],
            content: '',
          },
        ]}
      />,
    );

    const subjects = screen.getAllByTestId('accordionSection');

    expect(
      within(subjects[0]).queryByTestId('Content'),
    ).not.toBeInTheDocument();
  });

  test('renders no subject guidance when empty', () => {
    render(
      <ReleaseMetaGuidancePageContent
        metaGuidance="Test meta guidance content"
        subjects={[]}
      />,
    );

    expect(screen.queryAllByTestId('accordionSection')).toHaveLength(0);
  });
});
