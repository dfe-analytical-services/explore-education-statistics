import {
  IndicatorsMappingsPlan,
  LocationMappingsPlan,
} from '@admin/services/dataReplacementService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import DataFileReplacementTable, {
  TableMappingGroup,
} from '@admin/pages/release/data/components/DataFileReplacementTable';

jest.mock('@admin/services/dataReplacementService');

const mappingsPlan: IndicatorsMappingsPlan = {
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
};
const mappingsGroup: TableMappingGroup[] = [
  {
    label: 'Enrolment Group',
    mappings: ['enrolments_again', 'enrolments'],
  },
];

const mappingsToShow: string[] = ['enrolments_again', 'enrolments'];

const locationsMappingPlan: LocationMappingsPlan = {
  mappings: {
    'country-england': {
      source: {
        id: 'country-england',
        name: 'England',
        code: 'E92000001',
      },
      type: 'Unset',
    },
  },
  candidates: {
    'country-england': {
      id: 'country-england',
      name: 'England',
      code: 'E92000001',
    },
  },
};

const locationsMappingGroup: TableMappingGroup[] = [
  {
    label: 'Country',
    mappings: ['country-england'],
  },
];

const locationsMappingsToShow: string[] = ['country-england'];

describe('DataFileReplacementDifferences', () => {
  const handler = jest.fn(() => Promise.resolve());
  const sharedRender = () =>
    render(
      <DataFileReplacementTable
        tableId="replacements-differences-indicators-table"
        itemType="indicator"
        mappingsPlan={mappingsPlan}
        mappingGroups={mappingsGroup}
        mappingsToShow={mappingsToShow}
        handleIndicatorsMappingUpdate={handler}
        mappedDataLabels={['label', 'name']}
      />,
    );

  const tableId = 'replacements-differences-indicators-table';

  test('renders location replacement type', async () => {
    const locationsTableId = 'locations-table';

    render(
      <DataFileReplacementTable
        tableId={locationsTableId}
        itemType="location"
        handleIndicatorsMappingUpdate={handler}
        mappingsPlan={locationsMappingPlan}
        mappingsToShow={locationsMappingsToShow}
        mappingGroups={locationsMappingGroup}
        mappedDataLabels={['name', 'code']}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId(locationsTableId)).toBeInTheDocument();
    });

    const caption = screen
      .getByTestId(`${locationsTableId}`)
      .querySelector('caption');

    expect(caption).toBeInTheDocument();
    expect(caption).toHaveTextContent('Locations');
    expect(caption).toHaveTextContent('1 unmapped location');

    const tbody = screen.getByTestId(`${locationsTableId}-body`);
    expect(tbody).toBeInTheDocument();

    // check table items there
    expect(within(tbody).getAllByRole('row')).toHaveLength(1);

    // check actions
    expect(within(tbody).getAllByRole('button')).toHaveLength(2);

    const firstRow = within(tbody).getAllByRole('row')[0];

    expect(firstRow.childNodes[0]).toHaveTextContent('Country');
    expect(firstRow.childNodes[1]).toHaveTextContent('England');
  });

  test('renders indicator replacement type', async () => {
    sharedRender();

    await waitFor(() => {
      expect(screen.getByTestId(tableId)).toBeInTheDocument();
    });

    const caption = screen.getByTestId(`${tableId}`).querySelector('caption');

    expect(caption).toBeInTheDocument();
    expect(caption).toHaveTextContent('Indicators');
    expect(caption).toHaveTextContent('2 unmapped indicators');

    const tbody = screen.getByTestId(`${tableId}-body`);
    expect(tbody).toBeInTheDocument();

    // check table items there
    expect(within(tbody).getAllByRole('row')).toHaveLength(2);

    // check actions
    expect(within(tbody).getAllByRole('button')).toHaveLength(4);

    const firstRow = within(tbody).getAllByRole('row')[0];

    expect(firstRow.childNodes[0]).toHaveTextContent('Enrolment Group');
    expect(firstRow.childNodes[1]).toHaveTextContent('Enrolments_Again');

    const secondRow = within(tbody).getAllByRole('row')[1];

    expect(secondRow.childNodes[0]).toBeEmptyDOMElement();
    expect(secondRow.childNodes[1]).toHaveTextContent('Enrolments');
  });

  test('renders replacement table and dialog', async () => {
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

    await waitFor(async () => {
      expect(handler).toHaveBeenCalledWith({
        sourceKey: 'enrolments_again',
        candidateKey: 'number_of_enrolments_again',
      });
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

    await waitFor(async () => {
      expect(handler).toHaveBeenCalledWith({
        sourceKey: 'enrolments_again',
        candidateKey: undefined,
      });
    });
  });
});
