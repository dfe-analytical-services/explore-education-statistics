import SearchFormAzure from '@frontend/components/SearchFormAzure';
import _publicationService from '@frontend/services/azurePublicationService';
import { render, screen } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';
import { testPublicationSuggestions } from '@frontend/modules/find-statistics/__tests__/__data__/testPublicationSuggestions';

jest.mock('@azure/search-documents', () => ({
  SearchClient: jest.fn(),
  AzureKeyCredential: jest.fn(),
  odata: jest.fn(),
}));

jest.mock('@frontend/services/azurePublicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('SearchFormAzure', () => {
  test('renders correctly', () => {
    render(<SearchFormAzure onSubmit={noop} />);
    expect(screen.getByRole('combobox')).toBeInTheDocument();
    expect(screen.getByRole('combobox')).toHaveAttribute('type', 'search');
    expect(screen.getByRole('button', { name: 'Search' })).toBeInTheDocument();
    // N.b checking by 'not.toBeVisible' doesn't work as Jest isn't loading the global CSS
    expect(screen.getByRole('listbox')).toHaveClass(
      'autocomplete__menu--hidden',
    );
  });

  test('calls onSubmit when submitted', async () => {
    publicationService.suggestPublications.mockResolvedValue(
      testPublicationSuggestions,
    );
    const handleSubmit = jest.fn();
    render(<SearchFormAzure onSubmit={handleSubmit} />);
    await userEvent.type(screen.getByRole('combobox'), 'find me');
    await userEvent.click(screen.getByRole('button', { name: 'Search' }));
    expect(handleSubmit).toHaveBeenCalledWith('find me');
  });

  test('shows suggestions when entered term is at least 3 characters with results', async () => {
    publicationService.suggestPublications.mockResolvedValue(
      testPublicationSuggestions,
    );
    render(<SearchFormAzure onSubmit={noop} />);
    await userEvent.type(screen.getByRole('combobox'), 'find me');
    expect(screen.getByRole('listbox')).not.toHaveClass(
      'autocomplete__menu--hidden',
    );
  });

  test('shows suggestions when entered term is at least 3 characters with no results', async () => {
    publicationService.suggestPublications.mockResolvedValue([]);
    render(<SearchFormAzure onSubmit={noop} />);
    await userEvent.type(screen.getByRole('combobox'), 'find me');
    expect(screen.getByRole('listbox')).toHaveClass(
      'autocomplete__menu--hidden',
    );
  });

  test("doesn't show suggestions when entered term is less than 3 characters", async () => {
    render(<SearchFormAzure onSubmit={noop} />);
    await userEvent.type(screen.getByRole('combobox'), 'fi');
    expect(screen.getByRole('listbox')).toHaveClass(
      'autocomplete__menu--hidden',
    );
  });

  test('submits the form when enter is pressed', async () => {
    publicationService.suggestPublications.mockResolvedValue(
      testPublicationSuggestions,
    );
    const handleSubmit = jest.fn();
    render(<SearchFormAzure onSubmit={handleSubmit} />);
    await userEvent.type(screen.getByRole('combobox'), 'find me');
    await userEvent.keyboard('[Enter]');
    expect(handleSubmit).toHaveBeenCalledWith('find me');
  });
});
