import React from 'react';
import { ContentBlock } from '@common/services/types/blocks';
import { render, screen } from '@testing-library/react';
import ContentBlockRenderer from '../ContentBlockRenderer';

describe('ContentBlockRenderer', () => {
  const testHMTLRendererBlock: ContentBlock = {
    id: 'test-html-block',
    body: '<div><h2>test html block</h2><p><i>some italic text</i><em>some italic text</em>, <strong>this is bold</strong></p></div>',
    type: 'HtmlBlock',
    order: 0,
  };

  test('renders html block', async () => {
    render(<ContentBlockRenderer block={testHMTLRendererBlock} />);

    expect(
      screen.getByRole('heading', { name: 'test html block' }),
    ).toBeInTheDocument();
  });

  test('italicised/emphasized text to be plain text in html block', async () => {
    render(
      <ContentBlockRenderer
        block={{
          ...testHMTLRendererBlock,
          body: '<i>some italic text</i> <em>some emphasized text</em>',
        }}
      />,
    );

    expect(
      screen.getByText('some italic text some emphasized text'),
    ).toBeInTheDocument();

    const italics = document.getElementsByTagName('i');
    expect(italics.length).toBe(0);

    const emphasized = document.getElementsByTagName('em');
    expect(emphasized.length).toBe(0);
  });
});
