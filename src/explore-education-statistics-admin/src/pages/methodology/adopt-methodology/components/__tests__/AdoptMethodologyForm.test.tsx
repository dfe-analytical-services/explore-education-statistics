import AdoptMethodologyForm from '@admin/pages/methodology/adopt-methodology/components/AdoptMethodologyForm';
import { BasicMethodology } from '@admin/services/methodologyService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import noop from 'lodash/noop';

describe('AdoptMethodologyForm', () => {
  const testMethodologies: BasicMethodology[] = [
    {
      id: '1',
      methodologyId: 'm1',
      amendment: false,
      owningPublication: {
        title: 'owning pub 1',
      },
      status: 'Draft',
      title: 'methodology 1',
    } as BasicMethodology,
    {
      id: '2',
      methodologyId: 'm2',
      amendment: true,
      owningPublication: {
        title: 'owning pub 2',
      },
      published: '2021-06-08T09:04:17.9805585',
      status: 'Approved',
      title: 'methodology 2',
    } as BasicMethodology,
    {
      id: '3',
      methodologyId: 'm3',
      amendment: true,
      owningPublication: {
        title: 'owning pub 3',
      },
      status: 'Draft',
      title: 'methodology 3',
    } as BasicMethodology,
    {
      id: '4',
      methodologyId: 'm4',
      owningPublication: {
        title: 'owning pub 4',
      },
      published: '2021-06-10T09:04:17.9805585',
      status: 'Approved',
      title: 'methodology 4',
    } as BasicMethodology,
  ];

  test('renders the form', () => {
    render(
      <AdoptMethodologyForm
        methodologies={testMethodologies}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByText('Select a methodology')).toBeInTheDocument();

    expect(screen.getByLabelText('Search')).toBeInTheDocument();

    const radios = screen.getAllByRole('radio');
    expect(radios.length).toBe(4);
    expect(screen.getByLabelText('methodology 1')).toHaveAttribute(
      'value',
      'm1',
    );
    expect(screen.getByLabelText('methodology 2')).toHaveAttribute(
      'value',
      'm2',
    );
    expect(screen.getByLabelText('methodology 3')).toHaveAttribute(
      'value',
      'm3',
    );
    expect(screen.getByLabelText('methodology 4')).toHaveAttribute(
      'value',
      'm4',
    );

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('renders more details about each methodology', () => {
    render(
      <AdoptMethodologyForm
        methodologies={testMethodologies}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    const radio1 = screen.getByTestId('Radio item for methodology 1');
    expect(radio1).toBeInTheDocument();
    expect(within(radio1).getByText('owning pub 1'));
    expect(within(radio1).getByText('Draft'));
    expect(within(radio1).getByText('Not yet published'));

    const radio2 = screen.getByTestId('Radio item for methodology 2');
    expect(radio2).toBeInTheDocument();
    expect(within(radio2).getByText('owning pub 2'));
    expect(within(radio2).getByText('Approved'));
    expect(within(radio2).getByText('Amendment'));
    expect(within(radio2).getByText('8 June 2021'));

    const radio3 = screen.getByTestId('Radio item for methodology 3');
    expect(radio3).toBeInTheDocument();
    expect(within(radio3).getByText('owning pub 3'));
    expect(within(radio3).getByText('Draft'));
    expect(within(radio3).getByText('Amendment'));
    expect(within(radio3).getByText('Not yet published'));

    const radio4 = screen.getByTestId('Radio item for methodology 4');
    expect(radio4).toBeInTheDocument();
    expect(within(radio4).getByText('owning pub 4'));
    expect(within(radio4).getByText('Approved'));
    expect(within(radio4).getByText('10 June 2021'));
  });

  test('shows validation error if submit without selecting a methodology', async () => {
    const handleSubmit = jest.fn();
    render(
      <AdoptMethodologyForm
        methodologies={testMethodologies}
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Select a methodology to adopt' }),
    ).toBeInTheDocument();
    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('submits successfully with a selected methodology', async () => {
    const handleSubmit = jest.fn();
    render(
      <AdoptMethodologyForm
        methodologies={testMethodologies}
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(screen.getByLabelText('methodology 2'));
    expect(screen.getByLabelText('methodology 2')).toBeChecked();

    userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        methodologyId: 'm2',
      });
    });
  });
});
