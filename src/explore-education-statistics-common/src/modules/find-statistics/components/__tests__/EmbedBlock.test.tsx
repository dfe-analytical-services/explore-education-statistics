import EmbedBlock from '@common/modules/find-statistics/components/EmbedBlock';
import { render } from '@testing-library/react';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';

describe('EmbedBlock', () => {
  test('does not server-render the iframe', () => {
    const container = document.createElement('div');
    container.innerHTML = renderToStaticMarkup(
      <EmbedBlock title="Test dashboard" url="https://example.com" />,
    );

    expect(container.querySelector('iframe')).toBeNull();
    expect(container).toHaveTextContent('Loading Test dashboard');
  });

  test('renders the iframe once mounted', () => {
    const { container } = render(
      <EmbedBlock title="Test dashboard" url="https://example.com" />,
    );

    const iframe = container.querySelector('iframe');
    expect(iframe).toHaveAttribute('src', 'https://example.com');
    expect(iframe).toHaveAttribute('title', 'Test dashboard');
  });
});
