import SearchDataSearchForm from '@frontend/modules/search-data/components/SearchDataSearchForm';
import _dataSetService from '@frontend/services/azureDataSetService';
import _publicationService from '@frontend/services/azurePublicationService';
import { render, screen, within } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';
import { testPublicationSuggestions } from '@frontend/modules/find-statistics/__tests__/__data__/testPublicationSuggestions';
import { testDataSetSuggestions } from '@frontend/modules/search-data/__tests__/__data__/testDataSetSuggestions';

jest.mock('@azure/search-documents', () => ({
  SearchClient: jest.fn(),
  AzureKeyCredential: jest.fn(),
  odata: jest.fn(),
}));

jest.mock('@frontend/services/azurePublicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;
jest.mock('@frontend/services/azureDataSetService');
const dataSetService = _dataSetService as jest.Mocked<typeof _dataSetService>;

describe('SearchDataSearchForm', () => {
  test('renders correctly', () => {
    render(<SearchDataSearchForm onSubmit={noop} />);
    expect(screen.getByRole('combobox')).toBeInTheDocument();
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
    render(<SearchDataSearchForm onSubmit={handleSubmit} />);
    await userEvent.type(screen.getByRole('combobox'), 'find me');
    await userEvent.click(screen.getByRole('button', { name: 'Search' }));
    expect(handleSubmit).toHaveBeenCalledWith('find me');
  });

  test('shows suggestions when entered term is at least 3 characters with results', async () => {
    publicationService.suggestPublications.mockResolvedValue(
      testPublicationSuggestions,
    );
    render(<SearchDataSearchForm onSubmit={noop} />);
    await userEvent.type(screen.getByRole('combobox'), 'find me');
    expect(screen.getByRole('listbox')).not.toHaveClass(
      'autocomplete__menu--hidden',
    );
    const listItems = screen.getAllByRole('option');
    expect(listItems).toHaveLength(3);
    expect(within(listItems[0]).getAllByText('Publication')).toHaveLength(2);
  });

  test('searches datasets when isSearchData is true', async () => {
    dataSetService.suggestDatasets.mockResolvedValue(testDataSetSuggestions);
    render(<SearchDataSearchForm onSubmit={noop} isSearchData />);
    await userEvent.type(screen.getByRole('combobox'), 'find me');
    expect(screen.getByRole('listbox')).not.toHaveClass(
      'autocomplete__menu--hidden',
    );
    const listItems = screen.getAllByRole('option');
    expect(listItems).toHaveLength(2);
    expect(within(listItems[0]).getAllByText('Data set')).toHaveLength(2);
    expect(
      within(listItems[0]).queryByText('Publication'),
    ).not.toBeInTheDocument();
  });

  test('does not show suggestions when entered term is at least 3 characters with no results', async () => {
    publicationService.suggestPublications.mockResolvedValue([]);
    render(<SearchDataSearchForm onSubmit={noop} />);
    await userEvent.type(screen.getByRole('combobox'), 'find me');
    expect(screen.getByRole('listbox')).toHaveClass(
      'autocomplete__menu--hidden',
    );
  });

  test("doesn't show suggestions when entered term is less than 3 characters", async () => {
    render(<SearchDataSearchForm onSubmit={noop} />);
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
    render(<SearchDataSearchForm onSubmit={handleSubmit} />);
    await userEvent.type(screen.getByRole('combobox'), 'find me');
    await userEvent.keyboard('[Enter]');
    expect(handleSubmit).toHaveBeenCalledWith('find me');
  });
});
