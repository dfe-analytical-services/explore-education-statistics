import GlossaryItemInsertForm from '@admin/components/editable/GlossaryItemInsertForm';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import _glossaryService from '@admin/services/glossaryService';
import baseRender from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React, { ReactElement } from 'react';

jest.mock('@admin/services/glossaryService');

const glossaryService = _glossaryService as jest.Mocked<
  typeof _glossaryService
>;

describe('GlossaryItemInsertForm ', () => {
  beforeEach(() => {
    glossaryService.listEntries.mockResolvedValue([
      {
        heading: 'A',
        entries: [
          { body: 'body text', slug: 'aardvark', title: 'Aardvark' },
          { body: 'body text', slug: 'antelope', title: 'Antelope' },
          { body: 'body text', slug: 'ant', title: 'Ant' },
        ],
      },
      {
        heading: 'D',
        entries: [{ body: 'body text', slug: 'dog', title: 'Dog' }],
      },
      {
        heading: 'P',
        entries: [{ body: 'body text', slug: 'partridge', title: 'Partridge' }],
      },
    ]);
  });

  test('does not search if search term is less than 3 characters', async () => {
    render(<GlossaryItemInsertForm onCancel={noop} onSubmit={noop} />);

    await waitFor(() => {
      expect(screen.getByText('Glossary entry')).toBeInTheDocument();
    });

    userEvent.type(screen.getByLabelText('Glossary entry'), 'an');

    expect(screen.queryAllByRole('option')).toHaveLength(0);
  });

  test('shows search results', async () => {
    render(<GlossaryItemInsertForm onCancel={noop} onSubmit={noop} />);

    await waitFor(() => {
      expect(screen.getByText('Glossary entry')).toBeInTheDocument();
    });

    userEvent.type(screen.getByLabelText('Glossary entry'), 'ant');

    await waitFor(() => {
      expect(screen.getByText('Antelope')).toBeInTheDocument();
    });

    const options = screen.getAllByRole('option');
    expect(options).toHaveLength(2);
    expect(options[0]).toHaveTextContent('Antelope');
    expect(options[1]).toHaveTextContent('Ant');
  });

  test('selecting a search result shows the edit form', async () => {
    render(<GlossaryItemInsertForm onCancel={noop} onSubmit={noop} />);

    await waitFor(() => {
      expect(screen.getByText('Glossary entry')).toBeInTheDocument();
    });

    userEvent.type(screen.getByLabelText('Glossary entry'), 'ant');

    await waitFor(() => {
      expect(screen.getByText('Antelope')).toBeInTheDocument();
    });

    const options = screen.getAllByRole('option');
    userEvent.click(options[0]);

    expect(screen.getByLabelText('Link text')).toHaveValue('Antelope');
    expect(screen.getByText('Slug: antelope')).toBeInTheDocument();
  });

  test('successfully submitting form', async () => {
    const handleSubmit = jest.fn();
    render(<GlossaryItemInsertForm onCancel={noop} onSubmit={handleSubmit} />);

    await waitFor(() => {
      expect(screen.getByText('Glossary entry')).toBeInTheDocument();
    });

    userEvent.type(screen.getByLabelText('Glossary entry'), 'ant');

    await waitFor(() => {
      expect(screen.getByText('Antelope')).toBeInTheDocument();
    });

    const options = screen.getAllByRole('option');
    userEvent.click(options[0]);

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Insert' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        text: 'Antelope',
        url: 'http://localhost/glossary#antelope',
      });
    });
  });

  test('successfully submits the form after editing the link text', async () => {
    const handleSubmit = jest.fn();
    render(<GlossaryItemInsertForm onCancel={noop} onSubmit={handleSubmit} />);

    await waitFor(() => {
      expect(screen.getByText('Glossary entry')).toBeInTheDocument();
    });

    userEvent.type(screen.getByLabelText('Glossary entry'), 'ant');

    await waitFor(() => {
      expect(screen.getByText('Antelope')).toBeInTheDocument();
    });

    const options = screen.getAllByRole('option');
    userEvent.click(options[0]);

    userEvent.type(screen.getByLabelText('Link text'), ' edited');

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Insert' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        text: 'Antelope edited',
        url: 'http://localhost/glossary#antelope',
      });
    });
  });

  test('shows a validation error if submit without selecting a glossary entry', async () => {
    const handleSubmit = jest.fn();
    render(<GlossaryItemInsertForm onCancel={noop} onSubmit={handleSubmit} />);

    await waitFor(() => {
      expect(screen.getByText('Glossary entry')).toBeInTheDocument();
    });

    userEvent.click(screen.getByRole('button', { name: 'Insert' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'Select a glossary entry',
        }),
      ).toBeInTheDocument();
    });

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('shows a validation error if submit without link text', async () => {
    const handleSubmit = jest.fn();
    render(<GlossaryItemInsertForm onCancel={noop} onSubmit={handleSubmit} />);

    await waitFor(() => {
      expect(screen.getByText('Glossary entry')).toBeInTheDocument();
    });

    userEvent.type(screen.getByLabelText('Glossary entry'), 'ant');

    await waitFor(() => {
      expect(screen.getByText('Antelope')).toBeInTheDocument();
    });

    const options = screen.getAllByRole('option');
    userEvent.click(options[0]);

    userEvent.clear(screen.getByLabelText('Link text'));

    userEvent.click(screen.getByRole('button', { name: 'Insert' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'Enter link text',
        }),
      ).toBeInTheDocument();
    });

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  function render(element: ReactElement) {
    baseRender(
      <TestConfigContextProvider>{element}</TestConfigContextProvider>,
    );
  }
});
