import classNames from 'classnames';
import React, { CSSProperties, ReactNode } from 'react';
import styles from './AxisLabel.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  rotated?: boolean;
  style?: CSSProperties;
  width?: number;
}

const AxisLabel = ({ children, className, rotated, style, width }: Props) => {
  return (
    <div
      aria-hidden
      className={classNames(styles.label, className)}
      style={style}
    >
      <div
        className={classNames({
          [styles.labelRotated]: rotated,
        })}
      >
        <div
          className="dfe-align--centre"
          style={{
            width,
          }}
        >
          {children}
        </div>
      </div>
    </div>
  );
};

export default AxisLabel;
