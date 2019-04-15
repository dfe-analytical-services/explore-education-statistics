import _tableBuilderService, {
  PublicationMeta,
} from '@common/services/tableBuilderService';
import React from 'react';
import { fireEvent, render, wait, within } from 'react-testing-library';
import TableToolPage from '../TableToolPage';

jest.mock('@common/services/tableBuilderService');

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('TableToolPage', () => {
  test('renders list of publication options correctly', () => {
    const { getByLabelText } = render(<TableToolPage />);

    expect(getByLabelText('Pupil absence')).toHaveAttribute('type', 'radio');
    expect(getByLabelText('Exclusions')).toHaveAttribute('type', 'radio');
    expect(
      getByLabelText('Schools, pupils and their characteristics'),
    ).toHaveAttribute('type', 'radio');
  });

  test('clicking a publication option reveals the filter form with the correct section title', async () => {
    tableBuilderService.getCharacteristicsMeta.mockImplementation(() =>
      Promise.resolve<PublicationMeta>({
        characteristics: {},
        indicators: {},
        publicationId: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
      }),
    );

    const { getByText, getByLabelText } = render(<TableToolPage />);

    fireEvent.click(getByLabelText('Pupil absence'));

    await wait();

    expect(
      getByText(/2\. Filter statistics from 'Pupil absence'/),
    ).toBeDefined();
  });

  describe('filter form', () => {
    beforeEach(() => {
      tableBuilderService.getCharacteristicsMeta.mockImplementation(() =>
        Promise.resolve<PublicationMeta>({
          characteristics: {
            'Ethnic group major': [
              {
                label: 'Black',
                name: 'ethnicity_major_black',
              },
              {
                label: 'Chinese',
                name: 'ethnicity_major_chinese',
              },
            ],
            Gender: [
              {
                label: 'Boys',
                name: 'gender_male',
              },
              {
                label: 'Girls',
                name: 'gender_female',
              },
            ],
          },
          indicators: {
            'Absence by reason': [
              {
                label: 'Number of authorised holidays sessions',
                name: 'sess_auth_holiday',
              },
              {
                label: 'Number of excluded sessions',
                name: 'sess_auth_excluded',
              },
            ],
            'Absence fields': [
              {
                label: 'Authorised absence rate',
                name: 'sess_authorised_percent',
              },
              {
                label: 'Unauthorised absence rate',
                name: 'sess_unauthorised_percent',
              },
            ],
          },
          publicationId: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
        }),
      );
    });

    test('renders groups of checkboxes in Indicators field set', async () => {
      const page = render(<TableToolPage />);

      fireEvent.click(page.getByLabelText('Pupil absence'));

      await wait();

      const indicatorsFieldSet = within(page.container.querySelector(
        'fieldset#filter-indicators',
      ) as HTMLFieldSetElement);

      expect(indicatorsFieldSet.getByText('Indicators')).toBeDefined();

      const checkboxGroups = indicatorsFieldSet.getAllByRole('group');

      const group1 = within(checkboxGroups[0]);

      expect(group1.getByText('Absence by reason')).toBeDefined();

      const checkbox1 = group1.getByLabelText(
        'Number of authorised holidays sessions',
      );
      expect(checkbox1).toHaveAttribute('type', 'checkbox');
      expect(checkbox1).toHaveAttribute('value', 'sess_auth_holiday');

      const checkbox2 = group1.getByLabelText('Number of excluded sessions');
      expect(checkbox2).toHaveAttribute('type', 'checkbox');
      expect(checkbox2).toHaveAttribute('value', 'sess_auth_excluded');

      const group2 = within(checkboxGroups[1]);

      expect(group2.getByText('Absence fields')).toBeDefined();

      const checkbox3 = group2.getByLabelText('Authorised absence rate');
      expect(checkbox3).toHaveAttribute('type', 'checkbox');
      expect(checkbox3).toHaveAttribute('value', 'sess_authorised_percent');

      const checkbox4 = group2.getByLabelText('Unauthorised absence rate');
      expect(checkbox4).toHaveAttribute('type', 'checkbox');
      expect(checkbox4).toHaveAttribute('value', 'sess_unauthorised_percent');
    });

    test('renders groups of checkboxes in Characteristics fieldset', async () => {
      const page = render(<TableToolPage />);

      fireEvent.click(page.getByLabelText('Pupil absence'));

      await wait();

      const charactersticsFieldSet = within(page.container.querySelector(
        'fieldset#filter-characteristics',
      ) as HTMLFieldSetElement);

      expect(charactersticsFieldSet.getByText('Characteristics')).toBeDefined();

      const checkboxGroups = charactersticsFieldSet.getAllByRole('group');

      const group1 = within(checkboxGroups[0]);

      expect(group1.getByText('Ethnic group major')).toBeDefined();

      const checkbox1 = group1.getByLabelText('Black');
      expect(checkbox1).toHaveAttribute('type', 'checkbox');
      expect(checkbox1).toHaveAttribute('value', 'ethnicity_major_black');

      const checkbox2 = group1.getByLabelText('Chinese');
      expect(checkbox2).toHaveAttribute('type', 'checkbox');
      expect(checkbox2).toHaveAttribute('value', 'ethnicity_major_chinese');

      const group2 = within(checkboxGroups[1]);

      expect(group2.getByText('Gender')).toBeDefined();

      const checkbox3 = group2.getByLabelText('Boys');
      expect(checkbox3).toHaveAttribute('type', 'checkbox');
      expect(checkbox3).toHaveAttribute('value', 'gender_male');

      const checkbox4 = group2.getByLabelText('Girls');
      expect(checkbox4).toHaveAttribute('type', 'checkbox');
      expect(checkbox4).toHaveAttribute('value', 'gender_female');
    });
  });
});
