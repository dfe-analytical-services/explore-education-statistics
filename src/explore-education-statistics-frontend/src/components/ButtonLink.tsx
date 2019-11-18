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
  href,
  prefetch,
  ...props
}: Props) => {
  return (
    <Link {...props} prefetch={prefetch} href={href || to}>
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
    </Link>
  );
};

SetCommonButtonLink(ButtonLink as ButtonLinkType);

export default ButtonLink;
