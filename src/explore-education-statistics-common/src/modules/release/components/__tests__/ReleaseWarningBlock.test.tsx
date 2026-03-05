import { ContentBlock } from '@common/services/types/blocks';
import render from '@common-test/render';
import { screen } from '@testing-library/react';
import React from 'react';
import ReleaseWarningBlock from '../ReleaseWarningBlock';

const testBlock: ContentBlock = {
  id: 'test-html-block',
  body: '<p>Test warning block content. With <a href="https://example.com">test link</a></p>',
  type: 'HtmlBlock',
  order: 0,
};

describe('ReleaseWarningBlock', () => {
  test('renders the block correctly', () => {
    render(<ReleaseWarningBlock block={testBlock} />);

    expect(screen.getByText(/Test warning block content/)).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'test link (opens in new tab)' }),
    ).toBeInTheDocument();
  });
  test('does not render p tags inside strong', () => {
    render(<ReleaseWarningBlock block={testBlock} />);

    const paragraphs = document.getElementsByTagName('p');
    expect(paragraphs.length).toBe(0);
  });
});
