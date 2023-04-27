import { EditableContentBlock } from '@admin/services/types/content';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import MethodologyEditableBlock from '@admin/pages/methodology/edit-methodology/content/components/MethodologyEditableBlock';

describe('MethodologyEditableBlock', () => {
  const testHtmlBlock: EditableContentBlock = {
    id: 'block-1',
    type: 'HtmlBlock',
    body: `<h3>Test heading</h3>
           <p>Some test content</p>`,
    comments: [],
    order: 0,
  };

  test('renders HTML block', () => {
    render(
      <MethodologyEditableBlock
        methodologyId="methodology-1"
        block={testHtmlBlock}
        onSave={noop}
        onDelete={noop}
      />,
    );

    expect(
      screen.getByText('Test heading', { selector: 'h3' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Some test content', { selector: 'p' }),
    ).toBeInTheDocument();
  });

  test('renders HTML block with correct image urls', () => {
    const image1SrcSet =
      '/api/methodologies/{methodologyId}/images/some-image-id-100 100w, ' +
      '/api/methodologies/{methodologyId}/images/some-image-id-200 200w, ' +
      '/api/methodologies/{methodologyId}/images/some-image-id-300 300w';

    const image2SrcSet =
      'https://test/some-image-url-100.jpg 100w, ' +
      'https://test/some-image-url-200.jpg 200w, ' +
      'https://test/some-image-url-300.jpg 300w';

    render(
      <MethodologyEditableBlock
        methodologyId="methodology-1"
        block={{
          ...testHtmlBlock,
          body: `
          <h3>Test heading</h3>
          <p>Some test content</p>
          <img alt="Test image 1" src="/api/methodologies/{methodologyId}/images/some-image-id" srcset="${image1SrcSet}" />
          <img alt="Test image 2" src="https://test/some-image-url.jpg" srcset="${image2SrcSet}" />
          `,
        }}
        onSave={noop}
        onDelete={noop}
      />,
    );

    expect(screen.getByRole('img', { name: 'Test image 1' })).toHaveAttribute(
      'src',
      '/api/methodologies/methodology-1/images/some-image-id',
    );
    expect(screen.getByRole('img', { name: 'Test image 1' })).toHaveAttribute(
      'srcset',
      '/api/methodologies/methodology-1/images/some-image-id-100 100w, ' +
        '/api/methodologies/methodology-1/images/some-image-id-200 200w, ' +
        '/api/methodologies/methodology-1/images/some-image-id-300 300w',
    );

    expect(screen.getByRole('img', { name: 'Test image 2' })).toHaveAttribute(
      'src',
      'https://test/some-image-url.jpg',
    );
    expect(screen.getByRole('img', { name: 'Test image 2' })).toHaveAttribute(
      'srcset',
      image2SrcSet,
    );
  });
});
