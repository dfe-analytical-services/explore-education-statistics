import classNames from 'classnames';
import Link from 'next/link';
import React from 'react';
import {
  SetCommonButtonLink,
  ButtonLinkType,
} from '@common/components/ButtonLink';
import { LinkProps } from './Link';

type Props = {
  disabled?: boolean;
  variant?: 'secondary' | 'warning';
} & LinkProps;

const ButtonLink = ({
  children,
  className,
  disabled = false,
  prefetch,
  to,
  variant,
  ...props
}: Props) => {
  const isAbsolute = typeof to === 'string' && to.startsWith('http');

  const link = (
    <a
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...props}
      href={isAbsolute ? (to as string) : undefined}
      className={classNames(
        'govuk-button',
        {
          'govuk-button--disabled': disabled,
          'govuk-button--secondary': variant === 'secondary',
          'govuk-button--warning': variant === 'warning',
        },
        className,
      )}
      aria-disabled={disabled}
    >
      {children}
    </a>
  );

  if (isAbsolute) {
    return link;
  }

  return (
    <Link {...props} prefetch={prefetch} href={to} passHref>
      {link}
    </Link>
  );
};

SetCommonButtonLink(ButtonLink as ButtonLinkType);

export default ButtonLink;
