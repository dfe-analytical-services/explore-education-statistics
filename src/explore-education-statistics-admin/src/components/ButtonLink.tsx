import classNames from 'classnames';
import React, { AnchorHTMLAttributes, ReactNode } from 'react';

type Props = {
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  to?: string ;
} & AnchorHTMLAttributes<HTMLAnchorElement>;

const ButtonLink = ({
  children,
  className,
  disabled = false,
  to,
  type = 'button',
  ...props
}: Props) => {
  // We support href and to for backwards
  // compatibility with react-router.
  const href = props.href || to;

  return (
      <a
        {...props}
        className={classNames(
          'govuk-button',
          {
            'govuk-button--disabled': disabled,
          },
          className,
        )}
        role="button"
        aria-disabled={disabled}
      >
        {children}
      </a>
  );
};

export default ButtonLink;
