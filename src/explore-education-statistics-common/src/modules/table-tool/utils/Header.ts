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

  public readonly children: Header[] = [];

  public parent?: Header;

  constructor(id: string, text: string) {
    this.id = id;
    this.text = text;
  }

  public get depth(): number {
    if (!this.parent) {
      return 0;
    }

    return this.parent?.depth + 1;
  }

  public get crossSpan(): number {
    if (this.children.length > 1) {
      return 1;
    }

    let crossSpan = 1;
    let child = this.getFirstChild();

    while (child) {
      if (child.text === this.text && child.span === this.span) {
        crossSpan += 1;
      }

      if (child.children.length === 1) {
        child = child.getFirstChild();
      } else {
        child = undefined;
      }
    }

    return crossSpan;
  }

  public addChild(child: Header): this {
    const lastChild = this.getLastChild();

    if (lastChild?.id === child.id) {
      lastChild.span += child.span;
    } else {
      // eslint-disable-next-line no-param-reassign
      child.parent = this;
      this.children.push(child);
    }

    this.span = sum(this.children.map(c => c.span));

    this.updateParent();

    return this;
  }

  private updateParent(): void {
    let { parent } = this;

    while (parent) {
      parent.span = sum(parent.children.map(child => child.span));
      parent = parent.parent;
    }
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

  public hasChildren(): boolean {
    return this.children.length > 0;
  }

  public getFirstChild(): Header | undefined {
    return this.children[0];
  }

  public getLastChild(): Header | undefined {
    return last(this.children);
  }

  public hasSiblings(): boolean {
    return !!this.parent && this.parent.children.length > 1;
  }

  public getPrevSibling(): Header | undefined {
    if (!this.parent) {
      return undefined;
    }

    const index = this.parent.children.indexOf(this);

    return this.parent.children[index - 1];
  }

  public getNextSibling(): Header | undefined {
    if (!this.parent) {
      return undefined;
    }

    const index = this.parent.children.indexOf(this);

    return this.parent.children[index + 1];
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
