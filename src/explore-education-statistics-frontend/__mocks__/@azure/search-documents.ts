export default jest.mock('@azure/search-documents', () => ({
  SearchClient: jest.fn(),
  AzureKeyCredential: jest.fn().mockImplementation(() => {}),
  odata: jest.fn(),
}));
