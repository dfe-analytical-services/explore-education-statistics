/* eslint-disable @next/next/no-html-link-for-pages */
import flushPromises from '@common-test/flushPromises';
import { getDescribedBy } from '@common-test/queries';
import { SubjectFormValues } from '@common/modules/table-tool/components/SubjectForm';
import SubjectStep from '@common/modules/table-tool/components/SubjectStep';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { Subject, FeaturedTable } from '@common/services/tableBuilderService';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('SubjectStep', () => {
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
    },
  ];

  const testSingleSubject: Subject[] = [
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
    },
  ];

  const testFeaturedTables: FeaturedTable[] = [
    {
      id: 'highlight-1',
      name: 'Test highlight 1',
      description: 'Test highlight description 1',
    },
    {
      id: 'highlight-2',
      name: 'Test highlight 2',
      description: 'Test highlight description 2',
    },
    {
      id: 'highlight-3',
      name: 'Test highlight 3',
      description: 'Test highlight description 3',
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

  test('renders non-tabbed view with subjects', () => {
    jest.useFakeTimers();

    render(
      <SubjectStep
        {...wizardProps}
        subjects={testSubjects}
        subjectId=""
        onSubmit={noop}
      />,
    );

    expect(screen.queryByRole('tab')).not.toBeInTheDocument();

    const radios = screen.getAllByLabelText(/Subject/);

    expect(radios).toHaveLength(2);
    expect(radios[0]).toHaveAttribute('value', 'subject-1');
    expect(radios[1]).toHaveAttribute('value', 'subject-2');

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
  });

  test('renders tabbed view with highlights and subjects', () => {
    render(
      <SubjectStep
        {...wizardProps}
        featuredTables={testFeaturedTables}
        subjects={testSubjects}
        subjectId=""
        onSubmit={noop}
        renderFeaturedTable={highlight => <a href="/">{highlight.name}</a>}
      />,
    );

    const tabs = screen.getAllByRole('tabpanel', { hidden: true });

    expect(tabs).toHaveLength(2);
    expect(tabs[0]).toBeVisible();
    expect(tabs[1]).not.toBeVisible();

    const tab1 = within(tabs[0]);

    expect(
      tab1.getByRole('link', { name: 'Test highlight 1' }),
    ).toBeInTheDocument();
    expect(
      tab1.getByRole('link', { name: 'Test highlight 2' }),
    ).toBeInTheDocument();
    expect(
      tab1.getByRole('link', { name: 'Test highlight 3' }),
    ).toBeInTheDocument();

    expect(tab1.getByText('Test highlight description 1')).toBeInTheDocument();
    expect(tab1.getByText('Test highlight description 2')).toBeInTheDocument();
    expect(tab1.getByText('Test highlight description 3')).toBeInTheDocument();

    const tab2 = within(tabs[1]);

    expect(tab2.getAllByRole('radio', { hidden: true })).toHaveLength(2);
  });

  test('does not render tabbed view if no `renderHighlightLink`', () => {
    render(
      <SubjectStep
        {...wizardProps}
        featuredTables={testFeaturedTables}
        subjects={testSubjects}
        subjectId=""
        onSubmit={noop}
      />,
    );

    expect(screen.queryByRole('tab')).not.toBeInTheDocument();
    expect(screen.queryAllByText(/Test highlight/)).toHaveLength(0);

    expect(screen.getAllByRole('radio')).toHaveLength(2);
  });

  test('does not render tabbed view if no highlights', () => {
    render(
      <SubjectStep
        {...wizardProps}
        featuredTables={[]}
        subjects={testSubjects}
        subjectId=""
        onSubmit={noop}
        renderFeaturedTable={highlight => <a href="/">{highlight.name}</a>}
      />,
    );

    expect(screen.queryByRole('tab')).not.toBeInTheDocument();
    expect(screen.queryAllByText(/Test highlight/)).toHaveLength(0);

    expect(screen.getAllByRole('radio')).toHaveLength(2);
  });

  test('renders correct step heading when no highlights rendered', () => {
    render(
      <SubjectStep
        {...wizardProps}
        subjects={testSubjects}
        subjectId=""
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Step 1 (current) Choose a subject',
      }),
    ).toBeInTheDocument();
  });

  test('renders correct step heading when highlights rendered', () => {
    render(
      <SubjectStep
        {...wizardProps}
        featuredTables={testFeaturedTables}
        subjects={testSubjects}
        subjectId=""
        onSubmit={noop}
        renderFeaturedTable={highlight => <a href="/">{highlight.name}</a>}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Step 1 (current) View a featured table or create your own',
      }),
    ).toBeInTheDocument();
  });

  test('renders empty message when there are no subjects', () => {
    render(
      <SubjectStep
        {...wizardProps}
        subjects={[]}
        subjectId=""
        onSubmit={noop}
      />,
    );

    expect(screen.queryAllByRole('radio')).toHaveLength(0);
    expect(screen.getByText(/No subjects available/)).toBeInTheDocument();
  });

  test('automatically selects the subject if only one is available', () => {
    render(
      <SubjectStep
        {...wizardProps}
        subjects={testSingleSubject}
        onSubmit={noop}
      />,
    );

    const radios = screen.getAllByLabelText(/Subject/);

    expect(radios).toHaveLength(1);
    expect(radios[0]).toHaveAttribute('value', 'subject-1');
    expect(radios[0]).toBeChecked();
  });

  test('renders read-only view with initial `subjectId` when step is not active', () => {
    render(
      <SubjectStep
        {...wizardProps}
        featuredTables={testFeaturedTables}
        subjectId="subject-1"
        subjects={testSubjects}
        isActive={false}
        renderFeaturedTable={highlight => <a href="/">{highlight.name}</a>}
        onSubmit={noop}
      />,
    );

    expect(screen.queryByRole('tab')).not.toBeInTheDocument();
    expect(screen.queryAllByRole('radio')).toHaveLength(0);

    expect(screen.getByTestId('Subject')).toHaveTextContent('Subject 1');
  });

  test('renders read-only view without initial `subjectId` when step is not active', () => {
    render(
      <SubjectStep
        {...wizardProps}
        isActive={false}
        featuredTables={testFeaturedTables}
        subjects={testSubjects}
        onSubmit={noop}
        renderFeaturedTable={highlight => <a href="/">{highlight.name}</a>}
      />,
    );

    expect(screen.queryByRole('tab')).not.toBeInTheDocument();
    expect(screen.queryAllByRole('radio')).toHaveLength(0);

    expect(screen.getByTestId('Subject')).toHaveTextContent('None');
  });

  test('renders read-only view with correct step heading when no highlights rendered', () => {
    render(
      <SubjectStep
        {...wizardProps}
        isActive={false}
        subjects={testSubjects}
        subjectId=""
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Step 1 Choose a subject',
      }),
    ).toBeInTheDocument();
  });

  test('renders read-only view with correct step heading when highlights rendered', () => {
    render(
      <SubjectStep
        {...wizardProps}
        isActive={false}
        featuredTables={testFeaturedTables}
        subjects={testSubjects}
        subjectId=""
        onSubmit={noop}
        renderFeaturedTable={highlight => <a href="/">{highlight.name}</a>}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Step 1 View a featured table or create your own',
      }),
    ).toBeInTheDocument();
  });

  test('clicking `Next step` calls `onSubmit` with correct values', async () => {
    const handleSubmit = jest.fn();

    render(
      <SubjectStep
        {...wizardProps}
        subjects={testSubjects}
        onSubmit={handleSubmit}
      />,
    );

    expect(screen.queryByTestId('Subject')).not.toBeInTheDocument();

    userEvent.click(screen.getByLabelText('Subject 1'));

    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    const expected: SubjectFormValues = {
      subjectId: 'subject-1',
    };
    await flushPromises();
    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith(expected);
    });
  });
});
