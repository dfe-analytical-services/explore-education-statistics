import TableToolWizard from '@common/modules/table-tool/components/TableToolWizard';
import { Subject, SubjectMeta } from '@common/services/tableBuilderService';
import { Theme } from '@common/services/themeService';
import { within } from '@testing-library/dom';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';

describe('TableToolWizard', () => {
  const testThemeMeta: Theme[] = [
    {
      id: 'theme-1',
      title: 'Theme 1',
      summary: '',
      topics: [
        {
          id: 'topic-1',
          title: 'Topic 1',
          summary: '',
          publications: [
            {
              id: 'publication-1',
              title: 'Publication 1',
              slug: 'publication-1',
              latestReleaseType: 'OfficialStatistics',
            },
          ],
        },
      ],
    },
  ];

  const testSubjects: Subject[] = [
    {
      id: 'subject-1',
      name: 'Subject 1',
      content: 'Test content 1',
      timePeriods: {
        from: '2019/20',
        to: '2020/21',
      },
      geographicLevels: ['National', 'Local Authority'],
      file: {
        id: 'file-1',
        name: 'Subject 1',
        fileName: 'file-1.csv',
        extension: 'csv',
        size: '10 Mb',
        type: 'Data',
      },
    },
    {
      id: 'subject-2',
      name: 'Subject 2',
      content: 'Test content 2',
      timePeriods: {
        from: '2015/16',
        to: '2019/20',
      },
      geographicLevels: ['Local Authority District', 'Ward'],
      file: {
        id: 'file-2',
        name: 'Subject 2',
        fileName: 'file-2.csv',
        extension: 'csv',
        size: '20 Mb',
        type: 'Data',
      },
    },
  ];

  const testSubjectMeta: SubjectMeta = {
    filters: {
      Characteristic: {
        totalValue: '',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        name: 'characteristic',
        options: {
          EthnicGroupMajor: {
            label: 'Ethnic group major',
            options: [
              {
                label: 'Ethnicity Major Asian Total',
                value: 'ethnicity-major-asian-total',
              },
              {
                label: 'Ethnicity Major Black Total',
                value: 'ethnicity-major-black-total',
              },
            ],
          },
        },
      },
      SchoolType: {
        totalValue: '',
        hint: 'Filter by school type',
        legend: 'School type',
        name: 'school_type',
        options: {
          Default: {
            label: 'Default',
            options: [
              {
                label: 'State-funded primary',
                value: 'state-funded-primary',
              },
              {
                label: 'State-funded secondary',
                value: 'state-funded-secondary',
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
          {
            value: 'overall-absence-sessions',
            label: 'Number of overall absence sessions',
            unit: '',
            name: 'sess_overall',
            decimalPlaces: 2,
          },
        ],
      },
    },
    locations: {
      country: {
        legend: 'Country',
        options: [{ value: 'england', label: 'England' }],
      },
      localAuthority: {
        legend: 'Local authority',
        options: [
          { value: 'barnet', label: 'Barnet' },
          { value: 'barnsley', label: 'Barnsley' },
        ],
      },
    },
    timePeriod: {
      legend: 'Time period',
      options: [
        { label: '2013/14', code: 'AY', year: 2013 },
        { label: '2014/15', code: 'AY', year: 2014 },
      ],
    },
  };

  test('renders only first step when no `initialState` is provided', () => {
    render(<TableToolWizard themeMeta={testThemeMeta} />);

    const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

    expect(stepHeadings).toHaveLength(1);
    expect(stepHeadings[0]).toHaveTextContent(
      'Step 1 (current) Choose a publication',
    );

    expect(screen.getAllByRole('listitem')).toHaveLength(1);
  });

  test('renders `initialState.subjects` on step when it is the current step', async () => {
    render(
      <TableToolWizard
        themeMeta={testThemeMeta}
        initialState={{
          initialStep: 2,
          subjects: testSubjects,
          query: {
            publicationId: 'publication-1',
            subjectId: '',
            locations: {},
            filters: [],
            indicators: [],
          },
        }}
      />,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Subject 1')).toBeInTheDocument();
      expect(screen.getByLabelText('Subject 2')).toBeInTheDocument();
    });
  });

  test('does not render publication step if instructed to hide it', async () => {
    render(
      <TableToolWizard
        hidePublicationSelectionStage
        themeMeta={testThemeMeta}
        initialState={{
          initialStep: 1,
          query: {
            subjectId: '',
            releaseId: 'release-1',
            locations: {},
            filters: [],
            indicators: [],
          },
        }}
      />,
    );

    await waitFor(() => {
      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(1);
      expect(stepHeadings[0]).toHaveTextContent(
        'Step 1 (current) Choose a subject',
      );
    });
  });

  test('renders all steps correctly when full `initialState` is provided', async () => {
    render(
      <TableToolWizard
        themeMeta={testThemeMeta}
        initialState={{
          initialStep: 5,
          subjectMeta: testSubjectMeta,
          subjects: [testSubjects[0]],
          featuredTables: [],
          query: {
            publicationId: 'publication-1',
            subjectId: 'subject-1',
            locations: {
              localAuthority: ['barnet', 'barnsley'],
            },
            timePeriod: {
              startYear: 2013,
              startCode: 'AY',
              endYear: 2014,
              endCode: 'AY',
            },
            filters: ['ethnicity-major-asian-total'],
            indicators: ['authorised-absence-sessions'],
          },
        }}
      />,
    );

    await waitFor(() => {
      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(5);
      expect(stepHeadings[0]).toHaveTextContent('Step 1 Choose a publication');
      expect(stepHeadings[1]).toHaveTextContent('Step 2 Choose a subject');
      expect(stepHeadings[2]).toHaveTextContent('Step 3 Choose locations');
      expect(stepHeadings[3]).toHaveTextContent('Step 4 Choose time period');
      expect(stepHeadings[4]).toHaveTextContent(
        'Step 5 (current) Choose your filters',
      );

      // Step 1

      const step1 = within(screen.getByTestId('wizardStep-1'));
      expect(
        step1.getByText('Publication', { selector: 'dt' }),
      ).toBeInTheDocument();
      expect(
        step1.getByText('Publication 1', { selector: 'dd' }),
      ).toBeInTheDocument();

      // Step 2

      const step2 = within(screen.getByTestId('wizardStep-2'));
      expect(
        step2.getByText('Subject', { selector: 'dt' }),
      ).toBeInTheDocument();
      expect(
        step2.getByText('Subject 1', { selector: 'dd' }),
      ).toBeInTheDocument();

      // Step 3

      const step3 = within(screen.getByTestId('wizardStep-3'));
      expect(
        step3.getByText('Local authority', { selector: 'dt' }),
      ).toBeInTheDocument();

      const step3Locations = step3.getAllByRole('listitem');
      expect(step3Locations).toHaveLength(2);
      expect(step3Locations[0]).toHaveTextContent('Barnet');
      expect(step3Locations[1]).toHaveTextContent('Barnsley');

      // Step 4

      const step4 = within(screen.getByTestId('wizardStep-4'));

      expect(
        step4.getByText('Time period', { selector: 'dt' }),
      ).toBeInTheDocument();
      expect(
        step4.getByText('2013/14 to 2014/15', { selector: 'dd' }),
      ).toBeInTheDocument();

      // Step 5

      const step5 = within(screen.getByTestId('wizardStep-5'));
      expect(
        step5.getByLabelText('Ethnicity Major Asian Total'),
      ).toHaveAttribute('checked');
      expect(
        step5.getByLabelText('Number of authorised absence sessions'),
      ).toHaveAttribute('checked');
    });
  });
});
