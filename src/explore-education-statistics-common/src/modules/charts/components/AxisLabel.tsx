import classNames from 'classnames';
import React, { CSSProperties, ReactNode } from 'react';
import styles from './AxisLabel.module.scss';

interface Props {
  'data-testid'?: string;
  children: ReactNode;
  className?: string;
  rotated?: boolean;
  style?: CSSProperties;
  textStyle?: CSSProperties;
}

const AxisLabel = ({
  'data-testid': testId,
  children,
  className,
  rotated,
  style,
  textStyle,
}: Props) => {
  return (
    <div
      aria-hidden
      data-testid={testId}
      className={classNames(styles.label, className)}
      style={style}
    >
      <div
        className={classNames({
          [styles.labelRotated]: rotated,
        })}
      >
        <div className="dfe-align--centre" style={textStyle}>
          {children}
        </div>
      </div>
    </div>
  );
};

export default AxisLabel;
