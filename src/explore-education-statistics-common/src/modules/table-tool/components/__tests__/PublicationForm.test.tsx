import PublicationForm, {
  PublicationFormValues,
} from '@common/modules/table-tool/components/PublicationForm';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { Theme } from '@common/services/tableBuilderService';
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
      slug: 'further-education',
      topics: [
        {
          id: 'topic-1',
          title: 'Further education and skills',
          slug: 'further-education-and-skills',
          publications: [
            {
              id: 'publication-1',
              title: 'Apprenticeships and traineeships',
              slug: 'apprenticeships-and-traineeships',
            },
          ],
        },
        {
          id: 'topic-2',
          title: 'National achievement rates tables',
          slug: 'national-achievement-rates-tables',
          publications: [
            {
              id: 'publication-2',
              title: 'National achievement rates tables',
              slug: 'national-achievement-rates-tables',
            },
          ],
        },
      ],
    },
    {
      id: 'theme-2',
      title: 'Children, early years and social care',
      slug: 'children-and-early-years',
      topics: [
        {
          id: 'topic-3',
          title: 'Early years foundation stage profile',
          slug: 'early-years-foundation-stage-profile',
          publications: [
            {
              id: 'publication-3',
              title: 'Early years foundation stage profile results',
              slug: 'early-years-foundation-stage-profile-results',
            },
          ],
        },
      ],
    },
    {
      id: 'theme-3',
      title: 'Pupils and schools',
      slug: 'pupils-and-schools',
      topics: [
        {
          id: 'topic-4',
          title: 'School applications',
          slug: 'school-applications',
          publications: [
            {
              id: 'publication-4',
              title: 'Secondary and primary schools applications and offers',
              slug: 'secondary-and-primary-schools-applications-and-offers',
            },
          ],
        },
        {
          id: 'topic-5',
          title: 'Pupil absence',
          slug: 'pupil-absence',
          publications: [
            {
              id: 'publication-5',
              title: 'Pupil absence in schools in England',
              slug: 'pupil-absence-in-schools-in-england',
            },
          ],
        },
        {
          id: 'topic-6',
          title: 'Special educational needs (SEN)',
          slug: 'sen',
          publications: [
            {
              id: 'publication-6',
              title: 'Statements of SEN and EHC plans',
              slug: 'statements-of-sen-and-ehc-plans',
            },
          ],
        },
        {
          id: 'topic-7',
          title: 'Exclusions',
          slug: 'exclusions',
          publications: [
            {
              id: 'publication-7',
              title: 'Permanent and fixed-period exclusions in England',
              slug: 'permanent-and-fixed-period-exclusions-in-england',
            },
          ],
        },
      ],
    },
    {
      id: 'theme-4',
      title: 'School and college outcomes and performance',
      slug: 'outcomes-and-performance',
      topics: [
        {
          id: 'topic-8',
          title: 'Key stage 2',
          slug: 'key-stage-two',
          publications: [
            {
              id: 'publication-8',
              title: 'National curriculum assessments at key stage 2',
              slug: 'national-curriculum-assessments-key-stage2',
            },
          ],
        },
        {
          id: 'topic-9',
          title: 'GCSEs (key stage 4)',
          slug: 'key-stage-four',
          publications: [
            {
              id: 'publication-9',
              title:
                'GCSE and equivalent results, including pupil characteristics',
              slug: 'gcse-results-including-pupil-characteristics',
            },
          ],
        },
        {
          id: 'topic-10',
          title: '16 to 19 attainment',
          slug: 'sixteen-to-nineteen-attainment',
          publications: [
            {
              id: 'publication-10',
              title: 'Level 2 and 3 attainment by young people aged 19',
              slug: 'Level 2 and 3 attainment by young people aged 19',
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

    const expected: PublicationFormValues = {
      publicationId: 'publication-5',
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith(expected);
    });
  });
});
