import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { Link as RouterLink, LinkProps } from 'react-router-dom';

import {
  SetCommonButtonLink,
  ButtonLinkType,
} from '@common/components/ButtonLink';

type Props = {
  children: ReactNode;
  className?: string;
  variant?: 'secondary' | 'warning';
} & LinkProps;

const ButtonLink = ({ children, className, to, variant, ...props }: Props) => {
  return (
    <RouterLink
      {...props}
      to={to}
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
    </RouterLink>
  );
};

SetCommonButtonLink(ButtonLink as ButtonLinkType);

export default ButtonLink;
