// Leaflet touches `document` as soon as it is imported, so it must stay out
// of DataBlockTabs' static import graph for the frontend to server-render it.
// This test ensures that is the case.
jest.mock('leaflet', () => {
  throw new Error('leaflet must not be imported eagerly');
});
jest.mock('react-leaflet', () => {
  throw new Error('react-leaflet must not be imported eagerly');
});

describe('DataBlockTabs', () => {
  test('can be imported without importing Leaflet', async () => {
    await expect(
      import('@common/modules/find-statistics/components/DataBlockTabs'),
    ).resolves.toBeDefined();
  });
});
