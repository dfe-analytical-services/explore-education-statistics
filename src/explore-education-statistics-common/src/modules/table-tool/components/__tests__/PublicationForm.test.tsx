import PublicationForm, {
  PublicationFormValues,
} from '@common/modules/table-tool/components/PublicationForm';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { Theme } from '@common/services/themeService';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('PublicationForm', () => {
  const testOptions: Theme[] = [
    {
      id: 'theme-1',
      title: 'Further education',
      summary: '',
      topics: [
        {
          id: 'topic-1',
          title: 'Further education and skills',
          summary: '',
          publications: [
            {
              id: 'publication-1',
              title: 'Apprenticeships and traineeships',
              slug: 'apprenticeships-and-traineeships',
              summary: '',
            },
          ],
        },
        {
          id: 'topic-2',
          title: 'National achievement rates tables',
          summary: '',
          publications: [
            {
              id: 'publication-2',
              title: 'National achievement rates tables',
              slug: 'national-achievement-rates-tables',
              summary: '',
            },
          ],
        },
      ],
    },
    {
      id: 'theme-2',
      title: 'Children, early years and social care',
      summary: '',
      topics: [
        {
          id: 'topic-3',
          title: 'Early years foundation stage profile',
          summary: '',
          publications: [
            {
              id: 'publication-3',
              title: 'Early years foundation stage profile results',
              slug: 'early-years-foundation-stage-profile-results',
              summary: '',
            },
          ],
        },
      ],
    },
    {
      id: 'theme-3',
      title: 'Pupils and schools',
      summary: '',
      topics: [
        {
          id: 'topic-4',
          title: 'School applications',
          summary: '',
          publications: [
            {
              id: 'publication-4',
              title: 'Secondary and primary schools applications and offers',
              slug: 'secondary-and-primary-schools-applications-and-offers',
              summary: '',
            },
          ],
        },
        {
          id: 'topic-5',
          title: 'Pupil absence',
          summary: '',
          publications: [
            {
              id: 'publication-5',
              title: 'Pupil absence in schools in England',
              slug: 'pupil-absence-in-schools-in-england',
              summary: '',
            },
          ],
        },
        {
          id: 'topic-6',
          title: 'Special educational needs (SEN)',
          summary: '',
          publications: [
            {
              id: 'publication-6',
              title: 'Statements of SEN and EHC plans',
              slug: 'statements-of-sen-and-ehc-plans',
              summary: '',
            },
          ],
        },
        {
          id: 'topic-7',
          title: 'Exclusions',
          summary: '',
          publications: [
            {
              id: 'publication-7',
              title: 'Permanent and fixed-period exclusions in England',
              slug: 'permanent-and-fixed-period-exclusions-in-england',
              summary: '',
            },
          ],
        },
      ],
    },
    {
      id: 'theme-4',
      title: 'School and college outcomes and performance',
      summary: '',
      topics: [
        {
          id: 'topic-8',
          title: 'Key stage 2',
          summary: '',
          publications: [
            {
              id: 'publication-8',
              title: 'National curriculum assessments at key stage 2',
              slug: 'national-curriculum-assessments-key-stage2',
              summary: '',
            },
          ],
        },
        {
          id: 'topic-9',
          title: 'GCSEs (key stage 4)',
          summary: '',
          publications: [
            {
              id: 'publication-9',
              title:
                'GCSE and equivalent results, including pupil characteristics',
              slug: 'gcse-results-including-pupil-characteristics',
              summary: '',
            },
          ],
        },
        {
          id: 'topic-10',
          title: '16 to 19 attainment',
          summary: '',
          publications: [
            {
              id: 'publication-10',
              title: 'Level 2 and 3 attainment by young people aged 19',
              slug: 'Level 2 and 3 attainment by young people aged 19',
              summary: '',
            },
          ],
        },
      ],
    },
  ];

  const wizardProps: InjectedWizardProps = {
    shouldScroll: true,
    stepNumber: 1,
    currentStep: 1,
    setCurrentStep: () => undefined,
    isActive: true,
    isLoading: false,
    goToNextStep: () => undefined,
    goToPreviousStep: () => undefined,
  };

  test('renders publication options filtered by title when using search field', async () => {
    jest.useFakeTimers();

    render(
      <PublicationForm
        {...wizardProps}
        onSubmit={noop}
        options={testOptions}
      />,
    );

    expect(screen.getAllByRole('radio', { hidden: true })).toHaveLength(10);

    await userEvent.type(
      screen.getByLabelText('Search publications'),
      'Early years',
    );

    jest.runOnlyPendingTimers();

    expect(screen.getAllByRole('radio')).toHaveLength(1);
    expect(
      screen.getByLabelText('Early years foundation stage profile results'),
    ).toHaveAttribute('type', 'radio');
  });

  test('renders publication options filtered by case-insensitive title', async () => {
    jest.useFakeTimers();

    render(
      <PublicationForm
        {...wizardProps}
        onSubmit={noop}
        options={testOptions}
      />,
    );

    expect(screen.getAllByRole('radio', { hidden: true })).toHaveLength(10);

    await userEvent.type(
      screen.getByLabelText('Search publications'),
      'early years',
    );

    jest.runOnlyPendingTimers();

    expect(screen.getAllByRole('radio')).toHaveLength(1);
    expect(
      screen.getByLabelText('Early years foundation stage profile results'),
    ).toHaveAttribute('type', 'radio');
  });

  test('does not throw error if regex sensitive search term is used', async () => {
    jest.useFakeTimers();

    render(
      <PublicationForm
        {...wizardProps}
        onSubmit={noop}
        options={testOptions}
      />,
    );

    await userEvent.type(screen.getByLabelText('Search publications'), '[');

    expect(() => {
      jest.runOnlyPendingTimers();
    }).not.toThrow();
  });

  test('renders empty message when there are no publication options', () => {
    render(<PublicationForm {...wizardProps} onSubmit={noop} options={[]} />);

    expect(screen.queryAllByRole('radio')).toHaveLength(0);
    expect(screen.queryByText('No publications found')).not.toBeNull();
  });

  test('renders empty message when there are no filtered publication options', async () => {
    jest.useFakeTimers();

    render(
      <PublicationForm
        {...wizardProps}
        onSubmit={noop}
        options={testOptions}
      />,
    );

    expect(screen.queryByText('No publications found')).not.toBeInTheDocument();
    expect(screen.queryAllByRole('radio', { hidden: true })).toHaveLength(10);

    await userEvent.type(
      screen.getByLabelText('Search publications'),
      'not a publication',
    );

    jest.runOnlyPendingTimers();

    expect(screen.queryAllByRole('radio', { hidden: true })).toHaveLength(0);
    expect(screen.getByText('No publications found')).toBeInTheDocument();
  });

  test('renders selected publication option even if it does not match search field', async () => {
    jest.useFakeTimers();

    render(
      <PublicationForm
        {...wizardProps}
        onSubmit={noop}
        options={testOptions}
      />,
    );

    expect(screen.getAllByRole('radio', { hidden: true })).toHaveLength(10);
    expect(screen.queryByText('No publications found')).not.toBeInTheDocument();

    userEvent.click(
      screen.getByLabelText('Pupil absence in schools in England'),
    );

    await userEvent.type(
      screen.getByLabelText('Search publications'),
      'not a publication',
    );

    jest.runOnlyPendingTimers();

    expect(screen.getAllByRole('radio')).toHaveLength(1);
    expect(
      screen.getByLabelText('Pupil absence in schools in England'),
    ).toBeInTheDocument();
    expect(screen.queryByText('No publications found')).not.toBeInTheDocument();
  });

  test('renders dropdown for selected publication option as open', () => {
    const { container } = render(
      <PublicationForm
        {...wizardProps}
        onSubmit={noop}
        options={testOptions}
      />,
    );

    expect(container.querySelectorAll('details[open]')).toHaveLength(0);

    userEvent.click(
      screen.getByLabelText('Pupil absence in schools in England'),
    );

    const details = container.querySelectorAll('details[open]');

    expect(details).toHaveLength(2);
    expect(details[0]).toHaveTextContent('Pupils and schools');
    expect(details[1]).toHaveTextContent('Pupil absence');
  });

  test('renders read-only view with initial `publicationId` when step is not active', () => {
    render(
      <PublicationForm
        {...wizardProps}
        initialValues={{
          publicationId: 'publication-2',
        }}
        isActive={false}
        onSubmit={noop}
        options={testOptions}
      />,
    );

    expect(screen.queryAllByRole('radio')).toHaveLength(0);
    expect(screen.getByTestId('Publication')).toHaveTextContent(
      'National achievement rates tables',
    );
  });

  test('renders read-only view with selected publication when step is not active', () => {
    const { rerender } = render(
      <PublicationForm
        {...wizardProps}
        onSubmit={noop}
        options={testOptions}
      />,
    );

    userEvent.click(
      screen.getByLabelText('Pupil absence in schools in England'),
    );

    rerender(
      <PublicationForm
        {...wizardProps}
        isActive={false}
        onSubmit={noop}
        options={testOptions}
      />,
    );

    expect(screen.queryAllByRole('radio')).toHaveLength(0);
    expect(screen.getByTestId('Publication')).toHaveTextContent(
      'Pupil absence in schools in England',
    );
  });

  test('clicking `Next step` calls `onSubmit` with correct values', async () => {
    const handleSubmit = jest.fn();

    render(
      <PublicationForm
        {...wizardProps}
        onSubmit={handleSubmit}
        options={testOptions}
      />,
    );

    expect(screen.queryByTestId('Publication')).not.toBeInTheDocument();

    userEvent.click(screen.getByRole('button', { name: 'Pupils and schools' }));
    userEvent.click(screen.getByRole('button', { name: 'Pupil absence' }));
    userEvent.click(
      screen.getByLabelText('Pupil absence in schools in England'),
    );
    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    const expected: PublicationFormValues & {
      publicationSlug: string;
      publicationTitle: string;
    } = {
      publicationId: 'publication-5',
      publicationSlug: 'pupil-absence-in-schools-in-england',
      publicationTitle: 'Pupil absence in schools in England',
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith(expected);
    });
  });
});
