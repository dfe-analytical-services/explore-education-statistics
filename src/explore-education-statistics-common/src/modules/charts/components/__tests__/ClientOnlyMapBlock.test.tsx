import ClientOnlyMapBlock from '@common/modules/charts/components/ClientOnlyMapBlock';
import { MapBlockProps } from '@common/modules/charts/components/MapBlock';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';

let mockMapBlockError: Error | undefined;

jest.mock('@common/modules/charts/components/MapBlock', () => {
  if (mockMapBlockError) {
    throw mockMapBlockError;
  }

  return {
    __esModule: true,
    default: ({ id }: { id: string }) => `Map ${id}`,
  };
});

describe('ClientOnlyMapBlock', () => {
  const testProps = { id: 'test-map' } as MapBlockProps;

  beforeEach(() => {
    mockMapBlockError = undefined;
    // So each test imports MapBlock afresh.
    jest.resetModules();
  });

  test('server-renders a loading spinner instead of the map', () => {
    const container = document.createElement('div');
    container.innerHTML = renderToStaticMarkup(
      <ClientOnlyMapBlock {...testProps} />,
    );

    expect(container).toHaveTextContent('Loading map');
    expect(container).not.toHaveTextContent('Map test-map');
  });

  test('renders the map once loaded', async () => {
    render(<ClientOnlyMapBlock {...testProps} />);

    expect(await screen.findByText('Map test-map')).toBeInTheDocument();
    expect(screen.queryByText('Loading map')).not.toBeInTheDocument();
  });

  test('renders an error message if the map fails to load', async () => {
    mockMapBlockError = new Error('Chunk failed to load');

    render(<ClientOnlyMapBlock {...testProps} />);

    expect(await screen.findByText('Could not load map')).toBeInTheDocument();
    expect(screen.queryByText('Map test-map')).not.toBeInTheDocument();
  });
});
