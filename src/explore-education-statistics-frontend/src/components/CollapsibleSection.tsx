import classNames from 'classnames';
import React, { Component, ReactNode, SyntheticEvent } from 'react';
import styles from './CollapsibleSection.module.scss';

interface Props {
  caption?: string;
  children: ReactNode;
  className?: string;
  heading: string;
  // Only for accessibility/semantic markup,
  // does not change the actual styling
  headingTag?: 'h2' | 'h3' | 'h4';
  contentId?: string;
  open?: boolean;
}

interface State {
  isCollapsed: boolean;
}

let idCounter = 0;

class CollapsibleSection extends Component<Props, State> {
  public static defaultProps: Partial<Props> = {
    headingTag: 'h2',
    open: false,
  };

  public state = {
    contentId:
      this.props.contentId || `collapsible-section-${(idCounter += 1)}`,
    isCollapsed: !this.props.open,
  };

  public render() {
    const { className, heading, headingTag, caption, children } = this.props;
    const { isCollapsed, contentId } = this.state;

    return (
      <div className={className}>
        {React.createElement(
          `${headingTag}`,
          {
            className: styles.heading,
          },
          <>
            <button
              aria-controls={contentId}
              aria-expanded={!isCollapsed}
              className={styles.collapseToggle}
              onClick={this.handleClick}
            >
              {heading}
            </button>
            {caption && <span className={styles.caption}>{caption}</span>}
          </>,
        )}

        <div
          className={classNames(styles.content, {
            [styles.collapsed]: isCollapsed,
          })}
          hidden={isCollapsed}
          id={contentId}
        >
          {children}
        </div>
      </div>
    );
  }

  private handleClick = (e: SyntheticEvent) => {
    e.preventDefault();

    this.setState({
      isCollapsed: !this.state.isCollapsed,
    });
  };
}

export default CollapsibleSection;
