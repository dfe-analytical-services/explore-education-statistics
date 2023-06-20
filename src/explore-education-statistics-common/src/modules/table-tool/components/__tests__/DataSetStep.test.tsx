import { getDescribedBy } from '@common-test/queries';
import DataSetStep, {
  DataSetFormValues,
} from '@common/modules/table-tool/components/DataSetStep';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { SelectedRelease } from '@common/modules/table-tool/types/selectedPublication';
import { Subject, FeaturedTable } from '@common/services/tableBuilderService';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('DataSetStep', () => {
  const testSubjects: Subject[] = [
    {
      id: 'subject-1',
      name: 'Subject 1',
      content: '<p>Test content 1</p>',
      timePeriods: {
        from: '2018/19',
        to: '2020/21',
      },
      geographicLevels: ['Local Authority District', 'Ward'],
      file: {
        id: 'file-1',
        name: 'Subject 1',
        fileName: 'file-1.csv',
        extension: 'csv',
        size: '10 Mb',
        type: 'Data',
      },
      filters: ['School type'],
      indicators: ['Headcount', 'Percent'],
    },
    {
      id: 'subject-2',
      name: 'Subject 2',
      content: '<p>Test content 2</p>',
      timePeriods: {
        from: '2015',
        to: '2020',
      },
      geographicLevels: ['National', 'Local Authority'],
      file: {
        id: 'file-2',
        name: 'Subject 2',
        fileName: 'file-2.csv',
        extension: 'csv',
        size: '20 Mb',
        type: 'Data',
      },
      filters: ['Ethnicity', 'FSM'],
      indicators: ['Authorised absence rate'],
    },
  ];

  const testFeaturedTables: FeaturedTable[] = [
    {
      id: 'featured-1',
      name: 'Test featured 1',
      description: 'Test featured description 1',
      subjectId: 'subject-1',
      dataBlockId: 'dataBlock-1',
      order: 0,
    },
    {
      id: 'featured-2',
      name: 'Test featured 2',
      description: 'Test featured description 2 find me',
      subjectId: 'subject-1',
      dataBlockId: 'dataBlock-2',
      order: 1,
    },
    {
      id: 'featured-3',
      name: 'Test featured 3',
      description: 'Test featured description 3',
      subjectId: 'subject-3',
      dataBlockId: 'dataBlock-3',
      order: 3,
    },
  ];

  const wizardProps: InjectedWizardProps = {
    shouldScroll: true,
    stepNumber: 1,
    currentStep: 1,
    isActive: true,
    isEnabled: true,
    isLoading: false,
    setCurrentStep: (step, task) => task?.(),
    goToNextStep: task => task?.(),
    goToPreviousStep: task => task?.(),
  };

  const testRelease: SelectedRelease = {
    id: 'release-1',
    title: 'Release 1',
    slug: 'release-1',
    latestData: true,
  };

  test('renders radios with details if no `renderFeaturedTableLink `', () => {
    render(
      <DataSetStep {...wizardProps} subjects={testSubjects} onSubmit={noop} />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Step 1 (current) Select a data set',
      }),
    ).toBeInTheDocument();

    const radios = within(
      screen.getByRole('group', {
        name: 'Step 1 (current) Select a data set',
      }),
    ).getAllByRole('radio');
    expect(radios).toHaveLength(2);
    expect(radios[0]).toEqual(screen.getByLabelText('Subject 1'));
    expect(radios[1]).toEqual(screen.getByLabelText('Subject 2'));

    const subject1Hint = within(getDescribedBy(radios[0]));
    expect(
      within(subject1Hint.getByTestId('Content')).getByText('Test content 1', {
        selector: 'p',
      }),
    ).toBeInTheDocument();
    expect(subject1Hint.getByTestId('Geographic levels')).toHaveTextContent(
      'Local Authority District; Ward',
    );
    expect(subject1Hint.getByTestId('Time period')).toHaveTextContent(
      '2018/19 to 2020/21',
    );

    const indicators1 = within(
      subject1Hint.getByTestId('indicators'),
    ).getAllByRole('listitem');
    expect(indicators1).toHaveLength(2);
    expect(indicators1[0]).toHaveTextContent('Headcount');
    expect(indicators1[1]).toHaveTextContent('Percent');

    const filters1 = within(subject1Hint.getByTestId('filters')).getAllByRole(
      'listitem',
    );
    expect(filters1).toHaveLength(1);
    expect(filters1[0]).toHaveTextContent('School type');

    const subject2Hint = within(getDescribedBy(radios[1]));
    expect(
      within(subject2Hint.getByTestId('Content')).getByText('Test content 2', {
        selector: 'p',
      }),
    ).toBeInTheDocument();
    expect(subject2Hint.getByTestId('Geographic levels')).toHaveTextContent(
      'Local Authority; National',
    );
    expect(subject2Hint.getByTestId('Time period')).toHaveTextContent(
      '2015 to 2020',
    );

    const indicators2 = within(
      subject2Hint.getByTestId('indicators'),
    ).getAllByRole('listitem');
    expect(indicators2).toHaveLength(1);
    expect(indicators2[0]).toHaveTextContent('Authorised absence rate');

    const filters2 = within(subject2Hint.getByTestId('filters')).getAllByRole(
      'listitem',
    );
    expect(filters2).toHaveLength(2);
    expect(filters2[0]).toHaveTextContent('Ethnicity');
    expect(filters2[1]).toHaveTextContent('FSM');

    expect(
      screen.getByRole('button', {
        name: 'Next step',
      }),
    ).toBeInTheDocument();
  });

  test('when no `renderFeaturedTableLink`, clicking Next step calls `onSubmit` with correct values', async () => {
    const handleSubmit = jest.fn();

    render(
      <DataSetStep
        {...wizardProps}
        subjects={testSubjects}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(screen.getByLabelText('Subject 1'));

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    const expected: DataSetFormValues = {
      subjectId: 'subject-1',
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(expected);
    });
  });

  test('renders correctly when `renderFeaturedTableLink` is set', () => {
    render(
      <DataSetStep
        {...wizardProps}
        featuredTables={testFeaturedTables}
        renderFeaturedTableLink={table => <a href="/">{table.name}</a>}
        subjects={testSubjects}
        onSubmit={noop}
      />,
    );

    const radios = within(
      screen.getByRole('group', {
        name: 'View all featured tables or select a data set',
      }),
    ).getAllByRole('radio');
    expect(radios).toHaveLength(3);
    expect(radios[0]).toEqual(
      screen.getByLabelText('View all featured tables'),
    );
    expect(radios[1]).toEqual(screen.getByLabelText('Subject 1'));
    expect(radios[2]).toEqual(screen.getByLabelText('Subject 2'));

    expect(
      screen.getByText(
        'Please select a data set, you will then be able to see a summary of the data, create your own tables, view featured tables, or download the entire data file.',
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'More details for Subject 1' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'More details for Subject 2' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Next step' }),
    ).not.toBeInTheDocument();
  });

  test('shows the correct step heading and the `view all featured tables` option if the release has featured tables', () => {
    render(
      <DataSetStep
        {...wizardProps}
        featuredTables={testFeaturedTables}
        renderFeaturedTableLink={table => <a href="/">{table.name}</a>}
        subjects={testSubjects}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Step 1 (current) Select a data set or featured table',
      }),
    ).toBeInTheDocument();

    const radios = within(
      screen.getByRole('group', {
        name: 'View all featured tables or select a data set',
      }),
    ).getAllByRole('radio');
    expect(radios).toHaveLength(3);
    expect(radios[0]).toEqual(
      screen.getByLabelText('View all featured tables'),
    );
    expect(radios[1]).toEqual(screen.getByLabelText('Subject 1'));
    expect(radios[2]).toEqual(screen.getByLabelText('Subject 2'));
  });

  test('shows the correct step heading and does not show the `view all featured tables` option if the release has no featured tables', () => {
    render(
      <DataSetStep
        {...wizardProps}
        renderFeaturedTableLink={table => <a href="/">{table.name}</a>}
        subjects={testSubjects}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Step 1 (current) Select a data set',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByLabelText('View all featured tables'),
    ).not.toBeInTheDocument();

    const radios = within(
      screen.getByRole('group', {
        name: 'Select a data set',
      }),
    ).getAllByRole('radio');
    expect(radios).toHaveLength(2);
    expect(radios[0]).toEqual(screen.getByLabelText('Subject 1'));
    expect(radios[1]).toEqual(screen.getByLabelText('Subject 2'));
  });

  test('clicking `view all featured tables` shows all featured tables for the release', () => {
    render(
      <DataSetStep
        {...wizardProps}
        featuredTables={testFeaturedTables}
        renderFeaturedTableLink={table => <a href="/">{table.name}</a>}
        subjects={testSubjects}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('View all featured tables'));

    expect(
      screen.getByRole('heading', {
        name: 'All featured tables for this publication',
      }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Search featured tables')).toBeInTheDocument();

    const featuredTables = within(
      screen.getByTestId('featuredTables'),
    ).getAllByRole('listitem');
    expect(featuredTables).toHaveLength(3);

    expect(
      within(featuredTables[0]).getByRole('link', { name: 'Test featured 1' }),
    ).toBeInTheDocument();
    expect(
      within(featuredTables[0]).getByText('Test featured description 1'),
    ).toBeInTheDocument();

    expect(
      within(featuredTables[1]).getByRole('link', { name: 'Test featured 2' }),
    ).toBeInTheDocument();
    expect(
      within(featuredTables[1]).getByText(
        'Test featured description 2 find me',
      ),
    ).toBeInTheDocument();

    expect(
      within(featuredTables[2]).getByRole('link', { name: 'Test featured 3' }),
    ).toBeInTheDocument();
    expect(
      within(featuredTables[2]).getByText('Test featured description 3'),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(
        'Please select a data set, you will then be able to see a summary of the data, create your own tables, view featured tables, or download the entire data file.',
      ),
    ).not.toBeInTheDocument();
  });

  test('searching featured tables filters the list', async () => {
    render(
      <DataSetStep
        {...wizardProps}
        featuredTables={testFeaturedTables}
        renderFeaturedTableLink={table => <a href="/">{table.name}</a>}
        subjects={testSubjects}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('View all featured tables'));

    expect(
      within(screen.getByTestId('featuredTables')).getAllByRole('listitem'),
    ).toHaveLength(3);

    userEvent.type(screen.getByLabelText('Search featured tables'), 'find me');
    userEvent.click(screen.getByRole('button', { name: 'Search' }));

    await waitFor(() => {
      expect(screen.getByText('Clear search')).toBeInTheDocument();
    });

    const featuredTables = within(
      screen.getByTestId('featuredTables'),
    ).getAllByRole('listitem');

    expect(featuredTables).toHaveLength(1);

    expect(
      within(featuredTables[0]).getByRole('link', { name: 'Test featured 2' }),
    ).toBeInTheDocument();
    expect(
      within(featuredTables[0]).getByText(
        'Test featured description 2 find me',
      ),
    ).toBeInTheDocument();
  });

  test('clicking `clear search` shows all featured tables', async () => {
    render(
      <DataSetStep
        {...wizardProps}
        featuredTables={testFeaturedTables}
        renderFeaturedTableLink={table => <a href="/">{table.name}</a>}
        subjects={testSubjects}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('View all featured tables'));

    expect(
      within(screen.getByTestId('featuredTables')).getAllByRole('listitem'),
    ).toHaveLength(3);

    userEvent.type(screen.getByLabelText('Search featured tables'), 'find me');
    userEvent.click(screen.getByRole('button', { name: 'Search' }));

    await waitFor(() => {
      expect(screen.getByText('Clear search')).toBeInTheDocument();
    });

    expect(
      within(screen.getByTestId('featuredTables')).getAllByRole('listitem'),
    ).toHaveLength(1);

    userEvent.click(screen.getByRole('button', { name: 'Clear search' }));

    await waitFor(() => {
      expect(screen.queryByText('Clear search')).not.toBeInTheDocument();
    });

    expect(
      within(screen.getByTestId('featuredTables')).getAllByRole('listitem'),
    ).toHaveLength(3);
  });

  test('selecting a data set shows the details and featured tables', () => {
    render(
      <DataSetStep
        {...wizardProps}
        release={testRelease}
        featuredTables={testFeaturedTables}
        renderFeaturedTableLink={table => <a href="/">{table.name}</a>}
        subjects={testSubjects}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('Subject 1'));

    expect(
      screen.getByRole('heading', { name: 'Data set details' }),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('Content')).getByText('Test content 1', {
        selector: 'p',
      }),
    ).toBeInTheDocument();
    expect(screen.getByTestId('Geographic levels')).toHaveTextContent(
      'Local Authority District; Ward',
    );
    expect(screen.getByTestId('Time period')).toHaveTextContent(
      '2018/19 to 2020/21',
    );

    const indicators = within(screen.getByTestId('indicators')).getAllByRole(
      'listitem',
    );
    expect(indicators).toHaveLength(2);
    expect(indicators[0]).toHaveTextContent('Headcount');
    expect(indicators[1]).toHaveTextContent('Percent');

    const filters = within(screen.getByTestId('filters')).getAllByRole(
      'listitem',
    );
    expect(filters).toHaveLength(1);
    expect(filters[0]).toHaveTextContent('School type');

    expect(
      screen.getByRole('button', { name: 'Create your own table' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Download full data set (ZIP)' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'View our featured tables' }),
    ).toBeInTheDocument();

    const featuredTables = within(
      screen.getByTestId('featuredTables'),
    ).getAllByRole('listitem');
    expect(featuredTables).toHaveLength(2);

    expect(
      within(featuredTables[0]).getByRole('link', { name: 'Test featured 1' }),
    ).toBeInTheDocument();
    expect(
      within(featuredTables[0]).getByText('Test featured description 1'),
    ).toBeInTheDocument();

    expect(
      within(featuredTables[1]).getByRole('link', { name: 'Test featured 2' }),
    ).toBeInTheDocument();
    expect(
      within(featuredTables[1]).getByText(
        'Test featured description 2 find me',
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(
        'Please select a data set, you will then be able to see a summary of the data, create your own tables, view featured tables, or download the entire data file.',
      ),
    ).not.toBeInTheDocument();
  });

  test('featured tables are not shown if none are related to the selected data set', () => {
    render(
      <DataSetStep
        {...wizardProps}
        featuredTables={testFeaturedTables}
        renderFeaturedTableLink={table => <a href="/">{table.name}</a>}
        release={testRelease}
        subjects={testSubjects}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('Subject 2'));

    expect(
      screen.queryByRole('heading', { name: 'View our featured tables' }),
    ).not.toBeInTheDocument();

    expect(screen.queryByTestId('featuredTables')).not.toBeInTheDocument();
  });

  test('renders empty message when there are no subjects', () => {
    render(<DataSetStep {...wizardProps} subjects={[]} onSubmit={noop} />);

    expect(screen.getByText('No data sets available.')).toBeInTheDocument();
  });

  test('renders read-only view with initial `subjectId` when step is not active', () => {
    render(
      <DataSetStep
        {...wizardProps}
        isActive={false}
        featuredTables={testFeaturedTables}
        renderFeaturedTableLink={table => <a href="/">{table.name}</a>}
        release={testRelease}
        subjectId="subject-1"
        subjects={testSubjects}
        onSubmit={noop}
      />,
    );

    expect(screen.queryAllByRole('radio')).toHaveLength(0);
    expect(screen.getByTestId('Data set')).toHaveTextContent('Subject 1');
  });

  test('renders read-only view without initial `subjectId` when step is not active', () => {
    render(
      <DataSetStep
        {...wizardProps}
        isActive={false}
        featuredTables={testFeaturedTables}
        renderFeaturedTableLink={table => <a href="/">{table.name}</a>}
        release={testRelease}
        subjectId=""
        subjects={testSubjects}
        onSubmit={noop}
      />,
    );

    expect(screen.queryAllByRole('radio')).toHaveLength(0);
    expect(screen.getByTestId('Data set')).toHaveTextContent('None');
  });

  test('clicking `Create table` calls `onSubmit` with correct values', async () => {
    const handleSubmit = jest.fn();

    render(
      <DataSetStep
        {...wizardProps}
        featuredTables={testFeaturedTables}
        renderFeaturedTableLink={table => <a href="/">{table.name}</a>}
        release={testRelease}
        subjects={testSubjects}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(screen.getByLabelText('Subject 1'));

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(
      screen.getByRole('button', { name: 'Create your own table' }),
    );

    const expected: DataSetFormValues = {
      subjectId: 'subject-1',
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(expected);
    });
  });
});
