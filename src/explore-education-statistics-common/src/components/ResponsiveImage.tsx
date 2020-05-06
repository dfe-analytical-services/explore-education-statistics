import classNames from 'classnames';
import React from 'react';
import styles from './ResponsiveImage.module.scss';

interface Props {
  alt: string;
  className?: string;
  src: string;
  height?: string;
  width?: string;
}

const ResponsiveImage = ({ alt, className, src, height, width }: Props) => {
  return (
    <img
      className={classNames(styles.image, className)}
      alt={alt}
      src={src}
      style={{
        width,
        maxHeight: height,
      }}
    />
  );
};

export default ResponsiveImage;
