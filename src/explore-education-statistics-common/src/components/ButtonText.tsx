import styles from '@common/components/ButtonText.module.scss';
import useButton, { ButtonOptions } from '@common/hooks/useButton';
import classNames from 'classnames';
import React, { forwardRef, Ref } from 'react';

const ButtonText = (props: ButtonOptions, ref: Ref<HTMLButtonElement>) => {
  const { className, isDisabled, underline, variant, ...button } =
    useButton(props);

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
          [styles.disabled]: isDisabled,
        },
        className,
      )}
      ref={ref}
    />
  );
};

export default forwardRef(ButtonText);
