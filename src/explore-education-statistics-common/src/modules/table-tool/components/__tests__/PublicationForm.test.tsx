import PublicationForm from '@common/modules/table-tool/components/PublicationForm';
import React from 'react';
import { fireEvent, render } from 'react-testing-library';

describe('PublicationForm', () => {
  const testOptions = [
    {
      id: '92c5df93-c4da-4629-ab25-51bd2920cdca',
      title: 'Further education',
      slug: 'further-education',
      topics: [
        {
          id: '88d08425-fcfd-4c87-89da-70b2062a7367',
          title: 'Further education and skills',
          slug: 'further-education-and-skills',
          publications: [
            {
              id: 'cf0ec981-3583-42a5-b21b-3f2f32008f1b',
              title: 'Apprenticeships and traineeships',
              slug: 'apprenticeships-and-traineeships',
            },
          ],
        },
        {
          id: 'dc7b7a89-e968-4a7e-af5f-bd7d19c346a5',
          title: 'National achievement rates tables',
          slug: 'national-achievement-rates-tables',
          publications: [
            {
              id: '7a57d4c0-5233-4d46-8e27-748fbc365715',
              title: 'National achievement rates tables',
              slug: 'national-achievement-rates-tables',
            },
          ],
        },
      ],
    },
    {
      id: 'cc8e02fd-5599-41aa-940d-26bca68eab53',
      title: 'Children, early years and social care',
      slug: 'children-and-early-years',
      topics: [
        {
          id: '17b2e32c-ed2f-4896-852b-513cdf466769',
          title: 'Early years foundation stage profile',
          slug: 'early-years-foundation-stage-profile',
          publications: [
            {
              id: 'fcda2962-82a6-4052-afa2-ea398c53c85f',
              title: 'Early years foundation stage profile results',
              slug: 'early-years-foundation-stage-profile-results',
            },
          ],
        },
      ],
    },
    {
      id: 'ee1855ca-d1e1-4f04-a795-cbd61d326a1f',
      title: 'Pupils and schools',
      slug: 'pupils-and-schools',
      topics: [
        {
          id: '1a9636e4-29d5-4c90-8c07-f41db8dd019c',
          title: 'School applications',
          slug: 'school-applications',
          publications: [
            {
              id: '66c8e9db-8bf2-4b0b-b094-cfab25c20b05',
              title: 'Secondary and primary schools applications and offers',
              slug: 'secondary-and-primary-schools-applications-and-offers',
            },
          ],
        },
        {
          id: '67c249de-1cca-446e-8ccb-dcdac542f460',
          title: 'Pupil absence',
          slug: 'pupil-absence',
          publications: [
            {
              id: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
              title: 'Pupil absence in schools in England',
              slug: 'pupil-absence-in-schools-in-england',
            },
          ],
        },
        {
          id: '85349b0a-19c7-4089-a56b-ad8dbe85449a',
          title: 'Special educational needs (SEN)',
          slug: 'sen',
          publications: [
            {
              id: '88312cc0-fe1d-4ab5-81df-33fd708185cb',
              title: 'Statements of SEN and EHC plans',
              slug: 'statements-of-sen-and-ehc-plans',
            },
          ],
        },
        {
          id: '77941b7d-bbd6-4069-9107-565af89e2dec',
          title: 'Exclusions',
          slug: 'exclusions',
          publications: [
            {
              id: 'bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9',
              title: 'Permanent and fixed-period exclusions in England',
              slug: 'permanent-and-fixed-period-exclusions-in-england',
            },
          ],
        },
      ],
    },
    {
      id: '74648781-85a9-4233-8be3-fe6f137165f4',
      title: 'School and college outcomes and performance',
      slug: 'outcomes-and-performance',
      topics: [
        {
          id: 'eac38700-b968-4029-b8ac-0eb8e1356480',
          title: 'Key stage 2',
          slug: 'key-stage-two',
          publications: [
            {
              id: '10370062-93b0-4dde-9097-5a56bf5b3064',
              title: 'National curriculum assessments at key stage 2',
              slug: 'national-curriculum-assessments-key-stage2',
            },
          ],
        },
        {
          id: '1e763f55-bf09-4497-b838-7c5b054ba87b',
          title: 'GCSEs (key stage 4)',
          slug: 'key-stage-four',
          publications: [
            {
              id: 'bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f',
              title:
                'GCSE and equivalent results, including pupil characteristics',
              slug: 'gcse-results-including-pupil-characteristics',
            },
          ],
        },
        {
          id: '85b5454b-3761-43b1-8e84-bd056a8efcd3',
          title: '16 to 19 attainment',
          slug: 'sixteen-to-nineteen-attainment',
          publications: [
            {
              id: '2e95f880-629c-417b-981f-0901e97776ff',
              title: 'Level 2 and 3 attainment by young people aged 19',
              slug: 'Level 2 and 3 attainment by young people aged 19',
            },
          ],
        },
      ],
    },
  ];

  test('renders publication options filtered by title when using search field', () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <PublicationForm
        onSubmit={() => undefined}
        options={testOptions}
        stepNumber={1}
        currentStep={1}
        setCurrentStep={() => undefined}
        isActive
        goToNextStep={() => undefined}
        goToPreviousStep={() => undefined}
      />,
    );

    expect(container.querySelectorAll('input[type="radio"]')).toHaveLength(10);

    fireEvent.change(getByLabelText('Search publications'), {
      target: {
        value: 'Early years',
      },
    });

    jest.runOnlyPendingTimers();

    expect(container.querySelectorAll('input[type="radio"]')).toHaveLength(1);
    expect(
      getByLabelText('Early years foundation stage profile results'),
    ).toHaveAttribute('type', 'radio');
  });

  test('renders publication options filtered by case-insensitive title', () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <PublicationForm
        onSubmit={() => undefined}
        options={testOptions}
        stepNumber={1}
        currentStep={1}
        setCurrentStep={() => undefined}
        isActive
        goToNextStep={() => undefined}
        goToPreviousStep={() => undefined}
      />,
    );

    expect(container.querySelectorAll('input[type="radio"]')).toHaveLength(10);

    fireEvent.change(getByLabelText('Search publications'), {
      target: {
        value: 'early years',
      },
    });

    jest.runOnlyPendingTimers();

    expect(container.querySelectorAll('input[type="radio"]')).toHaveLength(1);
    expect(
      getByLabelText('Early years foundation stage profile results'),
    ).toHaveAttribute('type', 'radio');
  });

  test('does not throw error if regex sensitive search term is used', () => {
    jest.useFakeTimers();

    const { getByLabelText } = render(
      <PublicationForm
        onSubmit={() => undefined}
        options={testOptions}
        stepNumber={1}
        currentStep={1}
        setCurrentStep={() => undefined}
        isActive
        goToNextStep={() => undefined}
        goToPreviousStep={() => undefined}
      />,
    );

    fireEvent.change(getByLabelText('Search publications'), {
      target: {
        value: '[',
      },
    });

    expect(() => {
      jest.runOnlyPendingTimers();
    }).not.toThrow();
  });

  test('renders empty message when there are no publication options', () => {
    const { container, queryByText } = render(
      <PublicationForm
        onSubmit={() => undefined}
        options={[]}
        stepNumber={1}
        currentStep={1}
        setCurrentStep={() => undefined}
        isActive
        goToNextStep={() => undefined}
        goToPreviousStep={() => undefined}
      />,
    );

    expect(container.querySelectorAll('input[type="radio"]')).toHaveLength(0);
    expect(queryByText('No publications found')).not.toBeNull();
  });

  test('renders empty message when there are no filtered publication options', async () => {
    jest.useFakeTimers();

    const { container, queryByText, getByLabelText } = render(
      <PublicationForm
        onSubmit={() => undefined}
        options={testOptions}
        stepNumber={1}
        currentStep={1}
        setCurrentStep={() => undefined}
        isActive
        goToNextStep={() => undefined}
        goToPreviousStep={() => undefined}
      />,
    );

    expect(queryByText('No publications found')).toBeNull();
    expect(container.querySelectorAll('input[type="radio"]')).toHaveLength(10);

    fireEvent.change(getByLabelText('Search publications'), {
      target: {
        value: 'not a publication',
      },
    });

    jest.runOnlyPendingTimers();

    expect(container.querySelectorAll('input[type="radio"]')).toHaveLength(0);
    expect(queryByText('No publications found')).not.toBeNull();
  });

  test('renders selected publication option even if it does not match search field', () => {
    jest.useFakeTimers();

    const { container, queryByText, getByLabelText, queryByLabelText } = render(
      <PublicationForm
        onSubmit={() => undefined}
        options={testOptions}
        stepNumber={1}
        currentStep={1}
        setCurrentStep={() => undefined}
        isActive
        goToNextStep={() => undefined}
        goToPreviousStep={() => undefined}
      />,
    );

    expect(container.querySelectorAll('input[type="radio"]')).toHaveLength(10);
    expect(queryByText('No publications found')).toBeNull();

    fireEvent.click(getByLabelText('Pupil absence in schools in England'));

    fireEvent.change(getByLabelText('Search publications'), {
      target: {
        value: 'not a publication',
      },
    });

    jest.runOnlyPendingTimers();

    expect(container.querySelectorAll('input[type="radio"]')).toHaveLength(1);
    expect(
      queryByLabelText('Pupil absence in schools in England'),
    ).not.toBeNull();
    expect(queryByText('No publications found')).toBeNull();
  });

  test('renders dropdown for selected publication option as open', () => {
    const { container, getByLabelText, getByTestId } = render(
      <PublicationForm
        onSubmit={() => undefined}
        options={testOptions}
        stepNumber={1}
        currentStep={1}
        setCurrentStep={() => undefined}
        isActive
        goToNextStep={() => undefined}
        goToPreviousStep={() => undefined}
      />,
    );

    expect(container.querySelectorAll('details[open]')).toHaveLength(0);

    fireEvent.click(getByLabelText('Pupil absence in schools in England'));

    expect(container.querySelectorAll('details[open]')).toHaveLength(2);

    const details1 = getByTestId(
      'publicationForm-theme-ee1855ca-d1e1-4f04-a795-cbd61d326a1f',
    );
    expect(details1).toHaveAttribute('open');
    expect(details1).toHaveTextContent('Pupils and schools');

    const details2 = getByTestId(
      'publicationForm-topic-67c249de-1cca-446e-8ccb-dcdac542f460',
    );
    expect(details2).toHaveAttribute('open');
    expect(details2).toHaveTextContent('Pupil absence');
  });
});
