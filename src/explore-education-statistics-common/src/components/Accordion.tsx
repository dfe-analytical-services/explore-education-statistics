import isComponentType from '@common/lib/type-guards/components/isComponentType';
import classNames from 'classnames';
import React, { cloneElement, Component, createRef, ReactNode } from 'react';
import styles from './Accordion.module.scss';
import AccordionSection, {
  AccordionSectionProps,
  classes,
} from './AccordionSection';

export interface AccordionProps {
  children: ReactNode;
  id: string;
}

interface State {
  openSectionId: string;
}

class Accordion extends Component<AccordionProps, State> {
  public state = {
    openSectionId: '',
  };

  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    import('govuk-frontend/components/accordion/accordion').then(
      ({ default: GovUkAccordion }) => {
        if (this.ref.current) {
          new GovUkAccordion(this.ref.current).init();
        }
      },
    );

    this.goToHash();
    window.addEventListener('hashchange', this.goToHash);
  }

  public componentWillUnmount(): void {
    window.removeEventListener('hashchange', this.goToHash);
  }

  private goToHash = () => {
    if (this.ref.current && location.hash) {
      const locationHashEl = this.ref.current.querySelector(location.hash);

      if (locationHashEl) {
        const sectionEl = locationHashEl.closest(`.${classes.section}`);

        if (sectionEl) {
          const contentEl = sectionEl.querySelector(
            `.${classes.sectionContent}`,
          );

          if (contentEl) {
            if (window.sessionStorage.getItem(contentEl.id) === 'false') {
              window.sessionStorage.removeItem(contentEl.id);
            }

            this.setState(
              {
                openSectionId: contentEl.id,
              },
              () => {
                locationHashEl.scrollIntoView({ block: 'start' });
              },
            );
          }
        }
      }
    }
  };

  public render() {
    const { children, id } = this.props;
    const { openSectionId } = this.state;

    let sectionCounter = 0;

    return (
      <div
        className={classNames('govuk-accordion', styles.accordionPrint)}
        ref={this.ref}
        id={id}
      >
        {React.Children.map(children, child => {
          if (isComponentType(child, AccordionSection)) {
            sectionCounter += 1;

            const contentId = `${id}-content-${sectionCounter}`;
            const headingId = `${id}-heading-${sectionCounter}`;

            const isSectionOpen =
              openSectionId === headingId || openSectionId === contentId;

            return cloneElement<AccordionSectionProps>(child, {
              contentId,
              headingId,
              open: child.props.open || isSectionOpen,
            });
          }

          return child;
        })}
      </div>
    );
  }
}

export default Accordion;
