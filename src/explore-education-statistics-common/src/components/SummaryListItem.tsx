import React, { ReactNode, useState } from 'react';
import classNames from 'classnames';
import ButtonText from './ButtonText';

interface Props {
  actions?: ReactNode;
  children?: ReactNode;
  term: string;
  detailsNoMargin?: boolean;
  smallKey?: boolean;
  showActions?: boolean;
  shouldCollapse?: boolean;
  collapseAfter?: number;
}

const SummaryListItem = ({
  actions,
  children,
  term,
  detailsNoMargin,
  smallKey = false,
  shouldCollapse = false,
  collapseAfter = 5,
}: Props) => {
  const [collapsed, setCollapsed] = useState<boolean>(shouldCollapse);

  function getCollapsedList() {
    if (React.Children.count(children) > collapseAfter) {
      if (collapsed) {
        return (
          <>
            {React.Children.map(children, (child, i) => {
              if (i >= collapseAfter - 2) {
                return null;
              }
              return child;
            })}
            {React.Children.count(children) - (collapseAfter - 2) && (
              <strong>
                {`And ${React.Children.count(children) -
                  (collapseAfter - 2)} more...`}
                <br />
              </strong>
            )}
            <ButtonText onClick={() => setCollapsed(!collapsed)}>
              {collapsed ? 'Show All' : 'Collapse List'}
            </ButtonText>
          </>
        );
      }
      return (
        <>
          {children}
          <ButtonText onClick={() => setCollapsed(!collapsed)}>
            {collapsed ? 'Show All' : 'Collapse List'}
          </ButtonText>
        </>
      );
    }
    return children;
  }

  return (
    <div className="govuk-summary-list__row">
      <dt
        className={classNames('govuk-summary-list__key', {
          'dfe-details-no-margin': detailsNoMargin,
          'dfe-summary-list__key--small': smallKey,
        })}
      >
        {term}
      </dt>
      {children && (
        <dd
          className={classNames('govuk-summary-list__value', {
            'dfe-details-no-margin': detailsNoMargin,
          })}
        >
          {shouldCollapse ? getCollapsedList() : children}
        </dd>
      )}
      {actions && <dd className="govuk-summary-list__actions">{actions}</dd>}
      {!children && !actions && (
        <>
          <dd
            className={classNames('govuk-summary-list__value', {
              'dfe-details-no-margin': detailsNoMargin,
            })}
          />
          <dd className="govuk-summary-list__actions" />
        </>
      )}
    </div>
  );
};

export default SummaryListItem;
