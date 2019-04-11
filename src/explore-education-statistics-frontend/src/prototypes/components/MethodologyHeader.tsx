import React, { UIEvent } from 'react';
import { MethodologySection } from './MethodologySection';

export interface MethodologyHeaderProps {
  children: React.ReactNode;
  parent?: MethodologySection;
}

export class MethodologyHeader extends React.Component<MethodologyHeaderProps> {
  private updateElements() {
    if (this.element && this.props.parent && this.staticRef) {
      const parentTop = this.props.parent.scrollTop;
      const parentBottom = parentTop + this.props.parent.height;
      const headerHeight = this.element.clientHeight;
      const headerBottom = parentBottom - headerHeight;

      if (parentTop < 0) {
        this.staticRef.classList.add('fixed');
        if (headerBottom > 0) {
          this.staticRef.style.top = `${-parentTop}px`;
        } else {
          this.staticRef.style.top = `${this.props.parent.height -
            headerHeight}px`;
        }
      } else {
        this.staticRef.classList.remove('fixed');
        this.staticRef.style.top = '';
      }
    }
  }

  private scroll = (e: Event) => {
    requestAnimationFrame(() => this.updateElements());
  };
  private scrollHandler?: EventListener;

  private element: HTMLElement | null;
  private staticRef: HTMLElement | null;

  constructor(props: MethodologyHeaderProps) {
    super(props);
    this.element = null;
    this.staticRef = null;
  }

  public componentDidMount(): void {
    this.scrollHandler = e => this.scroll(e);
    window.addEventListener('scroll', this.scrollHandler);

    requestAnimationFrame(() => this.updateElements());
  }

  public componentWillUnmount(): void {
    if (this.scrollHandler) {
      window.removeEventListener('scroll', this.scrollHandler);
    }
  }

  public render() {
    return (
      <div
        className="govuk-grid-column-one-quarter"
        ref={ref => (this.element = ref)}
      >
        <div ref={ref => (this.staticRef = ref)}>{this.props.children}</div>
      </div>
    );
  }
}
