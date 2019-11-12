import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { Link as RouterLink, LinkProps } from 'react-router-dom';

import { SetRealButtonLink } from '@common/components/ButtonLink';

type Props = {
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  testId?: string;
} & LinkProps;

const ButtonLink = ({
  children,
  className,
  disabled = false,
  to,
  testId,
  ...props
}: Props) => {
  return (
    <RouterLink
      {...props}
      to={to}
      className={classNames(
        'govuk-button',
        {
          'govuk-button--disabled': disabled,
        },
        className,
      )}
      role="button"
      aria-disabled={disabled}
      data-testid={testId}
    >
      {children}
    </RouterLink>
  );
};


SetRealButtonLink(ButtonLink);

export default ButtonLink;
