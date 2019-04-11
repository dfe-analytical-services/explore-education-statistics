import classNames from 'classnames';
import React, {AnchorHTMLAttributes, ReactNode} from 'react';
import {Link as ReactLink} from 'react-router-dom';

type Props = {
  as?: string;
  children: ReactNode;
  className?: string;
  prefetch?: boolean;
  to?: string;
  unvisited?: boolean;
} & AnchorHTMLAttributes<HTMLAnchorElement>;

const Link = ({
                as,
                children,
                className,
                prefetch,
                to,
                unvisited = false,
                ...props
              }: Props) => {
  // We support href and to for backwards
  // compatibility with react-router.
  const href = props.href || to;

  return (
    <ReactLink to={to || ""} className={classNames('govuk-link', {
        'govuk-link--no-visited-state': unvisited,}, className,)}
    >
      {children}
    </ReactLink>
  );
};

export default Link;
