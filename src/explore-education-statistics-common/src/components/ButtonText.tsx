import styles from '@common/components/ButtonText.module.scss';
import useButton, { ButtonOptions } from '@common/hooks/useButton';
import classNames from 'classnames';
import React, { Ref } from 'react';

export interface ButtonTextProps extends ButtonOptions {
  ref?: Ref<HTMLButtonElement>;
  underline?: boolean;
  variant?: 'secondary' | 'warning';
}

const ButtonText = ({
  ref,
  underline = true,
  variant,
  ...props
}: ButtonTextProps) => {
  const { className, ...button } = useButton(props);

  return (
    // eslint-disable-next-line react/button-has-type
    <button
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...button}
      className={classNames(
        styles.button,
        {
          [styles.noUnderline]: !underline,
          [styles.warning]: variant === 'warning',
          [styles.disabled]: button.disabled || button['aria-disabled'],
        },
        className,
      )}
      ref={ref}
    />
  );
};

export default ButtonText;
