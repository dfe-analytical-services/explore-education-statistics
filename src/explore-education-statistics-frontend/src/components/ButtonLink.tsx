import classNames from 'classnames';
import Link from 'next/link';
import React from 'react';
import {
  SetCommonButtonLink,
  ButtonLinkType,
} from '@common/components/ButtonLink';
import { LinkProps } from './Link';

type Props = {
  variant?: 'secondary' | 'warning';
} & LinkProps;

const ButtonLink = ({
  children,
  className,
  prefetch,
  to,
  variant,
  ...props
}: Props) => {
  return (
    <Link
      href={to}
      prefetch={prefetch}
      {...props}
      className={classNames(
        'govuk-button',
        {
          'govuk-button--secondary': variant === 'secondary',
          'govuk-button--warning': variant === 'warning',
        },
        className,
      )}
    >
      {children}
    </Link>
  );
};

SetCommonButtonLink(ButtonLink as ButtonLinkType);

export default ButtonLink;
