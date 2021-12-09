import Header from '../Header';

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

  describe('addChild', () => {
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

    test('increases `span` correctly when there are nested children', () => {
      const header = new Header('0', 'Header 0')
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
            ),
        )
        .addChild(new Header('0b', 'Header 0b'));

      expect(header.span).toBe(5);

      expect(header.children[0].span).toBe(4);

      expect(header.children[0].children[0].span).toBe(2);
      expect(header.children[0].children[0].children[0].span).toBe(1);
      expect(header.children[0].children[0].children[1].span).toBe(1);

      expect(header.children[0].children[1].span).toBe(2);
      expect(header.children[0].children[1].children[0].span).toBe(1);
      expect(header.children[0].children[1].children[1].span).toBe(1);

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
    const header = new Header('0', 'Header 0')
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
      expect(header.getLastParent(0)?.id).toBe('0');
    });

    test('returns root when depth < 0', () => {
      expect(header.getLastParent(-1)?.id).toBe('0');
      expect(header.getLastParent(-2)?.id).toBe('0');
    });

    test('returns last parent for depth = 1', () => {
      expect(header.getLastParent(1)?.id).toBe('0b');
    });

    test('returns last parent for depth = 2', () => {
      expect(header.getLastParent(2)?.id).toBe('0b-1b');
    });

    test('returns last parent for depth = 3', () => {
      expect(header.getLastParent(3)?.id).toBe('0b-1b-2b');
    });

    test('returns undefined if depth is too high', () => {
      expect(header.getLastParent(4)).toBeUndefined();
      expect(header.getLastParent(5)).toBeUndefined();
    });
  });
});
