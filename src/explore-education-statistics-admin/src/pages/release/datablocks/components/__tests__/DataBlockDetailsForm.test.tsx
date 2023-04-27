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
          highlightDescription: 'Test highlight description',
          source: 'Test source',
        }}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Name')).toHaveValue('Test name');
    expect(screen.getByLabelText('Table title')).toHaveValue('Test heading');
    expect(screen.getByLabelText('Source')).toHaveValue('Test source');
    expect(screen.getByLabelText('Featured table name')).toHaveValue(
      'Test highlight name',
    );
    expect(screen.getByLabelText('Featured table description')).toHaveValue(
      'Test highlight description',
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

  test('shows validation error if no name when featured table checkbox is checked', async () => {
    render(<DataBlockDetailsForm onSubmit={noop} />);

    userEvent.click(
      screen.getByLabelText('Set as a featured table for this publication'),
    );

    expect(screen.getByLabelText('Featured table name')).toBeInTheDocument();

    userEvent.click(screen.getByLabelText('Featured table name'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Enter a featured table name' }),
      ).toHaveAttribute('href', '#dataBlockDetailsForm-highlightName');
    });
  });

  test('shows validation error if no description when featured table checkbox is checked', async () => {
    render(<DataBlockDetailsForm onSubmit={noop} />);

    userEvent.click(
      screen.getByLabelText('Set as a featured table for this publication'),
    );

    expect(
      screen.getByLabelText('Featured table description'),
    ).toBeInTheDocument();

    userEvent.click(screen.getByLabelText('Featured table description'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', {
          name: 'Enter a featured table description',
        }),
      ).toHaveAttribute('href', '#dataBlockDetailsForm-highlightDescription');
    });
  });

  test('submitting form with invalid values and featured table checked shows error messages', async () => {
    render(<DataBlockDetailsForm onSubmit={noop} />);

    userEvent.click(
      screen.getByLabelText('Set as a featured table for this publication'),
    );
    userEvent.click(screen.getByRole('button', { name: 'Save data block' }));

    await waitFor(() => {
      const alert = screen.getByRole('alert');
      const errors = within(alert).getAllByRole('link');

      expect(errors).toHaveLength(4);
      expect(errors[0]).toHaveTextContent('Enter a data block name');
      expect(errors[0]).toHaveAttribute('href', '#dataBlockDetailsForm-name');

      expect(errors[1]).toHaveTextContent('Enter a featured table name');
      expect(errors[1]).toHaveAttribute(
        'href',
        '#dataBlockDetailsForm-highlightName',
      );

      expect(errors[2]).toHaveTextContent('Enter a featured table description');
      expect(errors[2]).toHaveAttribute(
        'href',
        '#dataBlockDetailsForm-highlightDescription',
      );

      expect(errors[3]).toHaveTextContent('Enter a table title');
      expect(errors[3]).toHaveAttribute(
        'href',
        '#dataBlockDetailsForm-heading',
      );
    });
  });

  test('submitting form with invalid values and featured table unchecked shows error messages', async () => {
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

    userEvent.type(screen.getByLabelText('Name'), 'Test name');
    userEvent.type(screen.getByLabelText('Table title'), 'Test title');
    userEvent.type(screen.getByLabelText('Source'), 'Test source');

    userEvent.click(
      screen.getByLabelText('Set as a featured table for this publication'),
    );

    userEvent.type(
      screen.getByLabelText('Featured table name'),
      'Test highlight name',
    );
    userEvent.type(
      screen.getByLabelText('Featured table description'),
      'Test highlight description',
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save data block' }));

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
