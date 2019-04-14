import React from 'react';
import { MethodologySection } from 'src/prototypes/methodology/components/MethodologySection';

export interface MethodologyHeaderProps {
  children: React.ReactNode;
  parent?: MethodologySection;
}

export class MethodologyHeader extends React.Component<MethodologyHeaderProps> {
  private element: HTMLElement | null;
  private staticRef: HTMLElement | null;

  public constructor(props: MethodologyHeaderProps) {
    super(props);
    this.element = null;
    this.staticRef = null;
  }

  private scroll = () => requestAnimationFrame(() => this.updateElements());

  public componentDidMount(): void {
    window.addEventListener('scroll', this.scroll);

    requestAnimationFrame(() => this.updateElements());
  }

  public componentWillUnmount(): void {
      window.removeEventListener('scroll', this.scroll);
  }

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
