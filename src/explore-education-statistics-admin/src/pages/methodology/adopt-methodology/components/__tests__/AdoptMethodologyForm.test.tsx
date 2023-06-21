import AdoptMethodologyForm from '@admin/pages/methodology/adopt-methodology/components/AdoptMethodologyForm';
import { MethodologyVersion } from '@admin/services/methodologyService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import noop from 'lodash/noop';

describe('AdoptMethodologyForm', () => {
  const testMethodologies: MethodologyVersion[] = [
    {
      id: 'methodology-v1',
      methodologyId: 'methodology-1',
      amendment: false,
      owningPublication: {
        title: 'Owning publication 1',
      },
      status: 'Draft',
      title: 'Methodology 1',
    } as MethodologyVersion,
    {
      id: 'methodology-v2',
      methodologyId: 'methodology-2',
      amendment: true,
      owningPublication: {
        title: 'Owning publication 2',
      },
      published: '2021-06-08T09:04:17.9805585',
      status: 'Approved',
      title: 'Methodology 2',
    } as MethodologyVersion,
    {
      id: 'methodology-v3',
      methodologyId: 'methodology-3',
      amendment: true,
      owningPublication: {
        title: 'Owning publication 3',
      },
      status: 'Draft',
      title: 'Methodology 3',
    } as MethodologyVersion,
    {
      id: 'methodology-v4',
      methodologyId: 'methodology-4',
      owningPublication: {
        title: 'Owning publication 4',
      },
      published: '2021-06-10T09:04:17.9805585',
      status: 'Approved',
      title: 'Methodology 4',
    } as MethodologyVersion,
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

    expect(
      screen.getByLabelText('Search for a methodology'),
    ).toBeInTheDocument();

    const radios = screen.getAllByRole('radio');
    expect(radios.length).toBe(4);
    expect(screen.getByLabelText('Methodology 1')).toHaveAttribute(
      'value',
      'methodology-1',
    );
    expect(screen.getByLabelText('Methodology 2')).toHaveAttribute(
      'value',
      'methodology-2',
    );
    expect(screen.getByLabelText('Methodology 3')).toHaveAttribute(
      'value',
      'methodology-3',
    );
    expect(screen.getByLabelText('Methodology 4')).toHaveAttribute(
      'value',
      'methodology-4',
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

    const radio1 = screen.getByTestId('Radio item for Methodology 1');
    expect(radio1).toBeInTheDocument();
    expect(within(radio1).getByText('Owning publication 1'));
    expect(within(radio1).getByText('Draft'));
    expect(within(radio1).getByText('Not yet published'));

    const radio2 = screen.getByTestId('Radio item for Methodology 2');
    expect(radio2).toBeInTheDocument();
    expect(within(radio2).getByText('Owning publication 2'));
    expect(within(radio2).getByText('Approved'));
    expect(within(radio2).getByText('Amendment'));
    expect(within(radio2).getByText('8 June 2021'));

    const radio3 = screen.getByTestId('Radio item for Methodology 3');
    expect(radio3).toBeInTheDocument();
    expect(within(radio3).getByText('Owning publication 3'));
    expect(within(radio3).getByText('Draft'));
    expect(within(radio3).getByText('Amendment'));
    expect(within(radio3).getByText('Not yet published'));

    const radio4 = screen.getByTestId('Radio item for Methodology 4');
    expect(radio4).toBeInTheDocument();
    expect(within(radio4).getByText('Owning publication 4'));
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

    userEvent.click(screen.getByLabelText('Methodology 2'));
    expect(screen.getByLabelText('Methodology 2')).toBeChecked();

    userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        methodologyId: 'methodology-2',
      });
    });
  });
});
