import dataReplacementService, {
  DataReplacementPlan,
} from '@admin/services/dataReplacementService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import DataFileReplacementDifferences from '../DataFileReplacementDifferences';

jest.mock('@admin/services/dataReplacementService');

const testReplacementPlan: DataReplacementPlan = {
  originalSubjectId: 'subId',
  replacementSubjectId: 'repId',
  valid: false,
  footnotes: [],
  mapping: {
    indicators: {
      mappings: {
        enrolments_again: {
          source: {
            id: 'enrolments_again',
            name: 'Enrolments_Again',
            label: 'Enrolments_Again',
          },
          type: 'Unset',
        },
        enrolments: {
          source: {
            id: 'enrolments',
            name: 'Enrolments',
            label: 'Enrolments',
          },
          type: 'Unset',
        },
        sess_possible: {
          source: {
            id: 'sess_possible',
            name: 'Number of possible sessions',
            label: 'Number of possible sessions',
          },
          type: 'AutoSet',
          candidateKey: 'sess_possible',
        },
        sess_authorised: {
          source: {
            id: 'sess_authorised',
            name: 'Number of authorised sessions',
            label: 'Number of authorised sessions',
          },
          type: 'AutoSet',
          candidateKey: 'sess_authorised',
        },
        sess_unauthorised: {
          source: {
            id: 'sess_unauthorised',
            name: 'Number of unauthorised sessions',
            label: 'Number of unauthorised sessions',
          },
          type: 'AutoSet',
          candidateKey: 'sess_unauthorised',
        },
        sess_unauthorised_percent: {
          source: {
            id: 'sess_unauthorised_percent',
            name: 'Percentage of unauthorised sessions',
            label: 'Percentage of unauthorised sessions',
          },
          type: 'AutoSet',
          candidateKey: 'sess_unauthorised_percent',
        },
      },
      candidates: {
        // indicators from the replacement data
        sess_possible: {
          id: 'sess_possible',
          name: 'Number of possible sessions',
          label: 'Number of possible sessions',
        },
        sess_authorised: {
          id: 'sess_authorised',
          name: 'Number of authorised sessions',
          label: 'Number of authorised sessions',
        },
        sess_unauthorised: {
          id: 'sess_unauthorised',
          name: 'Number of unauthorised sessions',
          label: 'Number of unauthorised sessions',
        },
        number_of_enrolments_again: {
          id: 'number_of_enrolments_again',
          name: 'Number of enrolments again',
          label: 'Number of enrolments again',
        },
        number_of_enrolments: {
          id: 'number_of_enrolments',
          name: 'Number of enrolments',
          label: 'Number of enrolments',
        },
        sess_unauthorised_percent: {
          id: 'sess_unauthorised_percent',
          label: 'Percentage of unauthorised sessions',
          name: 'Percentage of unauthorised sessions',
        },
      },
    },
    locations: {
      mappings: {},
      candidates: {},
    },
  },
  dataBlocks: [
    {
      id: '5ad1c42b-763c-4dfb-b8dd-ad88ebfb9ad4',
      name: 'Dates',
      filters: {
        'fa36f29a-8184-4588-ac7b-08deb19a25e6': {
          id: 'fa36f29a-8184-4588-ac7b-08deb19a25e6',
          label: 'Date',
          groups: {
            '7f7dd166-b7bc-4d09-83b5-19756451278b': {
              id: '7f7dd166-b7bc-4d09-83b5-19756451278b',
              label: 'Default',
              filters: [
                {
                  id: '6424fca3-f3b7-4d99-b5c4-9711556aaec3',
                  label: 'Not specified',
                  valid: false,
                },
              ],
              valid: false,
            },
          },
          valid: false,
        },
      },
      indicatorGroups: {
        'Enrolment Group': {
          id: '1',
          label: 'Enrolment Group',
          indicators: [
            {
              name: 'enrolments_again',
              id: 'enrolments_again',
              label: 'Enrolments_Again',
              valid: false,
            },
            {
              name: 'enrolments',
              id: 'enrolments',
              label: 'Enrolments',
              valid: false,
            },
          ],
          valid: false,
        },
      },
      locations: {
        Country: {
          label: 'Country',
          locationAttributes: [
            {
              id: 'country-england',
              code: 'E92000001',
              label: 'England',
              target: 'f628134d-5235-4615-2438-08dc1c5c7fdf',
              valid: true,
            },
          ],
          valid: true,
        },
      },
      timePeriods: {
        start: {
          code: 'W13',
          year: 2020,
          label: '2020 Week 13',
          valid: true,
        },
        end: {
          code: 'W24',
          year: 2021,
          label: '2021 Week 24',
          valid: true,
        },
        valid: true,
      },
      valid: false,
    },
  ],
};

describe('DataFileReplacementDifferences', () => {
  const sharedRender = () =>
    render(
      <DataFileReplacementDifferences
        replacementFileId="replacementFileId"
        reloadPlan={jest.fn}
        fileId="fileId"
        releaseVersionId="releaseVersionId"
        plan={testReplacementPlan}
      />,
    );

  const tableId = 'replacements-differences-indicators-table';

  test('renders differences/mappings table with options and available actions', async () => {
    const { user } = sharedRender();
    // check table is there
    await waitFor(() => {
      expect(screen.getByTestId(tableId)).toBeInTheDocument();
    });

    const tbody = screen.getByTestId(`${tableId}-body`);
    expect(tbody).toBeInTheDocument();

    // check table items there
    expect(within(tbody).getAllByRole('row')).toHaveLength(2);

    // check actions
    expect(within(tbody).getAllByRole('button')).toHaveLength(4);

    // open action, check options listed
    await user.click(
      within(tbody).getByRole('button', { name: 'Map item Enrolments' }),
    );

    await waitFor(() => {
      expect(screen.getAllByRole('dialog')).toHaveLength(1);
    });

    const modal = screen.getByRole('dialog');
    const radioOptions = within(modal).getAllByRole('radio');
    expect(radioOptions).toHaveLength(3);

    expect(
      within(modal).getByLabelText('No mapping available'),
    ).toBeInTheDocument();
    expect(
      within(modal).getByLabelText('Number of enrolments'),
    ).toBeInTheDocument();
    expect(
      within(modal).getByLabelText('Number of enrolments again'),
    ).toBeInTheDocument();
  });

  test('mapping new indicator to a pre-existing indicator option', async () => {
    const { user } = sharedRender();

    await waitFor(() => {
      expect(screen.getByTestId(tableId)).toBeInTheDocument();
    });

    const tbody = screen.getByTestId(`${tableId}-body`);
    expect(tbody).toBeInTheDocument();

    const rows = within(tbody).getAllByRole('row');
    const enrolmentsAgainRow = rows[0];

    expect(enrolmentsAgainRow.childNodes[0]).toHaveTextContent(
      'Enrolment Group',
    );
    expect(enrolmentsAgainRow.childNodes[1]).toHaveTextContent(
      'Enrolments_Again',
    );
    expect(enrolmentsAgainRow.childNodes[2]).toHaveTextContent('not present');

    await user.click(
      within(enrolmentsAgainRow).getByRole('button', {
        name: 'Map item Enrolments_Again',
      }),
    );

    expect(await screen.findByRole('dialog')).toBeInTheDocument();

    const modal = screen.getByRole('dialog');
    const radioOptions = within(modal).getAllByRole('radio');
    expect(radioOptions).toHaveLength(3);

    const radio = screen.getByLabelText('Number of enrolments again');

    await user.click(radio);

    expect(radio).toBeChecked();

    await user.click(
      // submit
      within(modal).getByRole('button', { name: 'Save' }),
    );

    expect(modal).not.toBeInTheDocument();

    expect(enrolmentsAgainRow.childNodes[1]).toHaveTextContent(
      'Enrolments_Again',
    );

    waitFor(async () => {
      expect(
        within(enrolmentsAgainRow).getByText('Number of enrolments again'),
      ).toBeInTheDocument();
      expect(
        within(enrolmentsAgainRow).queryByText('No mapping'),
      ).not.toBeInTheDocument();
    });
  });

  test('mapping new indicator to "No mapping" action', async () => {
    const { user } = sharedRender();

    await waitFor(() => {
      expect(screen.getByTestId(tableId)).toBeInTheDocument();
    });

    const tbody = screen.getByTestId(`${tableId}-body`);
    expect(tbody).toBeInTheDocument();

    const rows = within(tbody).getAllByRole('row');
    const enrolmentsAgainRow = rows[0];

    expect(enrolmentsAgainRow.childNodes[0]).toHaveTextContent(
      'Enrolment Group',
    );
    expect(enrolmentsAgainRow.childNodes[1]).toHaveTextContent(
      'Enrolments_Again',
    );
    expect(enrolmentsAgainRow.childNodes[2]).toHaveTextContent('not present');

    const noMappingButton = within(enrolmentsAgainRow).getByRole('button', {
      name: 'No mapping for Enrolments_Again',
    });
    expect(noMappingButton).toBeInTheDocument();

    await user.click(noMappingButton);

    waitFor(async () => {
      expect(noMappingButton).not.toBeInTheDocument();
      expect(
        dataReplacementService.updatePlanIndicatorMappings,
      ).toHaveBeenCalled();
    });

    expect(enrolmentsAgainRow.childNodes[1]).toHaveTextContent(
      'Enrolments_Again',
    );

    expect(enrolmentsAgainRow.childNodes[2]).toHaveTextContent('No mapping');
  });
});
