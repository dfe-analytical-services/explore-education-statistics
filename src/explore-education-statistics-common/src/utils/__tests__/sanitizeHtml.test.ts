import sanitizeHtml from '@common/utils/sanitizeHtml';

describe('sanitizeHtml', () => {
  describe('filterTags option', () => {
    test('removes tag when filter is true', () => {
      const dirtyHtml = '<p>Some <span>target</span> in here.</p>';

      const html = sanitizeHtml(dirtyHtml, {
        filterTags: {
          span: () => true,
        },
      });

      expect(html).toBe('<p>Some  in here.</p>');
    });

    test('removes multiple tags when filter is true', () => {
      const dirtyHtml = '<p>Some <span>target</span> in <span>here.</span></p>';

      const html = sanitizeHtml(dirtyHtml, {
        filterTags: {
          span: () => true,
        },
      });

      expect(html).toBe('<p>Some  in </p>');
    });

    test('removes specific tag where filter condition is true', () => {
      const dirtyHtml =
        '<p>Some <span class="test">target</span> in <span>here.</span></p>';

      const html = sanitizeHtml(dirtyHtml, {
        filterTags: {
          span: frame => frame.attribs.class === 'test',
        },
      });

      expect(html).toBe('<p>Some  in <span>here.</span></p>');
    });

    test('does not remove any tag when filter is false', () => {
      const dirtyHtml = '<p>Some <span>target</span> in <span>here.</span></p>';

      const html = sanitizeHtml(dirtyHtml, {
        filterTags: {
          span: () => false,
        },
      });

      expect(html).toBe(dirtyHtml);
    });

    test('removes different types of tags where filters are true', () => {
      const dirtyHtml =
        '<p>Some <span>target</span> <strong>in</strong> <em>here.</em></p>';

      const html = sanitizeHtml(dirtyHtml, {
        filterTags: {
          span: () => true,
          em: () => true,
          // Does not remove this
          strong: () => false,
        },
      });

      expect(html).toBe('<p>Some  <strong>in</strong> </p>');
    });
  });
});
