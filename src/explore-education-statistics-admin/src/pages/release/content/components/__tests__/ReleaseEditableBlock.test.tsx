import ReleaseEditableBlock from '@admin/pages/release/content/components/ReleaseEditableBlock';
import { EditableBlock } from '@admin/services/types/content';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { testEditableRelease } from '@admin/pages/release/__data__/testEditableRelease';
import { render, screen } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';

describe('ReleaseEditableBlock', () => {
  const testHtmlBlock: EditableBlock = {
    id: 'block-1',
    type: 'HtmlBlock',
    body: `<h3>Test heading</h3>
           <p>Some test content</p>`,
    comments: [],
    order: 0,
  };

  const testValue = {
    release: testEditableRelease,
    canUpdateRelease: true,
    commentsPendingDeletion: {},
    availableDataBlocks: [],
    unresolvedComments: [],
  };

  test('renders HTML block', () => {
    render(
      <ReleaseContentProvider value={testValue}>
        <ReleaseEditableBlock
          releaseId="release-1"
          sectionId="section-1"
          block={testHtmlBlock}
          onBlockCommentsChange={noop}
          onSave={noop}
          onDelete={noop}
        />
      </ReleaseContentProvider>,
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
      '/api/releases/{releaseId}/images/some-image-id-100 100w, ' +
      '/api/releases/{releaseId}/images/some-image-id-200 200w, ' +
      '/api/releases/{releaseId}/images/some-image-id-300 300w';

    const image2SrcSet =
      'https://test/some-image-url-100.jpg 100w, ' +
      'https://test/some-image-url-200.jpg 200w, ' +
      'https://test/some-image-url-300.jpg 300w';

    render(
      <ReleaseContentProvider value={testValue}>
        <ReleaseEditableBlock
          releaseId="release-1"
          sectionId="section-1"
          block={{
            ...testHtmlBlock,
            body: `
          <h3>Test heading</h3>
          <p>Some test content</p>
          <img alt="Test image 1" src="/api/releases/{releaseId}/images/some-image-id" srcset="${image1SrcSet}" />
          <img alt="Test image 2" src="https://test/some-image-url.jpg" srcset="${image2SrcSet}" />
          `,
          }}
          onBlockCommentsChange={noop}
          onSave={noop}
          onDelete={noop}
        />
      </ReleaseContentProvider>,
    );

    expect(screen.getByRole('img', { name: 'Test image 1' })).toHaveAttribute(
      'src',
      '/api/releases/release-1/images/some-image-id',
    );
    expect(screen.getByRole('img', { name: 'Test image 1' })).toHaveAttribute(
      'srcset',
      '/api/releases/release-1/images/some-image-id-100 100w, ' +
        '/api/releases/release-1/images/some-image-id-200 200w, ' +
        '/api/releases/release-1/images/some-image-id-300 300w',
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
