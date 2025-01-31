import classNames from 'classnames';
import clamp from 'lodash/clamp';
import React from 'react';
import styles from './ProgressBar.module.scss';

interface Props {
  className?: string;
  height?: number | string;
  id?: string;
  max?: number;
  min?: number;
  showText?: boolean;
  value: number;
  width?: number | string;
}

const ProgressBar = ({
  className,
  height,
  id,
  min = 0,
  max = 100,
  showText = true,
  value,
  width,
}: Props) => {
  const scaledValue = value - min;
  const scaledMax = max - min;
  const percentage = clamp((scaledValue / scaledMax) * 100, 0, 100).toFixed(0);

  return (
    <>
      <div
        aria-valuemin={min}
        aria-valuemax={max}
        aria-valuenow={clamp(value, min, max)}
        className={classNames(styles.container, className)}
        id={id}
        role="progressbar"
        style={{ height, width }}
      >
        <div
          className={styles.bar}
          style={{
            height,
            width: `${percentage}%`,
          }}
        />
      </div>

      {showText && <small>{`${percentage}% complete`}</small>}
    </>
  );
};

export default ProgressBar;
