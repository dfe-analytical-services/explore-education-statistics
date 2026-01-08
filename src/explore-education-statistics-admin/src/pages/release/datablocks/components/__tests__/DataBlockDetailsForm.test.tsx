import DataBlockDetailsForm, {
  DataBlockDetailsFormValues,
} from '@admin/pages/release/datablocks/components/DataBlockDetailsForm';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('DataBlockDetailsForm', () => {
  test('renders correctly with initial values', () => {
    render(
      <DataBlockDetailsForm
        initialValues={{
          name: 'Test name',
          heading: 'Test heading',
          highlightName: 'Test highlight name',
          highlightDescription: 'Test highlight description',
          source: 'Test source',
        }}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Data block name')).toHaveValue('Test name');
    expect(screen.getByLabelText('Table title')).toHaveValue('Test heading');
    expect(screen.getByLabelText('Data source')).toHaveValue('Test source');
    expect(screen.getByLabelText('Featured table name')).toHaveValue(
      'Test highlight name',
    );
    expect(screen.getByLabelText('Featured table description')).toHaveValue(
      'Test highlight description',
    );
  });

  test('shows validation error if name is empty', async () => {
    const { user } = render(<DataBlockDetailsForm onSubmit={noop} />);

    await user.click(screen.getByRole('button', { name: 'Save data block' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Enter a data block name' }),
    ).toHaveAttribute('href', '#dataBlockDetailsForm-name');
  });

  test('shows validation error if table title is empty', async () => {
    const { user } = render(<DataBlockDetailsForm onSubmit={noop} />);

    await user.click(screen.getByRole('button', { name: 'Save data block' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Enter a table title' }),
    ).toHaveAttribute('href', '#dataBlockDetailsForm-heading');
  });

  test('shows validation error if no name when featured table checkbox is checked', async () => {
    const { user } = render(<DataBlockDetailsForm onSubmit={noop} />);

    await user.click(
      screen.getByLabelText('Set as a featured table for this publication'),
    );

    expect(screen.getByLabelText('Featured table name')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Save data block' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Enter a featured table name' }),
    ).toHaveAttribute('href', '#dataBlockDetailsForm-highlightName');
  });

  test('shows validation error if featured table name is entirely a whitespace string', async () => {
    const { user } = render(
      <DataBlockDetailsForm
        initialValues={{
          name: 'Test name',
          heading: 'Test heading',
          highlightName: '',
          highlightDescription: 'Test highlight description',
          source: 'Test source',
        }}
        onSubmit={noop}
      />,
    );

    await user.click(
      screen.getByLabelText('Set as a featured table for this publication'),
    );

    expect(screen.getByLabelText('Featured table name')).toBeInTheDocument();

    await user.type(screen.getByLabelText('Featured table name'), '     ');

    await user.click(screen.getByRole('button', { name: 'Save data block' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Enter a featured table name' }),
    ).toHaveAttribute('href', '#dataBlockDetailsForm-highlightName');
  });

  test('shows validation error if no description when featured table checkbox is checked', async () => {
    const { user } = render(<DataBlockDetailsForm onSubmit={noop} />);

    await user.click(
      screen.getByLabelText('Set as a featured table for this publication'),
    );

    expect(
      screen.getByLabelText('Featured table description'),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Save data block' }));

    await waitFor(() => {
      expect(
        screen.getByRole('link', {
          name: 'Enter a featured table description',
        }),
      ).toHaveAttribute('href', '#dataBlockDetailsForm-highlightDescription');
    });
  });

  test('submitting form with invalid values and featured table checked shows error messages', async () => {
    const { user } = render(<DataBlockDetailsForm onSubmit={noop} />);

    await user.click(
      screen.getByLabelText('Set as a featured table for this publication'),
    );
    await user.click(screen.getByRole('button', { name: 'Save data block' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();

    const alert = screen.getByRole('alert');
    const errors = within(alert).getAllByRole('link');

    expect(errors).toHaveLength(4);
    expect(errors[0]).toHaveTextContent('Enter a data block name');
    expect(errors[0]).toHaveAttribute('href', '#dataBlockDetailsForm-name');

    expect(errors[1]).toHaveTextContent('Enter a table title');
    expect(errors[1]).toHaveAttribute('href', '#dataBlockDetailsForm-heading');

    expect(errors[2]).toHaveTextContent('Enter a featured table name');
    expect(errors[2]).toHaveAttribute(
      'href',
      '#dataBlockDetailsForm-highlightName',
    );

    expect(errors[3]).toHaveTextContent('Enter a featured table description');
    expect(errors[3]).toHaveAttribute(
      'href',
      '#dataBlockDetailsForm-highlightDescription',
    );
  });

  test('submitting form with invalid values and featured table unchecked shows error messages', async () => {
    const { user } = render(<DataBlockDetailsForm onSubmit={noop} />);

    await user.click(screen.getByRole('button', { name: 'Save data block' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();

    const alert = screen.getByRole('alert');
    const errors = within(alert).getAllByRole('link');

    expect(errors).toHaveLength(2);
    expect(errors[0]).toHaveTextContent('Enter a data block name');
    expect(errors[0]).toHaveAttribute('href', '#dataBlockDetailsForm-name');
    expect(errors[1]).toHaveTextContent('Enter a table title');
    expect(errors[1]).toHaveAttribute('href', '#dataBlockDetailsForm-heading');
  });

  test('successfully submits form with valid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(<DataBlockDetailsForm onSubmit={handleSubmit} />);

    await user.type(screen.getByLabelText('Data block name'), 'Test name');
    await user.type(screen.getByLabelText('Table title'), 'Test title');
    await user.type(screen.getByLabelText('Data source'), 'Test source');

    await user.click(
      screen.getByLabelText('Set as a featured table for this publication'),
    );

    await user.type(
      screen.getByLabelText('Featured table name'),
      'Test highlight name',
    );
    await user.type(
      screen.getByLabelText('Featured table description'),
      'Test highlight description',
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Save data block' }));

    const expected: DataBlockDetailsFormValues = {
      name: 'Test name',
      heading: 'Test title',
      highlightName: 'Test highlight name',
      highlightDescription: 'Test highlight description',
      source: 'Test source',
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(expected);
    });
  });

  test('trim featured table name and description values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(<DataBlockDetailsForm onSubmit={handleSubmit} />);

    await user.type(screen.getByLabelText('Data block name'), 'Test name');
    await user.type(screen.getByLabelText('Table title'), 'Test title');
    await user.type(screen.getByLabelText('Data source'), 'Test source');

    await user.click(
      screen.getByLabelText('Set as a featured table for this publication'),
    );

    await user.type(
      screen.getByLabelText('Featured table name'),
      '   Test highlight name   ',
    );
    await user.type(
      screen.getByLabelText('Featured table description'),
      '    Test highlight description    ',
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Save data block' }));

    const expected: DataBlockDetailsFormValues = {
      name: 'Test name',
      heading: 'Test title',
      highlightName: 'Test highlight name',
      highlightDescription: 'Test highlight description',
      source: 'Test source',
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(expected);
    });
  });
});
