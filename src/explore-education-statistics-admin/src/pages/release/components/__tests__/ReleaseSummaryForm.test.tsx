import _metaService, {
  TimePeriodCoverageGroup,
} from '@admin/services/metaService';
import _organisationService from '@admin/services/organisationService';
import createAxiosErrorMock from '@common-test/createAxiosErrorMock';
import { Organisation } from '@common/services/types/organisation';
import { ValidationProblemDetails } from '@common/services/types/problemDetails';
import { releaseTypes } from '@common/services/types/releaseType';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import ReleaseSummaryForm from '../ReleaseSummaryForm';

const metaService = _metaService as jest.Mocked<typeof _metaService>;

jest.mock('@admin/services/metaService');

const organisationService = _organisationService as jest.Mocked<
  typeof _organisationService
>;

jest.mock('@admin/services/organisationService');

describe('ReleaseSummaryForm', () => {
  const testTimeIdentifiers: TimePeriodCoverageGroup[] = [
    {
      category: { label: 'Academic year' },
      timeIdentifiers: [
        { identifier: { value: 'AY', label: 'Academic year' } },
        { identifier: { value: 'AYQ1', label: 'Academic year Q1' } },
        { identifier: { value: 'AYQ2', label: 'Academic year Q2' } },
        { identifier: { value: 'AYQ3', label: 'Academic year Q3' } },
        { identifier: { value: 'AYQ4', label: 'Academic year Q4' } },
      ],
    },
  ];
  const testOrganisations: Organisation[] = [
    {
      id: '5e089801-cf1a-b375-acd3-88e9d8aece66',
      title: 'Department for Education',
      url: 'https://www.gov.uk/government/organisations/department-for-education',
      useGISLogo: true,
      logoFileName: 'logo.png',
    },
    {
      id: '8d26bfaa-44b8-461e-9260-2b0eed9631e0',
      title: 'Ofsted',
      url: 'https://example.com',
      useGISLogo: false,
      logoFileName: 'logo.png',
    },
  ];

  beforeEach(() => {
    organisationService.listOrganisations.mockResolvedValue(testOrganisations);
  });

  test('renders correctly with default initial values and without the template field when `templateRelease` is not provided', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: '',
          timePeriodCoverageStartYear: '',
          releaseType: undefined,
          releaseLabel: '',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        onSubmit={noop}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const selectYearType = screen.getByLabelText('Type');
    expect(selectYearType).toBeInTheDocument();

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    expect(inputYear).toBeInTheDocument();

    const inputReleaseLabel = screen.getByLabelText('Release label (optional)');
    expect(inputReleaseLabel).toBeInTheDocument();

    const releaseTypeRadios = within(
      screen.getByRole('group', { name: 'Release type' }),
    ).getAllByRole('radio');
    expect(releaseTypeRadios).toHaveLength(5);
    expect(releaseTypeRadios[0]).toEqual(
      screen.getByLabelText(releaseTypes.AccreditedOfficialStatistics),
    );
    expect(releaseTypeRadios[1]).toEqual(
      screen.getByLabelText(releaseTypes.AdHocStatistics),
    );
    expect(releaseTypeRadios[2]).toEqual(
      screen.getByLabelText(releaseTypes.ManagementInformation),
    );
    expect(releaseTypeRadios[3]).toEqual(
      screen.getByLabelText(releaseTypes.OfficialStatistics),
    );
    expect(releaseTypeRadios[4]).toEqual(
      screen.getByLabelText(releaseTypes.OfficialStatisticsInDevelopment),
    );

    expect(
      screen.queryByRole('group', { name: 'Select template' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('group', { name: 'Publishing Organisations' }),
    ).toBeInTheDocument();

    const publishingOrganisationCheckboxes = within(
      screen.getByRole('group', { name: 'Publishing Organisations' }),
    ).getAllByRole('checkbox');

    expect(publishingOrganisationCheckboxes[0]).toEqual(
      screen.getByLabelText('Department for Education'),
    );
    expect(publishingOrganisationCheckboxes[0]).toBeChecked();
    expect(publishingOrganisationCheckboxes[1]).not.toBeChecked();

    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });
    expect(buttonCreate).toBeInTheDocument();
    const buttonCancel = screen.getByRole('button', {
      name: 'Cancel',
    });
    expect(buttonCancel).toBeInTheDocument();
  });

  test('renders correctly with the template field when `templateRelease` is provided', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: '',
          timePeriodCoverageStartYear: '',
          releaseType: undefined,
          releaseLabel: '',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        templateRelease={{
          id: 'template-id',
          title: 'Template title',
        }}
        onSubmit={noop}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const selectYearType = screen.getByLabelText('Type');
    expect(selectYearType).toBeInTheDocument();

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    expect(inputYear).toBeInTheDocument();

    const inputReleaseLabel = screen.getByLabelText('Release label (optional)');
    expect(inputReleaseLabel).toBeInTheDocument();

    const releaseTypeRadios = within(
      screen.getByRole('group', { name: 'Release type' }),
    ).getAllByRole('radio');
    expect(releaseTypeRadios).toHaveLength(5);
    expect(releaseTypeRadios[0]).toEqual(
      screen.getByLabelText(releaseTypes.AccreditedOfficialStatistics),
    );
    expect(releaseTypeRadios[1]).toEqual(
      screen.getByLabelText(releaseTypes.AdHocStatistics),
    );
    expect(releaseTypeRadios[2]).toEqual(
      screen.getByLabelText(releaseTypes.ManagementInformation),
    );
    expect(releaseTypeRadios[3]).toEqual(
      screen.getByLabelText(releaseTypes.OfficialStatistics),
    );
    expect(releaseTypeRadios[4]).toEqual(
      screen.getByLabelText(releaseTypes.OfficialStatisticsInDevelopment),
    );

    expect(
      within(
        screen.getByRole('group', { name: 'Select template' }),
      ).getAllByRole('radio'),
    ).toHaveLength(2);
    expect(
      screen.getByLabelText('Copy existing template (Template title)'),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Create new template')).toBeInTheDocument();

    // This is the pre-existing, non-rollover template picker - unlike the rollover one, it must
    // stay AFTER "Release type" (its original, unchanged position), not move to the top of the
    // form - that reordering is a rollover-only change (`rolloverSource`), not something this
    // (unrelated, `templateRelease`-only) path should ever be affected by.
    const legends = Array.from(document.querySelectorAll('legend')).map(
      legend => legend.textContent,
    );
    expect(legends.indexOf('Release type')).toBeLessThan(
      legends.indexOf('Select template'),
    );

    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });
    expect(buttonCreate).toBeInTheDocument();
    const buttonCancel = screen.getByRole('button', {
      name: 'Cancel',
    });
    expect(buttonCancel).toBeInTheDocument();
  });

  test('renders correctly with the rollover field when `rolloverSource` is provided, and shows it before the time period section', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: '',
          timePeriodCoverageStartYear: '',
          creationMode: 'WithData',
          releaseType: undefined,
          releaseLabel: '',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        rolloverSource={{
          id: 'rollover-source-id',
          title: 'Academic year 2024/25',
          timePeriodCoverage: { value: 'AY', label: 'Academic year' },
          yearTitle: '2024/25',
        }}
        onSubmit={noop}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const templateRadios = within(
      screen.getByRole('group', { name: 'Select template' }),
    ).getAllByRole('radio');
    expect(templateRadios).toHaveLength(3);

    const withDataOption = screen.getByLabelText(
      'Copy existing release - release rollover (Academic year 2024/25) including data and data blocks',
    );
    const structureOnlyOption = screen.getByLabelText(
      'Copy existing release - (Academic year 2024/25) without data and data blocks',
    );
    const blankOption = screen.getByLabelText('Create new blank release');

    // The rollover option must render FIRST - this isn't alphabetical (it would otherwise sort
    // after the "without data" option, since "(" sorts before "r"), so relies on `order={[]}`
    // disabling FormRadioGroup's default alphabetical-by-label sort.
    expect(templateRadios).toEqual([
      withDataOption,
      structureOnlyOption,
      blankOption,
    ]);
    expect(withDataOption).toBeChecked();

    // The "Select template" group should appear before "Select time period coverage" in the form.
    const legends = Array.from(document.querySelectorAll('legend')).map(
      legend => legend.textContent,
    );
    expect(legends.indexOf('Select template')).toBeLessThan(
      legends.indexOf('Select time period coverage'),
    );

    // The "copied from" hint shows under the year field while a copy option is selected, and
    // hides once "blank" is chosen.
    const yearField = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    expect(yearField).toHaveAccessibleDescription(
      'This release will be copied from academic year 2024/25',
    );

    await userEvent.click(blankOption);

    expect(yearField).not.toHaveAccessibleDescription(
      'This release will be copied from academic year 2024/25',
    );
  });

  test('validation error when no year or release type selected', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: '',
          timePeriodCoverageStartYear: '',
          releaseType: undefined,
          releaseLabel: '',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });

    await userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(
        screen.getByText('Enter a year', { selector: 'a' }),
      ).toBeInTheDocument();
      expect(
        screen.getByText('Choose a release type', { selector: 'a' }),
      ).toBeInTheDocument();
    });
    expect(onSubmit).not.toHaveBeenCalled();
  });

  test('validation error when no publishing organisations selected', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: 'AY',
          timePeriodCoverageStartYear: '2022',
          releaseType: 'OfficialStatistics',
          releaseLabel: '',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    // DfE is pre-selected by default, so deselect it
    const dfeCheckbox = screen.getByLabelText('Department for Education');
    expect(dfeCheckbox).toBeChecked();
    await userEvent.click(dfeCheckbox);
    expect(dfeCheckbox).not.toBeChecked();

    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });

    await userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(
        screen.getByText('Select at least one publishing organisation', {
          selector: 'a',
        }),
      ).toBeInTheDocument();
    });
    expect(onSubmit).not.toHaveBeenCalled();
  });

  test('validation error when year "2"', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: '',
          timePeriodCoverageStartYear: '',
          releaseType: undefined,
          releaseLabel: '',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });

    await userEvent.type(inputYear, '2');
    await userEvent.click(screen.getByLabelText(releaseTypes.AdHocStatistics));
    await userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(
        screen.getByText('Year must be exactly 4 characters', {
          selector: 'a',
        }),
      ).toBeInTheDocument();
      expect(
        screen.queryByText('Choose a release type', {
          selector: 'a',
        }),
      ).not.toBeInTheDocument();
    });
    expect(onSubmit).not.toHaveBeenCalled();
  });

  test('validation error when year "202021"', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: '',
          timePeriodCoverageStartYear: '',
          releaseType: undefined,
          releaseLabel: '',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });

    await userEvent.type(inputYear, '202021');
    await userEvent.click(screen.getByLabelText(releaseTypes.AdHocStatistics));
    await userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(
        screen.getByText('Year must be exactly 4 characters', {
          selector: 'a',
        }),
      ).toBeInTheDocument();
      expect(
        screen.queryByText('Choose a release type', {
          selector: 'a',
        }),
      ).not.toBeInTheDocument();
    });
    expect(onSubmit).not.toHaveBeenCalled();
  });

  test('validation with valid year but no release type selected', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: '',
          timePeriodCoverageStartYear: '',
          releaseType: undefined,
          releaseLabel: '',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
      expect(
        screen.queryByText('Choose a release type', {
          selector: 'a',
        }),
      ).not.toBeInTheDocument();
    });

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });

    await userEvent.type(inputYear, '1966');
    await userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(
        screen.queryByText('Choose a release type', {
          selector: 'a',
        }),
      ).toBeInTheDocument();
    });
    expect(onSubmit).not.toHaveBeenCalled();
  });

  test('renders with provided initial values', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: 'AYQ4',
          timePeriodCoverageStartYear: '1966',
          releaseType: 'AccreditedOfficialStatistics',
          releaseLabel: 'initial',
          publishingOrganisations: ['8d26bfaa-44b8-461e-9260-2b0eed9631e0'],
        }}
        releaseVersion={0}
        onSubmit={noop}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const selectYearType = screen.getByLabelText('Type');
    expect(selectYearType).toHaveValue('AYQ4');

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    expect(inputYear).toHaveNumericValue(1966);

    const inputReleaseLabel = screen.getByLabelText('Release label (optional)');
    expect(inputReleaseLabel).toHaveValue('initial');

    const releaseTypeRadios = within(
      screen.getByRole('group', { name: 'Release type' }),
    ).getAllByRole('radio');
    expect(releaseTypeRadios).toHaveLength(5);
    expect(releaseTypeRadios[0]).toBeChecked();
    expect(releaseTypeRadios[1]).not.toBeChecked();
    expect(releaseTypeRadios[2]).not.toBeChecked();
    expect(releaseTypeRadios[3]).not.toBeChecked();
    expect(releaseTypeRadios[4]).not.toBeChecked();

    const publishingOrganisationCheckboxes = within(
      screen.getByRole('group', { name: 'Publishing Organisations' }),
    ).getAllByRole('checkbox');

    expect(publishingOrganisationCheckboxes[0]).toEqual(
      screen.getByLabelText('Department for Education'),
    );
    expect(publishingOrganisationCheckboxes[0]).not.toBeChecked();
    expect(publishingOrganisationCheckboxes[1]).toBeChecked();
  });

  test('renders with provided initial values with multiple publishing organisations', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: 'AYQ4',
          timePeriodCoverageStartYear: '1966',
          releaseType: 'AccreditedOfficialStatistics',
          releaseLabel: 'initial',
          publishingOrganisations: [
            '5e089801-cf1a-b375-acd3-88e9d8aece66',
            '8d26bfaa-44b8-461e-9260-2b0eed9631e0',
          ],
        }}
        releaseVersion={0}
        onSubmit={noop}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const publishingOrganisationCheckboxes = within(
      screen.getByRole('group', { name: 'Publishing Organisations' }),
    ).getAllByRole('checkbox');
    expect(publishingOrganisationCheckboxes[0]).toBeChecked();
    expect(publishingOrganisationCheckboxes[1]).toBeChecked();
  });

  test('submits form with valid values', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: '',
          timePeriodCoverageStartYear: '',
          releaseType: undefined,
          releaseLabel: '',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const selectYearType = screen.getByLabelText('Type', {
      selector: 'select',
    });
    await userEvent.selectOptions(selectYearType, 'AY');

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    await userEvent.type(inputYear, '1966');

    const inputReleaseLabel = screen.getByLabelText('Release label (optional)');
    await userEvent.type(inputReleaseLabel, 'initial');

    const radioOptionReleaseTypeNationalStats = screen.getByLabelText(
      releaseTypes.AccreditedOfficialStatistics,
    );
    await userEvent.click(radioOptionReleaseTypeNationalStats);

    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });
    expect(onSubmit).not.toHaveBeenCalled();
    await userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledTimes(1);
    });
  });

  test('validation error when release label over 20 characters', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: '',
          timePeriodCoverageStartYear: '',
          releaseType: undefined,
          releaseLabel: '',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const selectYearType = screen.getByLabelText('Type', {
      selector: 'select',
    });
    await userEvent.selectOptions(selectYearType, 'AY');

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    await userEvent.type(inputYear, '2020');

    const inputReleaseLabel = screen.getByLabelText('Release label (optional)');
    await userEvent.type(
      inputReleaseLabel,
      'aaaaaaaaaaaaaaaaaaaaa', // 21 characters
    );

    await userEvent.click(screen.getByLabelText(releaseTypes.AdHocStatistics));

    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });
    await userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(
        screen.getByText('Release label must be no longer than 20 characters', {
          selector: 'a',
        }),
      ).toBeInTheDocument();
    });
    expect(onSubmit).not.toHaveBeenCalled();
  });

  test('does not show the Experimental statistics release type when empty initial values', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: '',
          timePeriodCoverageStartYear: '',
          releaseType: undefined,
          releaseLabel: '',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        onSubmit={noop}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    expect(
      within(screen.getByRole('group', { name: 'Release type' })).getAllByRole(
        'radio',
      ),
    ).toHaveLength(5);

    expect(
      screen.queryByLabelText(releaseTypes.ExperimentalStatistics),
    ).not.toBeInTheDocument();
  });

  test('does not show the Experimental statistics release type when another type is in initial values', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: 'AYQ4',
          timePeriodCoverageStartYear: '1966',
          releaseType: 'AccreditedOfficialStatistics',
          releaseLabel: 'initial',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        onSubmit={noop}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    expect(
      within(screen.getByRole('group', { name: 'Release type' })).getAllByRole(
        'radio',
      ),
    ).toHaveLength(5);

    expect(
      screen.queryByLabelText(releaseTypes.ExperimentalStatistics),
    ).not.toBeInTheDocument();
  });

  test('shows the Experimental statistics release type when it is already in initial values', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: 'AYQ4',
          timePeriodCoverageStartYear: '1966',
          releaseType: 'ExperimentalStatistics',
          releaseLabel: 'initial',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        onSubmit={noop}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    expect(
      within(screen.getByRole('group', { name: 'Release type' })).getAllByRole(
        'radio',
      ),
    ).toHaveLength(6);

    expect(
      screen.getByLabelText(releaseTypes.ExperimentalStatistics),
    ).toBeInTheDocument();
  });

  test.each([1, 2, 3])(
    'disables inputs for time period coverage and release label when the release version is > 0',
    async releaseVersion => {
      metaService.getTimePeriodCoverageGroups.mockResolvedValue(
        testTimeIdentifiers,
      );

      render(
        <ReleaseSummaryForm
          submitText="Create new release"
          initialValues={{
            timePeriodCoverageCode: 'AYQ4',
            timePeriodCoverageStartYear: '1966',
            releaseType: 'AccreditedOfficialStatistics',
            releaseLabel: 'initial',
            publishingOrganisations: [],
          }}
          releaseVersion={releaseVersion}
          onSubmit={noop}
          onCancel={noop}
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByText('Select time period coverage'),
        ).toBeInTheDocument();
      });

      const selectYearType = screen.getByLabelText('Type');
      expect(selectYearType).toBeDisabled();

      const inputYear = screen.getByLabelText(
        testTimeIdentifiers[0].category.label,
      );
      expect(inputYear).toBeDisabled();

      const inputReleaseLabel = screen.getByLabelText(
        'Release label (optional)',
      );
      expect(inputReleaseLabel).toBeDisabled();
    },
  );

  test('Displays a validation error when the server responds with a slug not unique error', async () => {
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

    const error = createAxiosErrorMock<ValidationProblemDetails>({
      data: {
        errors: [{ code: 'SlugNotUnique', message: '' }],
        title: '',
        type: '',
        status: 400,
      },
    });
    onSubmit.mockRejectedValue(error);

    render(
      <ReleaseSummaryForm
        submitText="Create new release"
        initialValues={{
          timePeriodCoverageCode: 'AYQ4',
          timePeriodCoverageStartYear: '1966',
          releaseType: 'ExperimentalStatistics',
          releaseLabel: 'initial',
          publishingOrganisations: [],
        }}
        releaseVersion={0}
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });
    await userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(
        screen.getByText(
          'Choose a unique combination of type, start year and label',
          { selector: 'a' },
        ),
      ).toBeInTheDocument();
    });

    expect(onSubmit).toHaveBeenCalledTimes(1);
  });
});
