import isComponentType from '@common/lib/type-guards/components/isComponentType';
import React, { cloneElement, Component, createRef, ReactNode } from 'react';
import EditableAccordionSection, {
  EditableAccordionSectionProps,
} from './EditableAccordionSection';

export interface EditableAccordionProps {
  children: ReactNode;
  id: string;
  index: number;
}

interface State {
  openAll: boolean;
}

class EditableAccordion extends Component<EditableAccordionProps, State> {
  private ref = createRef<HTMLDivElement>();

  public state = {
    openAll: false,
  };

  public componentDidMount(): void {
    this.goToHash();
    window.addEventListener('hashchange', this.goToHash);
  }

  public componentWillUnmount(): void {
    window.removeEventListener('hashchange', this.goToHash);
  }

  private goToHash = () => {
    // this.setState({ hash: window.location.hash });

    if (this.ref.current && window.location.hash) {
      try {
        const anchor = this.ref.current.querySelector(
          window.location.hash,
        ) as HTMLButtonElement;

        if (anchor) {
          anchor.scrollIntoView();
        }
      } catch (_) {
        // ignoring any errors
      }
    }
  };

  private toggleAll() {
    const { openAll } = this.state;

    this.setState({
      openAll: !openAll,
    });
  }

  public render() {
    const { children, id, index } = this.props;

    return (
      <div className="govuk-accordion" ref={this.ref} id={id}>
        <div className="govuk-accordion__controls">
          <button
            type="button"
            className="govuk-accordion__open-all"
            aria-expanded="false"
            onClick={() => this.toggleAll()}
          >
            Open all<span className="govuk-visually-hidden"> sections</span>
          </button>
        </div>
        {React.Children.map(children, (child, thisIndex) => {
          if (isComponentType(child, EditableAccordionSection)) {
            const { openAll } = this.state;

            return cloneElement<EditableAccordionSectionProps>(child, {
              index: thisIndex,
              droppableIndex: index,
              open: openAll,
            });
          }

          return child;
        })}
      </div>
    );
  }
}

export default EditableAccordion;
