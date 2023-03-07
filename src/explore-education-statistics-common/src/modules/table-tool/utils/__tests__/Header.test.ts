import Header from '@common/modules/table-tool/utils/Header';

describe('Header', () => {
  test('maintains `depth` for nested headers', () => {
    const header = new Header('0', 'Header 0').addChild(
      new Header('1', 'Header 1').addChild(
        new Header('2', 'Header 2').addChild(new Header('3', 'Header 3')),
      ),
    );

    expect(header.depth).toBe(0);
    expect(header.getLastChild()?.depth).toBe(1);
    expect(header.getLastChild()?.getLastChild()?.depth).toBe(2);
    expect(header.getLastChild()?.getLastChild()?.getLastChild()?.depth).toBe(
      3,
    );
  });

  describe('crossSpan', () => {
    test('returns 2 if has identical child', () => {
      const header = new Header('1', '1').addChild(new Header('1', '1'));

      expect(header.crossSpan).toBe(2);
    });

    test('returns 1 if has identical and non-identical children', () => {
      const header = new Header('1', '1')
        .addChild(new Header('1', '1'))
        .addChild(new Header('2', '2'));

      expect(header.crossSpan).toBe(1);
    });

    test('returns 1 if has identical and non-identical children (in reverse order)', () => {
      const header = new Header('1', '1')
        .addChild(new Header('2', '2'))
        .addChild(new Header('1', '1'));

      expect(header.crossSpan).toBe(1);
    });

    test('returns 3 if has identical child and grandchild', () => {
      const header = new Header('1', '1').addChild(
        new Header('1', '1').addChild(new Header('1', '1')),
      );

      expect(header.crossSpan).toBe(3);
    });

    test('returns 2 if has identical child, and identical and non-identical grandchildren', () => {
      const header = new Header('1', '1').addChild(
        new Header('1', '1')
          .addChild(new Header('1', '1'))
          .addChild(new Header('2', '2')),
      );

      expect(header.crossSpan).toBe(2);
    });

    test('returns 2 if has identical child, and identical and non-identical grandchildren (in reverse order)', () => {
      const header = new Header('1', '1').addChild(
        new Header('1', '1')
          .addChild(new Header('2', '2'))
          .addChild(new Header('1', '1')),
      );

      expect(header.crossSpan).toBe(2);
    });

    test('return 1 if child doesnt have text', () => {
      const header = new Header('1', '1').addChild(new Header('2', ''));

      expect(header.crossSpan).toBe(1);
    });

    test('return 1 if child and grandchild dont have text', () => {
      const header = new Header('1', '1').addChild(
        new Header('2', '').addChild(new Header('3', '')),
      );

      expect(header.crossSpan).toBe(1);
    });
  });

  describe('addChild', () => {
    const testTreeWithSymmetricAdjacentBranches = () =>
      new Header('0', 'Header 0')
        .addChild(
          new Header('0a', 'Header 0a')
            .addChild(
              new Header('0a-1a', 'Header 0a-1a')
                .addChild(new Header('0a-1a-2a', 'Header 0a-1a-2a'))
                .addChild(new Header('0a-1a-2b', 'Header 0a-1a-2b')),
            )
            .addChild(
              new Header('0a-1b', 'Header 0a-1b')
                .addChild(new Header('0a-1b-2a', 'Header 0a-1b-2a'))
                .addChild(new Header('0a-1b-2b', 'Header 0a-1b-2b')),
            )
            .addChild(
              new Header('0a-1c', 'Header 0a-1c')
                .addChild(new Header('0a-1c-2a', 'Header 0a-1c-2a'))
                .addChild(new Header('0a-1c-2b', 'Header 0a-1c-2b')),
            ),
        )
        .addChild(new Header('0b', 'Header 0b'));

    const testTreeWithAsymmetricAdjacentBranches = () =>
      new Header('0', 'Header 0')
        .addChild(
          new Header('0a', 'Header 0a')
            .addChild(
              new Header('0a-1a', 'Header 0a-1a')
                .addChild(new Header('0a-1a-2a', 'Header 0a-1a-2a'))
                .addChild(new Header('0a-1a-2b', 'Header 0a-1a-2b')),
            )
            .addChild(new Header('0a-1b', 'Header 0a-1b'))
            .addChild(
              new Header('0a-1c', 'Header 0a-1c')
                .addChild(new Header('0a-1c-2a', 'Header 0a-1c-2a'))
                .addChild(new Header('0a-1c-2b', 'Header 0a-1c-2b')),
            ),
        )
        .addChild(new Header('0b', 'Header 0b'));

    test('increases `span` of the last child if the `id` matches', () => {
      const header = new Header('0', 'Header 0').addChild(
        new Header('0a', 'Header 0a'),
      );

      expect(header.children).toHaveLength(1);
      expect(header.getLastChild()?.id).toBe('0a');
      expect(header.getLastChild()?.span).toBe(1);

      header.addChild(new Header('0a', 'Header 0a'));

      expect(header.children).toHaveLength(1);
      expect(header.getLastChild()?.id).toBe('0a');
      expect(header.getLastChild()?.span).toBe(2);
    });

    test('adds new child if last child `id` does not match', () => {
      const header = new Header('0', 'Header 0').addChild(
        new Header('0a', 'Header 0a'),
      );

      expect(header.children).toHaveLength(1);
      expect(header.getLastChild()?.id).toBe('0a');
      expect(header.getLastChild()?.span).toBe(1);

      header.addChild(new Header('0b', 'Header 0b'));

      expect(header.children).toHaveLength(2);
      expect(header.getLastChild()?.id).toBe('0b');
      expect(header.getLastChild()?.span).toBe(1);
    });

    test('does not increase `span` if parent header only adds one child', () => {
      const header = new Header('0', 'Header 0').addChild(
        new Header('0a', 'Header 0a'),
      );

      expect(header.span).toBe(1);
    });

    test('increases `span` of parent header if adding multiple children', () => {
      const header = new Header('0', 'Header 0')
        .addChild(new Header('0a', 'Header 0a'))
        .addChild(new Header('0b', 'Header 0b'));

      expect(header.span).toBe(2);
    });

    test('updates `span` recursively for tree with symmetric adjacent branches', () => {
      const header = testTreeWithSymmetricAdjacentBranches();

      expect(header.span).toBe(7);

      expect(header.children[0].span).toBe(6);

      expect(header.children[0].children[0].span).toBe(2);
      expect(header.children[0].children[0].children[0].span).toBe(1);
      expect(header.children[0].children[0].children[1].span).toBe(1);

      expect(header.children[0].children[1].span).toBe(2);
      expect(header.children[0].children[1].children[0].span).toBe(1);
      expect(header.children[0].children[1].children[1].span).toBe(1);

      expect(header.children[0].children[2].span).toBe(2);
      expect(header.children[0].children[2].children[0].span).toBe(1);
      expect(header.children[0].children[2].children[1].span).toBe(1);

      expect(header.children[1].span).toBe(1);
    });

    test('updates `span` recursively for tree with asymmetric adjacent branches', () => {
      const header = testTreeWithAsymmetricAdjacentBranches();

      expect(header.span).toBe(6);

      expect(header.children[0].span).toBe(5);

      expect(header.children[0].children[0].span).toBe(2);
      expect(header.children[0].children[0].children[0].span).toBe(1);
      expect(header.children[0].children[0].children[1].span).toBe(1);

      expect(header.children[0].children[1].span).toBe(1);

      expect(header.children[0].children[2].span).toBe(2);
      expect(header.children[0].children[2].children[0].span).toBe(1);
      expect(header.children[0].children[2].children[1].span).toBe(1);

      expect(header.children[1].span).toBe(1);
    });
  });

  describe('addChildToLastParent', () => {
    test('adds new header at depth = 0', () => {
      const header = new Header('0', 'Header 0');
      header.addChildToLastParent(new Header('0a', 'Header 0a'), 0);

      expect(header.getLastChild()?.id).toBe('0a');
      expect(header.children).toHaveLength(1);
    });

    test('adds new header to nested header at depth = 0', () => {
      const header = new Header('0', 'Header 0')
        .addChild(new Header('0a', 'Header 0a'))
        .addChild(new Header('0b', 'Header 0b'));

      header.addChildToLastParent(new Header('1', 'Header 1'), 0);

      expect(header.children).toHaveLength(3);
      expect(header.getLastChild()?.id).toBe('1');
    });

    test('adds new header to nested header at depth = 1', () => {
      const header = new Header('0', 'Header 0')
        .addChild(new Header('0a', 'Header 0a'))
        .addChild(new Header('0b', 'Header 0b'));

      header.addChildToLastParent(new Header('1', 'Header 1'), 1);

      expect(header.children).toHaveLength(2);

      expect(header.getLastChild()?.id).toBe('0b');
      expect(header.getLastChild()?.children).toHaveLength(1);

      expect(header.getLastChild()?.getLastChild()?.id).toBe('1');
      expect(header.getLastChild()?.getLastChild()?.children).toHaveLength(0);
    });

    test('adds new header to nested header at depth = 2', () => {
      const header = new Header('0', 'Header 0')
        .addChild(new Header('0a', 'Header 0a'))
        .addChild(
          new Header('0b', 'Header 0b')
            .addChild(new Header('0b-1a', 'Header 0b-1a'))
            .addChild(new Header('0b-1b', 'Header 0b-1b')),
        );

      header.addChildToLastParent(new Header('2', 'Header 2'), 2);

      expect(header.children).toHaveLength(2);

      expect(header.getLastChild()?.id).toBe('0b');
      expect(header.getLastChild()?.children).toHaveLength(2);

      expect(header.getLastChild()?.getLastChild()?.id).toBe('0b-1b');
      expect(header.getLastChild()?.getLastChild()?.children).toHaveLength(1);

      expect(header.getLastChild()?.getLastChild()?.getLastChild()?.id).toBe(
        '2',
      );
      expect(
        header.getLastChild()?.getLastChild()?.getLastChild()?.children,
      ).toHaveLength(0);
    });
  });

  describe('getLastParent', () => {
    const testHeader = () =>
      new Header('0', 'Header 0')
        .addChild(new Header('0a', 'Header 0a'))
        .addChild(
          new Header('0b', 'Header 0b')
            .addChild(
              new Header('0b-1a', 'Header 0b-1a')
                .addChild(new Header('0b-1a-2a', 'Header 0b-1a-2a'))
                .addChild(new Header('0b-1a-2b', 'Header 0b-1a-2b')),
            )
            .addChild(
              new Header('0b-1b', 'Header 0b-1b')
                .addChild(new Header('0b-1b-2a', 'Header 0b-1b-2a'))
                .addChild(new Header('0b-1b-2b', 'Header 0b-1b-2b')),
            ),
        );

    test('returns root for depth = 0', () => {
      expect(testHeader().getLastParent(0)?.id).toBe('0');
    });

    test('returns root when depth < 0', () => {
      expect(testHeader().getLastParent(-1)?.id).toBe('0');
      expect(testHeader().getLastParent(-2)?.id).toBe('0');
    });

    test('returns last parent for depth = 1', () => {
      expect(testHeader().getLastParent(1)?.id).toBe('0b');
    });

    test('returns last parent for depth = 2', () => {
      expect(testHeader().getLastParent(2)?.id).toBe('0b-1b');
    });

    test('returns last parent for depth = 3', () => {
      expect(testHeader().getLastParent(3)?.id).toBe('0b-1b-2b');
    });

    test('returns undefined if depth is too high', () => {
      expect(testHeader().getLastParent(4)).toBeUndefined();
      expect(testHeader().getLastParent(5)).toBeUndefined();
    });
  });

  describe('getPrevSibling', () => {
    test('returns previous sibling', () => {
      const header = new Header('0b', 'Header 0b');

      new Header('0', 'Header 0')
        .addChild(new Header('0a', 'Header 0a'))
        .addChild(header)
        .addChild(new Header('0c', 'Header 0c'));

      expect(header.getPrevSibling()?.id).toEqual('0a');
    });

    test('returns previous sibling for nested child', () => {
      const header = new Header('0a-1b', 'Header 0a-1b');

      new Header('0', 'Header 0').addChild(
        new Header('0a', 'Header 0a')
          .addChild(new Header('0a-1a', 'Header 0a-1a'))
          .addChild(header)
          .addChild(new Header('0a-1c', 'Header 0a-1c')),
      );

      expect(header.getPrevSibling()?.id).toEqual('0a-1a');
    });

    test('returns undefined when there is previous sibling', () => {
      const header = new Header('0b', 'Header 0a');

      new Header('0', 'Header 0')
        .addChild(header)
        .addChild(new Header('0b', 'Header 0b'))
        .addChild(new Header('0c', 'Header 0c'));

      expect(header.getPrevSibling()).toBeUndefined();
    });

    test('returns undefined when there is no previous sibling to nested child', () => {
      const header = new Header('0a-1a', 'Header 0a-1a');

      new Header('0', 'Header 0').addChild(
        new Header('0a', 'Header 0a')
          .addChild(header)
          .addChild(new Header('0a-1b', 'Header 0a-1b'))
          .addChild(new Header('0a-1c', 'Header 0a-1c')),
      );

      expect(header.getPrevSibling()).toBeUndefined();
    });

    test('returns undefined when there are no siblings', () => {
      const header = new Header('0a', 'Header 0a');

      new Header('0', 'Header 0').addChild(header);

      expect(header.getPrevSibling()?.id).toBeUndefined();
    });
  });

  describe('getNextSibling', () => {
    test('returns next sibling', () => {
      const header = new Header('0b', 'Header 0b');

      new Header('0', 'Header 0')
        .addChild(new Header('0a', 'Header 0a'))
        .addChild(header)
        .addChild(new Header('0c', 'Header 0c'));

      expect(header.getNextSibling()?.id).toEqual('0c');
    });

    test('returns undefined when there is no next sibling', () => {
      const header = new Header('0b', 'Header 0a');

      new Header('0', 'Header 0')
        .addChild(new Header('0a', 'Header 0a'))
        .addChild(new Header('0b', 'Header 0b'))
        .addChild(header);

      expect(header.getNextSibling()).toBeUndefined();
    });

    test('returns undefined when there is no next sibling to nested child', () => {
      const header = new Header('0a-1c', 'Header 0a-1c');

      new Header('0', 'Header 0').addChild(
        new Header('0a', 'Header 0a')
          .addChild(new Header('0a-1a', 'Header 0a-1a'))
          .addChild(new Header('0a-1b', 'Header 0a-1b'))
          .addChild(header),
      );

      expect(header.getNextSibling()).toBeUndefined();
    });

    test('returns undefined when there are no siblings', () => {
      const header = new Header('0b', 'Header 0a');

      new Header('0', 'Header 0').addChild(header);

      expect(header.getNextSibling()?.id).toBeUndefined();
    });
  });

  describe('hasSiblings', () => {
    test('returns true if has siblings', () => {
      const header = new Header('0b', 'Header 0b');

      new Header('0', 'Header 0')
        .addChild(new Header('0a', 'Header 0a'))
        .addChild(header)
        .addChild(new Header('0c', 'Header 0c'));

      expect(header.hasSiblings()).toBe(true);
    });

    test('returns false if does not havee siblings', () => {
      const header = new Header('0b', 'Header 0a');
      expect(header.hasSiblings()).toBe(false);
    });
  });
});
