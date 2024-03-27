import FootnoteForm from '@admin/pages/release/footnotes/components/FootnoteForm';
import { Footnote, FootnoteMeta } from '@admin/services/footnoteService';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { noop } from 'lodash';

describe('FootnoteForm', () => {
  const testFootnoteMeta: FootnoteMeta = {
    subjects: {
      'subject-1-id': {
        filters: {
          'filter-1-id': {
            hint: 'Filter 1 hint',
            legend: 'Filter 1',
            options: {
              'filter-1-group-1-id': {
                label: 'Default',
                options: [
                  {
                    label: 'Filter 1 Group 1 Item 1',
                    value: 'filter-1-group-1-item-1-id',
                  },
                  {
                    label: 'Filter 1 Group 1 Item 2',
                    value: 'filter-1-group-1-item-2-id',
                  },
                  {
                    label: 'Filter 1 Group 1 Item 3',
                    value: 'filter-1-group-1-item-3-id',
                  },
                ],
              },
            },
          },
          'filter-2-id': {
            hint: 'Filter 2 hint',
            legend: 'Filter 2',
            options: {
              'filter-2-group-1-id': {
                label: 'Filter 2 Group 1',
                options: [
                  {
                    label: 'Filter 2 Group 1 Item 1',
                    value: 'filter-2-group-1-item-1-id',
                  },
                  {
                    label: 'Filter 2 Group 1 Item 2',
                    value: 'filter-2-group-1-item-2-id',
                  },
                ],
              },
              'filter-2-group-2-id': {
                label: 'Filter 2 Group 2',
                options: [
                  {
                    label: 'Filter 2 Group 2 Item 1',
                    value: 'filter-2-group-2-item-1-id',
                  },
                  {
                    label: 'Filter 2 Group 2 Item 2',
                    value: 'filter-2-group-2-item-2-id',
                  },
                ],
              },
            },
          },
        },
        indicators: {
          'indicator-group-1-id': {
            label: 'Indicator Group 1',
            options: [
              {
                label: 'Indicator Group 1 Indicator 1',
                value: 'indicator-group-1-indicator-1-id',
                unit: '',
              },
              {
                label: 'Indicator Group 1 Indicator 2',
                value: 'indicator-group-1-indicator-2-id',
                unit: '',
              },
            ],
          },
          'indicator-group-2-id': {
            label: 'Indicator Group 2',
            options: [
              {
                label: 'Indicator Group 2 Indicator 1',
                value: 'indicator-group-2-indicator-1-id',
                unit: '',
              },
              {
                label: 'Indicator Group 2 Indicator 2',
                value: 'indicator-group-2-indicator-2-id',
                unit: '',
              },
            ],
          },
        },
        subjectId: 'subject-1-id',
        subjectName: 'Subject 1 name',
      },
      'subject-2-id': {
        filters: {},
        indicators: {
          'indicator-group-3-id': {
            label: 'Indicator Group 3',
            options: [
              {
                label: 'Indicator Group 3 Indicator 1',
                value: 'indicator-group-3-indicator-1-id',
                unit: '',
              },
              {
                label: 'Indicator Group 3 Indicator 2',
                value: 'indicator-group-3-indicator-2-id',
                unit: '',
              },
            ],
          },
        },
        subjectId: 'subject-2-id',
        subjectName: 'Subject 2 name',
      },
    },
  };

  test('renders correctly', () => {
    render(<FootnoteForm footnoteMeta={testFootnoteMeta} onSubmit={noop} />);

    expect(screen.getByLabelText('Footnote')).toBeInTheDocument();

    const subjects = screen.getAllByTestId(/footnote-subject/);
    expect(subjects).toHaveLength(2);

    expect(subjects[0]).toEqual(
      screen.getByRole('group', {
        name: 'Subject: Subject 1 name - select indicators and filters',
      }),
    );
    expect(
      within(subjects[0]).getByRole('radio', { name: 'Does not apply' }),
    ).toBeInTheDocument();
    expect(
      within(subjects[0]).getByRole('radio', { name: 'Applies to all data' }),
    ).toBeInTheDocument();
    expect(
      within(subjects[0]).getByRole('radio', {
        name: 'Applies to specific data',
      }),
    ).toBeInTheDocument();

    expect(subjects[1]).toEqual(
      screen.getByRole('group', {
        name: 'Subject: Subject 2 name - select indicators and filters',
      }),
    );
    expect(
      within(subjects[0]).getByRole('radio', { name: 'Does not apply' }),
    ).toBeInTheDocument();
    expect(
      within(subjects[0]).getByRole('radio', { name: 'Applies to all data' }),
    ).toBeInTheDocument();
    expect(
      within(subjects[0]).getByRole('radio', {
        name: 'Applies to specific data',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Save footnote' }),
    ).toBeInTheDocument();
  });

  test('shows the indicators and filters when select "Applies to specific data"', async () => {
    const user = userEvent.setup();
    render(<FootnoteForm footnoteMeta={testFootnoteMeta} onSubmit={noop} />);

    const subject1 = within(
      screen.getByRole('group', {
        name: 'Subject: Subject 1 name - select indicators and filters',
      }),
    );

    const specificRadio = subject1.getByRole('radio', {
      name: 'Applies to specific data',
    });
    expect(specificRadio).toHaveAttribute('aria-expanded', 'false');

    await user.click(specificRadio);

    expect(specificRadio).toHaveAttribute('aria-expanded', 'true');

    expect(
      subject1.getByRole('heading', { name: 'Indicators' }),
    ).toBeInTheDocument();

    await user.click(subject1.getByRole('button', { name: 'Indicators' }));

    const indicatorGroup1 = within(
      subject1.getByRole('group', { name: 'Indicator Group 1' }),
    );
    const group1Checkboxes = indicatorGroup1.getAllByRole('checkbox');
    expect(group1Checkboxes).toHaveLength(2);
    expect(group1Checkboxes[0]).toEqual(
      screen.getByRole('checkbox', { name: 'Indicator Group 1 Indicator 1' }),
    );
    expect(group1Checkboxes[1]).toEqual(
      screen.getByRole('checkbox', { name: 'Indicator Group 1 Indicator 2' }),
    );

    const indicatorGroup2 = within(
      subject1.getByRole('group', { name: 'Indicator Group 2' }),
    );
    const group2Checkboxes = indicatorGroup2.getAllByRole('checkbox');
    expect(group2Checkboxes).toHaveLength(2);
    expect(group2Checkboxes[0]).toEqual(
      screen.getByRole('checkbox', { name: 'Indicator Group 2 Indicator 1' }),
    );
    expect(group2Checkboxes[1]).toEqual(
      screen.getByRole('checkbox', { name: 'Indicator Group 2 Indicator 2' }),
    );

    expect(
      subject1.getByRole('heading', { name: 'Filters' }),
    ).toBeInTheDocument();

    await user.click(subject1.getByRole('button', { name: 'Filter 1' }));

    // only has default group
    const filter1 = within(screen.getByTestId('filter-filter-1-id'));
    const filter1Checkboxes = filter1.getAllByRole('checkbox');
    expect(filter1Checkboxes).toHaveLength(4);
    expect(filter1Checkboxes[0]).toEqual(
      filter1.getByRole('checkbox', { name: 'Select all' }),
    );
    expect(filter1Checkboxes[1]).toEqual(
      filter1.getByRole('checkbox', { name: 'Filter 1 Group 1 Item 1' }),
    );
    expect(filter1Checkboxes[2]).toEqual(
      filter1.getByRole('checkbox', { name: 'Filter 1 Group 1 Item 2' }),
    );
    expect(filter1Checkboxes[3]).toEqual(
      filter1.getByRole('checkbox', { name: 'Filter 1 Group 1 Item 3' }),
    );

    // has two filter groups
    const filter2 = within(screen.getByTestId('filter-filter-2-id'));
    const filter2Checkboxes = filter2.getAllByRole('checkbox');
    expect(filter2Checkboxes).toHaveLength(7);

    expect(filter2Checkboxes[0]).toEqual(
      filter2.getByRole('checkbox', { name: 'Select all' }),
    );

    expect(filter2Checkboxes[1]).toEqual(
      filter2.getByRole('checkbox', { name: 'Filter 2 Group 1' }),
    );

    const filter2Group1Checkboxes = within(
      filter2.getByRole('group', { name: 'Filter 2 Group 1' }),
    ).getAllByRole('checkbox');
    expect(filter2Group1Checkboxes).toHaveLength(2);
    expect(filter2Group1Checkboxes[0]).toEqual(
      filter2.getByRole('checkbox', { name: 'Filter 2 Group 1 Item 1' }),
    );
    expect(filter2Group1Checkboxes[1]).toEqual(
      filter2.getByRole('checkbox', { name: 'Filter 2 Group 1 Item 2' }),
    );

    expect(filter2Checkboxes[4]).toEqual(
      filter2.getByRole('checkbox', { name: 'Filter 2 Group 2' }),
    );

    const filter2Group2Checkboxes = within(
      filter2.getByRole('group', { name: 'Filter 2 Group 2' }),
    ).getAllByRole('checkbox');
    expect(filter2Group2Checkboxes).toHaveLength(2);
    expect(filter2Group2Checkboxes[0]).toEqual(
      filter2.getByRole('checkbox', { name: 'Filter 2 Group 2 Item 1' }),
    );
    expect(filter2Group2Checkboxes[1]).toEqual(
      filter2.getByRole('checkbox', { name: 'Filter 2 Group 2 Item 2' }),
    );
  });

  test('selecting all for a filter disables the other checkboxes in the filter', async () => {
    const user = userEvent.setup();
    render(<FootnoteForm footnoteMeta={testFootnoteMeta} onSubmit={noop} />);

    const subject1 = within(
      screen.getByRole('group', {
        name: 'Subject: Subject 1 name - select indicators and filters',
      }),
    );

    await user.click(
      subject1.getByRole('radio', {
        name: 'Applies to specific data',
      }),
    );

    await user.click(
      subject1.getByRole('button', {
        name: 'Filter 1',
      }),
    );

    const filter1 = within(screen.getByTestId('filter-filter-1-id'));

    await user.click(filter1.getByRole('checkbox', { name: 'Select all' }));

    expect(
      filter1.getByRole('checkbox', { name: 'Filter 1 Group 1 Item 1' }),
    ).toBeDisabled();
    expect(
      filter1.getByRole('checkbox', { name: 'Filter 1 Group 1 Item 2' }),
    ).toBeDisabled();
    expect(
      filter1.getByRole('checkbox', { name: 'Filter 1 Group 1 Item 3' }),
    ).toBeDisabled();
  });

  test('selecting all for a filter group disables the other checkboxes in the group', async () => {
    const user = userEvent.setup();
    render(<FootnoteForm footnoteMeta={testFootnoteMeta} onSubmit={noop} />);

    const subject1 = within(
      screen.getByRole('group', {
        name: 'Subject: Subject 1 name - select indicators and filters',
      }),
    );

    await user.click(
      subject1.getByRole('radio', {
        name: 'Applies to specific data',
      }),
    );

    await user.click(
      subject1.getByRole('button', {
        name: 'Filter 2',
      }),
    );

    const filter2 = within(screen.getByTestId('filter-filter-2-id'));
    await user.click(
      filter2.getByRole('checkbox', { name: 'Filter 2 Group 1' }),
    );
    const filter2Group1 = within(
      filter2.getByRole('group', {
        name: 'Filter 2 Group 1',
      }),
    );
    expect(
      filter2Group1.getByRole('checkbox', { name: 'Filter 2 Group 1 Item 1' }),
    ).toBeDisabled();
    expect(
      filter2Group1.getByRole('checkbox', { name: 'Filter 2 Group 1 Item 2' }),
    ).toBeDisabled();
  });

  test('shows a validation error if the form is submitted without content', async () => {
    const user = userEvent.setup();
    const handleSubmit = jest.fn();
    render(
      <FootnoteForm footnoteMeta={testFootnoteMeta} onSubmit={handleSubmit} />,
    );

    await user.click(screen.getByRole('button', { name: 'Save footnote' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Footnote content must be added' }),
    ).toHaveAttribute('href', '#footnoteForm-content');
    expect(screen.getByTestId('footnoteForm-content-error')).toHaveTextContent(
      'Footnote content must be added',
    );

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('shows a validation error if the form is submitted when no subject, indicator or filter is selected', async () => {
    const user = userEvent.setup();
    const handleSubmit = jest.fn();
    render(
      <FootnoteForm footnoteMeta={testFootnoteMeta} onSubmit={handleSubmit} />,
    );

    await user.type(screen.getByLabelText('Footnote'), 'The footnote');

    await user.click(screen.getByRole('button', { name: 'Save footnote' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'At least one Subject, Indicator or Filter must be selected',
      }),
    ).toHaveAttribute('href', '#footnoteForm-submit');

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('submits successfully with selected values', async () => {
    const user = userEvent.setup();
    const handleSubmit = jest.fn();
    render(
      <FootnoteForm footnoteMeta={testFootnoteMeta} onSubmit={handleSubmit} />,
    );

    await user.type(screen.getByLabelText('Footnote'), 'The footnote');

    const subject1 = within(
      screen.getByRole('group', {
        name: 'Subject: Subject 1 name - select indicators and filters',
      }),
    );
    const subject2 = within(
      screen.getByRole('group', {
        name: 'Subject: Subject 2 name - select indicators and filters',
      }),
    );

    await user.click(
      subject1.getByRole('radio', {
        name: 'Applies to specific data',
      }),
    );

    await user.click(
      subject1.getByRole('checkbox', { name: 'Indicator Group 1 Indicator 1' }),
    );

    await user.click(
      within(subject1.getByTestId('filter-filter-1-id')).getByRole('checkbox', {
        name: 'Select all',
      }),
    );

    await user.click(
      subject1.getByRole('checkbox', { name: 'Filter 2 Group 1' }),
    );

    await user.click(
      subject1.getByRole('checkbox', { name: 'Filter 2 Group 2 Item 2' }),
    );

    await user.click(
      subject2.getByRole('radio', {
        name: 'Applies to all data',
      }),
    );

    expect(handleSubmit).not.toHaveBeenCalled();
    await user.click(screen.getByRole('button', { name: 'Save footnote' }));

    expect(handleSubmit).toHaveBeenCalledTimes(1);
    expect(handleSubmit).toHaveBeenCalledWith({
      content: 'The footnote',
      subjects: {
        'subject-1-id': {
          selected: false,
          selectionType: 'Specific',
          indicatorGroups: {
            'indicator-group-1-id': {
              selected: false,
              indicators: ['indicator-group-1-indicator-1-id'],
            },
            'indicator-group-2-id': {
              selected: false,
              indicators: [],
            },
          },
          filters: {
            'filter-1-id': {
              selected: 'true',
              filterGroups: {
                'filter-1-group-1-id': { selected: false, filterItems: [] },
              },
            },
            'filter-2-id': {
              selected: false,
              filterGroups: {
                'filter-2-group-1-id': {
                  selected: 'true',
                  filterItems: [],
                },
                'filter-2-group-2-id': {
                  selected: false,
                  filterItems: ['filter-2-group-2-item-2-id'],
                },
              },
            },
          },
        },
        'subject-2-id': {
          filters: {},
          indicatorGroups: {
            'indicator-group-3-id': {
              indicators: [],
              selected: false,
            },
          },
          selected: false,
          selectionType: 'All',
        },
      },
    });
  });

  test('renders correctly with selected indicators and filters', async () => {
    const user = userEvent.setup();
    const testFootnote: Footnote = {
      id: 'test-footnote',
      content: 'The footnote',
      subjects: {
        'subject-1-id': {
          selected: false,
          selectionType: 'Specific',
          indicatorGroups: {
            'indicator-group-1-id': {
              selected: false,
              indicators: ['indicator-group-1-indicator-1-id'],
            },
            'indicator-group-2-id': {
              selected: false,
              indicators: [],
            },
          },
          filters: {
            'filter-1-id': {
              selected: true,
              filterGroups: {
                'filter-1-group-1-id': { selected: false, filterItems: [] },
              },
            },
            'filter-2-id': {
              selected: false,
              filterGroups: {
                'filter-2-group-1-id': {
                  selected: true,
                  filterItems: [],
                },
                'filter-2-group-2-id': {
                  selected: false,
                  filterItems: ['filter-2-group-2-item-2-id'],
                },
              },
            },
          },
        },
        'subject-2-id': {
          filters: {},
          indicatorGroups: {
            'indicator-group-3-id': {
              indicators: [],
              selected: false,
            },
          },
          selected: false,
          selectionType: 'All',
        },
      },
    };
    render(
      <FootnoteForm
        footnote={testFootnote}
        footnoteMeta={testFootnoteMeta}
        onSubmit={noop}
      />,
    );

    const subject1 = within(
      screen.getByRole('group', {
        name: 'Subject: Subject 1 name - select indicators and filters',
      }),
    );
    const subject2 = within(
      screen.getByRole('group', {
        name: 'Subject: Subject 2 name - select indicators and filters',
      }),
    );

    expect(
      subject1.getByRole('radio', {
        name: 'Applies to specific data',
      }),
    ).toBeChecked();

    await user.click(subject1.getByRole('button', { name: 'Indicators' }));

    expect(
      subject1.getByRole('checkbox', { name: 'Indicator Group 1 Indicator 1' }),
    ).toBeChecked();

    await user.click(subject1.getByRole('button', { name: 'Filter 1 (All)' }));

    expect(
      within(subject1.getByTestId('filter-filter-1-id')).getByRole('checkbox', {
        name: 'Select all',
      }),
    ).toBeChecked();

    await user.click(subject1.getByRole('button', { name: 'Filter 2' }));

    expect(
      subject1.getByRole('checkbox', { name: 'Filter 2 Group 1 (All)' }),
    ).toBeChecked();

    expect(
      subject1.getByRole('checkbox', { name: 'Filter 2 Group 2 Item 2' }),
    ).toBeChecked();

    expect(
      subject2.getByRole('radio', {
        name: 'Applies to all data',
      }),
    ).toBeChecked();
  });

  // renders correctly with selected indicators and filters items
  // renders correctly with selected filters and filter groups
});
