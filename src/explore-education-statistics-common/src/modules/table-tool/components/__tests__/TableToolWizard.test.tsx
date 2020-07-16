import TableToolWizard from '@common/modules/table-tool/components/TableToolWizard';
import _tableBuilderService, {
  PublicationMeta,
  PublicationSubjectMeta,
  ReleaseMeta,
  ThemeMeta,
} from '@common/services/tableBuilderService';
import { within } from '@testing-library/dom';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';

jest.mock('@common/services/tableBuilderService');

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('TableToolWizard', () => {
  const testThemeMeta: ThemeMeta[] = [
    {
      id: 'theme-1',
      title: 'Theme 1',
      slug: 'theme-1',
      topics: [
        {
          id: 'topic-1',
          title: 'Topic 1',
          slug: 'topic-1',
          publications: [
            {
              id: 'publication-1',
              title: 'Publication 1',
              slug: 'publication-1',
            },
          ],
        },
      ],
    },
  ];

  const testSubjectMeta: PublicationSubjectMeta = {
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
      'Step 1 (current): Choose a publication',
    );

    expect(screen.getAllByRole('listitem')).toHaveLength(1);
  });

  test('renders fetched publication subjects if `initialState.query.publicationId` set', async () => {
    tableBuilderService.getPublicationMeta.mockImplementation(() =>
      Promise.resolve<PublicationMeta>({
        publicationId: 'publication-1',
        subjects: [{ id: 'subject-1', label: 'Subject 1' }],
        highlights: [],
      }),
    );

    render(
      <TableToolWizard
        themeMeta={testThemeMeta}
        initialState={{
          initialStep: 2,
          subjectMeta: testSubjectMeta,
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
      expect(tableBuilderService.getPublicationMeta).toHaveBeenCalledWith(
        'publication-1',
      );

      expect(screen.getByLabelText('Subject 1')).toBeInTheDocument();
    });
  });

  test('does not render publication step if `initialState.query.releaseId` is set', async () => {
    tableBuilderService.getReleaseMeta.mockImplementation(() =>
      Promise.resolve<ReleaseMeta>({
        releaseId: 'release-1',
        subjects: [{ id: 'subject-2', label: 'Subject 2' }],
      }),
    );

    render(
      <TableToolWizard
        themeMeta={testThemeMeta}
        initialState={{
          initialStep: 1,
          subjectMeta: testSubjectMeta,
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
        'Step 1 (current): Choose a subject',
      );
    });
  });

  test('renders fetched publication release subjects if `initialState.query.releaseId` is set', async () => {
    tableBuilderService.getPublicationMeta.mockImplementation(() =>
      Promise.resolve<PublicationMeta>({
        publicationId: 'publication-1',
        subjects: [{ id: 'subject-1', label: 'Subject 1' }],
        highlights: [],
      }),
    );

    tableBuilderService.getReleaseMeta.mockImplementation(() =>
      Promise.resolve<ReleaseMeta>({
        releaseId: 'release-1',
        subjects: [{ id: 'subject-2', label: 'Subject 2' }],
      }),
    );

    render(
      <TableToolWizard
        themeMeta={testThemeMeta}
        initialState={{
          initialStep: 1,
          subjectMeta: testSubjectMeta,
          query: {
            publicationId: 'publication-1',
            releaseId: 'release-1',
            subjectId: '',
            locations: {},
            filters: [],
            indicators: [],
          },
        }}
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getPublicationMeta).not.toHaveBeenCalled();
      expect(tableBuilderService.getReleaseMeta).toHaveBeenCalledWith(
        'release-1',
      );

      expect(screen.queryByText('Subject 1')).not.toBeInTheDocument();
      expect(screen.getByLabelText('Subject 2')).toBeInTheDocument();
    });
  });

  test('renders table highlights on step 2 when it is the current step', async () => {
    tableBuilderService.getPublicationMeta.mockImplementation(() =>
      Promise.resolve<PublicationMeta>({
        publicationId: 'publication-1',
        subjects: [{ id: 'subject-1', label: 'Subject 1' }],
        highlights: [
          { id: 'highlight-1', label: 'Test highlight 1' },
          { id: 'highlight-2', label: 'Test highlight 2' },
          { id: 'highlight-3', label: 'Test highlight 3' },
        ],
      }),
    );

    render(
      <TableToolWizard
        themeMeta={testThemeMeta}
        initialState={{
          initialStep: 2,
          subjectMeta: testSubjectMeta,
          query: {
            publicationId: 'publication-1',
            subjectId: '',
            locations: {},
            filters: [],
            indicators: [],
          },
        }}
        renderHighlights={highlights => (
          <ul>
            {highlights.map(highlight => (
              <li key={highlight.id} id={highlight.id}>
                {highlight.label}
              </li>
            ))}
          </ul>
        )}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('wizardStep-2')).toHaveAttribute(
        'aria-current',
        'step',
      );

      const step2 = within(screen.getByTestId('wizardStep-2'));

      expect(step2.getByText('Test highlight 1')).toHaveAttribute(
        'id',
        'highlight-1',
      );
      expect(step2.getByText('Test highlight 2')).toHaveAttribute(
        'id',
        'highlight-2',
      );
      expect(step2.getByText('Test highlight 3')).toHaveAttribute(
        'id',
        'highlight-3',
      );
    });
  });

  test('does not render table highlights when step 2 is not the current step', async () => {
    tableBuilderService.getPublicationMeta.mockImplementation(() =>
      Promise.resolve<PublicationMeta>({
        publicationId: 'publication-1',
        subjects: [{ id: 'subject-1', label: 'Subject 1' }],
        highlights: [
          { id: 'highlight-1', label: 'Test highlight 1' },
          { id: 'highlight-2', label: 'Test highlight 2' },
          { id: 'highlight-3', label: 'Test highlight 3' },
        ],
      }),
    );

    render(
      <TableToolWizard
        themeMeta={testThemeMeta}
        initialState={{
          initialStep: 3,
          subjectMeta: testSubjectMeta,
          query: {
            publicationId: 'publication-1',
            subjectId: 'subject-1',
            locations: {},
            filters: [],
            indicators: [],
          },
        }}
        renderHighlights={highlights => (
          <ul>
            {highlights.map(highlight => (
              <li key={highlight.id} id={highlight.id}>
                {highlight.label}
              </li>
            ))}
          </ul>
        )}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('wizardStep-2')).not.toHaveAttribute(
        'aria-current',
        'step',
      );

      const step2 = within(screen.getByTestId('wizardStep-2'));

      expect(step2.queryByText('Test highlight 1')).not.toBeInTheDocument();
      expect(step2.queryByText('Test highlight 2')).not.toBeInTheDocument();
      expect(step2.queryByText('Test highlight 3')).not.toBeInTheDocument();
    });
  });

  test('renders all steps correctly when full `initialState` is provided', async () => {
    tableBuilderService.getPublicationMeta.mockImplementation(() =>
      Promise.resolve<PublicationMeta>({
        publicationId: 'publication-1',
        subjects: [{ id: 'subject-1', label: 'Subject 1' }],
        highlights: [],
      }),
    );

    render(
      <TableToolWizard
        themeMeta={testThemeMeta}
        initialState={{
          initialStep: 5,
          subjectMeta: testSubjectMeta,
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
      expect(stepHeadings[0]).toHaveTextContent('Step 1: Choose a publication');
      expect(stepHeadings[1]).toHaveTextContent('Step 2: Choose a subject');
      expect(stepHeadings[2]).toHaveTextContent('Step 3: Choose locations');
      expect(stepHeadings[3]).toHaveTextContent('Step 4: Choose time period');
      expect(stepHeadings[4]).toHaveTextContent(
        'Step 5 (current): Choose your filters',
      );

      // Step 1

      const step1 = within(screen.getByTestId('wizardStep-1'));
      expect(step1.getByRole('term')).toHaveTextContent('Publication');
      expect(step1.getByRole('definition')).toHaveTextContent('Publication 1');

      // Step 2

      const step2 = within(screen.getByTestId('wizardStep-2'));
      expect(step2.getByRole('term')).toHaveTextContent('Subject');
      expect(step2.getByRole('definition')).toHaveTextContent('Subject 1');

      // Step 3

      const step3 = within(screen.getByTestId('wizardStep-3'));
      expect(step3.getByRole('term')).toHaveTextContent('Local authority');

      const step3Locations = within(step3.getByRole('definition')).getAllByRole(
        'listitem',
      );
      expect(step3Locations).toHaveLength(2);
      expect(step3Locations[0]).toHaveTextContent('Barnet');
      expect(step3Locations[1]).toHaveTextContent('Barnsley');

      // Step 4

      const step4 = within(screen.getByTestId('wizardStep-4'));
      const step4Terms = step4.getAllByRole('term');
      const step4Definitions = step4.getAllByRole('definition');

      expect(step4Terms).toHaveLength(2);
      expect(step4Terms[0]).toHaveTextContent('Start date');
      expect(step4Terms[1]).toHaveTextContent('End date');

      expect(step4Definitions).toHaveLength(2);
      expect(step4Definitions[0]).toHaveTextContent('2013/14');
      expect(step4Definitions[1]).toHaveTextContent('2014/15');

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
