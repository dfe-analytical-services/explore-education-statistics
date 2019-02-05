import GovUkDetails from 'govuk-frontend/components/details/details';
import React, {
  Component,
  createRef,
  MouseEventHandler,
  ReactNode,
} from 'react';
import styles from './MenuDetails.module.scss';

interface Props {
  children?: ReactNode;
  onToggle?: MouseEventHandler<HTMLDetailsElement>;
  open?: boolean;
  summary: string;
}

class MenuDetails extends Component<Props> {
  private ref = createRef<HTMLDetailsElement>();

  public componentDidMount(): void {
    new GovUkDetails(this.ref.current).init();
  }

  public render() {
    const { children, open, onToggle, summary } = this.props;

    return (
      <details
        className={styles.details}
        open={open}
        ref={this.ref}
        onClick={onToggle}
      >
        <summary className="govuk-details__summary">
          <span className="govuk-details__summary-text">{summary}</span>
        </summary>
        <div className={styles.content} onClick={e => e.stopPropagation()}>
          {children}
        </div>
      </details>
    );
  }
}

export default MenuDetails;
