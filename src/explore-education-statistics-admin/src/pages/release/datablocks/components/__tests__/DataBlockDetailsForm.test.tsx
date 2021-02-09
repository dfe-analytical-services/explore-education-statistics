import DataBlockDetailsForm, {
  DataBlockDetailsFormValues,
} from '@admin/pages/release/datablocks/components/DataBlockDetailsForm';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
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
          source: 'Test source',
        }}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Name')).toHaveValue('Test name');
    expect(screen.getByLabelText('Table title')).toHaveValue('Test heading');
    expect(screen.getByLabelText('Source')).toHaveValue('Test source');
    expect(screen.getByLabelText('Table highlight name')).toHaveValue(
      'Test highlight name',
    );
  });

  test('shows validation error if name is empty', async () => {
    render(<DataBlockDetailsForm onSubmit={noop} />);

    userEvent.click(screen.getByLabelText('Name'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Enter a data block name' }),
      ).toHaveAttribute('href', '#dataBlockDetailsForm-name');
    });
  });

  test('shows validation error if table title is empty', async () => {
    render(<DataBlockDetailsForm onSubmit={noop} />);

    userEvent.click(screen.getByLabelText('Table title'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Enter a table title' }),
      ).toHaveAttribute('href', '#dataBlockDetailsForm-heading');
    });
  });

  test('shows validation error if no table highlight when checkbox is checked', async () => {
    render(<DataBlockDetailsForm onSubmit={noop} />);

    userEvent.click(
      screen.getByLabelText('Set as a table highlight for this publication'),
    );

    expect(screen.getByLabelText('Table highlight name')).toBeInTheDocument();

    userEvent.click(screen.getByLabelText('Table highlight name'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Enter a table highlight name' }),
      ).toHaveAttribute('href', '#dataBlockDetailsForm-highlightName');
    });
  });

  test('submitting form with invalid values and highlight name shows error messages', async () => {
    render(<DataBlockDetailsForm onSubmit={noop} />);

    userEvent.click(
      screen.getByLabelText('Set as a table highlight for this publication'),
    );
    userEvent.click(screen.getByRole('button', { name: 'Save data block' }));

    await waitFor(() => {
      const alert = screen.getByRole('alert');
      const errors = within(alert).getAllByRole('link');

      expect(errors).toHaveLength(3);
      expect(errors[0]).toHaveTextContent('Enter a data block name');
      expect(errors[0]).toHaveAttribute('href', '#dataBlockDetailsForm-name');

      expect(errors[1]).toHaveTextContent('Enter a table highlight name');
      expect(errors[1]).toHaveAttribute(
        'href',
        '#dataBlockDetailsForm-highlightName',
      );

      expect(errors[2]).toHaveTextContent('Enter a table title');
      expect(errors[2]).toHaveAttribute(
        'href',
        '#dataBlockDetailsForm-heading',
      );
    });
  });

  test('submitting form with invalid values and no highlight name shows error messages', async () => {
    render(<DataBlockDetailsForm onSubmit={noop} />);

    userEvent.click(screen.getByRole('button', { name: 'Save data block' }));

    await waitFor(() => {
      const alert = screen.getByRole('alert');
      const errors = within(alert).getAllByRole('link');

      expect(errors).toHaveLength(2);
      expect(errors[0]).toHaveTextContent('Enter a data block name');
      expect(errors[0]).toHaveAttribute('href', '#dataBlockDetailsForm-name');

      expect(errors[1]).toHaveTextContent('Enter a table title');
      expect(errors[1]).toHaveAttribute(
        'href',
        '#dataBlockDetailsForm-heading',
      );
    });
  });

  test('successfully submits form with valid values', async () => {
    const handleSubmit = jest.fn();

    render(<DataBlockDetailsForm onSubmit={handleSubmit} />);

    await userEvent.type(screen.getByLabelText('Name'), 'Test name');
    await userEvent.type(screen.getByLabelText('Table title'), 'Test title');
    await userEvent.type(screen.getByLabelText('Source'), 'Test source');

    userEvent.click(
      screen.getByLabelText('Set as a table highlight for this publication'),
    );

    await userEvent.type(
      screen.getByLabelText('Table highlight name'),
      'Test highlight name',
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save data block' }));

    const expected: DataBlockDetailsFormValues = {
      name: 'Test name',
      heading: 'Test title',
      highlightName: 'Test highlight name',
      source: 'Test source',
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(expected);
    });
  });
});
