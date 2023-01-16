import ReleaseTypeFilters from '@frontend/modules/find-statistics/components/ReleaseTypeFilters';
import { render, screen, within } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

describe('ReleaseTypeFilters', () => {
  test('renders correctly', () => {
    render(<ReleaseTypeFilters onChange={noop} />);

    const releaseTypeFilterGroup = within(
      screen.getByRole('group', { name: 'Filter by release type' }),
    );
    const releaseTypeOptions = releaseTypeFilterGroup.getAllByRole('radio');
    expect(releaseTypeOptions).toHaveLength(6);
    expect(releaseTypeOptions[0]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Show all'),
    );
    expect(releaseTypeOptions[0]).toBeChecked();
    expect(releaseTypeOptions[1]).toEqual(
      releaseTypeFilterGroup.getByLabelText('National statistics'),
    );
    expect(releaseTypeOptions[1]).not.toBeChecked();
    expect(releaseTypeOptions[2]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Official statistics'),
    );
    expect(releaseTypeOptions[2]).not.toBeChecked();
    expect(releaseTypeOptions[3]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Experimental statistics'),
    );
    expect(releaseTypeOptions[3]).not.toBeChecked();
    expect(releaseTypeOptions[4]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Ad hoc statistics'),
    );
    expect(releaseTypeOptions[4]).not.toBeChecked();
    expect(releaseTypeOptions[5]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Management information'),
    );
    expect(releaseTypeOptions[5]).not.toBeChecked();

    expect(
      screen.getByRole('button', { name: 'What are release types?' }),
    ).toBeInTheDocument();
  });

  test('selects the correct option when releaseType is set', () => {
    render(
      <ReleaseTypeFilters
        releaseType="ManagementInformation"
        onChange={noop}
      />,
    );

    expect(screen.getByLabelText('Management information')).toBeChecked();
  });

  test('calls onChange when a theme is selected', () => {
    const handleChange = jest.fn();
    render(<ReleaseTypeFilters onChange={handleChange} />);

    expect(handleChange).not.toHaveBeenCalled();

    userEvent.click(screen.getByLabelText('Official statistics'));

    expect(handleChange).toHaveBeenCalledWith({
      filterType: 'releaseType',
      nextValue: 'OfficialStatistics',
    });
  });

  test('shows the guidance modal', () => {
    render(
      <ReleaseTypeFilters
        releaseType="ManagementInformation"
        onChange={noop}
      />,
    );

    userEvent.click(
      screen.getByRole('button', { name: 'What are release types?' }),
    );

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', { name: 'Release types guidance' }),
    ).toBeInTheDocument();
  });
});
