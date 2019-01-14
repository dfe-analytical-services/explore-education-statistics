import classNames from 'classnames';
import React, { Component, ReactNode, SyntheticEvent } from 'react';
import styles from './CollapsibleSection.module.scss';

interface Props {
  children: ReactNode;
  heading: ReactNode | string;
  contentId?: string;
  open?: boolean;
}

interface State {
  isCollapsed: boolean;
}

let idCounter = 0;

class CollapsibleSection extends Component<Props, State> {
  public static defaultProps = {
    open: false,
  };

  public state = {
    contentId:
      this.props.contentId || `collapsible-section-${(idCounter += 1)}`,
    isCollapsed: !this.props.open,
  };

  public render() {
    const { heading, children } = this.props;
    const { isCollapsed, contentId } = this.state;

    return (
      <div>
        <h3 className={styles.heading}>
          <button
            aria-controls={contentId}
            aria-expanded={!isCollapsed}
            className={styles.collapseToggle}
            onClick={this.handleClick}
          >
            {heading}
          </button>
        </h3>

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
