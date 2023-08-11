import styles from '@common/components/Button.module.scss';
import useButton, { ButtonOptions } from '@common/hooks/useButton';
import classNames from 'classnames';
import React, { forwardRef, Ref } from 'react';

function Button(props: ButtonOptions, ref: Ref<HTMLButtonElement>) {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const { className, isDisabled, underline, variant, ...button } =
    useButton(props);

  return (
    // eslint-disable-next-line react/button-has-type
    <button
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...button}
      className={classNames(
        'govuk-button',
        {
          [styles.disabled]: isDisabled,
          'govuk-button--disabled': isDisabled,
          'govuk-button--secondary': variant === 'secondary',
          'govuk-button--warning': variant === 'warning',
        },
        className,
      )}
      ref={ref}
    />
  );
}

export default forwardRef(Button);
