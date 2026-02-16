import styles from '@common/components/Button.module.scss';
import useButton, { ButtonOptions } from '@common/hooks/useButton';
import classNames from 'classnames';
import React, { Ref } from 'react';

export interface ButtonProps extends ButtonOptions {
  ref?: Ref<HTMLButtonElement>;
  variant?: 'secondary' | 'warning';
}

function Button({ ref, variant, ...props }: ButtonProps) {
  const { className, ...button } = useButton(props);

  return (
    // eslint-disable-next-line react/button-has-type
    <button
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...button}
      className={classNames(
        'govuk-button',
        {
          [styles.disabled]: button.disabled || button['aria-disabled'],
          'govuk-button--secondary': variant === 'secondary',
          'govuk-button--warning': variant === 'warning',
        },
        className,
      )}
      ref={ref}
    />
  );
}

export default Button;
