import FormColourInput from '@common/components/form/FormColourInput';
import { Colour } from '@common/modules/charts/util/chartUtils';
import render from '@common-test/render';
import React from 'react';
import { screen, within } from '@testing-library/react';

describe('FormColourInput', () => {
  const testColours: Colour[] = [
    { label: 'Colour 1', value: '#12436D' },
    { label: 'Colour 2', value: '#F46A25' },
    { label: 'Colour 3', value: '#801650' },
  ];

  test('renders correctly without an initial value', async () => {
    const { user } = render(
      <FormColourInput
        colours={testColours}
        id="test-id"
        label="Test label"
        name="test-name"
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Test label Colour 1, click to change colour',
      }),
    );

    const modal = within(screen.getByRole('dialog'));

    const group = modal.getByRole('group', { name: 'Select a colour' });
    const options = within(group).getAllByRole('radio');
    expect(options).toHaveLength(4);
    expect(options[0]).toEqual(modal.getByLabelText('Colour 1'));
    expect(options[0]).toBeChecked();
    expect(options[1]).toEqual(modal.getByLabelText('Colour 2'));
    expect(options[2]).toEqual(modal.getByLabelText('Colour 3'));
    expect(options[3]).toEqual(modal.getByLabelText('Custom'));

    expect(
      modal.queryByRole('button', { name: 'Select custom colour' }),
    ).not.toBeInTheDocument();

    expect(modal.getByRole('button', { name: 'Confirm' })).toBeInTheDocument();
    expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('renders correctly with an initial value from the provided colours', async () => {
    const { user } = render(
      <FormColourInput
        colours={testColours}
        id="test-id"
        label="Test label"
        name="test-name"
        initialValue="#F46A25"
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Test label Colour 2, click to change colour',
      }),
    );

    const modal = within(screen.getByRole('dialog'));

    const group = modal.getByRole('group', { name: 'Select a colour' });
    const options = within(group).getAllByRole('radio');
    expect(options).toHaveLength(4);
    expect(options[0]).toEqual(modal.getByLabelText('Colour 1'));
    expect(options[1]).toEqual(modal.getByLabelText('Colour 2'));
    expect(options[1]).toBeChecked();
    expect(options[2]).toEqual(modal.getByLabelText('Colour 3'));
    expect(options[3]).toEqual(modal.getByLabelText('Custom'));

    expect(
      modal.queryByRole('button', { name: 'Select custom colour' }),
    ).not.toBeInTheDocument();

    expect(modal.getByRole('button', { name: 'Confirm' })).toBeInTheDocument();
    expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('renders correctly with a custom initial value', async () => {
    const { user } = render(
      <FormColourInput
        colours={testColours}
        id="test-id"
        label="Test label"
        name="test-name"
        initialValue="#000000"
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Test label #000000, click to change colour',
      }),
    );

    const modal = within(screen.getByRole('dialog'));

    const group = modal.getByRole('group', { name: 'Select a colour' });
    const options = within(group).getAllByRole('radio');
    expect(options).toHaveLength(4);
    expect(options[0]).toEqual(modal.getByLabelText('Colour 1'));
    expect(options[1]).toEqual(modal.getByLabelText('Colour 2'));
    expect(options[2]).toEqual(modal.getByLabelText('Colour 3'));
    expect(options[3]).toEqual(modal.getByLabelText('Custom'));
    expect(options[3]).toBeChecked();

    expect(
      modal.getByRole('button', { name: 'Select custom colour' }),
    ).toBeInTheDocument();

    expect(modal.getByRole('button', { name: 'Confirm' })).toBeInTheDocument();
    expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('calls onConfirm with the selected value when confirm', async () => {
    const handleConfirm = jest.fn();
    const { user } = render(
      <FormColourInput
        colours={testColours}
        id="test-id"
        label="Test label"
        name="test-name"
        onConfirm={handleConfirm}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Test label Colour 1, click to change colour',
      }),
    );

    const modal = within(screen.getByRole('dialog'));

    await user.click(modal.getByLabelText('Colour 3'));

    expect(handleConfirm).not.toHaveBeenCalled();
    await user.click(modal.getByRole('button', { name: 'Confirm' }));

    expect(handleConfirm).toHaveBeenCalledTimes(1);
    expect(handleConfirm).toHaveBeenCalledWith('#801650');
  });
});
