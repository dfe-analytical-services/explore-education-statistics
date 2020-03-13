import Tag from '@common/components/Tag';
import React from 'react';
import classNames from 'classnames';
import styles from '@admin/pages/release/edit-release/data/ReleaseDataUploadsSection.module.scss';

export interface StatusBlockProps {
  className?: string;
  color?: 'blue' | 'orange' | 'red' | 'green';
  text: string;
}

const StatusBlock = ({ color, text, className }: StatusBlockProps) => {
  const colorClass = () => {
    switch (color) {
      case 'orange':
        return [styles.ragStatusAmber];
      case 'red':
        return [styles.ragStatusRed];
      case 'green':
        return [styles.ragStatusGreen];
      default:
        return undefined;
    }
  };

  return (
    <Tag className={classNames(colorClass(), className)} strong>
      {text}
    </Tag>
  );
};

export default StatusBlock;
