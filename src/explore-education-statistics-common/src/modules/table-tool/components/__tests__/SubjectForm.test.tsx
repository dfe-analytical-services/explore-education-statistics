import { getDescribedBy } from '@common-test/queries';
import SubjectForm, {
  SubjectFormValues,
} from '@common/modules/table-tool/components/SubjectForm';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { Subject } from '@common/services/tableBuilderService';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('SubjectForm', () => {
  const testOptions: Subject[] = [
    {
      id: 'subject-1',
      name: 'Subject 1',
      content: '<p>Test content 1</p>',
      timePeriods: {
        from: '2018/19',
        to: '2020/21',
      },
      geographicLevels: ['Local Authority District', 'Ward'],
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
    },
  ];

  const wizardProps: InjectedWizardProps = {
    shouldScroll: true,
    stepNumber: 1,
    currentStep: 1,
    setCurrentStep: noop,
    isActive: true,
    isLoading: false,
    goToNextStep: noop,
    goToPreviousStep: noop,
  };

  test('renders subjects correctly with details', () => {
    jest.useFakeTimers();

    const { container } = render(
      <SubjectForm {...wizardProps} onSubmit={noop} options={testOptions} />,
    );

    const radios = screen.getAllByLabelText(/Subject/);

    expect(radios).toHaveLength(2);
    expect(radios[0]).toHaveAttribute('value', 'subject-1');
    expect(radios[1]).toHaveAttribute('value', 'subject-2');

    const subject1Hint = within(getDescribedBy(container, radios[0]));

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

    const subject2Hint = within(getDescribedBy(container, radios[1]));

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

  test('renders empty message when there are no publication options', () => {
    render(<SubjectForm {...wizardProps} onSubmit={noop} options={[]} />);

    expect(screen.queryAllByRole('radio')).toHaveLength(0);
    expect(screen.getByText(/No subjects available/)).toBeInTheDocument();
  });

  test('renders read-only view with initial `subjectId` when step is not active', () => {
    render(
      <SubjectForm
        {...wizardProps}
        initialValues={{
          subjectId: 'subject-1',
        }}
        isActive={false}
        onSubmit={noop}
        options={testOptions}
      />,
    );

    expect(screen.queryAllByRole('radio')).toHaveLength(0);
    expect(screen.getByTestId('Subject')).toHaveTextContent('Subject 1');
  });

  test('renders read-only view without initial `subjectId` when step is not active', () => {
    render(
      <SubjectForm
        {...wizardProps}
        isActive={false}
        onSubmit={noop}
        options={testOptions}
      />,
    );

    expect(screen.queryAllByRole('radio')).toHaveLength(0);
    expect(screen.getByTestId('Subject')).toHaveTextContent('None');
  });

  test('renders read-only view with selected subject when step is not active', () => {
    const { rerender } = render(
      <SubjectForm {...wizardProps} onSubmit={noop} options={testOptions} />,
    );

    expect(screen.queryByTestId('Subject')).not.toBeInTheDocument();

    userEvent.click(screen.getByLabelText('Subject 1'));

    rerender(
      <SubjectForm
        {...wizardProps}
        isActive={false}
        onSubmit={noop}
        options={testOptions}
      />,
    );

    expect(screen.queryAllByRole('radio')).toHaveLength(0);
    expect(screen.getByTestId('Subject')).toHaveTextContent('Subject 1');
  });

  test('clicking `Next step` calls `onSubmit` with correct values', async () => {
    const handleSubmit = jest.fn();

    render(
      <SubjectForm
        {...wizardProps}
        onSubmit={handleSubmit}
        options={testOptions}
      />,
    );

    expect(screen.queryByTestId('Subject')).not.toBeInTheDocument();

    userEvent.click(screen.getByLabelText('Subject 1'));

    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    const expected: SubjectFormValues = {
      subjectId: 'subject-1',
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith(expected);
    });
  });
});
