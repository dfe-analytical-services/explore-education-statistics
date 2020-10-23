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
      subject1.getByRole('button', {
        name: 'Variable names and descriptions',
      }),
    );

    const section1VariableRows = subject1.getAllByRole('row');

    const section1VariableRow1Cells = within(
      section1VariableRows[1],
    ).getAllByRole('cell');

    expect(section1VariableRow1Cells[0]).toHaveTextContent('filter_1');
    expect(section1VariableRow1Cells[1]).toHaveTextContent('Filter 1');

    const section1VariableRow2Cells = within(
      section1VariableRows[2],
    ).getAllByRole('cell');

    expect(section1VariableRow2Cells[0]).toHaveTextContent('indicator_1');
    expect(section1VariableRow2Cells[1]).toHaveTextContent('Indicator 1');

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
      subject2.getByRole('button', {
        name: 'Variable names and descriptions',
      }),
    );

    const section2VariableRows = subject2.getAllByRole('row');

    const section2VariableRow1Cells = within(
      section2VariableRows[1],
    ).getAllByRole('cell');

    expect(section2VariableRow1Cells[0]).toHaveTextContent('filter_2');
    expect(section2VariableRow1Cells[1]).toHaveTextContent('Filter 2');

    const section2VariableRow2Cells = within(
      section2VariableRows[2],
    ).getAllByRole('cell');

    expect(section2VariableRow2Cells[0]).toHaveTextContent('indicator_2');
    expect(section2VariableRow2Cells[1]).toHaveTextContent('Indicator 2');
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
