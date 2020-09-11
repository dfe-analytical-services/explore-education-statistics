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
} & LinkProps;

const ButtonLink = ({
  children,
  className,
  disabled = false,
  to,
  prefetch,
  ...props
}: Props) => {
  const isAbsolute = typeof to === 'string' && to.startsWith('http');

  const link = (
    <a
      {...props}
      href={isAbsolute ? (to as string) : undefined}
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
