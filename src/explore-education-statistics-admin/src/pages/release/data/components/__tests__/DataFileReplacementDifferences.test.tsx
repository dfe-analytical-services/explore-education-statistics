import { IndicatorsMapping } from '@admin/services/releaseDataFileService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import DataFileReplacementDifferences from '../DataFileReplacementDifferences';

jest.mock('@admin/services/releaseDataFileService');

const dummyIndicatorsMappings: IndicatorsMapping = {
  mappings: {
    // old indicators and their mapping/lack there of
    enrolments_again: {
      source: {
        label: 'Enrolments_Again',
      },
      type: 'Unset',
    },
    enrolments: {
      source: {
        label: 'Enrolments',
      },
      type: 'Unset',
    },
    sess_possible: {
      source: {
        label: 'Number of possible sessions',
      },
      type: 'AutoSet',
      candidateKey: 'sess_possible',
    },
    sess_authorised: {
      source: {
        label: 'Number of authorised sessions',
      },
      type: 'AutoSet',
      candidateKey: 'sess_authorised',
    },
    sess_unauthorised: {
      source: {
        label: 'Number of unauthorised sessions',
      },
      type: 'AutoSet',
      candidateKey: 'sess_unauthorised',
    },
    sess_unauthorised_percent: {
      source: {
        label: 'Percentage of unauthorised sessions',
      },
      type: 'AutoSet',
      candidateKey: 'sess_unauthorised_percent',
    },
  },
  candidates: {
    // indicators from the replacement data
    sess_possible: {
      label: 'Number of possible sessions',
    },
    sess_authorised: {
      label: 'Number of authorised sessions',
    },
    sess_unauthorised: {
      label: 'Number of unauthorised sessions',
    },
    number_of_enrolments_again: {
      label: 'Number of enrolments again',
    },
    number_of_enrolments: {
      label: 'Number of enrolments',
    },
    sess_unauthorised_percent: {
      label: 'Percentage of unauthorised sessions',
    },
  },
};

describe('DataFileReplacementDifferences', () => {
  const tableId = 'replacements-differences-table';
  test('renders differences/mappings table with options and available actions', async () => {
    const { user } = render(
      <DataFileReplacementDifferences
        fileId="fileId"
        releaseVersionId="releaseVersionId"
        mappings={{ indicators: dummyIndicatorsMappings }}
      />,
    );

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
    const { user } = render(
      <DataFileReplacementDifferences
        fileId="fileId"
        releaseVersionId="releaseVersionId"
        mappings={{ indicators: dummyIndicatorsMappings }}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId(tableId)).toBeInTheDocument();
    });

    const tbody = screen.getByTestId(`${tableId}-body`);
    expect(tbody).toBeInTheDocument();

    const rows = within(tbody).getAllByRole('row');
    const enrolmentsAgainRow = rows[0];

    expect(enrolmentsAgainRow.childNodes[0]).toHaveTextContent('Indicator');
    expect(enrolmentsAgainRow.childNodes[1]).toHaveTextContent(
      'Enrolments_Again',
    );
    expect(enrolmentsAgainRow.childNodes[2]).toHaveTextContent('not present');

    expect(
      within(enrolmentsAgainRow).getByText('not present'),
    ).toBeInTheDocument();

    await user.click(
      within(enrolmentsAgainRow).getByRole('button', {
        name: 'Map item Enrolments_Again',
      }),
    );

    waitFor(() => {
      expect(screen.getAllByRole('dialog')).toHaveLength(1);
    });

    const modal = screen.getByRole('dialog');
    const radioOptions = within(modal).getAllByRole('radio');
    expect(radioOptions).toHaveLength(3);

    const radio = screen.getByLabelText('Number of enrolments again');

    await user.click(radio);

    expect(radio).toBeChecked();

    await user.click(
      // submit
      within(modal).getByRole('button', { name: 'Update indicator mapping' }),
    );

    expect(modal).not.toBeInTheDocument();

    /* waitFor(async () => {
      expect(
        releaseDataFileService.updateDataFileIndicatorsMapping,
      ).toHaveBeenCalled();
    }); */

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
    const { user } = render(
      <DataFileReplacementDifferences
        fileId="fileId"
        releaseVersionId="releaseVersionId"
        mappings={{ indicators: dummyIndicatorsMappings }}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId(tableId)).toBeInTheDocument();
    });

    const tbody = screen.getByTestId(`${tableId}-body`);
    expect(tbody).toBeInTheDocument();

    const rows = within(tbody).getAllByRole('row');
    const enrolmentsAgainRow = rows[0];

    expect(enrolmentsAgainRow.childNodes[1]).toHaveTextContent(
      'Enrolments_Again',
    );
    expect(
      within(enrolmentsAgainRow).getByText('not present'),
    ).toBeInTheDocument();

    const noMappingButton = within(enrolmentsAgainRow).getByRole('button', {
      name: 'No mapping for Enrolments_Again',
    });
    expect(noMappingButton).toBeInTheDocument();

    await user.click(noMappingButton);

    waitFor(async () => {
      expect(noMappingButton).not.toBeInTheDocument();
      /*       expect(
        releaseDataFileService.updateDataFileIndicatorsMapping,
      ).toHaveBeenCalled(); */
    });

    expect(enrolmentsAgainRow.childNodes[1]).toHaveTextContent(
      'Enrolments_Again',
    );

    expect(enrolmentsAgainRow.childNodes[2]).toHaveTextContent('No mapping');
  });
});
