import last from 'lodash/last';
import sum from 'lodash/sum';

/**
 * Class for defining a header within a table.
 * We represent this as a tree data structure so that
 * we can group together related table headers.
 */
export default class Header {
  public readonly id: string;

  public readonly text: string;

  public span = 1;

  public children: Header[] = [];

  public parent?: Header;

  constructor(id: string, text: string) {
    this.id = id;
    this.text = text;
  }

  private updateSpan(): void {
    this.span = sum(this.children.map(child => child.span));

    if (this.parent) {
      this.parent.updateSpan();
    }
  }

  public addChild(child: Header): this {
    const lastChild = last(this.children);

    if (lastChild?.id === child.id) {
      lastChild.span += child.span;
    } else {
      // eslint-disable-next-line no-param-reassign
      child.parent = this;
      this.children.push(child);
    }

    this.updateSpan();

    return this;
  }

  public addChildToLastParent(child: Header, depth: number) {
    const parent = this.getLastParent(depth);

    if (!parent) {
      throw new Error(
        `Could not add child to undefined parent header at depth '${depth}'`,
      );
    }

    return parent.addChild(child);
  }

  public get depth(): number {
    if (!this.parent) {
      return 0;
    }

    return this.parent?.depth + 1;
  }

  public hasChildren(): boolean {
    return this.children.length > 0;
  }

  public getLastChild(): Header | undefined {
    return last(this.children);
  }

  public getLastParent(depth = 0): Header | undefined {
    if (depth < 0) {
      return this;
    }

    let currentDepth = 0;
    // eslint-disable-next-line @typescript-eslint/no-this-alias
    let currentHeader: Header | undefined = this;

    while (currentDepth < depth && currentHeader) {
      currentHeader = currentHeader.getLastChild();
      currentDepth += 1;
    }

    if (currentDepth !== depth) {
      return undefined;
    }

    return currentHeader;
  }
}
